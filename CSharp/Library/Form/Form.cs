using Microsoft.Bot.Builder.Form.Advanced;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Form
{
    /// <summary>
    /// Form dialog manager for to fill in your state.
    /// </summary>
    /// <typeparam name="T">The type to fill in.</typeparam>
    /// <remarks>
    /// This is the root class for creating a form.  To use it, you:
    /// * Create an instance of this class parameterized with the class you want to fill in.
    /// * Optionally use the fluent API to specify the order of fields, messages and confirmations.
    /// * Register with the global dialog collection.
    /// * Start the form dialog.
    /// </remarks>
    [Serializable]
    public sealed class Form<T> : IForm<T>, ISerializable
         where T : class, new()
    {
        private readonly MakeModel _makeModel;
        private readonly IFormModel<T> _model;
        private FormState _form;
        private T _state;
        private IRecognize<T> _commands;

        public delegate IFormModel<T> MakeModel();

        /// <summary>
        /// Construct a form.
        /// </summary>
        /// <param name="id">Unique dialog id to register with dialog system.</param>
        public Form(MakeModel makeModel)
        {
            _makeModel = makeModel;
            _model = _makeModel();

            this._commands = this._model.BuildCommandRecognizer();
        }

        private Form(SerializationInfo info, StreamingContext context)
        {
            Microsoft.Bot.Builder.Field.SetNotNullFrom(out this._makeModel, nameof(this._makeModel), info);
            this._form = info.GetValue<FormState>(nameof(this._form));
            this._state = info.GetValue<T>(nameof(this._state));

            _model = _makeModel();
            this._commands = this._model.BuildCommandRecognizer();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this._makeModel), this._makeModel);

            info.AddValue(nameof(this._form), this._form);
            info.AddValue(nameof(this._state), this._state);
        }

        #region IForm<T> statics
#if DEBUG
        public static bool DebugRecognizers = false;
#endif
        #endregion

        #region IForm<T> implementation

        IFormModel<T> IForm<T>.Model { get { return this._model; } }

        #endregion

        #region IDialog implementation

        /// <summary>
        /// Initial state for a Microsoft.Bot.Builder.Form.Form.
        /// </summary>
        /// <remarks>
        /// If a parent dialog wants to pass in the initial state of the form, you would use this structure.
        /// It includes both the state and optionally initial entities from a LUIS dialog that will be used to 
        /// initially populate the form state.
        /// </remarks>
        public class InitialState
        {
            /// <summary>
            /// Default form state.
            /// </summary>
            public T State { get; set; }

            /// <summary>
            /// LUIS entities to put into state.
            /// </summary>
            /// <remarks>
            /// In order to set a field in the form state, the Entity must be named with the path to the field in the form state.
            /// </remarks>
            public Models.EntityRecommendation[] Entities { get; set; }

            /// <summary>
            /// Whether this form should prompt the user when started.
            /// </summary>
            public bool PromptInStart { get; set; }
        }

        async Task IDialog.StartAsync(IDialogContext context, IAwaitable<object> arguments)
        {
            var initialState = await arguments as InitialState;
            bool skipFields = false;

            if (initialState == null)
            {
                if (context.UserData.TryGetValue(typeof(T).Name, out _state))
                {
                    skipFields = true;
                }
                else
                {
                    _state = new T();
                }
            }
            else
            {
                _state = initialState.State;
                if (_state == null) _state = new T();
                skipFields = true;
            }
            // TODO: Hook up culture state in form

            _form = new FormState(_model.Steps.Count, CultureInfo.InvariantCulture);
            if (initialState != null && initialState.Entities != null)
            {
                var entities = (from entity in initialState.Entities group entity by entity.Type);
                foreach (var entityGroup in entities)
                {
                    var step = _model.Step(entityGroup.Key);
                    if (step != null)
                    {
                        _form.Step = _model.StepIndex(step);
                        _form.StepState = null;
                        var builder = new StringBuilder();
                        foreach (var entity in entityGroup)
                        {
                            builder.Append(entity.Entity);
                            builder.Append(' ');
                        }
                        var input = builder.ToString();
                        string feedback;
                        string prompt = step.Start(context, _state, _form);
                        var matches = MatchAnalyzer.Coalesce(step.Match(context, _state, _form, input, out prompt), input);
                        if (MatchAnalyzer.IsFullMatch(input, matches, 0.5))
                        {
                            // TODO: In the case of clarification I could
                            // 1) Go through them while supporting only quit or back and reset
                            // 2) Drop them
                            // 3) Just pick one (found in form.StepState, but that is opaque here)
                            step.Process(context, _state, _form, input, matches, out feedback, out prompt);
                        }
                        else
                        {
                            _form.SetPhase(StepPhase.Ready);
                        }
                    }
                }
                _form.Step = 0;
                _form.StepState = null;
            }

            if (skipFields)
            {
                // Mark all fields with values as completed.
                for (var i = 0; i < _model.Steps.Count; ++i)
                {
                    var step = _model.Steps[i];
                    if (step.Type == StepType.Field)
                    {
                        if (!step.Field.IsUnknown(_state))
                        {
                            _form.Phases[i] = StepPhase.Completed;
                        }
                    }
                }
            }
            if (initialState != null && initialState.PromptInStart)
            {
                await MessageReceived(context, null);
            }
            else
            {
                context.Wait(MessageReceived);
            }
        }

        public async Task MessageReceived(IDialogContext context, IAwaitable<Connector.Message> toBot)
        {
            var toBotText = toBot != null ? (await toBot).Text : null;
            string message = null;
            string prompt = null;
            bool useLastPrompt = false;
            bool requirePrompt = false;
            var next = (_form.Next == null ? new NextStep() : ActiveSteps(_form.Next, _state));
            while (prompt == null && (message == null || requirePrompt) && MoveToNext(_state, _form, next))
            {
                IStep<T> step;
                IEnumerable<TermMatch> matches = null;
                string lastInput = null;
                string feedback = null;
                if (next.Direction == StepDirection.Named && next.Names.Count() > 1)
                {
                    // We need to choose between multiple next steps
                    bool start = (_form.Next == null);
                    _form.Next = next;
                    step = new NavigationStep<T>(_model.Steps[_form.Step].Name, _model, _state, _form);
                    if (start)
                    {
                        prompt = step.Start(context, _state, _form);
                    }
                    else
                    {
                        matches = step.Match(context, _state, _form, toBotText, out lastInput);
                    }
                }
                else
                {
                    // Processing current step
                    step = _model.Steps[_form.Step];
                    if (_form.Phase() == StepPhase.Ready)
                    {
                        if (step.Type == StepType.Message)
                        {
                            feedback = step.Start(context, _state, _form);
                            requirePrompt = true;
                            useLastPrompt = false;
                            next = new NextStep();
                        }
                        else
                        {
                            prompt = step.Start(context, _state, _form);
                        }
                    }
                    else if (_form.Phase() == StepPhase.Responding)
                    {
                        matches = step.Match(context, _state, _form, toBotText, out lastInput);
                    }
                }
                if (matches != null)
                {
                    matches = MatchAnalyzer.Coalesce(matches, lastInput).ToArray();
                    if (MatchAnalyzer.IsFullMatch(lastInput, matches))
                    {
                        next = step.Process(context, _state, _form, lastInput, matches, out feedback, out prompt);
                        // 1) Not completed, not valid -> Not require, last
                        // 2) Completed, feedback -> require, not last
                        requirePrompt = (_form.Phase() == StepPhase.Completed);
                        useLastPrompt = !requirePrompt;
                    }
                    else
                    {
                        // Filter non-active steps out of command matches
                        var commands =
                            (from command in MatchAnalyzer.Coalesce(_commands.Matches(lastInput), lastInput)
                             where (command.Value is FormCommand
                                 || _model.Fields.Field(command.Value as string).Active(_state))
                             select command).ToArray();
                        if (MatchAnalyzer.IsFullMatch(lastInput, commands))
                        {
                            next = DoCommand(context, _state, _form, step, commands, out feedback);
                            requirePrompt = false;
                            useLastPrompt = true;
                        }
                        else
                        {
                            if (matches.Count() == 0 && commands.Count() == 0)
                            {
                                // TODO: If we implement fallback, opportunity to call parent dialogs
                                feedback = step.NotUnderstood(context, _state, _form, lastInput);
                                requirePrompt = false;
                                useLastPrompt = false;
                            }
                            else
                            {
                                // Go with response since it looks possible
                                var bestMatch = MatchAnalyzer.BestMatches(matches, commands);
                                if (bestMatch == 0)
                                {
                                    next = step.Process(context, _state, _form, lastInput, matches, out feedback, out prompt);
                                    requirePrompt = (_form.Phase() == StepPhase.Completed);
                                    useLastPrompt = !requirePrompt;
                                }
                                else
                                {
                                    next = DoCommand(context, _state, _form, step, commands, out feedback);
                                    requirePrompt = false;
                                    useLastPrompt = true;
                                }
                            }
                        }
                    }
                }
                next = ActiveSteps(next, _state);
                if (feedback != null)
                {
                    message = (message == null ? feedback : message + "\n\n" + feedback);
                }
            }
            if (next.Direction == StepDirection.Complete || next.Direction == StepDirection.Quit)
            {
                if (next.Direction == StepDirection.Complete)
                {
                    if (_model.Completion != null)
                    {
                        _model.Completion(context, _state);
                    }
                    context.Done(_state);
                }
                else if (next.Direction == StepDirection.Quit)
                {
                    throw new OperationCanceledException();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (message != null)
                {
                    if (requirePrompt)
                    {
                        _form.LastPrompt = prompt;
                        prompt = message + "\n\n" + prompt;
                    }
                    else if (useLastPrompt)
                    {
                        prompt = message + "\n\n" + _form.LastPrompt;
                    }
                    else
                    {
                        prompt = message;
                    }
                }
                else
                {
                    _form.LastPrompt = prompt;
                }

                await context.PostAsync(prompt);
                context.Wait(MessageReceived);
            }
        }

        #endregion

        #region Implementation

        private NextStep ActiveSteps(NextStep next, T state)
        {
            var result = next;
            if (next.Direction == StepDirection.Named)
            {
                var names = (from name in next.Names where _model.Fields.Field(name).Active(state) select name);
                var count = names.Count();
                if (count == 0)
                {
                    result = new NextStep();
                }
                else if (count != result.Names.Count())
                {
                    result = new NextStep(names);
                }
            }
            return result;
        }

        /// <summary>
        /// Find the next step to execute.
        /// </summary>
        /// <param name="state">The current state.</param>
        /// <param name="form">The current form state.</param>
        /// <param name="next">What step to execute next.</param>
        /// <returns>True if can switch to step.</returns>
        private bool MoveToNext(T state, FormState form, NextStep next)
        {
            bool found = false;
            switch (next.Direction)
            {
                case StepDirection.Complete:
                    break;
                case StepDirection.Named:
                    form.StepState = null;
                    if (next.Names.Count() == 0)
                    {
                        goto case StepDirection.Next;
                    }
                    else if (next.Names.Count() == 1)
                    {
                        var name = next.Names.First();
                        var nextStep = -1;
                        for (var i = 0; i < _model.Steps.Count(); ++i)
                        {
                            if (_model.Steps[i].Name == name)
                            {
                                nextStep = i;
                                break;
                            }
                        }
                        if (nextStep == -1)
                        {
                            throw new ArgumentOutOfRangeException("NextStep", "Does not correspond to a field in the form.");
                        }
                        if (_model.Steps[nextStep].Active(state))
                        {
                            var current = _model.Steps[form.Step];
                            form.SetPhase(_model.Fields.Field(current.Name).IsUnknown(state) ? StepPhase.Ready : StepPhase.Completed);
                            form.History.Push(form.Step);
                            form.Step = nextStep;
                            form.SetPhase(StepPhase.Ready);
                            found = true;
                        }
                        else
                        {
                            // If we went to a state which is not active fall through to the next active if any
                            goto case StepDirection.Next;
                        }
                    }
                    else
                    {
                        // Always mark multiple names as found so we can handle the user navigation
                        found = true;
                    }
                    break;
                case StepDirection.Next:
                    var start = form.Step;
                    // Next ready step including current one
                    for (var offset = 0; offset < _model.Steps.Count; ++offset)
                    {
                        form.Step = (start + offset) % _model.Steps.Count;
                        if (offset > 0)
                        {
                            form.StepState = null;
                            form.Next = null;
                        }
                        var step = _model.Steps[form.Step];
                        if ((form.Phase() == StepPhase.Ready || form.Phase() == StepPhase.Responding)
                            && step.Active(state))
                        {
                            if (step.Type == StepType.Confirm)
                            {
                                // Ensure all dependencies have values
                                foreach (var dependency in step.Dependencies())
                                {
                                    var dstep = _model.Step(dependency);
                                    var dstepi = _model.StepIndex(dstep);
                                    if (dstep.Active(state) && form.Phases[dstepi] != StepPhase.Completed)
                                    {
                                        form.Step = dstepi;
                                        break;
                                    }
                                }
                                found = true;
                            }
                            else
                            {
                                found = true;
                            }
                            if (form.Step != start && _model.Steps[start].Type != StepType.Message)
                            {
                                form.History.Push(start);
                            }
                            break;
                        }
                    }
                    if (!found)
                    {
                        next.Direction = StepDirection.Complete;
                    }
                    break;
                case StepDirection.Previous:
                    while (form.History.Count() > 0)
                    {
                        var lastStepIndex = form.History.Pop();
                        var lastStep = _model.Steps[lastStepIndex];
                        if (lastStep.Active(state))
                        {
                            var step = _model.Steps[form.Step];
                            form.SetPhase(step.Field.IsUnknown(state) ? StepPhase.Ready : StepPhase.Completed);
                            form.Step = lastStepIndex;
                            form.SetPhase(StepPhase.Ready);
                            form.StepState = null;
                            form.Next = null;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        next.Direction = StepDirection.Quit;
                    }
                    break;
                case StepDirection.Quit:
                    break;
                case StepDirection.Reset:
                    form.Reset();
                    // TODO: Should I reset the state as well?
                    // Because we redo phase they can go through everything again but with defaults.
                    found = true;
                    break;
            }
            return found;
        }

        private NextStep DoCommand(IDialogContext context, T state, FormState form, IStep<T> step, IEnumerable<TermMatch> matches, out string feedback)
        {
            // TODO: What if there are more than one command?
            feedback = null;
            var next = new NextStep();
            var value = matches.First().Value;
            if (value is FormCommand)
            {
                switch ((FormCommand)value)
                {
                    case FormCommand.Backup:
                        {
                            next.Direction = step.Back(context, state, form) ? StepDirection.Next : StepDirection.Previous;
                        }
                        break;
                    case FormCommand.Help:
                        {
                            var field = step.Field;
                            var builder = new StringBuilder();
                            foreach (var entry in _model.Configuration.Commands)
                            {
                                builder.Append("* ");
                                builder.AppendLine(entry.Value.Help);
                            }
                            var navigation = new Prompter<T>(field.Template(TemplateUsage.NavigationCommandHelp), _model, null);
                            var active = (from istep in _model.Steps
                                          where istep.Type == StepType.Field && istep.Active(state)
                                          select istep.Field.Description());
                            var activeList = Language.BuildList(active, navigation.Annotation().Separator, navigation.Annotation().LastSeparator);
                            builder.Append("* ");
                            builder.Append(navigation.Prompt(state, "", activeList));
                            feedback = step.Help(state, form, builder.ToString());
                        }
                        break;
                    case FormCommand.Quit: next.Direction = StepDirection.Quit; break;
                    case FormCommand.Reset: next.Direction = StepDirection.Reset; break;
                    case FormCommand.Status:
                        {
                            var prompt = new Prompt("{*}");
                            feedback = new Prompter<T>(prompt, _model, null).Prompt(state, "");
                        }
                        break;
                }
            }
            else
            {
                var name = value as string;
                var istep = _model.Step(name);
                if (istep != null && istep.Active(state))
                {
                    next = new NextStep(new string[] { name });
                }
            }
            return next;
        }

        #endregion

    }
}

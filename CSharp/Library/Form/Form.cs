using Microsoft.Bot.Builder.Form.Advanced;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
    public sealed class Form<T> : IForm<T>
         where T : class, new()
    {

        /// <summary>
        /// Construct a form.
        /// </summary>
        /// <param name="id">Unique dialog id to register with dialog system.</param>
        /// <param name="ignoreAnnotations">True if you want to ignore any annotations on classes when doing reflection.</param>
        public Form(string id, bool ignoreAnnotations = false)
        {
            _id = id;
            _ignoreAnnotations = ignoreAnnotations;
        }

        #region IForm<T> statics
#if DEBUG
        public static bool DebugRecognizers = false;
#endif
        #endregion

        #region  IForm<T> implementation

        public IForm<T> Message(string message, ConditionalDelegate<T> condition = null)
        {
            _steps.Add(new MessageStep(new Prompt(message), condition, this));
            return this;
        }

        public IForm<T> Message(Prompt prompt, ConditionalDelegate<T> condition = null)
        {
            _steps.Add(new MessageStep(prompt, condition, this));
            return this;
        }

        public IForm<T> Field(string name, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _ignoreAnnotations) : new Conditional<T>(name, condition, _ignoreAnnotations));
            if (validate != null)
            {
                field.Validate(validate);
            }
            return AddField(field);
        }

        public IForm<T> Field(string name, string prompt, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _ignoreAnnotations) : new Conditional<T>(name, condition, _ignoreAnnotations));
            if (validate != null)
            {
                field.Validate(validate);
            }
            field.Prompt(new Prompt(prompt));
            return AddField(field);
        }

        public IForm<T> Field(string name, Prompt prompt, ConditionalDelegate<T> condition = null, ValidateDelegate<T> validate = null)
        {
            var field = (condition == null ? new FieldReflector<T>(name, _ignoreAnnotations) : new Conditional<T>(name, condition, _ignoreAnnotations));
            if (validate != null)
            {
                field.Validate(validate);
            }
            field.Prompt(prompt);
            return AddField(field);
        }

        public IForm<T> Field(IField<T> field)
        {
            return AddField(field);
        }

        public IForm<T> AddRemainingFields(IEnumerable<string> exclude = null)
        {
            var exclusions = (exclude == null ? new string[0] : exclude.ToArray());
            var paths = new List<string>();
            FieldPaths(typeof(T), "", paths);
            foreach (var path in paths)
            {
                if (!exclusions.Contains(path))
                {
                    IField<T> field = _fields.Field(path);
                    if (field == null)
                    {
                        Field(new FieldReflector<T>(path, _ignoreAnnotations));
                    }
                }
            }
            return this;
        }

        public IForm<T> Confirm(string prompt, ConditionalDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            Confirm(new Prompt(prompt) { AllowNumbers = BoolDefault.False, AllowDefault = BoolDefault.False }, condition, dependencies);
            return this;
        }

        public IForm<T> Confirm(Prompt prompt = null, ConditionalDelegate<T> condition = null, IEnumerable<string> dependencies = null)
        {
            if (condition == null) condition = (state) => true;
            if (dependencies == null)
            {
                // Default next steps go from previous field ignoring confirmations back to next confirmation
                // Last field before confirmation
                var end = _steps.Count();
                while (end > 0)
                {
                    if (_steps[end - 1].Type() == StepType.Field)
                    {
                        break;
                    }
                    --end;
                }

                var start = end;
                while (start > 0)
                {
                    if (_steps[start - 1].Type() == StepType.Confirm)
                    {
                        break;
                    }
                    --start;
                }
                var fields = new List<string>();
                for (var i = start; i < end; ++i)
                {
                    if (_steps[i].Type() == StepType.Field)
                    {
                        fields.Add(_steps[i].Name());
                    }
                }
                dependencies = fields;
            }
            var confirmation = new Confirmation<T>(prompt, condition, dependencies);
            confirmation.SetForm(this);
            _fields.Add(confirmation);
            _steps.Add(new ConfirmStep(confirmation));
            return this;
        }

        public IForm<T> Confirm(IFieldPrompt<T> prompt)
        {
            // TODO: Need to fill this in
            return this;
        }

        public IForm<T> OnCompletion(CompletionDelegate<T> callback)
        {
            _completion = callback;
            return this;
        }

        public IFields<T> Fields()
        {
            return _fields;
        }

        public FormConfiguration Configuration()
        {
            return _configuration;
        }

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
            public T State;

            /// <summary>
            /// LUIS entities to put into state.
            /// </summary>
            /// <remarks>
            /// In order to set a field in the form state, the Entity must be named with the path to the field in the form state.
            /// </remarks>
            public Models.EntityRecommendation[] Entities;
        }

        public async Task<Connector.Message> BeginAsync(ISession session, Task<object> arguments)
        {
            var initialState = await arguments as InitialState;
            BuildCommandRecognizer();
            bool skipFields = false;
            T state;
            if (initialState == null)
            {
                state = session.SessionData.GetUserData(typeof(T).Name) as T;
                if (state == null)
                {
                    state = new T();
                }
                else
                {
                    skipFields = true;
                }
            }
            else
            {
                state = initialState.State;
                if (state == null) state = new T();
                skipFields = true;
            }
            // TODO: Hook up culture state in form
            if (!_steps.Any((step) => step.Type() == StepType.Field))
            {
                var paths = new List<string>();
                FieldPaths(typeof(T), "", paths);
                foreach (var path in paths)
                {
                    Field(new FieldReflector<T>(path, _ignoreAnnotations));
                }
                Confirm("Is ths your selection?\n{*}");
            }

            var form = new FormState(_steps.Count(), CultureInfo.InvariantCulture);
            if (initialState != null && initialState.Entities != null)
            {
                var entities = (from entity in initialState.Entities group entity by entity.Type);
                foreach (var entityGroup in entities)
                {
                    var step = Step(entityGroup.Key);
                    if (step != null)
                    {
                        form.Step = StepIndex(step);
                        form.StepState = null;
                        var builder = new StringBuilder();
                        foreach (var entity in entityGroup)
                        {
                            builder.Append(entity.Entity);
                            builder.Append(' ');
                        }
                        var input = builder.ToString();
                        string feedback;
                        string prompt = step.Start(session, state, form);
                        var matches = MatchAnalyzer.Coalesce(step.Match(session, state, form, input, out prompt), input);
                        if (MatchAnalyzer.IsFullMatch(input, matches, 0.5))
                        {
                            // TODO: In the case of clarification I could
                            // 1) Go through them while supporting only quit or back and reset
                            // 2) Drop them
                            // 3) Just pick one (found in form.StepState, but that is opaque here)
                            step.Process(session, state, form, input, matches, out feedback, out prompt);
                        }
                        else
                        {
                            form.SetPhase(StepPhase.Ready);
                        }
                    }
                }
                form.Step = 0;
                form.StepState = null;
            }

            if (skipFields)
            {
                // Mark all fields with values as completed.
                for (var i = 0; i < _steps.Count(); ++i)
                {
                    var step = _steps[i];
                    if (step.Type() == StepType.Field)
                    {
                        if (!step.Field().IsUnknown(state))
                        {
                            form.Phases[i] = StepPhase.Completed;
                        }
                    }
                }
            }
            session.SessionData.SetPerUserInConversationData(_id, state);
            session.Stack.SetLocal(_id, form);
            return await ReplyReceivedAsync(session);
        }

        public async Task<Connector.Message> ReplyReceivedAsync(ISession session)
        {
            var form = session.Stack.GetLocal(_id) as FormState;
            var state = session.SessionData.PerUserInConversationData[_id] as T;
            string message = null;
            string prompt = null;
            bool useLastPrompt = false;
            bool requirePrompt = false;
            var next = (form.Next == null ? new NextStep() : ActiveSteps(form.Next, state));
            while (prompt == null && (message == null || requirePrompt) && MoveToNext(state, form, next))
            {
                IStep step;
                IEnumerable<TermMatch> matches = null;
                string lastInput = null;
                string feedback = null;
                if (next.Direction == StepDirection.Named && next.Names.Count() > 1)
                {
                    // We need to choose between multiple next steps
                    bool start = (form.Next == null);
                    form.Next = next;
                    step = new NavigationStep(_steps[form.Step].Name(), this, state, form);
                    if (start)
                    {
                        prompt = step.Start(session, state, form);
                    }
                    else
                    {
                        matches = step.Match(session, state, form, session.Message.Text, out lastInput);
                    }
                }
                else
                {
                    // Processing current step
                    step = _steps[form.Step];
                    if (form.Phase() == StepPhase.Ready)
                    {
                        if (step.Type() == StepType.Message)
                        {
                            feedback = step.Start(session, state, form);
                            requirePrompt = true;
                            useLastPrompt = false;
                            next = new NextStep();
                        }
                        else
                        {
                            prompt = step.Start(session, state, form);
                        }
                    }
                    else if (form.Phase() == StepPhase.Responding)
                    {
                        matches = step.Match(session, state, form, session.Message.Text, out lastInput);
                    }
                }
                if (matches != null)
                {
                    matches = MatchAnalyzer.Coalesce(matches, lastInput).ToArray();
                    if (MatchAnalyzer.IsFullMatch(lastInput, matches))
                    {
                        next = step.Process(session, state, form, lastInput, matches, out feedback, out prompt);
                        // 1) Not completed, not valid -> Not require, last
                        // 2) Completed, feedback -> require, not last
                        requirePrompt = (form.Phase() == StepPhase.Completed);
                        useLastPrompt = !requirePrompt;
                    }
                    else
                    {
                        // Filter non-active steps out of command matches
                        var commands =
                            (from command in MatchAnalyzer.Coalesce(_commands.Matches(lastInput), lastInput)
                             where (command.Value is FormCommand
                                 || _fields.Field(command.Value as string).Active(state))
                             select command).ToArray();
                        if (MatchAnalyzer.IsFullMatch(lastInput, commands))
                        {
                            next = DoCommand(session, state, form, step, commands, out feedback);
                            requirePrompt = false;
                            useLastPrompt = true;
                        }
                        else
                        {
                            if (matches.Count() == 0 && commands.Count() == 0)
                            {
                                // TODO: If we implement fallback, opportunity to call parent dialogs
                                feedback = step.NotUnderstood(session, state, form, lastInput);
                                requirePrompt = false;
                                useLastPrompt = false;
                            }
                            else
                            {
                                // Go with response since it looks possible
                                var bestMatch = MatchAnalyzer.BestMatches(matches, commands);
                                if (bestMatch == 0)
                                {
                                    next = step.Process(session, state, form, lastInput, matches, out feedback, out prompt);
                                    requirePrompt = (form.Phase() == StepPhase.Completed);
                                    useLastPrompt = !requirePrompt;
                                }
                                else
                                {
                                    next = DoCommand(session, state, form, step, commands, out feedback);
                                    requirePrompt = false;
                                    useLastPrompt = true;
                                }
                            }
                        }
                    }
                }
                next = ActiveSteps(next, state);
                if (feedback != null)
                {
                    message = (message == null ? feedback : message + "\n\n" + feedback);
                }
            }
            if (next.Direction == StepDirection.Complete || next.Direction == StepDirection.Quit)
            {
                Task<object> result = Tasks.Cancelled;
                if (next.Direction == StepDirection.Complete)
                {
                    if (_completion != null)
                    {
                        _completion(session, state);
                    }
                    result = Task.FromResult<object>(state);
                }
                return await session.EndDialogAsync(this, result);
            }
            else
            {
                if (message != null)
                {
                    if (requirePrompt)
                    {
                        form.LastPrompt = prompt;
                        prompt = message + "\n\n" + prompt;
                    }
                    else if (useLastPrompt)
                    {
                        prompt = message + "\n\n" + form.LastPrompt;
                    }
                    else
                    {
                        prompt = message;
                    }
                }
                else
                {
                    form.LastPrompt = prompt;
                }
                return await session.CreateDialogResponse(prompt);
            }
        }

        public async Task<Connector.Message> DialogResumedAsync(ISession session, Task<object> result)
        {
            var form = session.Stack.GetLocal(_id) as FormState;
            ++form.Step;
            form.StepState = null;
            session.Message.Text = "";
            return await ReplyReceivedAsync(session);
        }

        public string ID
        {
            get
            {
                return _id;
            }
        }
        #endregion

        #region Implementation

        private IForm<T> AddField(IField<T> field)
        {
            _fields.Add(field);
            field.SetForm(this);
            var step = new FieldStep(field.Name(), this);
            var oldStep = Step(field.Name());
            if (oldStep != null)
            {
                _steps[StepIndex(oldStep)] = step;
            }
            else
            {
                _steps.Add(step);
            }
            return this;
        }

        private void FieldPaths(Type type, string path, List<string> paths)
        {
            var newPath = (path == "" ? path : path + ".");
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                TypePaths(field.FieldType, newPath + field.Name, paths);
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead && property.CanWrite)
                {
                    TypePaths(property.PropertyType, newPath + property.Name, paths);
                }
            }
        }

        private void TypePaths(Type type, string path, List<string> paths)
        {
            if (type.IsClass)
            {
                if (type == typeof(string))
                {
                    paths.Add(path);
                }
                else if (type.IsIEnumerable())
                {
                    var elt = type.GetGenericElementType();
                    if (elt.IsEnum)
                    {
                        paths.Add(path);
                    }
                    else
                    {
                        // TODO: What to do about enumerations of things other than enums?
                    }
                }
                else
                {
                    FieldPaths(type, path, paths);
                }
            }
            else if (type.IsEnum)
            {
                paths.Add(path);
            }
            else if (type == typeof(bool))
            {
                paths.Add(path);
            }
            else if (type.IsIntegral())
            {
                paths.Add(path);
            }
            else if (type.IsDouble())
            {
                paths.Add(path);
            }
            else if (type.IsNullable() && type.IsValueType)
            {
                paths.Add(path);
            }
            else if (type == typeof(DateTime))
            {
                paths.Add(path);
            }
        }

        private IStep Step(string name)
        {
            IStep result = null;
            foreach (var step in _steps)
            {
                if (step.Name() == name)
                {
                    result = step;
                    break;
                }
            }
            return result;
        }

        private NextStep ActiveSteps(NextStep next, T state)
        {
            var result = next;
            if (next.Direction == StepDirection.Named)
            {
                var names = (from name in next.Names where _fields.Field(name).Active(state) select name);
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

        [Serializable]
        private class FormState
        {
            // Last sent prompt which is used when feedback is supplied
            public string LastPrompt;

            // Used when navigating to reflect choices for next
            public NextStep Next;

            // Currently executing step
            public int Step;

            // History of executed steps
            public Stack<int> History;

            // Current phase of each step
            public StepPhase[] Phases;

            // Internal state of a step
            public object StepState;

            // Field name and recognized entities
            public List<Tuple<string, string>> FieldInputs;

            // Current culture. TODO: Not used
            public CultureInfo Culture;

            public FormState(int steps, CultureInfo culture)
            {
                Phases = new StepPhase[steps];
                Culture = culture;
                Reset();
            }

            public void Reset()
            {
                LastPrompt = "";
                Next = null;
                Step = 0;
                History = new Stack<int>();
                Phases = new StepPhase[Phases.Length];
                StepState = null;
                FieldInputs = null;
            }

            public StepPhase Phase()
            {
                return Phases[Step];
            }

            public StepPhase Phase(int step)
            {
                return Phases[step];
            }

            public void SetPhase(StepPhase phase)
            {
                Phases[Step] = phase;
            }
        }

        private enum StepPhase { Ready, Responding, Completed };
        private enum StepType { Field, Confirm, Navigation, Message };
        private interface IStep
        {
            string Name();

            StepType Type();

            IField<T> Field();

            bool Active(T state);

            string Start(ISession session, T state, FormState form);

            IEnumerable<TermMatch> Match(ISession session, T state, FormState form, string input, out string lastInput);

            NextStep Process(ISession session, T state, FormState form, string input, IEnumerable<TermMatch> matches,
                out string feedback, out string prompt);

            string NotUnderstood(ISession session, T state, FormState form, string input);

            string Help(T state, FormState form, string commandHelp);

            bool Back(ISession session, T state, FormState form);

            IEnumerable<string> Dependencies();
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
                        for (var i = 0; i < _steps.Count(); ++i)
                        {
                            if (_steps[i].Name() == name)
                            {
                                nextStep = i;
                                break;
                            }
                        }
                        if (nextStep == -1)
                        {
                            throw new ArgumentOutOfRangeException("NextStep", "Does not correspond to a field in the form.");
                        }
                        if (_steps[nextStep].Active(state))
                        {
                            var current = _steps[form.Step];
                            form.SetPhase(_fields.Field(current.Name()).IsUnknown(state) ? StepPhase.Ready : StepPhase.Completed);
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
                    for (var offset = 0; offset < _steps.Count(); ++offset)
                    {
                        form.Step = (start + offset) % _steps.Count();
                        if (offset > 0)
                        {
                            form.StepState = null;
                            form.Next = null;
                        }
                        var step = _steps[form.Step];
                        if ((form.Phase() == StepPhase.Ready || form.Phase() == StepPhase.Responding)
                            && step.Active(state))
                        {
                            if (step.Type() == StepType.Confirm)
                            {
                                // Ensure all dependencies have values
                                foreach (var dependency in step.Dependencies())
                                {
                                    var dstep = Step(dependency);
                                    var dstepi = StepIndex(dstep);
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
                            if (form.Step != start && _steps[start].Type() != StepType.Message)
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
                        var lastStep = _steps[lastStepIndex];
                        if (lastStep.Active(state))
                        {
                            var step = _steps[form.Step];
                            form.SetPhase(step.Field().IsUnknown(state) ? StepPhase.Ready : StepPhase.Completed);
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

        private int StepIndex(IStep step)
        {
            var index = -1;
            for (var i = 0; i < _steps.Count(); ++i)
            {
                if (_steps[i] == step)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private NextStep DoCommand(ISession session, T state, FormState form, IStep step, IEnumerable<TermMatch> matches, out string feedback)
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
                            next.Direction = step.Back(session, state, form) ? StepDirection.Next : StepDirection.Previous;
                        }
                        break;
                    case FormCommand.Help:
                        {
                            var field = step.Field();
                            var builder = new StringBuilder();
                            foreach (var entry in _configuration.Commands)
                            {
                                builder.Append("* ");
                                builder.AppendLine(entry.Value.Help);
                            }
                            var navigation = new Prompter<T>(field.Template(TemplateUsage.NavigationCommandHelp), this, null);
                            var active = (from istep in _steps
                                          where istep.Type() == StepType.Field && istep.Active(state)
                                          select istep.Field().Description());
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
                            feedback = new Prompter<T>(prompt, this, null).Prompt(state, "");
                        }
                        break;
                }
            }
            else
            {
                var name = value as string;
                var istep = Step(name);
                if (istep != null && istep.Active(state))
                {
                    next = new NextStep(new string[] { name });
                }
            }
            return next;
        }

        private class FieldStep : IStep
        {
            public FieldStep(string name, IForm<T> form)
            {
                _name = name;
                _form = form;
                _field = _form.Fields().Field(name);
            }

            public string Name()
            {
                return _name;
            }

            public StepType Type()
            {
                return StepType.Field;
            }

            public IField<T> Field()
            {
                return _field;
            }

            public bool Active(T state)
            {
                return _field.Active(state);
            }

            public string Start(ISession session, T state, FormState form)
            {
                form.SetPhase(StepPhase.Responding);
                form.StepState = new FieldStepState(FieldStepStates.SentPrompt);
                return _field.Prompt().Prompt(state, _name);
            }

            public IEnumerable<TermMatch> Match(ISession session, T state, FormState form, string input, out string lastInput)
            {
                IEnumerable<TermMatch> matches = null;
                Debug.Assert(form.Phase() == StepPhase.Responding);
                var stepState = form.StepState as FieldStepState;
                lastInput = input;
                if (stepState.State == FieldStepStates.SentPrompt)
                {
                    matches = _field.Prompt().Recognizer().Matches(input, _field.GetValue(state));
                }
                else if (stepState.State == FieldStepStates.SentClarify)
                {
                    var fieldState = form.StepState as FieldStepState;
                    var iprompt = _field.Prompt();
                    Ambiguous clarify;
                    var iChoicePrompt = NextClarifyPrompt(state, fieldState, iprompt.Recognizer(), out clarify);
                    matches = MatchAnalyzer.Coalesce(MatchAnalyzer.HighestConfidence(iChoicePrompt.Recognizer().Matches(input)), input).ToArray();
                    if (matches.Count() > 1)
                    {
                        matches = new TermMatch[0];
                    }
                }
#if DEBUG
                if (Form<T>.DebugRecognizers)
                {
                    MatchAnalyzer.PrintMatches(matches, 2);
                }
#endif
                return matches;
            }

            public NextStep Process(ISession session, T state, FormState form, string input, IEnumerable<TermMatch> matches,
                out string feedback, out string prompt)
            {
                feedback = null;
                prompt = null;
                var iprompt = _field.Prompt();
                var fieldState = form.StepState as FieldStepState;
                object response = null;
                if (fieldState.State == FieldStepStates.SentPrompt)
                {
                    // Response to prompt
                    var firstMatch = matches.FirstOrDefault();
                    if (matches.Count() == 1)
                    {
                        response = SetValue(state, firstMatch.Value, form, out feedback);
                    }
                    else if (matches.Count() > 1)
                    {
                        // Check multiple matches for ambiguity
                        var groups = MatchAnalyzer.GroupedMatches(matches);
                        // 1) Could be multiple match groups like for ingredients.
                        // 2) Could be overlapping matches like "onion".
                        // 3) Could be multiple matches where only one is expected.

                        if (!_field.AllowsMultiple())
                        {
                            // Create a single group of all possibilities if only want one value
                            var mergedGroup = groups.SelectMany((group) => group).ToList();
                            groups = new List<List<TermMatch>>() { mergedGroup };
                        }
                        var ambiguous = new List<Ambiguous>();
                        var settled = new List<object>();
                        foreach (var choices in groups)
                        {
                            if (choices.Count() > 1)
                            {
                                var unclearResponses = string.Join(" ", (from choice in choices select input.Substring(choice.Start, choice.Length)).Distinct());
                                var values = from match in choices select match.Value;
                                ambiguous.Add(new Ambiguous(unclearResponses, values));
                            }
                            else
                            {
                                var matchValue = choices.First().Value;
                                if (matchValue.GetType().IsIEnumerable())
                                {
                                    foreach (var value in matchValue as System.Collections.IEnumerable)
                                    {
                                        settled.Add(value);
                                    }
                                }
                                else
                                {
                                    settled.Add(choices.First().Value);
                                }
                            }
                        }

                        if (ambiguous.Count() > 0)
                        {
                            // Need 1 or more clarifications
                            Ambiguous clarify;
                            fieldState.State = FieldStepStates.SentClarify;
                            fieldState.Settled = settled;
                            fieldState.Clarifications = ambiguous;
                            response = SetValue(state, null);
                            var iChoicePrompt = NextClarifyPrompt(state, form.StepState as FieldStepState, iprompt.Recognizer(), out clarify);
                            prompt = iChoicePrompt.Prompt(state, _name, clarify.Response);
                        }
                        else
                        {
                            if (_field.AllowsMultiple())
                            {
                                response = SetValue(state, settled, form, out feedback);
                            }
                            else
                            {
                                Debug.Assert(settled.Count() == 1);
                                response = SetValue(state, settled.First(), form, out feedback);
                            }
                        }
                    }
                    var unmatched = MatchAnalyzer.Unmatched(input, matches);
                    var unmatchedWords = string.Join(" ", unmatched);
                    var nonNoise = Language.NonNoiseWords(Language.WordBreak(unmatchedWords)).ToArray();
                    fieldState.Unmatched = null;
                    if (_field.Prompt().Annotation().Feedback == FeedbackOptions.Always)
                    {
                        fieldState.Unmatched = string.Join(" ", nonNoise);
                    }
                    else if (_field.Prompt().Annotation().Feedback == FeedbackOptions.Auto
                            && nonNoise.Length > 0
                            && unmatched.Count() > 0)
                    {
                        fieldState.Unmatched = string.Join(" ", nonNoise);
                    }
                }
                else if (fieldState.State == FieldStepStates.SentClarify)
                {
                    Ambiguous clarify;
                    var iChoicePrompt = NextClarifyPrompt(state, fieldState, iprompt.Recognizer(), out clarify);
                    if (matches.Count() == 1)
                    {
                        // Clarified ambiguity
                        fieldState.Settled.Add(matches.First().Value);
                        fieldState.Clarifications.Remove(clarify);
                        Ambiguous newClarify;
                        var newiChoicePrompt = NextClarifyPrompt(state, fieldState, iprompt.Recognizer(), out newClarify);
                        if (newiChoicePrompt != null)
                        {
                            prompt = newiChoicePrompt.Prompt(state, _name, newClarify.Response);
                        }
                        else
                        {
                            // No clarification left, so set the field
                            if (_field.AllowsMultiple())
                            {
                                response = SetValue(state, fieldState.Settled, form, out feedback);
                            }
                            else
                            {
                                Debug.Assert(fieldState.Settled.Count() == 1);
                                response = SetValue(state, fieldState.Settled.First(), form, out feedback);
                            }
                            form.SetPhase(StepPhase.Completed);
                        }
                    }
                }
                if (form.Phase() == StepPhase.Completed)
                {
                    form.StepState = null;
                    if (fieldState.Unmatched != null)
                    {
                        if (fieldState.Unmatched != "")
                        {
                            feedback = new Prompter<T>(_field.Template(TemplateUsage.Feedback), _form, null).Prompt(state, _name, fieldState.Unmatched);
                        }
                        else
                        {
                            feedback = new Prompter<T>(_field.Template(TemplateUsage.Feedback), _form, null).Prompt(state, _name);
                        }
                    }
                }
                return _field.Next(response, state);
            }

            public string NotUnderstood(ISession session, T state, FormState form, string input)
            {
                string feedback = null;
                var iprompt = _field.Prompt();
                var fieldState = form.StepState as FieldStepState;
                if (fieldState.State == FieldStepStates.SentPrompt)
                {
                    feedback = Template(TemplateUsage.NotUnderstood).Prompt(state, _name, input);
                }
                else if (fieldState.State == FieldStepStates.SentClarify)
                {
                    feedback = Template(TemplateUsage.NotUnderstood).Prompt(state, "", input);
                }
                return feedback;
            }

            public bool Back(ISession session, T state, FormState form)
            {
                bool backedUp = false;
                var fieldState = form.StepState as FieldStepState;
                if (fieldState.State == FieldStepStates.SentClarify)
                {
                    var desc = _form.Fields().Field(_name);
                    if (desc.AllowsMultiple())
                    {
                        desc.SetValue(state, fieldState.Settled);
                    }
                    else if (fieldState.Settled.Count() > 0)
                    {
                        desc.SetValue(state, fieldState.Settled.First());
                    }
                    form.SetPhase(StepPhase.Ready);
                    backedUp = true;
                }
                return backedUp;
            }

            public string Help(T state, FormState form, string commandHelp)
            {
                var fieldState = form.StepState as FieldStepState;
                IPrompt<T> template;
                if (fieldState.State == FieldStepStates.SentClarify)
                {
                    Ambiguous clarify;
                    var recognizer = NextClarifyPrompt(state, fieldState, _field.Prompt().Recognizer(), out clarify).Recognizer();
                    template = Template(TemplateUsage.HelpClarify, recognizer);
                }
                else
                {
                    template = Template(TemplateUsage.Help, _field.Prompt().Recognizer());
                }
                return "* " + template.Prompt(state, _name, "* " + template.Recognizer().Help(state, _field.GetValue(state)), commandHelp);
            }

            public IEnumerable<string> Dependencies()
            {
                return new string[0];
            }

            private IPrompt<T> Template(TemplateUsage usage, IRecognize<T> recognizer = null)
            {
                var template = _field.Template(usage);
                return new Prompter<T>(template, _form, recognizer == null ? _field.Prompt().Recognizer() : recognizer);
            }

            private object SetValue(T state, object value)
            {
                var desc = _form.Fields().Field(_name);
                if (value == null)
                {
                    desc.SetUnknown(state);
                }
                else if (desc.AllowsMultiple())
                {
                    if (value is System.Collections.IEnumerable)
                    {
                        desc.SetValue(state, value);
                    }
                    else
                    {
                        desc.SetValue(state, new List<object> { value });
                    }
                }
                else
                {
                    // Singleton value
                    desc.SetValue(state, value);
                }
                return value;
            }

            private object SetValue(T state, object value, FormState form, out string feedback)
            {
                var desc = _form.Fields().Field(_name);
                feedback = desc.Validate(state, value);
                if (feedback == null)
                {
                    SetValue(state, value);
                    form.SetPhase(StepPhase.Completed);
                }
                return value;
            }

            private IPrompt<T> NextClarifyPrompt(T state, FieldStepState stepState, IRecognize<T> recognizer, out Ambiguous clarify)
            {
                IPrompt<T> prompter = null;
                clarify = null;
                foreach (var clarification in stepState.Clarifications)
                {
                    if (clarification.Values.Count() > 1)
                    {
                        clarify = clarification;
                        break;
                    }
                }
                if (clarify != null)
                {
                    var template = Template(TemplateUsage.Clarify);
                    var helpTemplate = _field.Template(template.Annotation().AllowNumbers != BoolDefault.False ? TemplateUsage.EnumOneNumberHelp : TemplateUsage.EnumManyNumberHelp);
                    var choiceRecognizer = new RecognizeEnumeration<T>(_form, "", null,
                        clarify.Values,
                        (value) => recognizer.ValueDescription(value),
                        (value) => recognizer.ValidInputs(value),
                        template.Annotation().AllowNumbers != BoolDefault.False, helpTemplate);
                    prompter = Template(TemplateUsage.Clarify, choiceRecognizer);
                }
                return prompter;
            }

            private enum FieldStepStates { Unknown, SentPrompt, SentClarify };

            [Serializable]
            private class Ambiguous
            {
                public readonly string Response;
                public object[] Values;
                public Ambiguous(string response, IEnumerable<object> values)
                {
                    Response = response;
                    Values = values.ToArray<object>();
                }
            }

            [Serializable]
            private class FieldStepState
            {
                public FieldStepStates State;
                public string Unmatched;
                public List<object> Settled;
                public List<Ambiguous> Clarifications;
                public FieldStepState(FieldStepStates state)
                {
                    State = state;
                }
            }

            private readonly string _name;
            private readonly IField<T> _field;
            private readonly IForm<T> _form;
        }

        private class ConfirmStep : IStep
        {
            public ConfirmStep(IField<T> field)
            {
                _field = field;
            }

            public bool Back(ISession session, T state, FormState form)
            {
                return false;
            }

            public IField<T> Field()
            {
                return _field;
            }

            public bool Active(T state)
            {
                return _field.Active(state);
            }

            public IEnumerable<TermMatch> Match(ISession session, T state, FormState form, string input, out string lastInput)
            {
                lastInput = input;
                return _field.Prompt().Recognizer().Matches(input);
            }

            public string Name()
            {
                return _field.Name();
            }

            public string NotUnderstood(ISession session, T state, FormState form, string input)
            {
                var template = _field.Template(TemplateUsage.NotUnderstood);
                var prompter = new Prompter<T>(template, _field.Form(), null);
                return prompter.Prompt(state, "", input);
            }

            public NextStep Process(ISession session, T state, FormState form, string input, IEnumerable<TermMatch> matches,
                out string feedback,
                out string prompt)
            {
                feedback = null;
                prompt = null;
                var value = matches.First().Value;
                form.StepState = null;
                form.SetPhase((bool)value ? StepPhase.Completed : StepPhase.Ready);
                return _field.Next(value, state);
            }

            public string Start(ISession session, T state, FormState form)
            {
                form.SetPhase(StepPhase.Responding);
                return _field.Prompt().Prompt(state, _field.Name());
            }

            public string Help(T state, FormState form, string commandHelp)
            {
                var template = _field.Template(TemplateUsage.HelpConfirm);
                var prompt = new Prompter<T>(template, _field.Form(), _field.Prompt().Recognizer());
                return "* " + prompt.Prompt(state, _field.Name(), "* " + prompt.Recognizer().Help(state, null), commandHelp);
            }

            public StepType Type()
            {
                return StepType.Confirm;
            }

            public IEnumerable<string> Dependencies()
            {
                return _field.Dependencies();
            }

            private readonly IField<T> _field;
        }

        private class NavigationStep : IStep
        {
            public NavigationStep(string name, Form<T> form, T state, FormState formState)
            {
                _name = name;
                _form = form;
                _fields = form.Fields();
                var field = _fields.Field(_name);
                var fieldPrompt = field.Template(TemplateUsage.NavigationFormat);
                var template = field.Template(TemplateUsage.Navigation);
                var recognizer = new RecognizeEnumeration<T>(_form, Name(), null,
                    formState.Next.Names,
                    (value) => new Prompter<T>(fieldPrompt, _form, _fields.Field(value as string).Prompt().Recognizer()).Prompt(state, value as string),
                    (value) => _fields.Field(value as string).Terms(),
                    _form.Configuration().DefaultPrompt.AllowNumbers != BoolDefault.False,
                    field.Template(TemplateUsage.NavigationHelp));
                _prompt = new Prompter<T>(template, form, recognizer);
            }

            public bool Back(ISession session, T state, FormState form)
            {
                form.Next = null;
                return false;
            }

            public IField<T> Field()
            {
                return _fields.Field(_name);
            }

            public bool Active(T state)
            {
                return true;
            }

            public IEnumerable<TermMatch> Match(ISession session, T state, FormState form, string input, out string lastInput)
            {
                lastInput = input;
                return _prompt.Recognizer().Matches(input);
            }

            public string Name()
            {
                return "Navigation";
            }

            public string NotUnderstood(ISession session, T state, FormState form, string input)
            {
                var field = _fields.Field(_name);
                var template = field.Template(TemplateUsage.NotUnderstood);
                return new Prompter<T>(template, _form, null).Prompt(state, _name, input);
            }

            public NextStep Process(ISession session, T state, FormState form, string input, IEnumerable<TermMatch> matches,
                out string feedback,
                out string prompt)
            {
                feedback = null;
                prompt = null;
                form.Next = null;
                return new NextStep(new string[] { matches.First().Value as string });
            }

            public string Start(ISession session, T state, FormState form)
            {
                return _prompt.Prompt(state, _name);
            }

            public StepType Type()
            {
                return StepType.Navigation;
            }

            public string Help(T state, FormState form, string commandHelp)
            {
                var recognizer = _prompt.Recognizer();
                var prompt = new Prompter<T>(Field().Template(TemplateUsage.HelpNavigation), _form, recognizer);
                return "* " + prompt.Prompt(state, _name, "* " + recognizer.Help(state, null), commandHelp);
            }

            public IEnumerable<string> Dependencies()
            {
                return new string[0];
            }

            private string _name;
            private readonly IForm<T> _form;
            private readonly IFields<T> _fields;
            private readonly IPrompt<T> _prompt;
        }

        private class MessageStep : IStep
        {
            public MessageStep(Prompt prompt, ConditionalDelegate<T> condition, Form<T> form)
            {
                _name = "message" + form._steps.Count().ToString();
                _prompt = new Prompter<T>(prompt, form, null);
                _condition = (condition == null ? (state) => true : condition);
            }

            public bool Active(T state)
            {
                return _condition(state);
            }

            public bool Back(ISession session, T state, FormState form)
            {
                return false;
            }

            public string Help(T state, FormState form, string commandHelp)
            {
                return null;
            }

            public IEnumerable<string> Dependencies()
            {
                throw new NotImplementedException();
            }

            public IField<T> Field()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<TermMatch> Match(ISession session, T state, FormState form, string input, out string lastInput)
            {
                throw new NotImplementedException();
            }

            public string Name()
            {
                return _name;
            }

            public string NotUnderstood(ISession session, T state, FormState form, string input)
            {
                throw new NotImplementedException();
            }

            public NextStep Process(ISession session, T state, FormState form, string input, IEnumerable<TermMatch> matches, out string feedback, out string prompt)
            {
                throw new NotImplementedException();
            }

            public string Start(ISession session, T state, FormState form)
            {
                form.SetPhase(StepPhase.Completed);
                return _prompt.Prompt(state, "");
            }

            public StepType Type()
            {
                return StepType.Message;
            }

            private readonly string _name;
            private readonly ConditionalDelegate<T> _condition;
            private readonly IPrompt<T> _prompt;
        }

        private void BuildCommandRecognizer()
        {
            var values = new List<object>();
            var descriptions = new Dictionary<object, string>();
            var terms = new Dictionary<object, string[]>();
            foreach (var entry in Configuration().Commands)
            {
                values.Add(entry.Key);
                descriptions[entry.Key] = entry.Value.Description;
                terms[entry.Key] = entry.Value.Terms;
            }
            foreach (var field in _fields)
            {
                var fterms = field.Terms();
                if (fterms != null)
                {
                    values.Add(field.Name());
                    descriptions.Add(field.Name(), field.Description());
                    terms.Add(field.Name(), fterms.ToArray());
                }
            }
            _commands = new RecognizeEnumeration<T>(this, "Form commands", null,
                values,
                    (value) => descriptions[value],
                    (value) => terms[value],
                    false, null);
        }

        private readonly string _id;
        private readonly FormConfiguration _configuration = new FormConfiguration();
        private bool _ignoreAnnotations;
        private List<IStep> _steps = new List<IStep>();
        private Fields<T> _fields = new Fields<T>();
        private IRecognize<T> _commands;
        private CompletionDelegate<T> _completion = null;
        #endregion

    }
}

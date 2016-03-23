using Microsoft.Bot.Builder.Form.Advanced;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Form
{
    [Serializable]
    internal class FormState
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

}

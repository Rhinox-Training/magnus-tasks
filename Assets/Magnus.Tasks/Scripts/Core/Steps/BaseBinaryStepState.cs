using System;

namespace Rhinox.Magnus.Tasks
{
    public abstract class BaseBinaryStepState : BaseStepState
    {
        public BaseStepState NextStep;
        public BaseStepState NextStepFailed;

        public override BaseStepState GetNextStep()
        {
            switch (CompletionState)
            {
                case CompletionState.None: // NOTE: when the state was not yet completed assume success for pathing reasons
                case CompletionState.Success:
                    return NextStep;
                case CompletionState.Failure:
                    return NextStepFailed;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool HasNextStep()
        {
            if (State != ProcessState.Finished)
                return NextStep != null;
            return CompletionState == CompletionState.Failure ? NextStepFailed != null : NextStep != null;
        }
    }
}
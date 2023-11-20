using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Magnus.Tasks;
using Rhinox.Utilities;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Tasks
{
    public enum StepTiming
    {
        OnStart,
        OnStart_EndOnComplete,
        OnComplete
    }

    [AssignableTypeFilter]
    public interface IStepTimingEvent
    {
        void Initialize(BaseStep step);
    }

 
}
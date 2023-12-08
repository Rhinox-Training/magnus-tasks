using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    public abstract class BinaryStepData : StepData
    {
        public StepData NextStepFailed;

        protected BinaryStepData(SerializableGuid id, string name, string description = "") :
             base(id, name, description)
        {
        }
        
        protected BinaryStepData(string name, string description = "") : base(name, description) { }
    }
}
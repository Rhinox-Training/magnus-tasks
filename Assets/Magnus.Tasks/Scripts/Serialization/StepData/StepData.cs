using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    [Serializable]
    public abstract class StepData : IUseReferenceGuid
    {
        public TagContainer TagContainer;
        
        public SerializableGuid ID;
        
        public string Name;
        public string Description;
        
        public List<BaseSubStepData> SubStepData = new List<BaseSubStepData>();

        public List<IStepTimingEvent> StepTimingEvents;
        
        public ValueReferenceEvent OnStarted;
        public ValueReferenceEvent OnCompleted;
        
        protected StepData(SerializableGuid id, string name, string description = "")
        {
            ID = id;
            Name = name;
            Description = description;
            TagContainer = new TagContainer();
        }
        
        protected StepData(string name, string description = "") : this(SerializableGuid.CreateNew(), name, description) { }

        protected StepData() : this(string.Empty) { }

        public virtual bool HasData()
        {
            if (!string.IsNullOrWhiteSpace(Description)) return true;
            if (!OnStarted.Events.IsNullOrEmpty()) return true;
            if (!OnCompleted.Events.IsNullOrEmpty()) return true;
            if (SubStepData != null && SubStepData.Any(x => x.HasData())) return true;

            return false;
        }

        public virtual bool UsesGuid(SerializableGuid guid)
        {
            if (OnStarted.UsesGuid(guid) || OnCompleted.UsesGuid(guid))
                return true;

            if (SubStepData != null)
            {
                foreach (var guidUser in SubStepData.OfType<IUseReferenceGuid>())
                {
                    if (guidUser.UsesGuid(guid))
                        return true;
                }
            }

            return false;
        }

        public virtual void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
        {
            OnStarted.ReplaceGuid(guid, replacement);
            OnCompleted.ReplaceGuid(guid, replacement);

            if (SubStepData != null)
            {
                foreach (var guidUser in SubStepData.OfType<IUseReferenceGuid>())
                    guidUser.ReplaceGuid(guid, replacement);
            }
        }
    }
}
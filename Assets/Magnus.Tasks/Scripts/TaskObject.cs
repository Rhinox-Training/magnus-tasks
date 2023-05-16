using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.VOLT.Training;
using Sirenix.OdinInspector;

namespace Rhinox.VOLT.Data
{
    [DataLayer]
    public class TaskObject
    {
        public int ID;
        public string Name;
        public List<StepData> Steps;
        public ValueReferenceLookup Lookup;
        
        // Serialization
        public TaskObject() : this(0)
        {
            ID = 0;
        }
        
        public TaskObject(int id)
        {
            ID = id;
            Name = "Unnamed Task";
            Steps = new List<StepData>();
            Lookup = new ValueReferenceLookup();
        }
        
        public TaskObject(int id, string name)
        {
            ID = id;
            Name = name;
            Steps = new List<StepData>();
            Lookup = new ValueReferenceLookup();
        }

        public void Add(ConditionStepObject o) => Steps.Add(o);

        public StepData GetStep(SerializableGuid id)
        {
            if (id == null) return null;

            for (var i = 0; i < Steps.Count; i++)
            {
                var x = Steps[i];
                if (id.Equals(x.ID))
                    return x;
            }

            return null;
        }
    }
    
    public class StepDataGeneratorAttribute : Attribute
    {
        public string ConvertMethodName;

        public StepDataGeneratorAttribute(string convertMethodName)
        {
            ConvertMethodName = convertMethodName;
        }
    }

    [Serializable]
    public abstract class StepData : IUseReferenceGuid
    {
        [DisplayAsString, ReadOnly]
        public SerializableGuid ID;
        
        public string Name;
        public string Description;
        
        public List<BaseSubStepData> SubStepData = new List<BaseSubStepData>();

        public ValueReferenceEvent OnStarted;
        public ValueReferenceEvent OnCompleted;
        
        protected StepData(SerializableGuid id, string name, string description = "")
        {
            ID = id;
            Name = name;
            Description = description;
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

    [Serializable]
    public class ConditionStepObject : StepData
    {
        public bool OrderedConditions;

        public TagContainer TagContainer;
        
        public List<ConditionData> Conditions;

        public ConditionStepObject(SerializableGuid id, string name, string description = "") : base(id, name, description)
        { 
            Conditions = new List<ConditionData>();
            TagContainer = new TagContainer();
        }
        
        public ConditionStepObject(string name, string description = "") : this(SerializableGuid.CreateNew(), name, description) { }
        
        public ConditionStepObject() : base("Unnamed ConditionStep")
        { 
            Conditions = new List<ConditionData>();
            TagContainer = new TagContainer();
        }

        public override bool HasData()
        {
            if (base.HasData()) return true;

            if (!Conditions.IsNullOrEmpty()) return true;

            return false;
        }

        public override bool UsesGuid(SerializableGuid guid)
        {
            if (base.UsesGuid(guid))
                return true;

            if (Conditions != null)
            {
                if (Conditions.Any(x => x.UsesGuid(guid)))
                    return true;
            }

            return false;
        }

        public override void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
        {
            base.ReplaceGuid(guid, replacement);
            
            if (Conditions != null)
            {
                foreach (var guidUser in Conditions)
                    guidUser.ReplaceGuid(guid, replacement);
            }
        }
    }

    // Does not need a SerializedGuidProcessor Attribute, due to never being used to do anything
    public class TaskDataStepObject : StepData
    {
        public TagContainer TagContainer;
        public int TaskId = -1;
        public ValueReferenceLookupOverride LookupOverride;

        public TaskDataStepObject() : base()
        {
            TagContainer = new TagContainer();
        }
        
        public void RefreshTaskData()
        {
            if (TaskId < 0) return;

            var currentOverride = LookupOverride?.Overrides;

            PLog.Info<MagnusLogger>($"Initializing TaskDataStepObject from data id: {TaskId}");
            LookupOverride = new ValueReferenceLookupOverride(TaskId)
            {
                Overrides = currentOverride
            };
        }
    }
}
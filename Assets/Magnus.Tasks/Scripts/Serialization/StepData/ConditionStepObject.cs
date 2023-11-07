using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Tasks
{    
    [Serializable]
    public class ConditionStepObject : StepData
    {
        [PropertyOrder(5)] public bool OrderedConditions;
        [PropertyOrder(7), ListDrawerSettings(HideAddButton = true)] 
        public List<ConditionData> Conditions;

        public ConditionStepObject(SerializableGuid id, string name, string description = "") : base(id, name, description)
        { 
            Conditions = new List<ConditionData>();
        }
        
        public ConditionStepObject(string name, string description = "") : this(SerializableGuid.CreateNew(), name, description) { }
        
        public ConditionStepObject() : base("Unnamed ConditionStep")
        { 
            Conditions = new List<ConditionData>();
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
}
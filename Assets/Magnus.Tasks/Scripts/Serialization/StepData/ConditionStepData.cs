using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{    
    [Serializable]
    public class ConditionStepData : BinaryStepData
    {
        [PropertyOrder(5)] public bool OrderedConditions;
        [PropertyOrder(7), ListDrawerSettings(HideAddButton = true), SerializeReference] 
        public List<BaseObjectDataContainer> Conditions;

        public ConditionStepData(SerializableGuid id, string name, string description = "") : base(id, name, description)
        { 
            Conditions = new List<BaseObjectDataContainer>();
        }
        
        public ConditionStepData(string name, string description = "") : this(SerializableGuid.CreateNew(), name, description) { }
        
        public ConditionStepData() : base("Unnamed ConditionStep")
        { 
            Conditions = new List<BaseObjectDataContainer>();
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
                throw new NotImplementedException(); // TODO:
                // if (Conditions.Any(x => x.UsesGuid(guid)))
                //     return true;
            }

            return false;
        }

        public override void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
        {
            base.ReplaceGuid(guid, replacement);
            
            if (Conditions != null)
            {
                throw new NotImplementedException(); // TODO:
                // foreach (var guidUser in Conditions)
                //     guidUser.ReplaceGuid(guid, replacement);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;

namespace Rhinox.Magnus.Tasks
{
    [DataLayer, Serializable, RefactoringOldNamespace("Rhinox.VOLT.Data", "com.rhinox.volt")]
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
    


}
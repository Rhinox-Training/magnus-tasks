using Rhinox.VOLT.Training;
using UnityEngine;

namespace Rhinox.VOLT.Domain
{
    public class ObjectProximityCondition : BaseCondition
    {
        public Transform Object1;
        public Transform Object2;
        public float Distance;

        protected override bool OnInit()
        {
            return Object1 != null && Object2 != null && Distance >= 0.0f;
        }
        
        protected override void Check()
        {
            if (Vector3.Distance(Object1.position, Object2.position) < Distance)
                SetConditionMet();
        }
    }
}
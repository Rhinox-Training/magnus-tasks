using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.VOLT.Training;
using UnityEngine;

namespace Rhinox.VOLT.Domain
{
    public class ObjectRotationCondition : BaseCondition
    {
        public Transform Object;
        public Axis Axis;
        public float MinAngle = 0f;
        public float MaxAngle = 180f;

        protected override bool OnInit()
        {
            return Object != null;
        }

        protected override void Check()
        {
            if (IsMet)
                return;

            var r = Object.rotation.eulerAngles;
            bool angleMet = false;
            switch (Axis)
            {
                case Axis.X:
                    angleMet = r.x > MinAngle && r.x < MaxAngle;
                    break;

                case Axis.Y:
                    angleMet = r.y > MinAngle && r.y < MaxAngle;
                    break;

                case Axis.Z:
                    angleMet = r.z > MinAngle && r.z < MaxAngle;
                    break;
            }
            
            if (angleMet)
                SetConditionMet();
            
            base.Check();
        }
    }
}
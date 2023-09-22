using System;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.domain")]
    public class TransformToggleEvent : ValueReferenceEventAction<Transform>
    {
        public bool State;

        protected override void HandleAction(IReferenceResolver resolver, Transform targetData)
        {
            targetData.gameObject.SetActive(State);
        }

        public override Delegate CreateDelegate(object target)
        {
            if (!(target is Transform t))
                throw new InvalidOperationException("Target must be of the Transform type.");

            var type = typeof(GameObject);
            var methodInfo = type.GetMethod("SetActive");
            return ReflectionUtility.CreateDelegate(methodInfo, t.gameObject);
        }

        public override object[] GetParameters()
        {
            return new object[] {State};
        }
    }
}
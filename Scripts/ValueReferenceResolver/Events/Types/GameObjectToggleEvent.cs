using System;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.domain")]
    public class GameObjectToggleEvent : ValueReferenceEventAction<GameObject>
    {
        public bool State;

        protected override void HandleAction(IReferenceResolver resolver, GameObject targetData)
        {
            targetData.SetActive(State);
        }

        public override Delegate CreateDelegate(object target)
        {
            var type = typeof(GameObject);
            var methodInfo = type.GetMethod("SetActive");
            return ReflectionUtility.CreateDelegate(methodInfo, target);
        }

        public override object[] GetParameters()
        {
            return new object[] {State};
        }
    }
}
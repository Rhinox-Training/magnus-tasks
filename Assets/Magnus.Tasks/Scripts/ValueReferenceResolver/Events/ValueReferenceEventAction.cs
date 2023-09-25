using System;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.training")]
    public abstract class ValueReferenceEventAction<T> : ValueReferenceEventAction
    {
        protected T _resolvedTarget;

        public override void Invoke(IReferenceResolver resolver, SerializableGuid target)
        {
            TryResolveValues(resolver, target);

            HandleAction(resolver, _resolvedTarget);
        }

        public override void TryResolveValues(IReferenceResolver resolver, SerializableGuid target)
        {
            TryResolveTarget(resolver, target, ref _resolvedTarget);
        }

        protected abstract void HandleAction(IReferenceResolver resolver, T targetData);

        private void TryResolveTarget(IReferenceResolver resolver, SerializableGuid key, ref T target)
        {
            if (resolver == null)
                return;

            if (resolver.Resolve(key, out T resolvedTarget))
                target = resolvedTarget;
        }
    }

    [Serializable]
    public abstract class ValueReferenceEventAction
    {
        public abstract void TryResolveValues(IReferenceResolver resolver, SerializableGuid target);
        public abstract void Invoke(IReferenceResolver resolver, SerializableGuid target);

        // Below methods are for creating a BetterEvent
        public abstract Delegate CreateDelegate(object target);
        public abstract object[] GetParameters();
    }
}
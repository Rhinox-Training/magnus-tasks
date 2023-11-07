using System;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    public static class UnityEventDataUtility
    {
        public static void AppendToUnityEvent(IReadOnlyReferenceResolver resolver, ValueReferenceEvent e, ref UnityEvent target)
        {
            if (e.Events == null) return;
            
            for (int i = 0; i < e.Events.Count; ++i)
                AppendToUnityEvent(resolver, e.Events[i], ref target);
        }
        
        public static void AppendToUnityEvent(IReadOnlyReferenceResolver resolver, ValueReferenceEventEntry entry, ref UnityEvent target)
        {
            // TODO: do this in another manner; note: Resolver is not yet ready when this is applied
            // Create local parameters of the things that will be scoped
            var targetGuid = entry.Target;
            var valueRefAction = entry.Action;
            var parameters = entry.Action.GetParameters();

            if (target == null)
                target = new UnityEvent();
            
            target.AddListener(() =>
            {
                // Delay the resolution of the resolver as long as possible
                var del = CreateDelegate(resolver, targetGuid, valueRefAction, parameters);
                del?.DynamicInvoke(parameters);
            });
        }

        private static Delegate CreateDelegate(IReadOnlyReferenceResolver resolver, SerializableGuid targetGuid, ValueReferenceEventAction valueRefAction, object[] parameters)
        {
            resolver.Resolve(targetGuid, out object resolvedTarget);
            var del = valueRefAction.CreateDelegate(resolvedTarget);
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i] is ArgumentDataContainer container)
                {
                    object resolvedParameter = null;
                    if (!container.TryGetData(resolver, ref resolvedParameter))
                        PLog.Error<MagnusLogger>("Failed to resolve dynamic argument.");
                    parameters[i] = resolvedParameter;
                }
            }

            return del;
        }
        
        public static BetterEventEntry ConvertToBetterEventEntry(IReadOnlyReferenceResolver resolver, ValueReferenceEventEntry entry)
        {
            // TODO Should we wait until usage of the event to resolve this variable?
            resolver.Resolve(entry.Target, out object resolvedTarget);
            var convertedEntry = new BetterEventEntry(entry.Action.CreateDelegate(resolvedTarget), entry.Action.GetParameters());
            
            return convertedEntry;
        }
       
    }
}
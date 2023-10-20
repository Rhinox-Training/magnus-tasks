using System;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [HideReferenceObjectPicker, Serializable]
    [RefactoringOldNamespace("Rhinox.VOLT.Training", "com.rhinox.volt.training")]
    public abstract class BaseCondition
    {
#if UNITY_EDITOR
        [HorizontalGroup("Title", order: -5), ReadOnly, ShowInInspector, HideLabel, DisplayAsString()]
        private string _type => GetType().FullName;
#endif

        [ShowInInspector, ReadOnly, HorizontalGroup("Title", width: 55f, Order = -5)]
        [LabelWidth(40f)]
        public bool IsMet => CompletionState != CompletionState.None;
        
        public CompletionState CompletionState { get; private set; }

        public BaseStep Step { get; set; }

        public bool IsStarted { get; private set; }

        public bool Initialized { get; private set; }

        protected IReferenceResolver _valueResolver;

        [PropertyOrder(int.MaxValue)] public BetterEvent OnConditionMet;

        // TODO: append to betterevent instead of having a separate data event (only the data should have this)
        [PropertyOrder(int.MaxValue)] public ValueReferenceEvent OnBetterConditionMet;

        /// <summary>
        /// Called when the Condition should start tracking.
        /// <para>DO NOT Set condition completed in this method!</para>
        /// </summary>
        public void Init(IReferenceResolver resolver)
        {
            _valueResolver = resolver;
            OnBetterConditionMet.Initialize(_valueResolver);
            Initialized = OnInit();
        }

        protected virtual bool OnInit()
        {
            return true; // NOTE: Do not change this, should always be 'return true;' and nothing else
        }

        protected virtual void OnMet(bool hasFailed = false)
        {
        }

        /// <summary>
        /// Called when the Condition stops tracking; regardless of whether it was initialized
        /// </summary>
        public virtual void Terminate()
        {
            Initialized = false;
        }

        /// <summary>
        /// Called when the step is Reset
        /// </summary>
        public virtual void ResetCondition()
        {
            CompletionState = CompletionState.None;
        }

        /// <summary>
        /// Called when the Condition becomes active.
        /// </summary>
        public virtual void Start()
        {
            IsStarted = true;
        }

        public void Update()
        {
            Check();
        }

        protected virtual void Check()
        {
        }

        // TODO: Make this protected but accessible by the autocompletor
        public void SetConditionMet(bool hasFailed = false)
        {
            if (IsMet)
                return;

            CompletionState = hasFailed ? CompletionState.Failure : CompletionState.Success;
            OnConditionMet.Invoke();
            OnBetterConditionMet.Invoke();

            OnMet(hasFailed);
        }

        public bool Resolve<T>(SerializableGuid key, ref T value)
        {
            if (key == null || _valueResolver == null) return false;
            if (!_valueResolver.Resolve(key, out T resolvedValue)) return false;

            value = resolvedValue;
            return true;
        }

        public T[] GetSiblingConditions<T>()
        {
            return ((ConditionStep)Step).Conditions.OfType<T>().ToArray();
        }

        public T GetSiblingCondition<T>() where T : BaseCondition
        {
            foreach (var condition in ((ConditionStep)Step).Conditions)
                if (condition is T typedCondition)
                    return typedCondition;
            return null;
        }

        public virtual void OnDrawGizmosSelected()
        {
        }
    }
}
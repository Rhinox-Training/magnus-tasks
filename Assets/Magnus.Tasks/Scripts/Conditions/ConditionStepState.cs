﻿using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [HideReferenceObjectPicker]
    [SmartFallbackDrawn(false)]
    public class ConditionStepState : BaseBinaryStepState
    {
        [TabGroup("Settings")]
        public bool OrderedConditions = false;
        
        [OnValueChanged(nameof(OnConditionsChanged)), ListDrawerSettings(Expanded = true)]
        [TabGroup("Settings")]
        [SerializeReference]
        public List<BaseCondition> Conditions = new List<BaseCondition>();

        protected List<BaseCondition> _activeConditions;
        public IReadOnlyList<BaseCondition> ActiveConditions => _activeConditions;

        // Just a helper field to help search for the conditions used in the TaskViewer
        private string _conditionsUsed => string.Join(";", Conditions.Select(x => x?.GetType().Name));

        public event Action ActiveConditionsChanged;
        
        protected override void OnInitialize()
        {
            _activeConditions = new List<BaseCondition>();

            for (var i = 0; i < Conditions.Count; i++)
            {
                var condition = Conditions[i];
                if (condition == null)
                {
                    PLog.Error<MagnusLogger>($"Condition is null!");
                    continue;
                }

                // PLog.Info<VOLTLogger>($"Initializing condition: {condition}");
                condition.Step = this;

                condition.OnConditionMet.AddListener(OnConditionCompleted, i);
            }
            
            for (var i = 0; i < Conditions.Count; i++)
            {
                if (Conditions[i].IsMet)
                    continue;
                
                Conditions[i].Init(_valueResolver); 
                if (!Conditions[i].Initialized)
                    PLog.Error<MagnusLogger>($"! Condition {i} failed to initialize -> Condition will be skipped!");
            }
        }

        protected override void OnTerminate()
        {
            foreach (var condition in Conditions)
            {
                condition.OnConditionMet.RemoveListener((Action<int>) OnConditionCompleted);
                condition.Terminate();
            }
            
            _activeConditions.Clear();
            ActiveConditionsChanged?.Invoke();
        }

        protected override void OnStepStarted()
        {
            ActivateConditions();
            
            // Step might already be completed (i.e. ValidateData returned false for all condition)
            TryHandleStepCompletion();
        }

        private bool ActivateConditions()
        {
            bool changed = false;
            foreach (var condition in Conditions)
            {
                if (!condition.Initialized || condition.IsMet || _activeConditions.Contains(condition))
                    continue;
                
                _activeConditions.Add(condition);
                if (!condition.IsStarted) condition.Start();
                changed = true;

                // If ordered, only 1 condition should be active
                if (OrderedConditions) break;
            }
            
            if (changed)
                ActiveConditionsChanged?.Invoke();

            return changed;
        }

        private bool IsStepCompleted()
        {
            // Do not convert to LINQ pls
            for (int i = 0; i < Conditions.Count; ++i)
            {
                var condition = Conditions[i];
                if (condition.Initialized && !condition.IsMet)
                    return false;
            }

            return true;
        }

        protected virtual void OnConditionCompleted([HideInInspector] int conditionIndex)
        {
            var condition = Conditions[conditionIndex];
            _activeConditions.Remove(condition);
            
            ActiveConditionsChanged?.Invoke();

            if (OrderedConditions) // Initialize Next condition if Ordered
                ActivateConditions();

            TryHandleStepCompletion();
            
        }

        private void TryHandleStepCompletion()
        {
            if (State == ProcessState.Running && !IsStepCompleted())
                return;

            bool hasFailed = false;
            foreach (var condition in Conditions)
            {
                if (condition.CompletionState == CompletionState.Failure)
                {
                    hasFailed = true;
                    break;
                }
            }
            
            foreach (var condition in Conditions)
                condition.Terminate();

            _activeConditions.Clear();
            
            ActiveConditionsChanged?.Invoke();

            SetCompleted(hasFailed);
        }

        public override void HandleUpdate()
        {
            for (var i = _activeConditions.Count - 1; i >= 0; i--)
                _activeConditions[i].Update();
        }

        private void OnConditionsChanged()
        {
            if (Conditions == null) return;

            foreach (var condition in Conditions)
            {
                if (condition.OnConditionMet.Events == null)
                    condition.OnConditionMet.Events = new List<BetterEventEntry>();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Conditions == null) return;
            
            foreach (var cond in Conditions)
                cond?.OnDrawGizmosSelected();
        }
#endif
    }
}

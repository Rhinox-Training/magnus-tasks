using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Utilities;
using Rhinox.Lightspeed;
using Rhinox.VOLT.Data;
using Rhinox.Vortex;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.VOLT.Editor.Drawers
{
    public class StepSelectorAttributeDrawer : OdinAttributeDrawer<StepSelectorAttribute, SerializableGuid>
    {
        private GUIContent _noTaskBtnLabel;
        private GUIContent _nullBtnLabel;
        private GUIContent _btnLabel;

        private PropertyMemberHelper<int> _taskIdGetter;
        private TaskObject _currentTask;
        
        protected override void Initialize()
        {
            base.Initialize();
            _nullBtnLabel = new GUIContent("Choose a step");
            _noTaskBtnLabel = new GUIContent("Choose a task first.");

            _taskIdGetter = new PropertyMemberHelper<int>(Property, Attribute.TaskIdMember);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Refresh task info if needed
            var newTaskId = _taskIdGetter.GetValue();

            if (!_taskIdGetter.ErrorMessage.IsNullOrEmpty())
            {
                SirenixEditorGUI.ErrorMessageBox(_taskIdGetter.ErrorMessage);
                return;
            }
            
            if (_currentTask == null || newTaskId != _currentTask.ID)
                UpdateTaskInfo(newTaskId);
            
            // Draw dropdown
            var btnLabel = ValueEntry.SmartValue.IsNullOrEmpty() ? _nullBtnLabel : _btnLabel;
            if (_currentTask == null)
                btnLabel = _noTaskBtnLabel;
            OdinSelector<StepData>.DrawSelectorDropdown(label, btnLabel, this.ShowSelector);
        }

        private void UpdateTaskInfo(int taskId)
        {
            if (taskId < 0)
            {
                _currentTask = null;
                return;
            }
            
            DataLayer.PushEndPointFromSceneOrDefault();
            var table = DataLayer.ReadTable<TaskObject>();
            _currentTask = table.GetData(taskId);
            DataLayer.PopEndPoint();
            var step = _currentTask?.GetStep(ValueEntry.SmartValue);

            UpdateStepLabel(step);
        }

        private void UpdateStepLabel(StepData step)
        {
            int i = -1;
            if (_currentTask != null) // _currentTask should always have a value but eh just protect anyways
                i = _currentTask.Steps.IndexOf(step);
            if (step != null)
                _btnLabel = new GUIContent($"[{i}] {step.Name}");
            else
                _btnLabel = new GUIContent("Unknown Step");
        }

        private OdinSelector<StepData> ShowSelector(Rect rect)
        {
            GenericSelector<StepData> selector = this.CreateSelector();
            selector.SelectionConfirmed += ConfirmSelection;
            selector.ShowInPopup(rect);
            return selector;
        }

        private void ConfirmSelection(IEnumerable<StepData> obj)
        {
            var step = obj.FirstOrDefault();

            UpdateStepLabel(step);
            
            ValueEntry.SmartValue = step?.ID;
        }

        private GenericSelector<StepData> CreateSelector()
        {
            ICollection<StepData> steps = _currentTask?.Steps;
            if (steps == null)
                steps = Array.Empty<StepData>();
            
            var source = steps
                .Select((x, i) => new GenericSelectorItem<StepData>($"{i}: {x.Name}", x))
                .ToArray();

            GenericSelector<StepData> genericSelector = new GenericSelector<StepData>("Steps", false, source);
            genericSelector.EnableSingleClickToSelect();

            if (!ValueEntry.SmartValue.IsNullOrEmpty())
                genericSelector.SetSelection(steps.FirstOrDefault(x => ValueEntry.SmartValue.Equals(x.ID)));
            
            return genericSelector;
        }
    }
}
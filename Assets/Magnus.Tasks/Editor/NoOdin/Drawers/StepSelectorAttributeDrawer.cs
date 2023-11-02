using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Vortex;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(StepSelectorAttribute))]
    public class StepSelectorAttributeDrawer : BasePropertyDrawer<SerializableGuid, StepSelectorAttributeDrawer.DrawerData>
    {
        public class DrawerData
        {
            public GenericHostInfo Info;
            public IPropertyMemberHelper<int> TaskIdGetter;
            public TaskObject CurrentTask;
            public GUIContent ButtonLabel;
        }
        
        private static GUIContent _nullBtnLabel;
        private static GUIContent _noTaskBtnLabel;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            if (_nullBtnLabel == null)
                _nullBtnLabel = new GUIContent("Choose a step");
            if (_noTaskBtnLabel == null)
                _noTaskBtnLabel = new GUIContent("Choose a task first.");
        }
        
        protected override DrawerData CreateData(GenericHostInfo info)
        {
            var data = new DrawerData
            {
                Info = info
            };
            
            if (info.TryGetAttribute(out StepSelectorAttribute attr))
            {
                data.TaskIdGetter = MemberHelper.Create<int>(info, attr.TaskIdMember);
            }

            return data;
        }

        protected override GenericHostInfo GetHostInfo(DrawerData data)
        {
            return data.Info;
        }

        protected override void DrawProperty(Rect position, ref DrawerData data, GUIContent label)
        {
            // Refresh task info if needed
            data.TaskIdGetter.DrawError(position);
            
            var newTaskId = data.TaskIdGetter.GetSmartValue();
            
            if (data.CurrentTask == null || newTaskId != data.CurrentTask.ID)
                UpdateTaskInfo(data, newTaskId);
            
            // Draw dropdown
            var btnLabel = SmartValue.IsNullOrEmpty() ? _nullBtnLabel : data.ButtonLabel;
            if (data.CurrentTask == null)
                btnLabel = _noTaskBtnLabel;

            var valueRect = EditorGUI.PrefixLabel(position, label);
            if (EditorGUI.DropdownButton(valueRect, btnLabel, FocusType.Passive))
                ShowPicker(data, valueRect);
        }

        private void ShowPicker(DrawerData data, Rect valueRect)
        {
            var options = GetOptions(data).ToArray();
            GenericPicker.Show(valueRect, options, (x) => ConfirmSelection(data, x),
                textSelector: (x) => { return $"{options.IndexOf(x)}: {x.Name}"; });
        }
        
        
        private void ConfirmSelection(DrawerData data, StepData step)
        {
            UpdateStepLabel(data, step);
            SmartValue = step?.ID;
        }

        private ICollection<StepData> GetOptions(DrawerData drawerData)
        {
            ICollection<StepData> steps = drawerData.CurrentTask?.Steps;
            if (steps == null)
                steps = Array.Empty<StepData>();
            return steps;
        }

        private void UpdateTaskInfo(DrawerData drawerData, int taskId)
        {
            if (taskId < 0)
            {
                drawerData.CurrentTask = null;
                return;
            }
            
            DataLayer.PushEndPointFromSceneOrDefault();
            var table = DataLayer.ReadTable<TaskObject>();
            drawerData.CurrentTask = table.GetData(taskId);
            DataLayer.PopEndPoint();
            
            var step = drawerData.CurrentTask?.GetStep(SmartValue);
            UpdateStepLabel(drawerData, step);
        }
        
        private void UpdateStepLabel(DrawerData data, StepData step)
        {
            int i = -1;
            if (data.CurrentTask != null) // _currentTask should always have a value but eh just protect anyways
                i = data.CurrentTask.Steps.IndexOf(step);
            if (step != null)
                data.ButtonLabel = new GUIContent($"[{i}] {step.Name}");
            else
                data.ButtonLabel = new GUIContent("Unknown Step");
        }
    }
}
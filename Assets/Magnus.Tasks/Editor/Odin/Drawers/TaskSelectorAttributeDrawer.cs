using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Vortex;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class TaskSelectorAttributeDrawer : OdinAttributeDrawer<TaskSelectorAttribute, int>
    {
        private GUIContent _nullBtnLabel;
        private GUIContent _btnLabel;

        private int _cachedTaskLabel;

        protected override void Initialize()
        {
            base.Initialize();
            _nullBtnLabel = new GUIContent("Choose a task");

            if (ValueEntry.SmartValue >= 0)
            {
                DataLayer.PushEndPointFromSceneOrDefault();
                var table = DataLayer.ReadTable<TaskObject>();
                var id = ValueEntry.SmartValue;
                var task = table.GetData(id);
                UpdateTaskLabel(task);
                DataLayer.PopEndPoint();
            }
        }

        private void UpdateTaskLabel(TaskObject task)
        {
            if (task != null)
                _btnLabel = GetTaskLabel(task);
            else
                _btnLabel = new GUIContent("Unknown task: ID " + ValueEntry.SmartValue);

            _cachedTaskLabel = ValueEntry.SmartValue;
        }

        private static GUIContent GetTaskLabel(TaskObject task)
        {
            return new GUIContent($"[{task.ID}] {task.Name}");
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (ValueEntry.SmartValue != _cachedTaskLabel)
            {
                var table = DataLayer.ReadTable<TaskObject>();
                var task = table.GetData(ValueEntry.SmartValue);
                UpdateTaskLabel(task);
            }
            var btnLabel = ValueEntry.SmartValue >= 0 ? _btnLabel : _nullBtnLabel;
            OdinSelector<TaskObject>.DrawSelectorDropdown(label, btnLabel, this.ShowSelector);
        }

        private OdinSelector<TaskObject> ShowSelector(Rect rect)
        {
            GenericSelector<TaskObject> selector = this.CreateSelector();
            selector.SelectionConfirmed += ConfirmSelection;
            selector.ShowInPopup(rect);
            return selector;
        }

        private void ConfirmSelection(IEnumerable<TaskObject> obj)
        {
            var task = obj.FirstOrDefault();

            if (task != null)
                UpdateTaskLabel(task);
            
            ValueEntry.SmartValue = task?.ID ?? -1;
        }

        private GenericSelector<TaskObject> CreateSelector()
        {
            DataLayer.PushEndPointFromSceneOrDefault();
            var table = DataLayer.ReadTable<TaskObject>();
            var tasks = table.GetAllData();
            var source = tasks
                .Select(x => new GenericSelectorItem<TaskObject>(x.Name, x))
                .Prepend(new GenericSelectorItem<TaskObject>("<None>", null))
                .ToArray();
            DataLayer.PopEndPoint();

            GenericSelector<TaskObject> genericSelector = new GenericSelector<TaskObject>("Tasks", false, source);
            genericSelector.EnableSingleClickToSelect();

            genericSelector.SetSelection(tasks.FirstOrDefault(x => x.ID == ValueEntry.SmartValue));
            
            return genericSelector;
        }
    }
}
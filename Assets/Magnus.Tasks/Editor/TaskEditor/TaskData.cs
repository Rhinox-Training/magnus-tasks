using System;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Editor
{
    // [Serializable]
    [HideReferenceObjectPicker]
    public class TaskData
    {
        public TaskObject Task { get; private set; }
        public EndPointTaskSet ParentSet { get; private set; }
        
        [ShowInInspector, HideLabel, DisplayAsString] 
        [HorizontalGroup("TaskEntry/Left", 45), HorizontalGroup("TaskEntry")]
        public int TaskID => Task.ID;
        
        [ShowInInspector, HideLabel, DisplayAsString] 
        [HorizontalGroup("TaskEntry/Left")]
        public string TaskName => Task.Name;
        
        [ShowInInspector, HideLabel]
        [HorizontalGroup("TaskEntry/Left", MaxWidth = 90)]
        [CustomValueDrawer(nameof(NumberOfStepsDrawer))]
        public int NumberOfSteps => Task.Steps != null ? Task.Steps.Count : 0;
        
        public TaskData(EndPointTaskSet parentSet, TaskObject task)
        {
            ParentSet = parentSet;
            Task = task;
        }

        [ButtonGroup("TaskEntry/Right/Buttons"), Button(), HorizontalGroup("TaskEntry/Right", MaxWidth = 120)]
        private void Edit()
        {
            EditorApplication.delayCall += () => ParentSet.ParentPage.EditPage(this);
        }

        [ButtonGroup("TaskEntry/Right/Buttons"), Button()]
        private void Remove()
        {
            if (!EditorUtility.DisplayDialog("Confirmation",
                "This will remove the task from the DataLayer. \nAre you sure you want to continue?",
                "Confirm", "Cancel"))
                return;
            EditorApplication.delayCall += () => ParentSet.Remove(this);
        }
        
        
        private int NumberOfStepsDrawer(int value, GUIContent label)
        {
            EditorGUILayout.LabelField($"{value} step{(value != 1 ? "s" : string.Empty)}");
            return value;
        }
    }
}
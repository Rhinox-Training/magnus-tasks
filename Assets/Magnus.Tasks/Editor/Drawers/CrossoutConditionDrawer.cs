using System.Collections.Generic;
using Rhinox.GUIUtils;
using Rhinox.Lightspeed;
using Rhinox.Magnus.Tasks;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    /// <summary>
    /// This script will cross out the names of the step GameObjects in the hierarchy that hold a Condition that has been completed for the task.
    /// </summary>
    [InitializeOnLoad]
    public static class CrossoutConditionDrawer
    {
        private static bool _needsRefresh = true;
        private static Dictionary<int, BaseStep> _stepById;

        private static TickDelay _delay;

        static CrossoutConditionDrawer()
        {
            _stepById = new Dictionary<int, BaseStep>();

            EditorApplication.update += OnEditorUpdate;
            EditorApplication.hierarchyWindowItemOnGUI += DrawCrossOutCondition;
        }

        private static void OnEditorUpdate()
        {
            if (_delay.Tick(60))
                _needsRefresh = true;
        }

        private static void DrawCrossOutCondition(int instanceID, Rect selectionRect)
        {
            if (!Application.isPlaying || !TaskManager.HasInstance) return;

            if (_needsRefresh)
                _stepById.Clear();

            BaseStep step;

            if (_stepById.ContainsKey(instanceID))
            {
                step = _stepById[instanceID];
            }
            else
            {
                var currentObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                if (currentObject == null) return;
                step = currentObject?.GetComponent<BaseStep>();
                _stepById[instanceID] = step;
            }

            if (step == null || step.State != ProcessState.Finished)
                return;

            if (step.CompletionState == CompletionState.Success)
            {
                var rect = selectionRect.SetHeight(1);
                rect = RectExtensions.AddY(rect, selectionRect.height / 2 - 1);
                // Crossout
                EditorGUI.DrawRect(rect, CustomGUIStyles.HoverColor);
            }

            if (step.CompletionState == CompletionState.Failure)
            {
                var rect = selectionRect.SetHeight(1);
                rect = RectExtensions.AddY(rect, selectionRect.height - 1);
                // Crossout
                EditorGUI.DrawRect(rect, Color.red);
            }
        }
    }
}
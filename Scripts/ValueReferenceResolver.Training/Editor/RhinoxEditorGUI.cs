using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Data
{
    public static class RhinoxEditorGUI
    {
        private static float slideRectSensitivity = 0.0f;
        private static float slideDeltaBuffer;
        
        public static int SlideRectInt(Rect rect, int id, int value)
        {
            if (!GUI.enabled)
                return value;
            UnityEngine.EventType type = Event.current.type;
            if (type == UnityEngine.EventType.Layout)
                return value;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);
            if (type == UnityEngine.EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                EditorGUIUtility.SetWantsMouseJumping(1);
                Event.current.Use();
                GUIUtility.keyboardControl = 0;
                RhinoxEditorGUI.slideRectSensitivity = Mathf.Max(0.5f, Mathf.Pow((float) Mathf.Abs(value), 0.5f) * 0.03f);
                RhinoxEditorGUI.slideDeltaBuffer = 0.0f;
            }
            else if (GUIUtility.hotControl == id)
            {
                if (Event.current.rawType == UnityEngine.EventType.MouseDrag)
                {
                    float num = (HandleUtility.niceMouseDelta + RhinoxEditorGUI.slideDeltaBuffer) * RhinoxEditorGUI.slideRectSensitivity;
                    value += (int) num;
                    RhinoxEditorGUI.slideDeltaBuffer = num - (float) (int) num;
                    GUI.changed = true;
                    Event.current.Use();
                }
                else if (Event.current.rawType == UnityEngine.EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Event.current.Use();
                }
            }
            return value;
        }
    }
}
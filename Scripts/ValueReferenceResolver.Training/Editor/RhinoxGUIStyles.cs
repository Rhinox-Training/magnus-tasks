using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Data
{
    public static class RhinoxGUIStyles
    {

        private static GUIStyle toolbarButton = null;
        private static GUIStyle label = null;
        private static GUIStyle labelCentered = null;

        public static GUIStyle ToolbarButton
        {
            get
            {
                if (RhinoxGUIStyles.toolbarButton == null)
                    RhinoxGUIStyles.toolbarButton = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        fixedHeight = 0.0f,
                        alignment = TextAnchor.MiddleCenter,
                        stretchHeight = true,
                        stretchWidth = false
                    };
                return RhinoxGUIStyles.toolbarButton;
            }
        }
        public static GUIStyle Label
        {
            get
            {
                if (RhinoxGUIStyles.label == null)
                    RhinoxGUIStyles.label = new GUIStyle(EditorStyles.label)
                    {
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                return RhinoxGUIStyles.label;
            }
        }
        public static GUIStyle LabelCentered
        {
            get
            {
                if (RhinoxGUIStyles.labelCentered == null)
                    RhinoxGUIStyles.labelCentered = new GUIStyle(RhinoxGUIStyles.Label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                return RhinoxGUIStyles.labelCentered;
            }
        }
    }
}
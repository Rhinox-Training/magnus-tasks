using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Data
{
    public static class GUIHelper
    {
        public static void RemoveFocusControl()
        {
            GUIUtility.hotControl = 0;
            DragAndDrop.activeControlID = 0;
            GUIUtility.keyboardControl = 0;
        }

    }
}
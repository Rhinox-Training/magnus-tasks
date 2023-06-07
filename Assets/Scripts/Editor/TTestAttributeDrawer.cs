
using UnityEditor;
using UnityEngine;

namespace Rhinox
{
    [CustomPropertyDrawer(typeof(TTestAttribute))]
    public class TTestAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            
            base.OnGUI(position, property, label);
        }
    }

}

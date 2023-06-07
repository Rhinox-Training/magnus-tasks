using UnityEditor;
using UnityEngine;

namespace Rhinox
{
    //[CustomEditor(typeof(Test))]
    public class TestEditor : Editor
    {

        private string[] _tabs = {"Tab 1", "Tab 2", "Tab 3"};
        private int _index;
        public override void OnInspectorGUI()
        {
            //EditorGUILayout.BeginVertical();
            _index = GUILayout.Toolbar(_index, _tabs);
            //EditorGUILayout.EndVertical();
            
            base.OnInspectorGUI();
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MagnusBehaviour), true)]
    public class MagnusEditor : Editor
    {
        private void OnEnable()
        {
            
        }


        private int _index = 0;

        public override void OnInspectorGUI()
        {
            var target = serializedObject;
            var prop = target.GetIterator();
            
            
            
            List<string> _names = new List<string>();
            Dictionary<string, List<SerializedProperty>> props = new Dictionary<string, List<SerializedProperty>>();

            while (prop.Next(true))
            {

                var attr = prop.BetterGetAttribute<TabAttribute>();
                if (attr != null)
                {
                    if (!_names.Contains(attr.Name))
                    {
                        _names.Add(attr.Name);
                    }

                    List<SerializedProperty> currProps = null;

                    if (props.ContainsKey(attr.Name)) currProps = props[attr.Name];
                    else
                    {
                        currProps = new List<SerializedProperty>();
                        props.Add(attr.Name, currProps);
                    }
                    
                    //currProps.Add(prop.Copy());
                    currProps.Add(serializedObject.FindProperty(prop.propertyPath));
                }
            }
            
            _index = GUILayout.Toolbar(_index, _names.ToArray());

            

            foreach (string str in _names)
            {
                
                if(_names[_index] != str) continue;
                
                
                List<SerializedProperty> ps = props[str];

                foreach (var s in ps)
                {
                    //Debug.Log(s.name);
                    EditorGUILayout.PropertyField(s);
                }
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}
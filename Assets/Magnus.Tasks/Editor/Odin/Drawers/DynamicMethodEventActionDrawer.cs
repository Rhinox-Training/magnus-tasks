using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class DynamicMethodEventActionDrawer<TValue, TDrawer> : SimpleOdinValueDrawer<TDrawer> where TDrawer : DynamicMethodEventAction<TValue>
    {
        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<TDrawer> valueEntry)
        {
            GUILayout.BeginVertical();
            GetChildProperty(nameof(DynamicMethodEventAction<TValue>.MethodInfo)).Draw();
            var argumentsProperty = GetChildProperty(nameof(DynamicMethodEventAction<TValue>.Data));

            foreach (var argumentProperty in argumentsProperty.Children)
            {
                GUILayout.BeginHorizontal();
                var actualArgumentProperty = argumentProperty.Children[0];
                GUILayout.BeginVertical();
                actualArgumentProperty.Draw();
                GUILayout.EndVertical();
                if (argumentProperty.ValueEntry.WeakSmartValue != null)
                {
                    var dynamicArg = argumentProperty.ValueEntry.WeakSmartValue as DynamicMethodArgument;
                    EditorGUI.BeginDisabledGroup(dynamicArg == null);
                    if (SirenixEditorGUI.IconButton(EditorIcons.Refresh))
                        dynamicArg.Switch();
                    EditorGUI.EndDisabledGroup();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            valueEntry.ApplyChanges();
        }
    }
}
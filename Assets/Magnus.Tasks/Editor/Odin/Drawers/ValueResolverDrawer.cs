using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class ValueResolverDrawer<T> : SimpleOdinValueDrawer<T> where T : IValueResolver
    {
        private ICollection<Type> _resolverTypes;

        private Type _targetType;
        private GUIContent _buttonContent;

        protected override void Initialize()
        {
            _targetType = ValueEntry.SmartValue.GetTargetType();
            _resolverTypes = GetValueResolverTypes();

            _buttonContent = new GUIContent();
            
            base.Initialize();
        }

        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<T> valueEntry)
        {
            var valueResolver = valueEntry.SmartValue;
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayoutOptions.Width(EditorGUIUtility.labelWidth - GUI.skin.button.margin.right));

            _buttonContent.text = valueResolver.SimpleName;
            _buttonContent.tooltip = valueResolver.ComplexName;
            
            if (GUILayout.Button(_buttonContent))
            {
                DrawTypeDropdown(GUILayoutUtility.GetLastRect(), _resolverTypes, (selectedType) =>
                {
                    if (selectedType.ContainsGenericParameters)
                        selectedType = selectedType.MakeGenericType(_targetType);
                    ValueEntry.SmartValue = (T) Activator.CreateInstance(selectedType);
                });
            }
            
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            foreach (var child in Property.Children)
                child.Draw();
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
        }
        
        private ICollection<Type> GetValueResolverTypes()
        {
            bool isUnityObject = _targetType.InheritsFrom(typeof(UnityEngine.Object));

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.ImplementsOrInherits(typeof(IValueResolver)))
                .Where(x => !x.IsInterface && !x.IsAbstract && x.IsPublic)
                .Where(x => !x.ContainsGenericParameters || x.AreGenericConstraintsSatisfiedBy(_targetType))
                .Where(x => !isUnityObject || !x.InheritsFrom(typeof(ConstValueResolver<>)))
                .ToArray();
        }
    }

    public class ValueResolverAttributeProcessor<T> : OdinAttributeProcessor<T> where T : IValueResolver
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            //attributes.Add(new ShowDrawerChainAttribute());
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    [CustomPropertyDrawer(typeof(IValueResolver), true)]
    public class ValueResolverDrawer : BasePropertyDrawer<IValueResolver, ValueResolverDrawer.DrawerData>
    {
        private ICollection<Type> _resolverTypes;

        private Type _targetType;
        private GUIContent _buttonContent;
        protected GUIContent _noneContent;
        protected const string NULL_STRING = "<None>";
        
        public class DrawerData
        {
            public GenericHostInfo Info;
            public BasePicker Picker;
            public GUIContent ActiveContent;
        }

        protected override GenericHostInfo GetHostInfo(DrawerData data) => data.Info;
        protected override void OnInitialize()
        {
            base.OnInitialize();

            _noneContent = new GUIContent(NULL_STRING);
        }
        
        protected override void OnUpdateActiveData()
        {
            base.OnUpdateActiveData();
            _targetType = SmartValue.GetTargetType();
            _resolverTypes = GetValueResolverTypes();

            _buttonContent = new GUIContent();
        }
        
        protected override DrawerData CreateData(GenericHostInfo info)
        {
            var data = new DrawerData
            {
                Info = info,
                Picker = null,
                ActiveContent = new GUIContent()
            };

            var value = info.GetValue();
            if (value != null)
            {
                data.ActiveContent.text = GetNameForSelection(value.GetType());
            }
            return data;
        }
        
        protected override void DrawProperty(Rect position, ref DrawerData data, GUIContent label)
        {
            float preferredWidth = EditorGUIUtility.labelWidth - GUI.skin.button.margin.right;

            Rect dropdownRect = default, valueRect = default;
            if (position.IsValid())
                position.SplitX(preferredWidth, out dropdownRect, out valueRect);
            
            
            _buttonContent.text = SmartValue.SimpleName;
            _buttonContent.tooltip = SmartValue.ComplexName;
            
            var content = SmartValue == null ? _noneContent : _buttonContent;
            
            if (EditorGUI.DropdownButton(dropdownRect, content, FocusType.Keyboard))
                DoPickerDropdown(position, data);
            
            CallInnerDrawer(valueRect, GUIContent.none);
        }

        protected void OnOptionSelected(object selection, string selectionText, DrawerData data)
        {
            data.ActiveContent.text = selectionText;
            
            var selectedType = selection as Type;
            if (selectedType == null)
                return;
            if (selectedType.ContainsGenericParameters)
                selectedType = selectedType.MakeGenericType(_targetType);
            SmartValue = (IValueResolver) Activator.CreateInstance(selectedType);
        }

        protected string GetNameForSelection(object selection)
        {
            if (selection == null)
                return _noneContent.text;
            return ((Type) selection).GetCSharpName(includeNameSpace: false);
        }

        protected virtual void DoPickerDropdown(Rect position, DrawerData data)
        {
            if (data.Picker == null)
            {
                data.Picker = new TypePicker(_resolverTypes);
                data.Picker.OptionSelectedGeneric += x => OnOptionSelected(x, GetNameForSelection(x), data);
            }
            data.Picker.Show(position);
        }

        private ICollection<Type> GetValueResolverTypes()
        {
            bool isUnityObject = _targetType.InheritsFrom(typeof(UnityEngine.Object));

            return AppDomain.CurrentDomain.GetDefinedTypesOfType<IValueResolver>()
                .Where(x => GenericMatchesFor(x))
                .Where(x => !isUnityObject || !x.InheritsFrom(typeof(ConstValueResolver<>)))
                .ToArray();
        }

        private bool GenericMatchesFor(Type type)
        {
            var arguments = type.GetArgumentsOfInheritedOpenGenericClass(typeof(IValueResolver<>));
            if (arguments.Length > 0)
                return arguments[0] == type || type.InheritsFrom(arguments[0]);
            return false;
        }
    }
}
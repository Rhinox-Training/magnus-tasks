using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Rhinox.VOLT.Training;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using RectExtensions = Rhinox.Lightspeed.RectExtensions;
using TypeExtensions = Sirenix.Utilities.TypeExtensions;


namespace Rhinox.VOLT.Editor
{
    public class ConditionDataDrawer : SimpleOdinValueDrawer<ConditionData>
    {
        private IReferenceResolver _referenceResolver;
        private InspectorProperty _paramsProperty;
        private IPropertyValueEntry<ParamData[]> _paramsValueEntry;

        protected override void Initialize()
        {
            base.Initialize();
            _referenceResolver = Property.FindReferenceResolver();
            _paramsProperty = GetChildProperty<ParamData[]>(nameof(ConditionData.Params), out _paramsValueEntry);

        }
        
        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<ConditionData> valueEntry)
        {
            // Title
            GUILayout.BeginHorizontal();
            
            GUILayout.Label(valueEntry.SmartValue.ConditionType.Name, EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            if (SirenixEditorGUI.IconButton(EditorIcons.Refresh))
                CleanParams();
            
            GUILayout.EndHorizontal();
            

            if (_paramsValueEntry == null || _paramsValueEntry.SmartValue.Length == 0)
            {
                var rect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(rect, "<No parameters>", EditorStyles.miniLabel);
                return;
            }
            
            for (int i = 0; i < _paramsValueEntry.SmartValue.Length; ++i)
            {
                var paramData = _paramsValueEntry.SmartValue[i];

                Type paramType = valueEntry.SmartValue.GetParamType(paramData);
                if (paramType == null)
                    continue;
                
                if (ReflectionUtility.IsSimpleType(paramType))
                {
                    var rect = EditorGUILayout.GetControlRect();
                    if (typeof(float) == paramType)
                    {
                        paramData.MemberData = EditorGUI.FloatField(rect, paramData.Name, (float) paramData.MemberData);
                    }
                    else if (typeof(bool) == paramType)
                    {
                        paramData.MemberData = EditorGUI.Toggle(rect, paramData.Name, (bool) paramData.MemberData);
                    }
                    else if (typeof(int) == paramType)
                    {
                        paramData.MemberData = EditorGUI.IntField(rect, paramData.Name, (int) paramData.MemberData);
                    }
                    else if (typeof(string) == paramType)
                    {
                        paramData.MemberData = EditorGUI.TextField(rect, paramData.Name, (string) paramData.MemberData);
                    }
                }
                else if (TypeExtensions.InheritsFrom(typeof(UnityEngine.Object), paramType))
                {
                    var rect = EditorGUILayout.GetControlRect();
                    EditorGUI.ObjectField(rect, paramData.Name, (UnityEngine.Object)paramData.MemberData, paramType, false);
                }
                else if (typeof(SerializableGuid) == paramType)
                {
                    if (paramData.MemberData == null)
                        paramData.MemberData = SerializableGuid.Empty;
                    //var rect = EditorGUILayout.GetControlRect();
                    DrawSerializedGuid(valueEntry, i, paramData);
                }
                else if (CheckValue(_paramsProperty.Children[i], out var dataType))
                {
                    var editorDataProperty = _paramsProperty.Children[i];
                    if (dataType.ImplementsOrInherits(typeof(ValueReferenceEvent)))
                        editorDataProperty.Draw(null);
                    else
                        editorDataProperty.Draw(new GUIContent(paramData.Name));
                }
                else
                {
                    GetChildProperty("MemberData", _paramsProperty.Children[i])
                        .Draw(new GUIContent(paramData.Name));
                }
                
                
                _paramsValueEntry.SmartValue[i] = paramData;
            }
        }

        private void CleanParams()
        {
            var wantedFields = ConditionDataHelper.GetParamDataFields(ValueEntry.SmartValue.ConditionType.Type, true);
            var currentData = _paramsValueEntry.SmartValue;

            List<ParamData> result = new List<ParamData>();

            foreach (var info in wantedFields)
            {
                var data = currentData.FirstOrDefault(x => ParamDataMatchesMember(x, info));
                if (data != null)
                {
                    result.Add(data);
                    continue;
                }

                var paramData = ParamData.CreateWithValue(info, TypeExtensions.GetReturnType(info).GetDefault());
                paramData = EditorParamDataHelper.Convert(paramData);
                result.Add(paramData);
            }

            _paramsValueEntry.SmartValue = result.ToArray();
            _paramsValueEntry.ApplyChanges();
        }

        private bool ParamDataMatchesMember(ParamData data, MemberInfo info)
        {
            if (data.MemberType != info.MemberType) return false;
            if (data.Name != info.Name) return false;
            if (data.Flags != info.GetFlags()) return false;
            
            if (data.Type != TypeExtensions.GetReturnType(info)) return false;
            
            return true;
        }

        private bool CheckValue(InspectorProperty prop, out Type dataType)
        {
            var t = typeof(EditorParamData<>);
            var propValueType = prop.ValueEntry.WeakSmartValue.GetType();
            if (!propValueType.IsGenericType)
            {
                dataType = null;
                return false;
            }

            dataType = propValueType.GetGenericArguments().First();
            return propValueType.GetGenericTypeDefinition() == t;

        }

        private void DrawSerializedGuid(IPropertyValueEntry<ConditionData> conditionDataVal, int paramIndex, ParamData paramData)
        {
            var paramsProperty = GetChildProperty(nameof(ConditionData.Params), conditionDataVal.Property);
            var valueProperty = GetChildProperty(nameof(ParamData.MemberData), paramsProperty.Children[paramIndex]);
            
            
            
            var memberInfo = conditionDataVal.SmartValue.GetMemberInfo(paramData);
            if (!ValueReferenceHelper.TryGetValueReference(memberInfo, out ValueReferenceInfo info))
            {
                valueProperty?.Draw(new GUIContent(paramData.Name));
                return;
            }
            
            GUILayout.BeginHorizontal();
            {
                // Label with dropdown
                valueProperty?.Draw(new GUIContent(paramData.Name));

                var takenWidth = CustomGUIStyles.Label.CalcSize(paramData.Name).x;

                var typeRect = GUILayoutUtility.GetLastRect();
                typeRect.width = GUIHelper.BetterLabelWidth;
                typeRect = RectExtensions.AlignRight(typeRect, typeRect.width - takenWidth);
                GUI.Label(typeRect, info.ReferenceType.Name, SirenixGUIStyles.RightAlignedGreyMiniLabel);

                GUILayout.BeginHorizontal(GUILayoutOptions.Width(100));
                // Default Button
                {
                    SerializableGuid selectedGuid = paramData.MemberData as SerializableGuid;
                    SerializableGuid defaultGuid = _referenceResolver.GetDefault(new SerializableType(info.ReferenceType), memberInfo as FieldInfo);
                    bool isDefault = selectedGuid != null && selectedGuid.Equals(defaultGuid);
                    EditorGUI.BeginDisabledGroup(isDefault);
                    if (GUILayout.Button( /*fieldButtonRect, */"Mark Default"))
                        _referenceResolver.RegisterDefault(memberInfo as FieldInfo, selectedGuid);
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }
    }
}
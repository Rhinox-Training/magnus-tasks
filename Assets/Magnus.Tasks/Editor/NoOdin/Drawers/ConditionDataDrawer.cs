using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Utilities;
using UnityEditor;
using UnityEngine;
using RectExtensions = Rhinox.Lightspeed.RectExtensions;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(ConditionData))]
    public class ConditionDataDrawer : BasePropertyDrawer<ConditionData>
    {
        private IReferenceResolver _referenceResolver;
        private TypedHostInfoWrapper<ParamData[]> _paramsValueEntry;
        private IEditorDrawable _paramsDrawer;

        protected override void OnUpdateActiveData()
        {
            base.OnUpdateActiveData();
            _referenceResolver = this.FindReferenceResolver();

            HostInfo.TryGetChild<ParamData[]>(nameof(ConditionData.Params), out _paramsValueEntry);
        }

        protected override float GetPropertyHeight(GUIContent label, in GenericHostInfo data)
        {
            float height = base.GetPropertyHeight(label, in data);
            if (SmartValue == null)
                return height;

            if (_paramsDrawer != null)
            {
                return height + _paramsDrawer.ElementHeight;
            }
            
            int rowCount = SmartValue.Params != null ? SmartValue.Params.Length + 1 : 2;
            return rowCount * height;
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            if (HostInfo == null || SmartValue == null)
            {
                CallInnerDrawer(position, label);
                return;
            }
            // Title
            var topRow = position.SetHeight(EditorGUIUtility.singleLineHeight);
            GUI.Label(topRow.AlignLeft(topRow.width - 24.0f), SmartValue.ConditionType.Name, EditorStyles.boldLabel);
            if (CustomEditorGUI.IconButton(topRow.AlignRight(16.0f), UnityIcon.AssetIcon("Fa_Redo")))
                CleanParams();

            var dataRow = topRow.AddY(EditorGUIUtility.singleLineHeight);
            

            if (_paramsValueEntry == null || _paramsValueEntry.SmartValue.Length == 0)
            {
                var rect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(rect, "<No parameters>", EditorStyles.miniLabel);
                return;
            }

            if (_paramsDrawer != null)
            {
                _paramsDrawer.Draw(dataRow, GUIContent.none);
                return;
            }
            
            for (int i = 0; i < _paramsValueEntry.SmartValue.Length; ++i)
            {
                var paramData = _paramsValueEntry.SmartValue[i];

                var childDrawer = GetChildDrawer(nameof(ConditionData.Params), i);
                if (childDrawer == null)
                    continue;
                
                childDrawer.Draw(dataRow, new GUIContent(paramData.Name));
                // Type paramType = SmartValue.GetParamType(paramData);
                // if (paramType == null)
                //     continue;
                //
                // if (ReflectionUtility.IsSimpleType(paramType))
                // {
                //     if (paramData.MemberData == null)
                //         paramData.MemberData = Activator.CreateInstance(paramType);
                //     var rect = dataRow;
                //     if (typeof(float) == paramType)
                //     {
                //         paramData.MemberData = EditorGUI.FloatField(rect, paramData.Name, (float) paramData.MemberData);
                //     }
                //     else if (typeof(bool) == paramType)
                //     {
                //         paramData.MemberData = EditorGUI.Toggle(rect, paramData.Name, (bool) paramData.MemberData);
                //     }
                //     else if (typeof(int) == paramType)
                //     {
                //         paramData.MemberData = EditorGUI.IntField(rect, paramData.Name, (int) paramData.MemberData);
                //     }
                //     else if (typeof(string) == paramType)
                //     {
                //         paramData.MemberData = EditorGUI.TextField(rect, paramData.Name, (string) paramData.MemberData);
                //     }
                // }
                // else if (TypeExtensions.InheritsFrom(typeof(UnityEngine.Object), paramType))
                // {
                //     var rect = dataRow;
                //     EditorGUI.ObjectField(rect, paramData.Name, (UnityEngine.Object)paramData.MemberData, paramType, false);
                // }
                // else if (typeof(SerializableGuid) == paramType)
                // {
                //     if (paramData.MemberData == null)
                //         paramData.MemberData = SerializableGuid.Empty;
                //     DrawSerializedGuid(dataRow, i, paramData);
                // }
                // // else if (CheckValue(_paramsProperty.Children[i], out var dataType))
                // // {
                // //     var editorDataProperty = _paramsProperty.Children[i];
                // //     if (dataType.ImplementsOrInherits(typeof(ValueReferenceEvent)))
                // //         editorDataProperty.Draw(null);
                // //     else
                // //         editorDataProperty.Draw(new GUIContent(paramData.Name));
                // // }
                // // else
                // // {
                // //     GetChildProperty("MemberData", _paramsProperty.Children[i])
                // //         .Draw(new GUIContent(paramData.Name));
                // // }
                //
                //
                // _paramsValueEntry.SmartValue[i] = paramData;
                dataRow = dataRow.AddY(EditorGUIUtility.singleLineHeight);
            }
        }

        

        private void CleanParams()
        {
            var wantedFields = ConditionDataHelper.GetParamDataFields(SmartValue.ConditionType.Type);
            var currentData = SmartValue.Params;
        
            List<ParamData> result = new List<ParamData>();
        
            foreach (var info in wantedFields)
            {
                var data = currentData.FirstOrDefault(x => ParamDataMatchesMember(x, info));
                if (data != null)
                {
                    result.Add(data);
                    continue;
                }
        
                var paramData = ParamData.CreateWithValue(info, info.GetReturnType().GetDefault());
                //paramData = EditorParamDataHelper.Convert(paramData);
                result.Add(paramData);
            }
        
            SmartValue.Params = result.ToArray();
            Apply();
        }
        
        private bool ParamDataMatchesMember(ParamData data, MemberInfo info)
        {
            if (data.MemberType != info.MemberType) return false;
            if (data.Name != info.Name) return false;
            if (data.Flags != info.GetFlags()) return false;
            
            if (data.Type != info.GetReturnType()) return false;
            
            return true;
        }
        //
        // private bool CheckValue(InspectorProperty prop, out Type dataType)
        // {
        //     var t = typeof(EditorParamData<>);
        //     var propValueType = prop.ValueEntry.WeakSmartValue.GetType();
        //     if (!propValueType.IsGenericType)
        //     {
        //         dataType = null;
        //         return false;
        //     }
        //
        //     dataType = propValueType.GetGenericArguments().First();
        //     return propValueType.GetGenericTypeDefinition() == t;
        //
        // }
        //
        private void DrawSerializedGuid(Rect r, int paramIndex, ParamData paramData)
        {
            _paramsValueEntry.HostInfo.CreateArrayElement(paramIndex)
                .TryGetChild(nameof(ParamData.MemberData), out var valueProperty);

            var propertyView = new DrawablePropertyView(valueProperty);
            
            var memberInfo = SmartValue.GetMemberInfo(paramData);
            if (!ValueReferenceHelper.TryGetValueReference(memberInfo, out ValueReferenceInfo info))
            {
                propertyView.Draw(r);
                //valueProperty?.Draw(new GUIContent(paramData.Name));
            }
            else
            {
                propertyView.Draw(r);
                // Label with dropdown
                //valueProperty?.Draw(new GUIContent(paramData.Name));
        
                var takenWidth = CustomGUIStyles.Label.CalcSize(paramData.Name).x;
        
                var typeRect = r;
                
#if ODIN_INSPECTOR
                var labelWidth = Sirenix.Utilities.Editor.GUIHelper.BetterLabelWidth;
#else
                var labelWidth = EditorGUIUtility.labelWidth;
#endif
                typeRect.width = labelWidth;
                typeRect = RectExtensions.AlignRight(typeRect, typeRect.width - takenWidth);
                GUI.Label(typeRect, info.ReferenceType.Name, CustomGUIStyles.MiniLabelRight);

                // Default Button
                if (_referenceResolver == null)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    GUI.Label(r.AlignRight(170), "Resolver missing");
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    var defaultButtonRect = r.AlignRight(100);
                    {
                        SerializableGuid selectedGuid = paramData.MemberData as SerializableGuid;
                        SerializableGuid defaultGuid =
                            _referenceResolver.GetDefault(new SerializableType(info.ReferenceType),
                                memberInfo as FieldInfo);
                        bool isDefault = selectedGuid != null && selectedGuid.Equals(defaultGuid);
                        EditorGUI.BeginDisabledGroup(isDefault);
                        if (GUI.Button(defaultButtonRect, "Mark Default"))
                            _referenceResolver.RegisterDefault(memberInfo as FieldInfo, selectedGuid);
                        EditorGUI.EndDisabledGroup();
                    }
                }
            }
        }
    }
}
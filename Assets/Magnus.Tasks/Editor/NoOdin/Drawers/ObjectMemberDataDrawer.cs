using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(ObjectMemberData), true)]
    public class ObjectMemberDataDrawer : BasePropertyDrawer<ObjectMemberData>
    {
        private IEditorDrawable _memberDataDrawer;
        private TypedHostInfoWrapper<IMemberDataSource> _memberData;

        protected override void OnUpdateActiveData()
        {
            if (SmartValue == null)
            {
                _memberDataDrawer = null;
                return;
            }

            HostInfo.TryGetChild<IMemberDataSource>(nameof(ObjectMemberData.MemberData), out _memberData);
            _memberData.HostInfo.TryGetChild("Data", out var grandChildhostInfo); // TODO: how to find
            _memberDataDrawer = GetChildDrawer(grandChildhostInfo);
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            if (data.Parent == null || data.Parent.Parent == null || !data.Parent.Parent.GetReturnType().InheritsFrom(typeof(BaseObjectDataContainer)))
            {
                CallInnerDrawer(position, label);
                return;
            }
            
            var dataContainer = data.Parent.Parent.GetSmartValue<BaseObjectDataContainer>();
            
            position.SplitX(18, out var switchButtonRect, out var memberRect);
            if (CustomEditorGUI.IconButton(switchButtonRect, UnityIcon.AssetIcon("Fa_Exchange")))
            {
                var refData = dataContainer.FindReference(SmartValue.MemberInfo, createIfNotFound: true);
                refData.IsEnabled = !refData.IsEnabled;
                data.Parent.Parent.SetValue(dataContainer);
            }

            var referenceData = dataContainer.FindReference(SmartValue.MemberInfo);

            if (referenceData != null && referenceData.IsEnabled)
            {
                EditorGUI.LabelField(memberRect, "This is a reference LOL"); 
            }
            else
            {
                if (_memberDataDrawer != null)
                    _memberDataDrawer.Draw(memberRect, label);
            }
        }
    }
}
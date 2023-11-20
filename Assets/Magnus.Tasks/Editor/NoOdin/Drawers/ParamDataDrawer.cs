using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(ParamData), true)]
    public class ParamDataDrawer : BasePropertyDrawer<ParamData>
    {
        private IEditorDrawable _memberDataDrawer;

        protected override void OnUpdateActiveData()
        {
            if (SmartValue == null)
            {
                _memberDataDrawer = null;
                return;
            }

            _memberDataDrawer = GetChildDrawer(nameof(ParamData.MemberData));
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            if (_memberDataDrawer != null)
                _memberDataDrawer.Draw(position, label);
        }
    }
}
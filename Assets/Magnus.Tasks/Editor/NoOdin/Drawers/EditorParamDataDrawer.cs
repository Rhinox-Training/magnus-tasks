using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed.Reflection;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Editor
{
    [CustomPropertyDrawer(typeof(EditorParamData<>), true)]
    public class EditorParamDataDrawer<T> : BasePropertyDrawer<EditorParamData<T>>
    {
        private DrawableAsUnityProperty _drawableMember;
        // protected override void OnInitialized()
        // {
        //     var value = Property.ValueEntry.WeakSmartValue as EditorParamData<T>;
        //     var property = Property.FindChild(x => x.Name == nameof(ParamData.MemberData), false);
        //     property.Label = new GUIContent(value.Name);
        // }

        protected override void OnUpdateActiveData()
        {
            var drawerType = PropertyDrawerHelper.GetDrawerTypeFor(typeof(T));
            if (drawerType != null && drawerType.HasInterfaceType<IHostInfoDrawer>())
            {
                _drawableMember = new DrawableAsUnityProperty(HostInfo, drawerType);
            }
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            _drawableMember.Draw(position, label);

            // Propagate changes to MemberData TODO: do this better
            //if (EditorGUI.EndChangeCheck() && value != null)
            {
                //value.MemberData = value.SmartValue;
            }
        }
    }
}
using System;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(NotConvertedToDataLayerAttribute))]
    public class NotConvertedToDataLayerAttributeDrawer : BasePropertyDrawer<NotConvertedToDataLayerAttribute>
    {
        private bool _inDataLayerContext;

        protected override void OnUpdateActiveData()
        {
            base.OnUpdateActiveData();
            _inDataLayerContext = IsInDataLayerContext();
        }

        private bool IsInDataLayerContext()
        {
            return this.FindAttribute<DataLayerAttribute>(out var attr);
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            if (_inDataLayerContext)
                return;
            
            CallInnerDrawer(position, label);
        }
    }
}
using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public class NotConvertedToDataLayerAttributeDrawer : OdinAttributeDrawer<NotConvertedToDataLayerAttribute>
    {
        private bool _inDataLayerContext;
        
        protected override void Initialize()
        {
            base.Initialize();
            _inDataLayerContext = IsInDataLayerContext();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (_inDataLayerContext)
                return;
            
            CallNextDrawer(label);
        }
        
        private bool IsInDataLayerContext()
        {
            InspectorProperty p = Property;
            DataLayerAttribute attr = null;
            Type searchType = p.Info.TypeOfValue;
            
            while (p != null)
            {
                attr = searchType.GetCustomAttribute<DataLayerAttribute>();
                if (attr != null)
                    break;
                
                if (p.Parent == null)
                    break;
                searchType = p.ParentType;
                p = p.Parent;
            }
            
            return attr != null;
        }
    }
}
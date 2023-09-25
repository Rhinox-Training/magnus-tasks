using System;
using System.Linq;
using Rhinox.GUIUtils.Editor;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.Utilities;
#endif

namespace Rhinox.Magnus.Tasks.Editor
{
    public static class DrawerReferenceResolverFinder
    {
        public static IReferenceResolver FindReferenceResolver(this BasePropertyDrawer drawer)
        {
            if (!drawer.FindAttribute<SerializedGuidProcessorAttribute>(out GenericHostInfo hostInfo, out var processorAttribute))
                return null;

            var propertyHelper = MemberHelper.Create<IReferenceResolver>(hostInfo.GetValue(), processorAttribute.MemberName);
            var referenceResolver = propertyHelper.ForceGetValue();
            return referenceResolver;
        }
        
#if ODIN_INSPECTOR
        public static IReferenceResolver FindReferenceResolver(this InspectorProperty searchProperty)
        {
            SerializedGuidProcessorAttribute processorAttribute = null;
            Type searchType = searchProperty.Info.TypeOfValue;
            
            while (searchProperty != null)
            {
                processorAttribute = searchType.GetCustomAttribute<SerializedGuidProcessorAttribute>();
                if (processorAttribute != null)
                    break;
                
                if (searchProperty.Parent == null)
                    break;
                searchType = searchProperty.ParentType;
                searchProperty = searchProperty.Parent;
            }
            
            if (processorAttribute == null)
                return null;
            
            var propertyHelper = new PropertyMemberHelper<IReferenceResolver>(searchProperty, processorAttribute.MemberName);
            var referenceResolver = propertyHelper.GetValue();
            return referenceResolver;
        }
#endif
    }
}
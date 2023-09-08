using System;
using System.Linq;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace Rhinox.VOLT.Editor
{
    public static class DrawerReferenceResolverFinder
    {
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
    }
}
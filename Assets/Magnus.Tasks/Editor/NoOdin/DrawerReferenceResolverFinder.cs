using System;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.VOLT.Data;

namespace Rhinox.VOLT.Editor
{
    public static class DrawerReferenceResolverFinder
    {
        public static IReferenceResolver FindReferenceResolver(this BasePropertyDrawer drawer)
        {
            if (!drawer.FindAttribute<SerializedGuidProcessorAttribute>(out GenericHostInfo hostInfo, out var processorAttribute))
                return null;

            var propertyHelper = MemberHelper.Create<IReferenceResolver>(hostInfo.GetValue(), processorAttribute.MemberName);
            var referenceResolver = propertyHelper.GetSmartValue();
            return referenceResolver;
        }
    }
}
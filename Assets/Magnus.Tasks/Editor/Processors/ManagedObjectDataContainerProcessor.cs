using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    public static class ManagedObjectDataContainerEditorExtensions
    {
        public static string GetTypeName(this ManagedObjectDataContainer container)
        {
            return container.Type.Type.GetCSharpName(includeNameSpace: false);
        }
    }
    
    public class ManagedObjectDataContainerProcessor : BaseObjectDataContainerProcessor<ManagedObjectDataContainer>
    {
        public override void ProcessMember(MemberInfo memberInfo, ref List<Attribute> attributes)
        {
            base.ProcessMember(memberInfo, ref attributes);
            switch (memberInfo.Name)
            {
                case nameof(ManagedObjectDataContainer.Type):
                    attributes.Add(new HideInInspector());
                    break;
                case nameof(ManagedObjectDataContainer.ManagedData):
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineEditorAttribute());
                    attributes.Add(new TitleAttribute($"${nameof(ManagedObjectDataContainerEditorExtensions.GetTypeName)}"));
                    break;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    public abstract class BaseObjectDataContainerProcessor<T> : BaseAttributeProcessor<T> where T : BaseObjectDataContainer
    {
        public override void ProcessMember(MemberInfo memberInfo, ref List<Attribute> attributes)
        {
            switch (memberInfo.Name)
            {
                case nameof(BaseObjectDataContainer.ReferenceDatas):
                    attributes.Add(new HideInInspector());
                    break;
            }
        }
    }
}
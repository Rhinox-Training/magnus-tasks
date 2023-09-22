using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class ValueReferenceLookupResolver : BaseMemberPropertyResolver<ValueReferenceLookup>, IDisposable
    {
        private List<OdinPropertyProcessor> processors;

        public virtual void Dispose()
        {
            if (this.processors == null)
                return;
            for (int index = 0; index < this.processors.Count; ++index)
            {
                if (this.processors[index] is IDisposable processor)
                    processor.Dispose();
            }
        }

        protected override InspectorPropertyInfo[] GetPropertyInfos()
        {
            if (this.processors == null)
                this.processors = OdinPropertyProcessorLocator.GetMemberProcessors(this.Property);
            bool includeSpeciallySerializedMembers = true;// !this.Property.ValueEntry.SerializationBackend.IsUnity;
            List<InspectorPropertyInfo> memberProperties =
                InspectorPropertyInfoUtility.CreateMemberProperties(this.Property, typeof(ValueReferenceLookup),
                    includeSpeciallySerializedMembers);
            for (int index = 0; index < this.processors.Count; ++index)
            {
                ProcessedMemberPropertyResolverExtensions.ProcessingOwnerType = typeof(ValueReferenceLookup);
                this.processors[index].ProcessMemberProperties(memberProperties);
            }

            return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(this.Property,
                typeof(ValueReferenceLookup), memberProperties, includeSpeciallySerializedMembers);
        }
    }
}
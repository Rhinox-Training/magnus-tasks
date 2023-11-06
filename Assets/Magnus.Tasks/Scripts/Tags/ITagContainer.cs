using System.Collections.Generic;

namespace Rhinox.Magnus.Tasks
{
    public interface ITagContainer
    {
        bool HasTag(string tag, bool tagMustBeGlobal = true);
        bool HasAnyTag(ICollection<string> tags, bool tagMustBeGlobal = true);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Vortex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.VOLT.Training
{
    [HideReferenceObjectPicker, HideLabel]
    [JsonConverter(typeof(TagContainerConverter))]
    [Serializable]
    public class TagContainer
    {
        [ValueDropdown("GetAllTags"), ListDrawerSettings(Expanded = true, DraggableItems = false)]
        [OnValueChanged(nameof(OnTagsChanged))]
        public List<string> Tags;

        public event Action Changed;
        
        public TagContainer()
        {
            Tags = new List<string>();
        }
        
        public TagContainer(ICollection<string> tags) : this()
        {
            if (tags != null)
                Tags.AddRange(tags);
        }
        
        public bool HasTag(string tag, bool tagMustBeGlobal = true)
        {
            if (tagMustBeGlobal && !DataLayerContainsTag(tag)) return false;
            return Tags.Contains(tag);
        }
        
        private static bool DataLayerContainsTag(string tag)
        {
            return DataLayer.GetTable<TagData>().GetAllData().Any(x => x.Name.Equals(tag, StringComparison.InvariantCulture));
        }
        
        public bool HasAnyTag(ICollection<string> tags, bool tagMustBeGlobal = true)
        {
            var allGlobalTags = DataLayer.GetTable<TagData>().GetAllData();
            foreach (var tag in tags)
            {
                if (tagMustBeGlobal && !allGlobalTags.Any(x => x.Name.Equals(tag, StringComparison.InvariantCulture)))
                    continue;
                
                if (Tags.Contains(tag))
                    return true;
            }
            return false;
        }

        public void Add(string tag)
        {
            if (Tags.AddUnique(tag))
                Changed?.Invoke();
        }

        public void AddRange(ICollection<string> tags)
        {
            if (Tags == null)
                Tags = new List<string>();

            bool changed = false;
            foreach (var item in tags)
                changed |= Tags.AddUnique(item);
            
            if (changed)
                Changed?.Invoke();
        }
        
        public void RemoveDoubles()
        {
            if (Tags == null)
            {
                Tags = new List<string>();
                return;
            }

            var count = Tags.Count;
            
            Tags = Tags.Distinct().ToList();
            
            if (count != Tags.Count)
                Changed?.Invoke();
        }
        
#if UNITY_EDITOR
        private ICollection<ValueDropdownItem> GetAllTags()
        {
            var table = DataLayer.GetTable<TagData>();
            if (table == null)
            {
                PLog.Warn<MagnusLogger>("No Tag table set up");
                return Array.Empty<ValueDropdownItem>();
            }
            return table.GetAllData()
                .Select(x => new ValueDropdownItem(x.Name, x.Name))
                .ToArray();
            
        }
#endif

        public void ChangeTag(string oldTag, string newTag)
        {
            if (string.IsNullOrWhiteSpace(oldTag) || Tags == null)
                return;
            
            if (string.IsNullOrWhiteSpace(newTag))
            {
                Tags.Remove(oldTag);
                return;
            }

            var i = Tags.IndexOf(oldTag);
            
            if (i < 0) return;
            
            Tags[i] = newTag;
            
            Changed?.Invoke();
        }
        
        public static implicit operator List<string>(TagContainer container)
        {
            return container.Tags;
        }
        
        public static implicit operator TagContainer(List<string> tags)
        {
            return new TagContainer(tags);
        }

        private void OnTagsChanged()
        {
            Changed?.Invoke();
        }
    }

    public static class TagContainerExtensions
    {
        public static bool IsNullOrEmpty(this TagContainer container)
        {
            if (container == null || container.Tags.IsNullOrEmpty()) return true;
            return false;
        }
    }
}
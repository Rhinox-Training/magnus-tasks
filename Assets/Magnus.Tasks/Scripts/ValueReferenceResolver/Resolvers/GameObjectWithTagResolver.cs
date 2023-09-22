using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.domain")]
    public class GameObjectWithTagResolver : BaseValueResolver<GameObject>
    {
        public override string SimpleName => "Object with Tag";

        [HideLabel, TagSelector] public string Tag;

        private GameObject _cache;

        public override bool TryResolve(ref GameObject value)
        {
            if (_cache == null)
                _cache = GameObject.FindWithTag(Tag);
            value = _cache;
            return true;
        }

        public void Refresh()
        {
            _cache = null;
        }

        public override bool Equals(IValueResolver other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;
            return Equals((GameObjectWithTagResolver) other);
        }

        protected bool Equals(GameObjectWithTagResolver other)
        {
            return Equals(Tag, other.Tag);
        }
    }
}
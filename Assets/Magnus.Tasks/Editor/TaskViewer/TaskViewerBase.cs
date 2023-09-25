using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Editor.Helpers;
using Sirenix.OdinInspector;
#if ODIN_VALIDATOR
using Sirenix.OdinInspector.Editor.Validation;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class TaskViewerBase : PagerPage
    {
        private EditorWrapper _targetWrapper;

        private EditorWrapper[] _components;

        public TaskViewerBase(SlidePageNavigationHelper<object> pager) : base(pager)
        {
        }

        public void SetTarget(object target)
        {
            _targetWrapper = new EditorWrapper(target);
            _components = null;
        }

        protected override void OnDraw()
        {
            FetchComponents();

            _targetWrapper.Draw();

            for (var i = 0; i < _components.Length; i++)
                _components[i].Draw();
        }

        private void FetchComponents()
        {
            if (_components != null) return;
            var allComponents = new List<Component>();

            var comp = _targetWrapper.Target as BaseStep;

            if (comp == null)
            {
                _components = Array.Empty<EditorWrapper>();
                return;
            }

            var go = comp.gameObject;

            foreach (var l in TaskViewerSettings.All)
            {
                if (typeof(BaseStep).IsAssignableFrom(l.Type))
                    continue;

                var comps = go.GetComponentsInChildren(l.Type);
                allComponents.AddRange(comps);
            }

            _components = allComponents.Distinct().Select(x => new EditorWrapper(x)).ToArray();
        }
    }
}
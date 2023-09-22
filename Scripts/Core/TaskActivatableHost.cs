using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Magnus;
using Rhinox.Utilities;
using Rhinox.Utilities.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    /// <summary>
    /// Component that activates when one of the child activatables activates. Usefull for providing i.e. a background for UI, where the activatables are possible elements
    /// </summary>
    [ExecuteAfter(typeof(TaskActivatable))]
    public class TaskActivatableHost : MonoBehaviour
    {
        private TaskActivatable[] _children;

        [ShowReadOnlyInPlayMode] private List<TaskActivatable> _activeActivatables;

        private bool _isActive;

        private void Awake()
        {
            // Required due to datatasks only spawning in later
            SceneReadyHandler.YieldToggleControl(this);

            _activeActivatables = new List<TaskActivatable>();
        }

        void Start()
        {
            _children = GetComponentsInChildren<TaskActivatable>(true);

            foreach (var child in _children)
            {
                child.StateChanged += OnStateChanged;
                if (child.IsActive)
                    _activeActivatables.Add(child);
            }

            CheckActivationState();
        }

        private void OnDestroy()
        {
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    if (child)
                        child.StateChanged -= OnStateChanged;
                }
            }

            SceneReadyHandler.RevertToggleControl(this);
        }

        private void OnStateChanged(TaskActivatable activatable, bool state)
        {
            if (state)
                _activeActivatables.Add(activatable);
            else
                _activeActivatables.Remove(activatable);

            CheckActivationState();
        }

        private void CheckActivationState()
        {
            var isActive = _activeActivatables.Any();
            SetState(isActive);
        }

        private void SetState(bool state)
        {
            _isActive = state;
            gameObject.SetActive(state);
        }
    }
}
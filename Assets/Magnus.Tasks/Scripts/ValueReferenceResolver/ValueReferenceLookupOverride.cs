using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.Vortex;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Rhinox.GUIUtils.Editor;
using UnityEditor;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif
#endif

namespace Rhinox.Magnus.Tasks
{
    [HideLabel]
    [HideReferenceObjectPicker]
    public class ValueReferenceLookupOverride : IReferenceResolver
    {
        public int TaskId => _taskId;
        [ShowReadOnlyInPlayMode] public IReferenceResolver OverridesResolver { get; set; }

        [ListDrawerSettings(HideAddButton = true, OnTitleBarGUI = nameof(DrawOverrideButtons)), DisableInPlayMode]
        public List<ReferenceKeyOverride> Overrides;

        private ValueReferenceLookup _lookup;
        [SerializeField, HideInInspector] private int _taskId;

        public ValueReferenceLookupOverride(int taskId)
        {
            _taskId = taskId;
            Initialize();
        }

        private void Initialize()
        {
            if (_lookup != null) return;

            if (!Application.isPlaying)
                DataLayer.PushEndPointFromSceneOrDefault();

            // DataLayer takes into account applied overrides
            var table = DataLayer.GetTable<TaskObject>();

            if (table == null)
            {
                PLog.Error<VortexLogger>($"Tried to setup ValueReferenceLookup but TaskObject table was not found.");
                return;
            }

            var task = table.GetData(TaskId);

            if (task == null)
            {
                PLog.Error<VortexLogger>(
                    $"Tried to setup ValueReferenceLookup but task with id '{TaskId}' was not found.");
                return;
            }

            _lookup = task.Lookup;

            if (_lookup == null)
            {
                PLog.Error<VortexLogger>($"Tried to setup ValueReferenceLookup but _lookup was null.");
                return;
            }

            if (Overrides == null)
                Overrides = new List<ReferenceKeyOverride>();

            if (!Application.isPlaying)
                DataLayer.PopEndPoint();
        }

        public bool RegisterDefault(FieldInfo field, SerializableGuid newDefaultGuid, bool overwriteIfExists = true)
            => _lookup.RegisterDefault(field, newDefaultGuid, overwriteIfExists);

        public SerializableGuid Register(string defaultName, IValueResolver resolver, bool overwriteOnNotNull = false)
            => _lookup.Register(defaultName, resolver, overwriteOnNotNull);


        public bool Resolve(SerializableGuid key, out object value)
        {
            value = default;
            if (key == null) return false;

            var overrideData = Overrides?.FirstOrDefault(x => key.Equals(x.Guid));
            if (overrideData != null)
                return overrideData.GetValue(OverridesResolver, out value);

            if (!_lookup.Resolve(key, out object resolvedValue))
                return false;
            value = resolvedValue;
            return true;
        }

        public bool Resolve<T>(SerializableGuid key, out T value)
        {
            value = default;
            if (key == null) return default;

            var overrideData = Overrides?.FirstOrDefault(x => key.Equals(x.Guid));
            if (overrideData != null)
                return overrideData.GetValue(OverridesResolver, out value);

            return _lookup.Resolve(key, out value);
        }

        public ICollection<ReferenceKey> GetKeysFor(Type t)
            => _lookup.GetKeysFor(t);

        public IReadOnlyCollection<ReferenceKey> GetKeys()
            => _lookup.GetKeys();

        public SerializableGuid GetDefault(SerializableType keyType, FieldInfo field)
            => _lookup.GetDefault(keyType, field);

        public ReferenceKey FindKey(SerializableGuid guid)
            => _lookup.FindKey(guid);

        public IValueResolver FindResolverByName(string key)
        {
            return _lookup.FindResolverByName(key);
        }

        public IValueResolver FindResolverByID(SerializableGuid id)
        {
            return _lookup.FindResolverByID(id);
        }

        public IEnumerable<IValueResolver> FindResolversByType(Type resolveTargetType)
        {
            return _lookup.FindResolversByType(resolveTargetType);
        }

        public void Refresh()
        {
            _lookup = null;
            Initialize();

            // Clean up invalid overrides
            for (var i = Overrides.Count - 1; i >= 0; i--)
            {
                var o = Overrides[i];
                var key = _lookup.Keys.FirstOrDefault(x => x.Guid.Equals(o.Guid));

                if (key == null) Overrides.RemoveAt(i);
                else Overrides[i].Update(key);
            }
        }

        private void DrawOverrideButtons()
        {
#if UNITY_EDITOR
            if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Redo"), 22))
                EditorApplication.delayCall += Refresh;
#endif
        }

#if UNITY_EDITOR
        [Button, HideInPlayMode]
        private void AddOverrideFor()
        {
            if (_lookup == null) Initialize();

            var newKeys = _lookup.Keys
                .Where(x => Overrides.All(y => y.Guid != x.Guid))
                .ToArray();
#if ODIN_INSPECTOR
        var selector = new GenericSelector<ReferenceKey>("Create an override for:", false, x => x.Name, newKeys);
        selector.SelectionConfirmed += x =>
        {
            foreach (var key in x)
                Overrides.Add(new ReferenceKeyOverride(key));
        };
        selector.ShowInPopup(GUIHelper.GetCurrentLayoutRect());
#else
            var menu = new GenericMenu();
            foreach (var key in newKeys)
            {
                menu.AddItem(new GUIContent(key.Name), false, () => { Overrides.Add(new ReferenceKeyOverride(key)); });
            }

            menu.ShowAsContext();
#endif

        }
#endif
    }
}
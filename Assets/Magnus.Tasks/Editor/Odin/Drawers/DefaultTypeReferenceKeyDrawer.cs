using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class DefaultTypeReferenceKeyDrawer : SimpleOdinValueDrawer<DefaultTypeReferenceKey>
    {
        private IReferenceResolver _resolver;

        protected override void Initialize()
        {
            base.Initialize();
            TryInitializeResolver();
        }

        private void TryInitializeResolver()
        {
            if (_resolver == null)
                _resolver = Property.FindReferenceResolver();
        }

        protected override void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<DefaultTypeReferenceKey> valueEntry)
        {
            GetLabelFieldRects(out Rect labelRect, out Rect fieldRect);
            EditorGUI.LabelField(labelRect, "GUID");
            if (GUI.Button(fieldRect, valueEntry.SmartValue.KeyGuid.ToString()))
            {
                DrawDropdown(fieldRect, nameof(SerializableGuid),
                    GetGuidOptions(valueEntry.SmartValue.FieldData.ReferenceKeyType), (x) =>
                    {
                        GetChildProperty<SerializableGuid>(nameof(DefaultTypeReferenceKey.KeyGuid), out var guidEntry);
                        guidEntry.SmartValue = x;
                    });
            }

            GetChildProperty(nameof(DefaultTypeReferenceKey.FieldData)).Draw();
        }

        private ICollection<SerializableGuid> GetGuidOptions(Type referenceType)
        {
            TryInitializeResolver();
            if (_resolver == null || referenceType == null)
                return Array.Empty<SerializableGuid>();
            return _resolver.GetKeysFor(referenceType).Select(x => x.Guid).ToArray();
        }
    }
}
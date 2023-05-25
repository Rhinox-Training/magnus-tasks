using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEngine;

[Serializable]
public struct ValueReferenceEvent : ISerializationCallbackReceiver, IUseReferenceGuid
{
    [HideDuplicateReferenceBox]
    [HideReferenceObjectPicker, ListDrawerSettings(OnTitleBarGUI = "DrawInvokeButton", Expanded = true, CustomAddFunction = nameof(AddEvent))]
    [LabelText("@GetMemberName($property)")]
    public List<ValueReferenceEventEntry> Events;

    private IReferenceResolver _referenceResolver;

    public ValueReferenceEvent(params ValueReferenceEventEntry[] entries)
    {
        Events = new List<ValueReferenceEventEntry>(entries);
        _referenceResolver = null;
    }
    
    public void Initialize(IReferenceResolver valueResolver)
    {
        if (valueResolver == null)
            return;
        
        _referenceResolver = valueResolver;
        foreach (var entry in Events)
            entry.Initialize(_referenceResolver);
    }

    private void AddEvent()
    {
        var valueReferenceEvent = new ValueReferenceEventEntry();
        if (_referenceResolver != null)
            valueReferenceEvent.Initialize(_referenceResolver);
        Events.Add(valueReferenceEvent);
    }

    public void Invoke()
    {
        if (this.Events == null) return;
        for (int i = 0; i < this.Events.Count; i++)
        {
            this.Events[i].Invoke();
        }
    }
    
    public void AddListener(ValueReferenceEventEntry e)
    {
        if (Events == null)
            Events = new List<ValueReferenceEventEntry>();
        
        Events.Add(e);
    }
    
    public void RemoveListener(ValueReferenceEventEntry e)
    {
        Events?.Remove(e);
    }
    
    public static ValueReferenceEvent operator+ (ValueReferenceEvent e, ValueReferenceEventEntry a)
    {
        e.AddListener(a);
        return e;
    }

    public static ValueReferenceEvent operator- (ValueReferenceEvent e, ValueReferenceEventEntry a)
    {
        e.RemoveListener(a);
        return e;
    }

#if UNITY_EDITOR
    private void DrawInvokeButton()
    {
        if (GUILayout.Button("Invoke"))
        {
            this.Invoke();
        }
    }

    #if ODIN_INSPECTOR
    private string GetMemberName(InspectorProperty property)
    {
        var parent = property.Parent;
        // Edit the parent's label to reflect this; example: EditorParamDataDrawer
        return parent.Label?.text ?? parent.Name;
    }
    #endif
#endif

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        if (Events == null)
            Events = new List<ValueReferenceEventEntry>();
    }

    public bool UsesGuid(SerializableGuid guid)
    {
        if (Events == null) return false;
        
        return Events.Any(x => x != null && x.UsesGuid(guid));
    }

    public void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
    {
        if (Events == null) return;

        foreach (var e in Events.OfType<IUseReferenceGuid>())
            e.ReplaceGuid(guid, replacement);
    }
}

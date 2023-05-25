using System;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;

[Serializable]
public class ValueReferenceEventEntry : IUseReferenceGuid
{
    [ValueReference(typeof(object), "Target")]
    [DoNotDrawAsReference, HideReferenceObjectPicker]
    public SerializableGuid Target;
    
    [HideLabel]
    public ValueReferenceEventAction Action;

    private IReferenceResolver _resolver;

    public void Initialize(IReferenceResolver resolver)
    {
        _resolver = resolver;
        Action.TryResolveValues(_resolver, Target);
    }
    
    public void Invoke()
    {
        Action?.Invoke(_resolver, Target);
    }

    public bool UsesGuid(SerializableGuid guid)
    {
        if (Target.Equals(guid)) return true;

        return Action is IUseReferenceGuid guidUser && guidUser.UsesGuid(guid);
    }

    public void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
    {
        if (Target.Equals(guid))
            Target = replacement;
        
        if (Action is IUseReferenceGuid guidUser)
            guidUser.ReplaceGuid(guid, replacement);
    }
}

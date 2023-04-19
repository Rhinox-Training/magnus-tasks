using System;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Object = UnityEngine.Object;

[Serializable]
public class SceneGuidValueResolver : BaseValueResolver<Object>
{
    private string TargetType => _resolvedIdentifier == null ? "Object" : _resolvedIdentifier.TargetType?.Name;
    public override string SimpleName => "Scene Object";
    public override string ComplexName => $"Scene Object of type [{TargetType}]";
    
    public SerializableGuid GuidAssetIdentifier;

    private GuidIdentifier _resolvedIdentifier;
    
    public override bool TryResolve(ref Object value)
    {
        var guidAsset = GuidAsset.Find(GuidAssetIdentifier);
        _resolvedIdentifier = GuidIdentifier.GetFor(guidAsset);

        if (_resolvedIdentifier.TargetComponent == null)
            return _resolvedIdentifier.gameObject;
        
        return _resolvedIdentifier.TargetComponent;
    }
    
    public override bool Equals(IValueResolver other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != this.GetType()) return false;
        return Equals((SceneGuidValueResolver) other);
    }

    protected bool Equals(SceneGuidValueResolver other)
    {
        if (GuidAssetIdentifier.IsNullOrEmpty())
            return other.GuidAssetIdentifier.IsNullOrEmpty();
        return GuidAssetIdentifier.Equals(other.GuidAssetIdentifier);
    }
}
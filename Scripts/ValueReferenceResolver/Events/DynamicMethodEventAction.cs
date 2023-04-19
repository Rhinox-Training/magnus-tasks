using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using UnityEngine;

public class DataViewer<T> : ArgumentDataContainer
{
    public Type TargetType => typeof(T);
    
    [OnValueChanged(nameof(ValueChanged))]
    public T SmartData;

    public DataViewer(DynamicMethodArgument owner) : base(owner)
    {
    }

    public override bool TryGetData(IReadOnlyReferenceResolver resolver, ref object data)
    {
        data = SmartData;
        return true;
    }

    public override object GetRawData()
    {
        return SmartData;
    }

    public override bool UsesGuid(SerializableGuid guid)
    {
        if (guid.IsNullOrEmpty()) return false;
        return guid.Equals(SmartData);
    }

    public override void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
    {
        if (typeof(T) != typeof(SerializableGuid) || guid.IsNullOrEmpty()) return;
        if (guid.Equals(SmartData))
        {
            SmartData = (T) Convert.ChangeType(guid, typeof(T));
            ValueChanged();
        }
    }

    private void ValueChanged()
    {
        _owner.Data = SmartData;
    }
    
    public override void SetData(object data)
    {
        if (data is T smartData)
        {
            SmartData = smartData;
            base.SetData(data);
        }
        else
        {
            SmartData = default(T);
            base.SetData(default(T));
        }
    }
}

public class SerializedViewer : ArgumentDataContainer
{
    [OnValueChanged(nameof(ValueChanged))]
    [ValueReference(nameof(TargetType), "GUID")]
    public SerializableGuid GUID;
    
    [HideInInspector]
    public Type TargetType;

    public SerializedViewer(DynamicMethodArgument owner)
        : base(owner)
    {
        GUID = SerializableGuid.Empty;
        TargetType = owner.Type;
    }

    public override bool TryGetData(IReadOnlyReferenceResolver resolver, ref object data)
    {
        if (resolver == null || !resolver.Resolve(GUID, out object resolvedValue))
            return false;
        
        data = resolvedValue;
        return true;
    }
    
    public override object GetRawData()
    {
        return GUID;
    }

    public override bool UsesGuid(SerializableGuid guid)
    {
        if (guid.IsNullOrEmpty()) return false;
        return guid.Equals(GUID);
    }

    public override void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
    {
        if (guid.IsNullOrEmpty()) return;
        if (guid.Equals(GUID))
            GUID = replacement;
    }

    private void ValueChanged()
    {
        if (_owner == null)
            return;
        _owner.Data = GUID;
    }

    public override void SetData(object data)
    {
        if (data is SerializableGuid guid)
        {
            GUID = guid;
            base.SetData(data);
        }
        else
        {
            GUID = SerializableGuid.Empty;
            base.SetData(SerializableGuid.Empty);
        }
    }
}

[HideReferenceObjectPicker]
public abstract class ArgumentDataContainer : IUseReferenceGuid
{
    protected DynamicMethodArgument _owner;

    protected ArgumentDataContainer(DynamicMethodArgument owner)
    {
        _owner = owner;
    }

    public virtual void SetData(object data)
    {
    }

    public abstract bool TryGetData(IReadOnlyReferenceResolver resolver, ref object data);
    
    public abstract object GetRawData();

    public abstract bool UsesGuid(SerializableGuid guid);
    public abstract void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement);
}

[HideReferenceObjectPicker, Serializable]
public class DynamicMethodArgument : ISerializationCallbackReceiver, IUseReferenceGuid
{
    [HideInInspector]
    public object Data;

    [NonSerialized, ShowInInspector, HideLabel]
    public ArgumentDataContainer DataContainer;

    [SerializeField, HideInInspector]
    private SerializableType _containerType;

    public SerializableType Type;

    public DynamicMethodArgument(SerializableType type)
    {
        Type = type;
        Data = type.Type.GetDefault();

        DataContainer = CreateDataContainer(type.Type);
    }

    private ArgumentDataContainer CreateDataContainer(Type t)
    {
        if (t == null)
            throw new ArgumentNullException(nameof(t));

        // If we have a container type serialized, we are probably deserializing and can recreate our container
        if (_containerType != null)
            return Activator.CreateInstance(_containerType, new [] { this }) as ArgumentDataContainer;
        
        // If we are starting fresh, we want to create a GUID based approach for object types by default
        if ( t.InheritsFrom(typeof(UnityEngine.Object)))
            return new SerializedViewer(this)
            {
                TargetType = t
            };
        
        // Or a constant value approach for other values
        var viewerType = typeof(DataViewer<>).MakeGenericType(t);
        return Activator.CreateInstance(viewerType, new [] { this }) as ArgumentDataContainer;
    }

    public bool GetData(IReferenceResolver resolver, ref object data)
    {
        return DataContainer.TryGetData(resolver, ref data);
    }

    public bool UsesGuid(SerializableGuid guid)
        => DataContainer.UsesGuid(guid);
    
    public void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
        => DataContainer.ReplaceGuid(guid, replacement);
    
    public void Switch()
    {
        if (DataContainer is SerializedViewer)
        {
            var dataViewerType = typeof(DataViewer<>).MakeGenericType(Type);
            var instance = Activator.CreateInstance(dataViewerType, new[] {this}) as ArgumentDataContainer;
            DataContainer = instance;
        }
        else
        {
            DataContainer = new SerializedViewer(this)
            {
                TargetType = Type
            };
        }
    }

    public void OnBeforeSerialize()
    {
        Data = DataContainer.GetRawData();
        _containerType = new SerializableType(DataContainer?.GetType());
    }

    public void OnAfterDeserialize()
    {
        DataContainer = CreateDataContainer(Type);
        DataContainer.SetData(Data);
    }
}



[Serializable]
public class DynamicMethodEventAction<T> : ValueReferenceEventAction<T>, IUseReferenceGuid
{
    [ValueDropdown(nameof(GetMethodDropdownOptions)), OnValueChanged(nameof(MethodChanged))] 
    public string MethodInfo;
    
    public DynamicMethodArgument[] Data;
    protected object[] _resolvedArguments;

    public DynamicMethodEventAction()
    {
        Data = Array.Empty<DynamicMethodArgument>();
    }

    public void SetMethodInfo(string method)
    {
        if (method != null)
        {
            var result = FindMethod(method);
            if (result == null)
                method = null;
        }

        MethodInfo = method;
        MethodChanged();
    }
    
    private void MethodChanged()
    {
        if (MethodInfo == null)
        {
            Data = Array.Empty<DynamicMethodArgument>();
            return;
        }

        List<DynamicMethodArgument> args = new List<DynamicMethodArgument>();
        foreach (var param in FindMethod(MethodInfo).GetParameters())
        {
            args.Add(new DynamicMethodArgument(new SerializableType(param.ParameterType)));
        }

        Data = args.ToArray();
    }
    
    
    protected override void HandleAction(IReferenceResolver resolver, T targetData)
    {
        FindMethod(MethodInfo).Invoke(targetData, _resolvedArguments);
    }

    public override void TryResolveValues(IReferenceResolver resolver, SerializableGuid target)
    {
        base.TryResolveValues(resolver, target);
        
        if (_resolvedArguments == null || _resolvedArguments.Length != Data.Length)
            _resolvedArguments = new object[Data.Length];
        for (int i = 0; i < Data.Length; ++i)
        {
            // TODO test if success?
            Data[i].GetData(resolver, ref _resolvedArguments[i]);
        }
    }

    protected virtual ICollection<MethodInfo> GetMethodOptions()
    {
        var methods = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public);
        methods = methods.Where(x => !x.ContainsGenericParameters).ToArray();
        return methods;
    }

    protected virtual MethodInfo FindMethod(string name)
    {
        return GetMethodOptions().FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCulture));
    }

    protected ICollection<ValueDropdownItem> GetMethodDropdownOptions()
    {
        return GetMethodOptions().Select(x => new ValueDropdownItem(x.Name, x.Name)).ToArray();
    }

    public override Delegate CreateDelegate(object target)
    {
        var method = FindMethod(MethodInfo);
        return ReflectionUtility.CreateDelegate(method, target);
    }

    public override object[] GetParameters()
    {
        return Data.Select(x => (object) x.DataContainer).ToArray();
    }

    public bool UsesGuid(SerializableGuid guid)
    {
        if (Data == null) return false;
        return Data.Any(x => x != null && x.UsesGuid(guid));
    }

    public void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
    {
        if (Data == null) return;
        for (var i = 0; i < Data.Length; i++)
        {
            if (Data[i] == null) continue;
            Data[i].ReplaceGuid(guid, replacement);
        }
    }
}
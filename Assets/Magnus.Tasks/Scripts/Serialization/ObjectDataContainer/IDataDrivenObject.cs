using System;

namespace Rhinox.Magnus.Tasks
{
    public interface IDataDrivenObject
    {
        object FindDataGeneric();
    }
    
    public interface IDataDrivenObject<TData> : IDataDrivenObject where TData : BaseDataDriverObject
    {
        TData FindData();
    }

    [Serializable]
    public abstract class BaseDataDriverObject
    {
        
    }
}
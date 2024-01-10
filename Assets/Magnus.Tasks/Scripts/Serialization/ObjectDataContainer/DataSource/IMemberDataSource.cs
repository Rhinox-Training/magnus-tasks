using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.Magnus.Tasks
{
    public interface IMemberDataSource
    {
        bool ResetValue();
        object GetValue();
        void SetValue(object val);
        Type GetMemberType();
    }
}
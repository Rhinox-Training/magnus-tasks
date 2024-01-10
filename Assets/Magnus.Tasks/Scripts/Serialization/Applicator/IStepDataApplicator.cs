using System.Collections.Generic;
using System.Linq;
using Rhinox.Utilities;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Vortex;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    public interface IStepDataApplicator
    {
        void Apply(IReferenceResolver hostResolver, ref BaseStepState stepState);
    }

    public interface IStepDataApplicator<T> : IStepDataApplicator
    {
        void Init(T data);
        
        T Data { get; }
    }
    
   

   

   
}
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhinox.Magnus.Tasks
{
    [SmartFallbackDrawn]
    public class StepBehaviour : MonoBehaviour
    {
        [SerializeReference, HideLabel]
        public StepData StepData;
    }
}
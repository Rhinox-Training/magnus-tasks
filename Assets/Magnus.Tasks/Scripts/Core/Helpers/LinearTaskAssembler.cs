using System.Linq;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [RequireComponent(typeof(TaskBehaviour))]
    public class LinearTaskAssembler : MonoBehaviour
    {
        private void Awake()
        {
            var task = GetComponent<TaskBehaviour>();
            var steps = GetComponentsInChildren<BaseStep>();
            if (task.StartStep == null)
                task.StartStep = steps.FirstOrDefault();
            for (int i = 0; i < steps.Length; ++i)
            {
                var step = steps[i];
                if (step is BaseBinaryStep binaryStep)
                {
                    if (i < steps.Length - 1)
                        binaryStep.NextStep = steps[i + 1];
                }
            }
        }
    }
}
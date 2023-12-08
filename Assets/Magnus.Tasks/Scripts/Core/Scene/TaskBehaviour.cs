using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhinox.Magnus.Tasks
{
    public abstract class TaskBehaviour : MonoBehaviour
    {
        public TaskObject TaskData;
    }
}

using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox
{
    [SmartFallbackDrawn]
    public class Test : MonoBehaviour
    {

        [SerializeField] [TabGroup("tab 1")] private int _int1;
        [SerializeField] [TabGroup("tab 1")] private float _float1;
        [SerializeField] [TabGroup("tab 1")] private bool _bool1;
        
        [SerializeField] [TabGroup("tab 2")] private int _int2;
        [SerializeField] [TabGroup("tab 2")] private int _int3;
        [SerializeField] [TabGroup("tab 2")] private int _int4;
        
        
        //[SerializeField] [TabGroup("new")] private int _tabgroup;
        
        private void t()
        {
            
            
        }
    }

}

using Rhinox.VOLT.Training;
using UnityEngine;

namespace Rhinox.VOLT.Domain
{
    public class ObjectDestroyedCondition : BaseCondition
    {
        public GameObject Object;

        protected override void Check()
        {
            if (IsMet)
                return;

            if (Object == null)
                SetConditionMet();
            
            base.Check();
        }
    }
}
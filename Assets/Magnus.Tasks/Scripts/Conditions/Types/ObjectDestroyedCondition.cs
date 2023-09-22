using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [RefactoringOldNamespace("Rhinox.VOLT.Domain", "com.rhinox.volt.domain")]
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
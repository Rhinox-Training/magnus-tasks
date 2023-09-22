using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [RefactoringOldNamespace("Rhinox.VOLT.Domain", "com.rhinox.volt.domain")]
    public class TimeElapsedCondition : BaseCondition
    {
        [SuffixLabel("sec")]
        public float TimeToWait;
        private float _elapsedTime;

        protected override bool OnInit()
        {
            if (TimeToWait < -1 || TimeToWait > 3600)
                PLog.Warn<MagnusLogger>($"TimeElapsedCondition: TimeToWait set to unreasonably large amount {TimeToWait} s");
            return true;
        }

        protected override void Check()
        {
            if (IsMet)
                return;

            _elapsedTime += Time.deltaTime;
            
            if (_elapsedTime >= TimeToWait)
                SetConditionMet();
            
            base.Check();
        }
    }
}
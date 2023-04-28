using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.VOLT.Training;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.VOLT.Domain
{
    public class TimeElapsedCondition : BaseCondition
    {
        [SuffixLabel("sec")]
        public float TimeToWait;
        private float _elapsedTime;

        protected override bool OnInit()
        {
            if (TimeToWait < -1 || TimeToWait > 3600)
                PLog.Warn<VOLTLogger>($"TimeElapsedCondition: TimeToWait set to unreasonably large amount {TimeToWait} s");
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
using System;
using System.Collections;
using UnityEngine;


namespace Utilities
{
    public class RepeatTimer : MonoBehaviour
    {
        public event Action OnTimerElapsed = delegate { };


        private float _interval = 1f;
        private Coroutine _timerCoroutine;


        private void OnDestroy()
        {
            ClearCoroutine();
        }


        public void SetInterval(float interval)
        {
            _interval = interval;
        }


        public void StartTimer()
        {
            if (_timerCoroutine != null) return;
            
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }


        public void StopTimer()
        {
            ClearCoroutine();
        }


        private IEnumerator TimerCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_interval);

                OnTimerElapsed?.Invoke();
            }
        }


        private void ClearCoroutine()
        {
            if (_timerCoroutine == null) return;
            
            StopCoroutine(_timerCoroutine);
                
            _timerCoroutine = null;
        }
    }
}

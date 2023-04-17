using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Newtonsoft.Json;
using WebRequesters;


namespace EventServiceSystem
{
    public class EventService : MonoBehaviour
    {
        [Serializable]
        private class EventData
        {
            public string type;
            public string data;
        }


        [Serializable]
        private class EventDataArray
        {
            public EventData[] events;
        }


        private const string PlayerPrefsKey = "UnsentEvents";


        [SerializeField] private string _serverUrl;
        [SerializeField] private float _cooldownBeforeSend = 10f;
        [SerializeField] private int _requestTimeout = 5;

    
        private List<EventData> _eventDataBuffer = new List<EventData>();
        private List<EventData> _eventDataInRequest = new List<EventData>();
        
        private IWebRequester _webRequester;
        private RepeatTimer _timer;
        private bool _isCommunicating;


        private void Awake() 
        {
            _webRequester = new UnityUnencryptedWebRequester();

            LoadEvents();
            StartTimer();
        }


        private void OnDestroy()
        {
            if (_timer != null)
            {
                _timer.OnTimerElapsed -= TrySendEvents;
                _timer.StopTimer();
            }
        }


        public void TrackEvents(string type, string data)
        {
            EventData newEventData = new EventData
            {
                type = type,
                data = data
            };
        
            _eventDataBuffer.Add(newEventData);
            
            SaveEvents();
        }


        public void StartTimer()
        {
            if (_timer != null) return;
        
            _timer = new GameObject("RepeatTimer").AddComponent<RepeatTimer>();
            _timer.transform.SetParent(transform);

            _timer.OnTimerElapsed += TrySendEvents;
            _timer.SetInterval(_cooldownBeforeSend);
            _timer.StartTimer();
        }


        private async void TrySendEvents()
        {
            if (_eventDataBuffer.Count == 0) return;
            if (_isCommunicating) return;

            _isCommunicating = true;
            _eventDataInRequest.AddRange(_eventDataBuffer);

            EventDataArray eventDataArray = new EventDataArray { events = _eventDataInRequest.ToArray() };
            string jsonData = JsonConvert.SerializeObject(eventDataArray);

            if (await _webRequester.SendRequest(_serverUrl, jsonData, _requestTimeout))
            {
                foreach (var dataToSend in _eventDataInRequest)
                {
                    _eventDataBuffer.Remove(dataToSend);
                }
                
                SaveEvents();
            }
            
            _eventDataInRequest.Clear();

            _isCommunicating = false;
        }


        private void SaveEvents()
        {
            EventDataArray eventDataArray = new EventDataArray { events = _eventDataBuffer.ToArray()};
            string jsonData = JsonConvert.SerializeObject(eventDataArray);

            PlayerPrefs.SetString(PlayerPrefsKey, jsonData);
        }


        private void LoadEvents()
        {
            string jsonData = PlayerPrefs.GetString(PlayerPrefsKey, "");

            if (!string.IsNullOrEmpty(jsonData))
            {
                EventDataArray eventDataArray = JsonConvert.DeserializeObject<EventDataArray>(jsonData);

                _eventDataBuffer.AddRange(eventDataArray.events);
                
                TrySendEvents();
            }
        }
    }
}

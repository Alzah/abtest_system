using System;
using Classes.Helpers;
using Newtonsoft.Json;
using UnityEngine;

namespace Classes.Managers.NetworkTime
{
    /// <summary>
    /// Менеджер глобального времени, синхронизирует время приложения с сервером.
    /// Защита от перемоток времени на устройстве
    /// </summary>
    public class NetworkTimeManager : MonoBehaviour
    {
        public const string TimeKey = "user_time";
        public const string IsGlobalKey = "is_global_time";

        private const string _timeApiUrl = "https://www.timeapi.io/api/Time/current/zone?timeZone=Etc/Utc";

        // timeouts at seconds
        private const int _defaultSyncTimeout = 600;
        private const int _errorSyncTimeout = 60;

        private ESyncStatus _syncStatus;

        private float _syncTime;
        private float _syncTimeout;
        private float _secondsFromLastSync;

        private TimeApiResponse _networkTimeData;

        public delegate void NetworkTimeUpdated(DateTime dateTime, bool isSync);
        public event NetworkTimeUpdated NetworkTimeUpdate;
        
        private void Awake()
        {
            _syncStatus = ESyncStatus.Error;
            WebRequestAsync();
        }

        private void Update()
        {
            if (_syncStatus == ESyncStatus.Pending) return;

            _syncTime += Time.deltaTime;
            _secondsFromLastSync += Time.deltaTime;

            if (_syncTime >= _syncTimeout)
            {
                WebRequest();
            }
        }

        private void WebRequest()
        {
            WebRequestAsync();
        }

        private async void WebRequestAsync()
        {
            _syncStatus = ESyncStatus.Pending;
            _syncTime = 0;

            var response = await AsyncWebRequest.Download(_timeApiUrl, 5);

            if (!string.IsNullOrEmpty(response))
            {
                _secondsFromLastSync = 0;
                _syncTimeout = _defaultSyncTimeout;

                _networkTimeData = JsonConvert.DeserializeObject<TimeApiResponse>(response);

                _syncStatus = ESyncStatus.Success;

                Debug.Log($"[NetworkTimeManager] Time synchronized UTC+0: " +
                    $"{_networkTimeData.hour:00}:{_networkTimeData.minute:00}:{_networkTimeData.seconds:00} " +
                    $"{_networkTimeData.day:00}/{_networkTimeData.month:00}/{_networkTimeData.year:00}, " +
                    $"next update in {_syncTimeout}s");
            }
            else
            {
                _syncTimeout = _errorSyncTimeout;
                _syncStatus = ESyncStatus.Error;
                Debug.LogError($"[NetworkTimeManager] Time synchronization error: {response}, next attempt in {_syncTimeout}s");
            }

            NetworkTimeUpdate?.Invoke(DateTimeUTC, Status == ESyncStatus.Success);
        }

        public ESyncStatus Status => _syncStatus;

        public bool IsAnySyncComplete => _networkTimeData != null;

        public DateTime DateTimeUTC
        {
            get
            {
                if (_syncStatus == ESyncStatus.Success)
                {
                    return _networkTimeData.GetDateTime().AddSeconds(_secondsFromLastSync);
                }

                if (_syncStatus == ESyncStatus.Pending || _syncStatus == ESyncStatus.Error)
                {
                    if (_networkTimeData == null)
                    {
                        Debug.LogError($"[NetworkTimeManager] No network time data, wrong time");

                        return DateTime.MinValue;
                    }
                    else
                    {
                        return _networkTimeData.GetDateTime().AddSeconds(_secondsFromLastSync);
                    }
                }

                Debug.LogError($"[NetworkTimeManager] Unknown sync status, wrong time");

                return DateTime.MinValue;
            }
        }

        public enum ESyncStatus
        {
            Pending,
            Success,
            Error
        }
    }

    [Serializable]
    internal class TimeApiResponse
    {
        public int year;
        public int month;
        public int day;
        public int hour;
        public int minute;
        public int seconds;
        public int milliSeconds;
        public string dateTime;
        public string date;
        public string time;
        public string timeZone;
        public string dayOfWeek;
        public bool dstActive;

        public DateTime GetDateTime()
        {
            return new DateTime(year, month, day, hour, minute, seconds, milliSeconds);
        }
    }
}

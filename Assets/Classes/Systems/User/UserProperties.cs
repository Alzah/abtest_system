using Classes.Managers.NetworkTime;
using Classes.Systems.Save;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Classes.Systems.User
{
    /// <summary>
    /// Хранилище пользовательских данных
    /// </summary>
    public class UserProperties : MonoBehaviour
    {
        private const string _userPropertiesKey = "user_properties";

        private Dictionary<string, object> _userProperties = new Dictionary<string, object>() {
            { NetworkTimeManager.TimeKey, DateTime.UtcNow },
            { NetworkTimeManager.IsGlobalKey, false },
            { "current_money", 500 },
            { "current_level", 0 },
        };

        private SaveSystem _saveSystem;
        private NetworkTimeManager _networkTimeManager;

        public delegate void UserPropertyUpdated(Dictionary<string, object> userProperties);
        public event UserPropertyUpdated UserPropertyUpdate;

        private void Awake()
        {
            _saveSystem = FindObjectOfType<SaveSystem>();
            _networkTimeManager = FindObjectOfType<NetworkTimeManager>();

            _networkTimeManager.NetworkTimeUpdate += OnNetworkTimeUpdated;
            UserPropertyUpdate += Save;

            _userProperties = _saveSystem.GetValue(_userPropertiesKey, _userProperties);
        }

        private void Start()
        {
            UserPropertyUpdate?.Invoke(Get());
        }

        private void OnNetworkTimeUpdated(DateTime dateTime, bool isSync)
        {
            AddOrUpdateMulti(new (string, object) []{
                (NetworkTimeManager.TimeKey, dateTime),
                (NetworkTimeManager.IsGlobalKey, isSync)
            });
        }

        public void AddOrUpdateSingle(string key, object value)
        {
            AddOrUpdateInner(key, value);
            UserPropertyUpdate?.Invoke(Get());
        }

        public void AddOrUpdateMulti(IEnumerable<(string, object)> tuples)
        {
            foreach(var tuple in tuples)
            {
                AddOrUpdateInner(tuple.Item1, tuple.Item2);
            }
            UserPropertyUpdate?.Invoke(Get());
        }

        public bool Delete(string key)
        {
            var flag = _userProperties.Remove(key);
            UserPropertyUpdate?.Invoke(Get());
            return flag;
        }

        private void AddOrUpdateInner(string key, object value)
        {
            if (_userProperties.ContainsKey(key))
            {
                _userProperties[key] = value;
            }
            else
            {
                _userProperties.Add(key, value);
            }
        }

        private void Save(Dictionary<string, object> userProperties)
        {
            _saveSystem.SetValue(_userPropertiesKey, _userProperties);
        }

        public Dictionary<string, object> Get()
        {
            return _userProperties;
        }

        private void OnDestroy()
        {
            Save(_userProperties);

            _networkTimeManager.NetworkTimeUpdate -= OnNetworkTimeUpdated;
            UserPropertyUpdate -= Save;
        }

    }
}

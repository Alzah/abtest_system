using Classes.Helpers;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace Classes.Systems.Save
{
    /// <summary>
    /// Интерфейс работы с загрузкой, сохранением и оперированием сейвов пользователя
    /// </summary>
    public interface ISaveProvider
    {
        void Save();
        void Load();
        void Set<T>(string key, T value);
        T Get<T>(string key, T defaultValue = default);
    }

    /// <summary>
    /// Реализация ISaveProvider, основанная на PlayerPrefs
    /// </summary>
    public class PlayerPrefsSaveProvider : ISaveProvider
    {
        private const string _saveKey = "save_data";
        private Hashtable _saveData;

        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            Converters = {new JsonInt32Converter()}
        };

        public void Load()
        {
            if (PlayerPrefs.HasKey(_saveKey))
            {
                var json = PlayerPrefs.GetString(_saveKey);
                _saveData = JsonConvert.DeserializeObject<Hashtable>(json, _jsonSerializerSettings);
            }
            else
            {
                _saveData = new Hashtable();
            }
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(_saveData, _jsonSerializerSettings);
            PlayerPrefs.SetString(_saveKey, json);
        }

        public void Set<T>(string key, T value)
        {
            _saveData[key] = value;
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (_saveData.ContainsKey(key))
            {
                return (T)_saveData[key];
            }
            else
            {
                return defaultValue;
            }
        }
    }
}

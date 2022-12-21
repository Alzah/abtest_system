using UnityEngine;

namespace Classes.Systems.Save
{
    /// <summary>
    /// Система сейвов
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private ISaveProvider _saveProvider;

        private void Awake()
        {
            _saveProvider = new PlayerPrefsSaveProvider();
            _saveProvider.Load();
        }

        public void SetValue<T>(string key, T value)
        {
            _saveProvider.Set(key, value);
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            return _saveProvider.Get<T>(key, defaultValue);
        }

        private void OnDestroy()
        {
            _saveProvider.Save();
        }
    }
}

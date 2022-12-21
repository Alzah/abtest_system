using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Classes.Systems.Save;
using Newtonsoft.Json;
using Classes.Helpers;
using Classes.Managers.NetworkTime;
using Classes.Systems.User;

namespace Classes.Systems.AbTests
{
    /// <summary>
    /// —истема аб-тестов
    /// ѕример конфига: Assets/StreamingAssets/abtest_config.json
    /// </summary>
    public class AbTestSystem : MonoBehaviour
    {
        private string _abTestConfigUrl = $"file:///{Application.streamingAssetsPath}/abtest_config.json";

        private SaveSystem _saveSystem;
        private UserProperties _userProperties;
        private NetworkTimeManager _networkTimeManager;

        private readonly int[] _primeNumbers = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53 };
        private long _deviceValue;
        private readonly List<AbTest> _runningAbTests = new List<AbTest>();
        private readonly List<AbTest> _waitingAbTests = new List<AbTest>();
        private HashSet<string> _closedAbTest = new HashSet<string>();

        public delegate void AbTestUpdated(List<AbTest> abTests);
        public event AbTestUpdated AbTestUpdate;

        private void Awake()
        {
            _deviceValue = GetDeviceValue();
            _saveSystem = FindObjectOfType<SaveSystem>();
            _userProperties = FindObjectOfType<UserProperties>();
            _networkTimeManager = FindObjectOfType<NetworkTimeManager>();

            //вытаскиваем из сейвов абтесты
            Debug.Log($"[AbTestSystem] Load ab tests from device");
            var savedAbTest = _saveSystem.GetValue("ab_test", new List<AbTest>());
            _closedAbTest = _saveSystem.GetValue("ab_test_closed", new HashSet<string>());

            _runningAbTests.AddRange(savedAbTest);
            
            //закрываем абтесты подход€щие под услови€ закрыти€
            TryEndRunningAbTests();

            //стартуем загрузку конфига с cdn
            LoadConfig();

            SubscribeEvent();
        }

        private async void LoadConfig()
        {
            Debug.Log($"[AbTestSystem] Load config from {_abTestConfigUrl}");
            var json = await AsyncWebRequest.Download(_abTestConfigUrl);
            var abtests = ParseJson(json);

            //апдейтим текущие запущенные
            for (int i = 0; i < abtests.Count;)
            {
                var abtest = abtests[i];
                if (TryUpdateOldAbTest(abtest))
                {
                    abtests.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            //запускаем новые или откладываем не запущенные дл€ дальшейшей обработки
            for (int i = 0; i < abtests.Count; i++)
            {
                var abtest = abtests[i];
                if(_closedAbTest.Contains(abtest.Id))
                {
                    //пропускаем инициализацию если аб-тест уже закрыт
                    continue;
                }
                if (abtest.NeedStart(_userProperties.Get()))
                {
                    RunAbTest(abtest);
                    _runningAbTests.Add(abtest);
                }
                else
                {
                    _waitingAbTests.Add(abtest);
                }
            }
            abtests.Clear();

            //сортируем вперЄд более приоритетные абтесты
            SortRunningEvent();

            AbTestUpdate?.Invoke(_runningAbTests);
        }

        private bool TryUpdateOldAbTest(AbTest abTest)
        {
            var oldTestIndex = _runningAbTests.FindIndex(x => x.EqualsId(abTest));
            if (oldTestIndex >= 0) 
            {
                var oldTest = _runningAbTests[oldTestIndex];
                if (oldTest.GetVersion() < abTest.GetVersion()) 
                {
                    Debug.Log($"[AbTestSystem] Updating abtest {abTest.GetId()}");
                    var oldOption = oldTest.SelectedOption;
                    if (oldTest.OptionsCount == abTest.OptionsCount)
                    {
                        abTest.SelectedOption = oldOption;
                    }
                    else
                    {
                        Debug.Log($"[AbTestSystem] When update abtest {abTest.GetId()} rerunning test, because old test's OptionsCount != new test's OptionsCount");
                        RunAbTest(abTest);
                    }
                    _runningAbTests[oldTestIndex] = abTest;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SortRunningEvent()
        {
            _runningAbTests.Sort(new Comparison<AbTest>((a, b) => b.Priority - a.Priority));
        }

        private AbTest RunAbTest(AbTest abTest)
        {
            var option = GetDistribution(abTest.GetDistributionId(), abTest.Options);
            abTest.SelectedOption = option;
            return abTest;
        }

        private List<AbTest> ParseJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                var loadedAbTest = JsonConvert.DeserializeObject<List<AbTest>>(json, new JsonAbTestsConverter());
                return loadedAbTest;
            }
            else
            {
                Debug.Log($"[AbTestSystem] Load config from {_abTestConfigUrl}");
                return new List<AbTest>();
            }
        }

        /// <summary>
        /// ѕолучение параметра по имени.
        /// ¬ случае повтора параметров в аб-тестах, параметр возвращаетс€ из аб-теста с более высоким приоритетом
        /// </summary>
        /// <typeparam name="T">“ип параметра аб-теста</typeparam>
        /// <param name="name">»м€ параметра</param>
        /// <param name="defaultValue">«начение по умолчанию, в случае если параметр не найден или неверно указан тип</param>
        /// <returns>Ќайденный параметр или значение по умолчанию(defaultValue)</returns>
        public T GetValueForParameterName<T>(string name, T defaultValue)
        {
            var abtest = _runningAbTests.FirstOrDefault(x => x.HasParameter(name));
            try
            {
                if (abtest != null)
                {
                    return (T)abtest.GetParameter(name).GetValue();
                }
                else
                {
                    Debug.Log($"[AbTestSystem] No found {name} parameter in ab tests, return defaultValue");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[AbTestSystem] Try return parameter from {abtest.GetId()} throw: {e}, return defaultValue");
            }
            return defaultValue;
        }

        /// <summary>
        /// ѕолучание выбранной когорты дл€ аб-теста
        /// </summary>
        /// <param name="id">»дентификатор аб-теста</param>
        /// <returns> огорта аб-теста или null</returns>
        public Option GetOptionForAbTest(string id)
        {
            var abtest = _runningAbTests.FirstOrDefault(x => x.GetId().Equals(id));
            if (abtest == null)
            {
                Debug.Log($"[AbTestSystem] No found {id} ab tests, return null");
            }
            return abtest.GetOption();
        }

        /// <summary>
        /// ƒанные о запущенных у пользовател€ аб-тестах
        /// </summary>
        /// <returns>“аблица(id аб-теста; id выбранной когорты)</returns>
        public Dictionary<string, string> GetAnalyticsData()
        {
            return new Dictionary<string, string>(_runningAbTests.Select(x => new KeyValuePair<string, string>(x.GetId(), x.GetOption().Id)));
        }

        /// <summary>
        /// «акрытие тестов
        /// public - дл€ тестов, запуск/закрытие тестов нужно делать геймплейном цикле при изменении ключевых параметров или при смене игровоко стейта, например выхода в меню
        /// </summary>
        public void TryEndRunningAbTests()
        {
            //закрываем абтесты подход€щие под услови€ закрыти€
            var closedCount = 0;
            var closingTests = _runningAbTests.Where(x => x.NeedEnd(_userProperties.Get())).ToArray();
            foreach (var test in closingTests)
            {
                _runningAbTests.Remove(test);
                _closedAbTest.Add(test.Id);
            }
            if (closedCount > 0)
            {
                Debug.Log($"[AbTestSystem] Closed {closedCount} ab tests");
            }
            AbTestUpdate?.Invoke(_runningAbTests);
        }

        /// <summary>
        /// «апуск отложенных тестов
        /// public - дл€ тестов, запуск/закрытие тестов нужно делать геймплейном цикле при изменении ключевых параметров или при смене игровоко стейта, например выхода в меню
        /// </summary>
        public void TryStartWaitingAbTests()
        {
            var updateFlag = false;
            for (int i = 0; i < _waitingAbTests.Count;)
            {
                var abtest = _waitingAbTests[i];
                if (abtest.NeedStart(_userProperties.Get()))
                {
                    _waitingAbTests.Remove(abtest);
                    RunAbTest(abtest);
                    _runningAbTests.Add(abtest);
                    updateFlag = true;
                }
                else
                {
                    i++;
                }
            }

            if (updateFlag)
            {
                SortRunningEvent();
                AbTestUpdate?.Invoke(_runningAbTests);
            }
        }

        private int GetDistribution(int setNumber, Option[] options)
        {
            if (setNumber >= _primeNumbers.Length)
            {
                throw new NotImplementedException();
            }

            var uniqueSetNum = _deviceValue % _primeNumbers[setNumber];
            var k = 0;
            while (uniqueSetNum < 100 && k < 100 && uniqueSetNum > 0)
            {
                uniqueSetNum *= _primeNumbers[setNumber];
                k++;
            }

            var chanceInPercent = (_deviceValue + uniqueSetNum) % 100;
            
            int weight = 0;
            for (int i = 0; i < options.Length; i++)
            {
                weight += options[i].Weight;
                if (chanceInPercent < weight)
                {
                    return i;
                }
            }

            return 0;
        }

        private long GetDeviceValue()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
            }

            var devHashSum = deviceId.ToCharArray().Sum(x =>
            {
                var n = x - '0';
                return n + n + n * n;
            });

            if (devHashSum == 0)
            {
                deviceId = Guid.NewGuid().ToString();
                devHashSum = deviceId.ToCharArray().Sum(x =>
                {
                    var n = x - '0';
                    return n + n + n * n;
                });
            }

            if (devHashSum == 0)
            {
                devHashSum = UnityEngine.Random.Range(1000, 10000);
            }

            return devHashSum;
        }

        private void OnNetworkTimeUpdated(DateTime dateTime, bool isSync)
        {
            TryEndRunningAbTests();
            TryStartWaitingAbTests();
        }

        private void SubscribeEvent()
        {
            _networkTimeManager.NetworkTimeUpdate += OnNetworkTimeUpdated;
        }

        private void UnsubscribeEvent()
        {
            _networkTimeManager.NetworkTimeUpdate -= OnNetworkTimeUpdated;
        }

        private void OnDestroy()
        {
            UnsubscribeEvent();

            _saveSystem.SetValue("ab_test", _runningAbTests);
            _saveSystem.SetValue("ab_test_closed", _closedAbTest);

        }
    }
}

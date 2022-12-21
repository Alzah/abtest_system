using Classes.Systems.AbTests;
using System.Collections.Generic;
using UnityEngine;

namespace Classes.UI
{
    public class UIAbTestScrollView : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _content;
        [SerializeField]
        private GameObject _itemPrefab;

        private List<GameObject> _childs = new List<GameObject>();

        private AbTestSystem _abTest;

        // Start is called before the first frame update
        private void Awake()
        {
            _abTest = FindObjectOfType<AbTestSystem>();
            _abTest.AbTestUpdate += AbTestUpdated;
        }

        private void AbTestUpdated(List<AbTest> abTests)
        {
            _childs.ForEach(x => Destroy(x));
            _childs.Clear();
            foreach (var abTest in abTests)
            {
                var go = Instantiate(_itemPrefab, _content);
                var item = go.GetComponent<UIAbTestItem>();
                item.UpdateAbTest(abTest);
                _childs.Add(go);
            }
        }

        private void OnDestroy()
        {
            _abTest.AbTestUpdate -= AbTestUpdated;
        }
    }
}
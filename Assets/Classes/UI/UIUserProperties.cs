using Classes.Systems.AbTests;
using Classes.Systems.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Classes.Systems.User.UserProperties;

namespace Classes.UI
{
    public class UIUserProperties : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _content;
        [SerializeField]
        private GameObject _itemPrefab;

        private List<GameObject> _childs = new List<GameObject>();

        private UserProperties _userProperties;

        private void Awake()
        {
            _userProperties = FindObjectOfType<UserProperties>();
            _userProperties.UserPropertyUpdate += UserPropertyUpdated;
        }

        private void UserPropertyUpdated(Dictionary<string, object> userProperties)
        {
            _childs.ForEach(x => Destroy(x));
            _childs.Clear();
            foreach (var property in userProperties)
            {
                var go = Instantiate(_itemPrefab, _content);
                var item = go.GetComponent<UIItem>();
                item.UpdateText($"{property.Key}\n{property.Value}");
                _childs.Add(go);
            }
        }

        private void OnDestroy()
        {
            _userProperties.UserPropertyUpdate -= UserPropertyUpdated;
        }
    }
}

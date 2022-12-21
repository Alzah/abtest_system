using Classes.Systems.AbTests;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Classes.UI
{
    public class UIAbTestItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private TextMeshProUGUI _textMesh;
        private UIOptions _uiOptions;
        private AbTest _abTest;

        private void Awake()
        {
            _uiOptions = FindObjectOfType<UIOptions>(); 
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _uiOptions.ShowAbTestInfo(_abTest);         
        }

        public void UpdateAbTest(AbTest test)
        {
            _textMesh.text = test.ToString();
            _abTest = test;
        }
    }
}

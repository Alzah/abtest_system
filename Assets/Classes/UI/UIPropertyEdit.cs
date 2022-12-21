using Classes.Systems.User;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Classes.UI
{
    public class UIPropertyEdit : MonoBehaviour
    {
        [SerializeField]
        private Button _add;
        [SerializeField]
        private Button _delete;
        [SerializeField]
        private TMP_Dropdown _dropdown;
        [SerializeField]
        private TMP_InputField _inputFieldKey;
        [SerializeField]
        private TMP_InputField _inputFieldValue;
        [SerializeField]
        private TextMeshProUGUI _textMesh;

        private EInputType _inputType = EInputType.Int;
        private UserProperties _userProperties;

        private void Awake()
        {
            _userProperties = FindObjectOfType<UserProperties>();

            _dropdown.onValueChanged.AddListener(DropdownOnValueChanged);
            _add.onClick.AddListener(AddClick);
            _delete.onClick.AddListener(DeleteClick);
        }

        private void DropdownOnValueChanged(int value)
        {
            _inputType = (EInputType)value;
        }

        private void AddClick()
        {
            if (string.IsNullOrEmpty(_inputFieldKey.text))
            {
                _textMesh.text = "Key is empty!!!";
                return;
            }

            var valueString = _inputFieldValue.text;

            if (string.IsNullOrEmpty(valueString))
            {
                _textMesh.text = "Value is empty!!!";
                return;
            }

            switch (_inputType)
            {
                case EInputType.Int:
                    if(int.TryParse(valueString, out var intValue))
                    {
                        _userProperties.AddOrUpdateSingle(_inputFieldKey.text, intValue);
                    }
                    else
                    {
                        _textMesh.text = "Check input field";
                    }
                    break;
                case EInputType.Float:
                    if (float.TryParse(valueString, out var floatValue))
                    {
                        _userProperties.AddOrUpdateSingle(_inputFieldKey.text, floatValue);
                    }
                    else
                    {
                        _textMesh.text = "Check input field";
                    }
                    break;
                case EInputType.String:
                    _userProperties.AddOrUpdateSingle(_inputFieldKey.text, valueString);
                    break; 
                case EInputType.Bool:
                    if (bool.TryParse(valueString, out var boolValue))
                    {
                        _userProperties.AddOrUpdateSingle(_inputFieldKey.text, boolValue);
                    }
                    else
                    {
                        _textMesh.text = "Check input field";
                    }
                    break; 
            }
        }

        private void DeleteClick()
        {
            _textMesh.text = "Delete clicked";
            if (string.IsNullOrEmpty(_inputFieldKey.text))
            {
                _textMesh.text = "Key is empty!!!";
                return;
            }

            if (_userProperties.Delete(_inputFieldKey.text))
            {
                _textMesh.text = $"Property {_inputFieldKey.text} deleted";
            }
            else
            {
                _textMesh.text = $"Property {_inputFieldKey.text} no found!";
            }
        }

        private void OnDestroy()
        {
            _dropdown.onValueChanged.RemoveListener(DropdownOnValueChanged);
            _add.onClick.RemoveListener(AddClick);
            _delete.onClick.RemoveListener(DeleteClick);
        }

        private enum EInputType
        {
            Int,
            Float,
            String,
            Bool,
        }
    }
}

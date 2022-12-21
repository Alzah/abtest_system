using Classes.Systems.AbTests;
using System.Text;
using TMPro;
using UnityEngine;

namespace Classes.UI
{
    public class UIOptions : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _textMesh;
        private StringBuilder _builder;

        private AbTestSystem _abTestSystem;

        private void Awake()
        {
            _builder = new StringBuilder();
            _abTestSystem = FindObjectOfType<AbTestSystem>();   
        }

        public void ShowAbTestInfo(AbTest abTest)
        {
            var option = abTest.GetOption();
            _builder.Clear();

            _builder
                .AppendLine(abTest.Id)
                .AppendLine(string.Empty)
                .Append("Id: ")
                .AppendLine(option.Id)
                .Append("Weight: ")
                .AppendLine(option.Weight.ToString())
                .AppendLine("Option:");

            for (int i = 0; i < option.Parameters.Length; i++)
            {
                _builder.Append(option.Parameters[i].Name).Append(": ")
                    .AppendLine(_abTestSystem.GetValueForParameterName<object>(option.Parameters[i].Name, null).ToString());
            }

            _textMesh.text = _builder.ToString();
        }
    }
}

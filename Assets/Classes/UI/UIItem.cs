using TMPro;
using UnityEngine;

namespace Classes.UI
{
    public class UIItem : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _textMesh;

        public void UpdateText(string text)
        {
            _textMesh.text = text;
        }
    }
}

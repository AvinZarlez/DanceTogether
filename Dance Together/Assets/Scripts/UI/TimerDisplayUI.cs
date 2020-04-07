using UnityEngine;
using TMPro;

namespace App.UI
{
    public class TimerDisplayUI : MonoBehaviour
    {
        public bool ShowAsWholeNumber = true;
        [SerializeField]
        private TextMeshProUGUI numberTextField;

        public void UpdateNumber(float _value)
        {
            if (ShowAsWholeNumber)
            {
                numberTextField.text = Mathf.CeilToInt(_value).ToString();
            }
            else
            {
                numberTextField.text = _value.ToString();
            }
        }
    }
}

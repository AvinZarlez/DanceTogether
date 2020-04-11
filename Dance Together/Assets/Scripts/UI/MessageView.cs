using UnityEngine;
using TMPro;

[RequireComponent(typeof(View))]
public class MessageView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI TextDisplay;
    
    /// <summary>
    /// Method to simply change the text message displayed.
    /// </summary>
    /// <param name="_message"></param>
    public void UpdateMessage(string _message)
    {
        TextDisplay.text = _message;
    }
}

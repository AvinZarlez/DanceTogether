using UnityEngine;
using UnityEngine.UI;
using TMPro;
using App.Utility;

public class LocalPlayerIcon : MonoBehaviour
{
    [SerializeField]
    private Image IconImage;
    [SerializeField]
    private Image ColorButtonImage;
    [SerializeField]
    private Image CheckMarkImage;
    [SerializeField]
    private TextMeshProUGUI PlayerNumber;
    [SerializeField]
    private TextMeshProUGUI ColorButtonText;

    public void UpdateColor(IndexedColor _color)
    {
        IconImage.color = _color.Color;
        ColorButtonImage.color = _color.Color;
        ColorButtonText.text = _color.Name;
        
    }
    public void UpdateNumber(int _value)
    {
        PlayerNumber.text = _value.ToString();
    }

    public void IsReady(bool _bool)
    {
        // simply show and hide the image associated.
        CheckMarkImage.enabled = _bool;
    }

    public void UpdateAll(IndexedColor _color, int _playerNumber)
    {
        IconImage.color = _color.Color;
        ColorButtonImage.color = _color.Color;
        ColorButtonText.text = _color.Name;

        PlayerNumber.text = _playerNumber.ToString();
    }

    public void Reset()
    {
        UpdateAll(DanceTogetherUtility.GetIndexColor(0), 1);
    }
}

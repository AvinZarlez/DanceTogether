using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using App.Data;

public class ChoosePlayerButton : MonoBehaviour
{
    /// <summary>
    /// Button CallBack
    /// </summary>
    public event Action<PlayerDataSnapShot> ButtonClickEvent;

    [SerializeField]
    private TextMeshProUGUI text; // use text to display player id on button.
    [SerializeField]
    private Image buttonColorImage;
    [SerializeField, Tooltip("Image used to indicate player is selected or not.")]
    private Image selectedImage;
    [SerializeField]
    private Color correctColor = Color.green;
    [SerializeField]
    private Color inCorrectColor = Color.red;

    private bool isSelected = false;

    public PlayerDataSnapShot player;

    public bool IsSelected
    {
        get { return isSelected; }
    }

    public void UpdateButtonInfo(PlayerDataSnapShot _player, bool isButton = true)
    {
        if (_player.PlayerID == -1)
            return;

        player = _player; // save player reference

        text.text = player.PlayerID.ToString(); // display player id.

        buttonColorImage.color = player.PlayerColor.Color; // set image of button to player color.

        selectedImage.gameObject.SetActive(false);

        // let us toggle the button use. this is so we can use this prefab as an icon or a button.
        Button localButton = GetComponentInChildren<Button>();
        localButton.enabled = isButton;
    }

    public void SelectPlayer(bool _isSelected, bool _correct)
    {
        isSelected = _isSelected;
        if (_correct)
        {
            selectedImage.color = correctColor;
        } else
        {
            selectedImage.color = inCorrectColor;
        }

        selectedImage.gameObject.SetActive(_isSelected);
    }

    public void ButtonClick()
    {
        if (player.PlayerID == -1)
            return;

        if(ButtonClickEvent != null)
        {
            ButtonClickEvent(player);
        }
    }

    public void OnDestroy()
    {
        // clean up links
        ButtonClickEvent = null;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using App.Controllers;
using App.Data;

[RequireComponent(typeof(Button))]
public class ChoosePlayerButton : MonoBehaviour
{
    EndGameManager controller;

    [SerializeField]
    private TextMeshProUGUI Text; // use text to display player id on button.
    public PlayerDataSnapShot player;

    public void UpdateButtonInfo(EndGameManager _controller, PlayerDataSnapShot _player)
    {
        if (_player.PlayerID == -1  || _controller == null)
            return;

        controller = _controller; // save reference to controller.
        player = _player; // save player reference

        Text.text = player.PlayerID.ToString(); // display player id.

        GetComponent<Image>().color = player.PlayerColor.Color; // set image of button to player color.
    }

    public void ButtonClick()
    {
        if (player.PlayerID == -1 || controller == null)
            return;

        controller.ChoosePlayer(player);
    }
}

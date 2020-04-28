using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using App.Networking;
using App.Data;
using App.Controllers;

public class ActiveGameViewManager : MonoBehaviour
{
    [SerializeField, Header("Player UI Reference")]
    private Image playerColor;
    [SerializeField]
    private TextMeshProUGUI playerId;

    [SerializeField, Header("Selection UI Reference")]
    private RectTransform availablePlayersContainer;

    [SerializeField, Header("Prefab References")]
    private ChoosePlayerButton selectablePlayerButton;

    private Dictionary<PlayerDataSnapShot, ChoosePlayerButton> playerButtons = new Dictionary<PlayerDataSnapShot, ChoosePlayerButton>();

    private void OnEnable()
    {
        if(!NetworkController.s_InstanceExists)
        {
            // Hard Stop if instance doesnt exist.
            return;
        }

        playerColor.color = NetworkController.s_Instance.LocalPlayer.playerColor.Color;
        playerId.text = NetworkController.s_Instance.LocalPlayer.PlayerID.ToString();

        UpdatePlayerList();
    }

    private void OnDisable()
    {
        ClearPlayerList();
    }

    public void UpdatePlayerList()
    {
        ClearPlayerList();

        foreach (PlayerDataSnapShot playerSS in MainController.s_Instance.GameController.ActivePlayerData)
        {
            // dont add ourselves to the choosable list.
            if (!playerSS.IsLocalPlayer)
            {
                ChoosePlayerButton newButton = Instantiate(selectablePlayerButton, availablePlayersContainer);
                newButton.UpdateButtonInfo(playerSS);
                newButton.ButtonClickEvent += ChoosePlayer;
                playerButtons.Add(playerSS,newButton);
            }
        }

        Debug.Log("active players found : " + MainController.s_Instance.GameController.ActivePlayerData.Count);
    }

    public void ClearPlayerList()
    {
        foreach (var kvp in playerButtons)
        {
            kvp.Value.ButtonClickEvent -= ChoosePlayer;
            Destroy(kvp.Value.gameObject);
        }
        playerButtons.Clear();
    }

    public void ChoosePlayer(PlayerDataSnapShot _player)
    {
        Debug.Log("choose player has been called");
        // does local player have selected player in list already?
        if (NetworkController.s_Instance.LocalPlayer.SelectedPlayers.Contains(_player))
        {
            // remove player from list
            NetworkController.s_Instance.LocalPlayer.SelectedPlayers.Remove(_player);
            // reset button
            playerButtons[_player].SelectPlayer(false, false);
        } else
        {
            // check to see if newly selected player is correct or not.
            if (_player.SongID == NetworkController.s_Instance.LocalPlayer.SongID)
            {
                playerButtons[_player].SelectPlayer(true, true); // show button selected and correct color
            } else
            {
                playerButtons[_player].SelectPlayer(true, false); // show button selected, and incorrect color
            }

            // add selected player to local players selected player list.
            NetworkController.s_Instance.LocalPlayer.SelectedPlayers.Add(_player);
        }

    }
}

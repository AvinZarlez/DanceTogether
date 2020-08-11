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
    [SerializeField]
    private Button continueButton;

    [SerializeField, Header("Selection UI Reference")]
    private RectTransform availablePlayersContainer;
    [SerializeField]
    private RectTransform selectedPlayersContainer;

    [SerializeField, Header("Prefab References")]
    private ChoosePlayerButton selectablePlayerButton;

    [SerializeField, Header("Particle System Reference")]
    private ParticleSystem pSysytem;

    private Dictionary<PlayerDataSnapShot, ChoosePlayerButton> playerButtons = new Dictionary<PlayerDataSnapShot, ChoosePlayerButton>();
    private Dictionary<PlayerDataSnapShot, ChoosePlayerButton> chosenPlayerButtons = new Dictionary<PlayerDataSnapShot, ChoosePlayerButton>();

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
        ClearSelectedList();
    }

    private void OnDisable()
    {
        ClearPlayerList();
        ClearSelectedList();
    }

    private void CheckIfCanContinue()
    {
        // if more players have been selected than are allowed in the group
        if (NetworkController.s_Instance.LocalPlayer.SelectedPlayers.Count >= MainController.s_Instance.CurrentGameType.Data.GroupSize)
        {
            continueButton.interactable = false;
        }
        // if the active players are greater than 1, and the local player hasnt made any choice, set continue button to be innactive.
        else if (MainController.s_Instance.GameController.ActivePlayerData.Count > 1 && NetworkController.s_Instance.LocalPlayer.SelectedPlayers.Count < MainController.s_Instance.CurrentGameType.Data.GroupSize -1)
        {
            continueButton.interactable = false; // default behaviour.
        }
        else
        {
            continueButton.interactable = true; // for testing app with single player. // else only 1 player in game, and cant choose. allow continue.
        }
    }

    /// <summary>
    /// Clears and repopulates list of available players to choose from.
    /// </summary>
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

        //Debug.Log("active players found : " + MainController.s_Instance.GameController.ActivePlayerData.Count);
    }

    /// <summary>
    /// Clears list of available players to choose from.
    /// Warning!!! does not re populate list.
    /// </summary>
    public void ClearPlayerList()
    {
        foreach (var kvp in playerButtons)
        {
            kvp.Value.ButtonClickEvent -= ChoosePlayer;
            Destroy(kvp.Value.gameObject);
        }
        playerButtons.Clear();
    }
    public void ClearSelectedList()
    {
        foreach (var kvp in chosenPlayerButtons)
        {
            Destroy(kvp.Value.gameObject);
        }
        chosenPlayerButtons.Clear();
    }

    /// <summary>
    /// This function handles the data selectioin of what players are selected, and gives feedback on the correctness of the choice visually.
    /// </summary>
    /// <param name="_player"></param>
    public void ChoosePlayer(PlayerDataSnapShot _player)
    {
        // does local player have selected player in list already?
        if (NetworkController.s_Instance.LocalPlayer.SelectedPlayers.Contains(_player))
        {
            // remove player from list
            NetworkController.s_Instance.LocalPlayer.SelectedPlayers.Remove(_player);
            // reset button
            playerButtons[_player].SelectPlayer(false, false);

            // remove chosen player icon from chosen list.
            if (chosenPlayerButtons.ContainsKey(_player))
            {
                Destroy(chosenPlayerButtons[_player].gameObject);
                chosenPlayerButtons.Remove(_player);
            }
        } else
        {
            if(!chosenPlayerButtons.ContainsKey(_player))
            {
                ChoosePlayerButton newButton = Instantiate(selectablePlayerButton, selectedPlayersContainer);
                newButton.UpdateButtonInfo(_player, false); // update the icon, false removes button use.
                chosenPlayerButtons.Add(_player, newButton); // track the selected player and icon.
            }
            // check to see if newly selected player is correct or not.
            if (_player.SongID == NetworkController.s_Instance.LocalPlayer.SongID)
            {
                playerButtons[_player].SelectPlayer(true, true); // show button selected and correct color
                chosenPlayerButtons[_player].SelectPlayer(true, true); // update the selected players list as well
            } else
            {
                playerButtons[_player].SelectPlayer(true, false); // show button selected, and incorrect color
                chosenPlayerButtons[_player].SelectPlayer(true, false); // update the selected players list as well
            }

            // add selected player to local players selected player list.
            NetworkController.s_Instance.LocalPlayer.SelectedPlayers.Add(_player);
        }

        CheckIfCanContinue();
    }



    /// <summary>
    /// this function allows for the easy toggle of game complete and game active states.
    /// </summary>
    /// <param name="_value"></param>
    public void GameIsActive(bool _value)
    {
        //pSysytem.gameObject.SetActive(_value);
        continueButton.gameObject.SetActive(!_value);

        CheckIfCanContinue();
    }

    /// <summary>
    /// Call continue from UI button
    /// </summary>
    public void Continue()
    {
        MainController.s_Instance.GameController.LocalGotoPostGame();
    }

}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.UI;
using DG.Tweening;

public class NetworkedPlayerScript : CaptainsMessPlayer
{
    [SerializeField]
    private float movementSpeed = 1.0f;
    public float fastMovementSpeed = 0.5f;

    public GameObject playerButton;
    private Outline playerButtonOutline;

    [SerializeField]
    public LocalPlayerScript localPScript; //TEMP made public for checking in sort players
    [HideInInspector]
    public RemotePlayerScript remotePScript;

    public GameObject playerParent;

    [SyncVar]
    private int score;
    [SyncVar]
    private int scoredThisRound;

    [SyncVar]
    public bool scored_GuessedCorrect;

    [SyncVar]
    private int songID;

    [SyncVar]
    private int matchSongID; // The other player this player has picked as a match

    [SyncVar]
    public float matchTime;

    [SyncVar]
    private int color = -1;

    [SyncVar(hook = "OnNameTextChanged")]
    public string nameText = "";

    [SyncVar, HideInInspector]
    public float captainsCountdown = 0;

    [HideInInspector]
    public CaptainsMess mess;
    public void Awake()
    {
        mess = FindObjectOfType(typeof(CaptainsMess)) as CaptainsMess;
    }

    void SetColor()
    {
        if (color != -1)
        {
            Color c = ColorScript.GetColor(color);
            playerButton.GetComponent<Image>().color = c;
        }
    }

    bool AreAllPlayersMatched()
    {
        List<CaptainsMessPlayer> players = mess.Players();

        bool allPlayersMatched = true;
        foreach (CaptainsMessPlayer player in players)
        {
            if (player.GetComponent<NetworkedPlayerScript>().GetMatchSongID() == -1)
            {
                allPlayersMatched = false;
                break;
            }
        }

        return allPlayersMatched;
    }

    void SetReady(bool ready)
    {
        if (ready)
        {
            SendReadyToBeginMessage();

            GUIManagerScript.SetButtonText("Cancel");
        }
        else
        {
            SendNotReadyToBeginMessage();
            
            GUIManagerScript.SetButtonText("Dance");
        }

        GUIManagerScript.SetMainButtonHighlight(ready);
    }

    void SortPlayers()
    {
        List<CaptainsMessPlayer> players = mess.Players();
        int i = 0;
        int size = players.Count;

        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
            if (!nps.isLocalPlayer)
            {

                RemotePlayerScript rps = player.GetComponent<RemotePlayerScript>();

                Vector3 goal = rps.SetPosition(++i, size);

                if (nps.playerButton.activeSelf == false)
                {
                    nps.playerButton.SetActive(true);
                    nps.playerButton.transform.localPosition = new Vector3(i*160,-340,0);
                }

                nps.playerButton.transform.DOLocalMove(goal, movementSpeed);
                nps.playerButton.transform.DOScale(Vector3.one, fastMovementSpeed);
            }
            nps.SetColor();
            nps.SetNameText();
        }

        if (size >= 4)
        {
            GUIManagerScript.SetButton(true);
        }
        else
        {
            GUIManagerScript.SetButton(false);
        }

        playerParent.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * (size + 1), 340);
    }

    void Start()
    {
        playerParent = GameObject.FindWithTag("PlayerParent");
        playerButton = transform.Find("PlayerButton").gameObject;

        playerButton.transform.SetParent(playerParent.transform, false);
        playerButton.transform.localScale = Vector3.one;

        playerButtonOutline = playerButton.GetComponent<Outline>();
        playerButtonOutline.enabled = false;

        playerButton.SetActive(false);

        if (isLocalPlayer)
        {
            playerButton.GetComponent<Button>().interactable = false;
            playerButton.transform.localPosition = Vector3.zero;
            
            GUIManagerScript.DisableInput(false);
            GUIManagerScript.SetBackButton(false);

            nameText = "";
            GUIManagerScript.FillPlayerText(nameText);
        }
    }

    public void Update()
    {
        if (isServer)
            captainsCountdown = mess.CountdownTimer();

        // Hacky bullshit that might end up being too many calls? But hopefully not?
        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
        Assert.IsNotNull<GameManagerScript>(gameManager);
        if (gameManager.IsGameStarted())
        {
            if (GetMatchSongID() != -1)
            {
                playerButtonOutline.enabled = true;
            }
            else
            {
                playerButtonOutline.enabled = false;
            }
        }
        else
        {
            if (ready)
            {
                playerButtonOutline.enabled = true;
            }
            else
            {
                playerButtonOutline.enabled = false;
            }
        }
    }

    // Redundant? Possible. But I am not sure
    // Unity says this is the one to use, yet OnDestroy works and possibly works better?
    // Let's do both to be safe.
    void OnDestroy()
    {
        //print("Player was was destroyed");
        if (playerButton != null)
            Destroy(playerButton);

        if (isLocalPlayer)
        {
            GUIManagerScript.SetInput(false);
            GUIManagerScript.SetButton(false);
            GUIManagerScript.SetRulesButton(false);
            GUIManagerScript.SetBackButton(false);
            GUIManagerScript.HideColorShow();
            GUIManagerScript.countdownText.enabled = false;
        }

        // If this is a client player on the server then OnClientExitLobby will not be called.
        // Call it here instead.
        if (networkManager.IsHost() && networkManager.localPlayer != this)
        {
            OnClientExitLobby();
        }
    }
    public override void OnNetworkDestroy()
    {
        OnDestroy();

        base.OnNetworkDestroy();
    }

    public override void OnStartLocalPlayer()
    {
        gameObject.name = "LOCAL Player";

        remotePScript.enabled = false;
        localPScript.enabled = true;

        CmdSetColor();

        score = 0;
        scoredThisRound = 0;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.SFXClips.DanceTogether);

        GUIManagerScript.SetRulesButton(true);
        GUIManagerScript.SetInput(true);

        SetReady(false);

        base.OnStartLocalPlayer();
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        // Brief delay to let SyncVars propagate
        Invoke("SortPlayers", 0.5f);
    }

    public void SetNameText()
    {
        OnNameTextChanged(nameText);
    }
    public void OnNameTextChanged(string s)
    {
        if (s == "")
        {
            nameText = ColorScript.GetColorName(color);
        }
        else
        {
            nameText = s;
        }

        playerButton.GetComponentInChildren<Text>().text = nameText;
    }

    public List<CaptainsMessPlayer> GetPlayers()
    {
        return mess.Players();
    }

    public int GetColor()
    {
        return color;
    }

    public int GetScoredThisRound()
    {
        return scoredThisRound;
    }

    public int GetSongID()
    {
        return songID;
    }

    public int GetMatchSongID()
    {
        return matchSongID;
    }

    public void ResetMatch()
    {
        matchSongID = -1;
        matchTime = -1;
        scored_GuessedCorrect = false;
    }

    public void ToggleReady()
    {
        SetReady(!ready);
    }

    public void MainButtonPressed()
    {
        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();

        Assert.IsNotNull<GameManagerScript>(gameManager);

        if (gameManager.IsInPostGame())
        {
            CmdReplayGame();
        }
        else
        {
            ToggleReady();
        }
    }

    public void PlayerButtonPressed()
    {
        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();

        Assert.IsNotNull<GameManagerScript>(gameManager);

        if (isLocalPlayer)
        {
            if (!gameManager.IsGameStarted())
            {
                ToggleReady();
            }
        }
        else
        {
            if (gameManager.IsGameStarted())
            {

                List<CaptainsMessPlayer> players = GetPlayers();
                foreach (CaptainsMessPlayer player in players)
                {
                    NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
                    if (player.name == "LOCAL Player")
                    {
                        nps.CmdSetMatchSongID(songID);
                    }
                    nps.playerButton.SetActive(false);
                    nps.playerButton.GetComponent<Button>().interactable = false;
                }

                playerButton.SetActive(true);
                playerButton.GetComponent<Button>().interactable = false;
                playerButton.transform.DOLocalMove(new Vector3(40,0,0), fastMovementSpeed);
                playerButton.transform.DOScale(new Vector3(1.5f, 1.5f, 1f), fastMovementSpeed);

                playerParent.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 340);

                GUIManagerScript.SetBackButton(true);
            }
        }
    }

    [Command]
    public void CmdRulesButtonPressed()
    {
        RpcRulesButtonPressed();
    }

    [ClientRpc]
    public void RpcRulesButtonPressed()
    {
        AudioManagerScript.instance.PlayRules();
    }

    [Command]
    public void CmdSetColor()
    {
        List<int> playerColors = new List<int>();

        int length = ColorScript.colors.Length;
        for (int i = 0; i < length; i++)
        {
            playerColors.Add(i);
        }

        List<CaptainsMessPlayer> players = GetPlayers();
        foreach (CaptainsMessPlayer player in players)
        {
            playerColors.Remove(player.GetComponent<NetworkedPlayerScript>().GetColor());
        }

        //RpcSetColor(playerColors[0]);   // Always get first. 
        RpcSetColor(playerColors[Random.Range(0, playerColors.Count)]);   //old random way 
    }

    [ClientRpc]
    void RpcSetColor(int c)
    {
        int oldColor = color;

        color = c;
        SetColor();

        string clr_name = ColorScript.GetColorName(c);

        if (isLocalPlayer)
        {
            Color clr = ColorScript.GetColor(c);
            GUIManagerScript.SetInputColor(clr, clr_name);
            clr = clr * 0.5f;
            GUIManagerScript.SetBGColor(clr);
        }

        if (nameText == ColorScript.GetColorName(oldColor) || nameText == "") OnNameTextChanged(clr_name);
    }

    [Command]
    public void CmdSetPlayerText(string t)
    {
        RpcSetPlayerText(t);
    }

    [ClientRpc]
    public void RpcSetPlayerText(string t)
    {
        nameText = t;
    }

    [ClientRpc]
    public void RpcAddScore(int value)
    {
        score += value;
        scoredThisRound += value;

        if (isLocalPlayer)
        {
            GUIManagerScript.SetScoreText(score);
        }
    }

    [Command]
    public void CmdSetMatchSongID(int song)
    {
        Assert.AreNotEqual<int>(-1, song, "No player was matched");

        RpcSetMatchSongID(song);
    }

    [ClientRpc]
    public void RpcSetMatchSongID(int song)
    {
        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();

        Assert.IsNotNull<GameManagerScript>(gameManager);

        matchSongID = song;
        matchTime = gameManager.countDown;

        if (songID == matchSongID)
        {
            scored_GuessedCorrect = true;
        }

        if (AreAllPlayersMatched())
        {
            //Every player is matched, end the game early.
            gameManager.CmdEndGame();
        }
    }

    [Command]
    public void CmdResetMatchSongID()
    {
        RpcResetMatchSongID();
    }
    [ClientRpc]
    public void RpcResetMatchSongID()
    {
        ResetMatch();
    }

    [Command]
    public void CmdReplayGame()
    {
        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();

        Assert.IsNotNull<GameManagerScript>(gameManager);

        gameManager.CmdReplayGame();

        List<CaptainsMessPlayer> players = GetPlayers();
        foreach (CaptainsMessPlayer player in players)
        {
            player.GetComponent<NetworkedPlayerScript>().RpcReplayGame();
        }
    }

    [ClientRpc]
    public void RpcReplayGame()
    {
        GUIManagerScript.DisableInput(false);

        List<CaptainsMessPlayer> players = GetPlayers();
        int size = players.Count;

        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
            if (!nps.isLocalPlayer)
            {
                nps.playerButton.SetActive(true);
            }
            nps.playerButton.GetComponent<Button>().interactable = false;
            
            Vector3 goal = player.GetComponent<RemotePlayerScript>().GetPosition();
            nps.playerButton.transform.DOLocalMove(goal, nps.fastMovementSpeed);
            nps.playerButton.transform.DOScale(Vector3.one, nps.fastMovementSpeed);
        }

        playerParent.GetComponent<RectTransform>().sizeDelta = new Vector2(160 * (size + 1), 340);

        SetReady(true); //Auto advance
    }

    [Command]
    public void CmdStartGame()
    {
        if (mess.AreAllPlayersReady()) //Redundant?
        {

            List<CaptainsMessPlayer> players = GetPlayers();

            int length = players.Count;

            Assert.IsTrue(length >= 4, "There must be >=4 players!");

            int numSongsToPick = (length / 2);

            List<int> songs = new List<int>(); //List of the songID's we'll use this game.

            int numberOfSongs = AudioManagerScript.instance.GetNumSongs();
            for (int i = 0; i < numSongsToPick; i++)
            {
                int rand;
                do
                {
                    rand = Random.Range(0, numberOfSongs);
                }
                while (songs.Contains(rand));
                songs.Add(rand);
            }

            List<int> playerSongChoice = new List<int>(); // Final list will pull from

            // Remember, final list will have at least 2 of every choice!
            int j = 0;
            if (length % 2 != 0) { j = -1; }
            for (int k = 0; k < length; k++)
            {
                if (j == -1)
                {
                    playerSongChoice.Add(songs[Random.Range(0, numSongsToPick)]);
                }
                else
                {
                    playerSongChoice.Add(songs[(int)(j / 2)]);
                }
                j++;
            }

            foreach (CaptainsMessPlayer player in players)
            {
                //Recycle local J variable, don't care about last value
                j = Random.Range(0, playerSongChoice.Count);

                NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
                //Tell the player which song they got
                nps.RpcStartGame(playerSongChoice[j]);

                //Remove that entry from list. 
                playerSongChoice.RemoveAt(j);
            }

        } //Close if statement for checking if all players ready
    }
    [ClientRpc]
    public void RpcStartGame(int s)
    {
        songID = s;
        scoredThisRound = 0;

        //playerParent.GetComponent<PlayerParentScript>().Unlock();

        // Bullshit code. Temp? Maybe not?
        // What if player WAS ready, but now that we're actually starting they are no longer?
        // Too late for them! Let's double check
        if (!ready)
        {
            Assert.IsFalse(ready, "Player wasn't ready, but the server thought they were!");
            // Oh noes! What do we do? Let's cheat:
            SetReady(true);
            // See buddy, you were ready the whole time, right?
        }
        ResetMatch();

        if (isLocalPlayer)
        {
            GUIManagerScript.FillPlayerText(nameText);

            GUIManagerScript.SetButton(false);

            GUIManagerScript.DisableInput(true);
            GUIManagerScript.SetBackButton(false);
            
            AudioManagerScript.instance.StartGameMusic();

            GUIManagerScript.SetColorShow(nameText, ColorScript.GetColor(color), ColorScript.GetColorName(color));

            localPScript.reminded = false;
        }

        List<CaptainsMessPlayer> players = GetPlayers();
        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
            if (!nps.isLocalPlayer)
            {
                player.GetComponent<NetworkedPlayerScript>().playerButton.SetActive(true);
                player.GetComponent<NetworkedPlayerScript>().playerButton.GetComponent<Button>().interactable = true;
            }
        }
    }

    [Command]
    public void CmdEndGame()
    {
        List<CaptainsMessPlayer> players = GetPlayers();
        NetworkedPlayerScript bonusPlayer = null;
        float longestMatchTime = -1;
        List<int> scoringSongs = new List<int>();
        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();

            if (nps.scored_GuessedCorrect)
            {
                float currentMatchTime = nps.matchTime;

                nps.RpcAddScore(5 * Mathf.FloorToInt(currentMatchTime));

                if (currentMatchTime > longestMatchTime)
                {
                    longestMatchTime = currentMatchTime;
                    bonusPlayer = nps;
                }

                scoringSongs.Add(nps.GetSongID());
            }
        }

        //Bonus for player who guessed first.
        if (bonusPlayer != null) //If this is null, nobody guessed anything. Lame!
        {
            bonusPlayer.RpcAddScore(100);
        }

        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();

            if (nps.scored_GuessedCorrect)
            {
                nps.RpcAddScore(250);
            }
            if (scoringSongs.Contains(nps.songID))
            {
                nps.RpcAddScore(500);
            }

            nps.RpcEndGame();
        }

        mess.FinishGame();
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        SetReady(false);

        if (isLocalPlayer)
        {
            GUIManagerScript.SetButton(true);

            GUIManagerScript.SetBackButton(false);

            AudioManagerScript.instance.EndGameMusic();

            AudioManagerScript.instance.PlayRoundEnd(scored_GuessedCorrect);

            GUIManagerScript.HideColorShow();
        }
    }
}
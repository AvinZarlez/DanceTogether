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

    private GameObject playerParent;

    [SyncVar]
    private int score;
    [SyncVar]
    private int scoredThisRound;

    [SyncVar]
    private bool scored_GuessedCorrect;
    [SyncVar]
    private bool scored_WasGuessed;

    [SyncVar]
    private int songID;

    [SyncVar]
    private int matchSongID; // The other player this player has picked as a match

    [SyncVar]
    public float matchTime;

    [SyncVar]
    private int color = -1;

    [SyncVar]
    private string nameText = "";

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
                    Vector3 start = goal;
                    if (goal.x > 0)
                        start.x = 200 + (Screen.width / 2);
                    else
                        start.x = -200 - (Screen.width / 2);
                    nps.playerButton.transform.localPosition = start;

                    nps.SetNameText();
                }

                nps.playerButton.transform.DOLocalMove(goal, movementSpeed);
            }
            nps.SetColor();
        }

        if (size >= 4)
        {
            GUIManagerScript.SetButton(true);
        }
        else
        {
            GUIManagerScript.SetButton(false);
        }
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

            GUIManagerScript.SetInput(true);
            GUIManagerScript.DisableInput(false);
            GUIManagerScript.SetBackButton(false);
        }
    }

    public void Update()
    {
        if (isServer)
            captainsCountdown = mess.CountdownTimer();


        if (ready)
            playerButtonOutline.enabled = true;
        else
            playerButtonOutline.enabled = false;
    }

    // Redundant? Possible. But I am not sure
    // Unity says this is the one to use, yet OnDestroy works and possibly works better?
    // Let's do both to be safe.
    void OnDestroy()
    {
        print("Player was was destroyed");
        Destroy(playerButton);

        if (isLocalPlayer)
        {
            GUIManagerScript.SetInput(false);
            GUIManagerScript.SetButton(false);
            GUIManagerScript.SetBackButton(false);
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
        if (nameText == "")
            nameText = ColorScript.GetColorName(color);

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

                    if (player.name == "LOCAL Player")
                    {
                        player.GetComponent<NetworkedPlayerScript>().CmdSetMatchSongID(GetSongID());
                    }
                    player.GetComponent<NetworkedPlayerScript>().playerButton.SetActive(false);
                    player.GetComponent<NetworkedPlayerScript>().playerButton.GetComponent<Button>().interactable = false;
                }

                playerButton.SetActive(true);
                playerButton.GetComponent<Button>().interactable = false;
                playerButton.transform.DOLocalMove(Vector3.zero, fastMovementSpeed);

                GUIManagerScript.SetBackButton(true);
            }
        }
    }

    [Command]
    void CmdSetColor()
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

        RpcSetColor(playerColors[0]);   //<- new, always get first way. Old random way: Random.Range(0, playerColors.Count)]);
    }

    [ClientRpc]
    void RpcSetColor(int c)
    {
        color = c;
        SetColor();

        if (isLocalPlayer)
        {
            Color clr = ColorScript.GetColor(c);
            GUIManagerScript.SetInputColor(clr);
            clr = clr * 0.5f;
            Camera.main.backgroundColor = clr;
        }
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
        SetNameText();
    }

    [Command]
    public void CmdAddScore(int value)
    {
        // Don't need this assert, because maybe it's ok if 0 is passed?
        // Example: When the guess is locked in right when the clock hits 0?
        //Assert.AreNotEqual<int>(0, value, "Score not modified - passed 0");
        RpcAddScore(value);
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

        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();

        Assert.IsNotNull<GameManagerScript>(gameManager);

        RpcSetMatchSongID(song, gameManager.countDown);
    }

    [ClientRpc]
    public void RpcSetMatchSongID(int song, float count)
    {
        matchSongID = song;
        matchTime = count;

        if (AreAllPlayersMatched())
        {
            GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();

            Assert.IsNotNull<GameManagerScript>(gameManager);

            //Every player is matched, end the game early.
            gameManager.CmdEndGame();
        }
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
        GUIManagerScript.SetReplayButton(false);

        GUIManagerScript.DisableInput(false);

        List<CaptainsMessPlayer> players = GetPlayers();
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
        }

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

                //Tell the player which song they got
                player.GetComponent<NetworkedPlayerScript>().RpcStartGame(playerSongChoice[j]);

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
        GUIManagerScript.SetButton(false);
        
        GUIManagerScript.DisableInput(true);
        GUIManagerScript.SetBackButton(false);


        AudioManagerScript.instance.StartGameMusic();

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
        CaptainsMessPlayer bonusPlayer = null;
        float longestMatchTime = -1;
        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
            nps.RpcEndGame();

            float currentMatchTime = nps.matchTime;
            if (currentMatchTime != -1)
            {
                int msid = nps.GetMatchSongID();
                if (msid != -1)
                {
                    if (msid == GetSongID())
                    {
                        nps.CmdAddScore(5 * Mathf.FloorToInt(currentMatchTime));
                        nps.scored_GuessedCorrect = true;

                        foreach (CaptainsMessPlayer sub_player in players)
                        {
                            if (!sub_player.Equals(player))
                            {
                                NetworkedPlayerScript sub_nps = player.GetComponent<NetworkedPlayerScript>();

                                if (sub_nps.GetSongID() == nps.GetMatchSongID())
                                {
                                    nps.scored_WasGuessed = true;
                                }
                            }
                        }

                    }

                    if (currentMatchTime > longestMatchTime)
                    {
                        longestMatchTime = currentMatchTime;
                        bonusPlayer = player;
                    }
                }

            }
        }

        //Bonus for player who guessed first.
        if (bonusPlayer != null) //If this is null, nobody guessed anything. Lame!
        {
            bonusPlayer.GetComponent<NetworkedPlayerScript>().CmdAddScore(100);
        }

        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();

            if (nps.scored_GuessedCorrect)
            {
                nps.CmdAddScore(250);
            }
            if (nps.scored_WasGuessed)
            {
                nps.CmdAddScore(500);
            }
        }
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        SetReady(false);

        GUIManagerScript.SetReplayButton(true);
        GUIManagerScript.SetBackButton(false);

        AudioManagerScript.instance.EndGameMusic();
    }

    [ClientRpc]
    public void RpcRotatePlayers(bool l)
    {
        if (l)
        {
            //playerParent.GetComponent<PlayerParentScript>().LockAndSpin();
            AudioManagerScript.instance.PrepareGameMusic();
        }
        else {
            //playerParent.GetComponent<PlayerParentScript>().Unlock();
        }
    }
}
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Analytics;
using System.Linq;

public class NetworkedPlayerScript : CaptainsMessPlayer
{
    private const string versionNum = "0.0.5";

    private enum Score
    {
        Time, First, WasGuessed, Correct
    };

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
    private int scored_ThisRound;

    [SyncVar]
    private int scored_TimeBonus;

    [SyncVar]
    private bool scored_FirstBonus;

    [SyncVar]
    private bool scored_WasGuessed;

    [SyncVar]
    public bool scored_GuessedCorrect;

    [SyncVar]
    private int songID;

    [SyncVar]
    private int matchSongID; // The other player this player has picked as a match
    
    [SyncVar]
    public int picked_color;
    [SyncVar]
    public int match_color;

    [SyncVar]
    public float matchTime;

    [SyncVar]
    private int color = -1;
    
    private bool to_sort = false;

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
            playerButton.GetComponentInChildren<Text>().text = color.ToString();
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

    public int DoesPlayerNumberExist(int s)
    {
        List<CaptainsMessPlayer> players = mess.Players();
        
        foreach (CaptainsMessPlayer player in players)
        {
            if (player.GetComponent<NetworkedPlayerScript>().GetColor() == s)
            {
                return player.GetComponent<NetworkedPlayerScript>().GetSongID();
            }
        }

        return -1;
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

        List<CaptainsMessPlayer> SortedList = players.OrderBy(o => o.GetComponent<NetworkedPlayerScript>().color).ToList();

        foreach (CaptainsMessPlayer player in SortedList)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();
            if (!nps.isLocalPlayer)
            {

                RemotePlayerScript rps = player.GetComponent<RemotePlayerScript>();

                Vector3 goal = rps.SetPosition(++i, size);

                if (nps.playerButton.activeSelf == false)
                {
                    nps.playerButton.SetActive(true);
                    nps.playerButton.transform.localPosition = new Vector3(240,i * -120, 0);
                }

                nps.playerButton.transform.DOLocalMove(goal, movementSpeed);
                nps.playerButton.transform.DOScale(Vector3.one, movementSpeed);
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

        playerParent.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 120 * (size));
    }

    void Start()
    {
        playerParent = GUIManagerScript.playerParent;
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
            
            GUIManagerScript.SetBackButton(false);
        }
    }

    public void Update()
    {
        if (isServer)
        {
            captainsCountdown = mess.CountdownTimer();
        }

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

        if (isLocalPlayer)
        {
            if (to_sort && !gameManager.IsGameStarted() )
            {
                to_sort = false;
                SortPlayers();
            }
        }
    }

    // Redundant? Possible. But I am not sure
    // Unity says this is the one to use, yet OnDestroy works and possibly works better?
    // Let's do both to be safe.
    new void OnDestroy()
    {
        //print("Player was was destroyed");
        if (playerButton != null)
            Destroy(playerButton);

        if (isLocalPlayer)
        {
            GUIManagerScript.SetPregameParent(false);
            GUIManagerScript.SetButton(false);
            GUIManagerScript.SetRulesButton(false);
            GUIManagerScript.SetBackButton(false);
            GUIManagerScript.HideClassicGameParent();
            if (GUIManagerScript.countdownText != null)
                GUIManagerScript.countdownText.enabled = false;

            if (AudioManagerScript.instance != null)
            {
                AudioManagerScript.instance.StopSFX();
                AudioManagerScript.instance.EndGameMusic();
                AudioManagerScript.instance.StartMenuMusic();
            }
        }

        // If this is a client player on the server then OnClientExitLobby will not be called.
        // Call it here instead.
        if (networkManager.IsHost() && networkManager.localPlayer != this)
        {
            OnClientExitLobby();
        }

        base.OnDestroy();
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
        scored_ThisRound = 0;
        scored_TimeBonus = 0;
        scored_FirstBonus = false;
        scored_WasGuessed = false;

        AudioManagerScript.instance.PlaySFX(AudioManagerScript.SFXClips.DanceTogether);

        GUIManagerScript.SetRulesButton(true);
        GUIManagerScript.SetPregameParent(true);

        SetReady(false);

        if (isServer)
        {
            GUIManagerScript.versionText.text = "v" + versionNum + " Host";
        }
        else
        {
            GUIManagerScript.versionText.text = "v" + versionNum + " Client";
        }


        base.OnStartLocalPlayer();
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();

        // Brief delay to let SyncVars propagate
        Invoke("SortPlayers", 0.5f);
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
        return scored_ThisRound;
    }

    public int GetTimeBonus()
    {
        return scored_TimeBonus;
    }

    public bool GetWasGuessed()
    {
        return scored_WasGuessed;
    }

    public bool GetFirstBonus()
    {
        return scored_FirstBonus;
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
        
        picked_color = -1;

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
                        nps.CmdSetMatchSongID(songID, color);
                    }
                    nps.playerButton.SetActive(false);
                    nps.playerButton.GetComponent<Button>().interactable = false;
                }

                playerButton.SetActive(true);
                playerButton.GetComponent<Button>().interactable = false;
                playerButton.transform.DOLocalMove(new Vector3(30, 20, 0), fastMovementSpeed);
                playerButton.transform.DOScale(new Vector3(1.5f, 1.5f, 1f), fastMovementSpeed);

                playerParent.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 140);

                GUIManagerScript.SetBackButton(true);
            }
        }
    }

    public void LockChoiceButtonPressed()
    {
        InputField numberTextField = GUIManagerScript.numberTextField;

        int input = -1;
        if (System.Int32.TryParse(numberTextField.text, out input))
        {
            int s = DoesPlayerNumberExist(input);
            if (s != -1)
            {
                CmdSetMatchSongID(s, input);
                GUIManagerScript.NumberInputLocked(true);
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
        color = c;
        to_sort = true;

        SetColor();

        if (isLocalPlayer)
        {
            string clr_name = ColorScript.GetColorName(c);

            Color clr = ColorScript.GetColor(c);
            GUIManagerScript.SetInputColor(clr, clr_name);
            clr = clr * 0.5f;
            GUIManagerScript.SetBGColor(clr);
            GUIManagerScript.FillPlayerNumber(c);
            GUIManagerScript.SetNumberInputFieldColor(clr);
            GUIManagerScript.ClearNumberInput();
        }
    }

    [ClientRpc]
    public void RpcAddScore(int value, int s)
    {
        score += value;
        scored_ThisRound += value;

        if (isLocalPlayer)
        {
            GUIManagerScript.SetScoreText(score);

            if (s == (int)(Score.First))
                scored_FirstBonus = true;
            if (s == (int)(Score.WasGuessed))
                scored_WasGuessed = true;
            if (s == (int)(Score.Time))
                scored_TimeBonus = value;
        }
    }

    public void ResetScore()
    {
        Debug.Log("NP ResetScore");
        CmdResetScore();
    }

    [Command]
    public void CmdResetScore()
    {
        Debug.Log("NP CmdResetScore");
        RpcResetScore();
    }

    [ClientRpc]
    public void RpcResetScore()
    {
        Debug.Log("NP RpsResetScore");
        score = 0;
        if (isLocalPlayer)
        {
            Debug.Log("NP RpsResetScore Local Player");
            GUIManagerScript.SetScoreText(score);
        }
    }

    [Command]
    public void CmdSetMatchSongID(int song, int color)
    {
        Assert.AreNotEqual<int>(-1, song, "No player was matched");

        RpcSetMatchSongID(song, color);
    }

    [ClientRpc]
    public void RpcSetMatchSongID(int song, int c)
    {
        GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();

        Assert.IsNotNull<GameManagerScript>(gameManager);

        matchSongID = song;
        matchTime = gameManager.countDown;
        
        picked_color = c;

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

            //Move there
            /*
            nps.playerButton.transform.DOLocalMove(goal, nps.fastMovementSpeed);
            nps.playerButton.transform.DOScale(Vector3.one, nps.fastMovementSpeed);
            */
            //Insta go there
            nps.playerButton.transform.localPosition = goal;
            nps.playerButton.transform.localScale = Vector3.one;
        }

        playerParent.GetComponent<RectTransform>().sizeDelta = new Vector2(170,120 * (size));

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

            //Hacky bullshit for finding which kind of game we're in?
            GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
            Assert.IsNotNull<GameManagerScript>(gameManager);
            int index = gameManager.GetSongType();

            int numberOfSongs = AudioManagerScript.instance.GetNumSongs(index);
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
        scored_ThisRound = 0;
        scored_TimeBonus = 0;
        scored_FirstBonus = false;
        scored_WasGuessed = false;

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
            GUIManagerScript.SetButton(false);

            GUIManagerScript.SetBackButton(false);

            Color clr = ColorScript.GetColor(GetColor());
            clr = clr * 0.5f;
            GUIManagerScript.SetNumberInputFieldColor(clr);
            GUIManagerScript.ClearNumberInput();

            //Hacky bullshit for finding which kind of game we're in?
            GameManagerScript gameManager = FindObjectOfType<GameManagerScript>();
            Assert.IsNotNull<GameManagerScript>(gameManager);
            int index = gameManager.GetSongType();

            AudioManagerScript.instance.StartGameMusic(index);

            GUIManagerScript.SetClassicGameParent(color, ColorScript.GetColor(color), ColorScript.GetColorName(color));

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

            int sid = nps.GetSongID();

            if (nps.scored_GuessedCorrect)
            {
                float currentMatchTime = nps.matchTime;

                nps.RpcAddScore(5 * Mathf.FloorToInt(currentMatchTime), (int)Score.Time);

                if (currentMatchTime > longestMatchTime)
                {
                    longestMatchTime = currentMatchTime;
                    bonusPlayer = nps;
                }

                scoringSongs.Add(sid);

                Analytics.CustomEvent("guessedCorrect", new Dictionary<string, object>
                  {
                    { "matchTime", currentMatchTime },
                    { "songID", sid }
                });
            }
            else
            {
                for (int i = 0; i < players.Count; i++)
                {
                    NetworkedPlayerScript other_nps = players[i].GetComponent<NetworkedPlayerScript>();

                    if (sid == other_nps.GetSongID())
                    {
                        if (nps.color != other_nps.color)
                        {
                            nps.match_color = other_nps.color;
                            break;
                        }
                    }
                }
            }
        }

        //Bonus for player who guessed first.
        if (bonusPlayer != null) //If this is null, nobody guessed anything. Lame!
        {
            bonusPlayer.RpcAddScore(100, (int)Score.First);
        }

        foreach (CaptainsMessPlayer player in players)
        {
            NetworkedPlayerScript nps = player.GetComponent<NetworkedPlayerScript>();

            if (nps.scored_GuessedCorrect)
            {
                nps.RpcAddScore(250, (int)Score.Correct);
            }
            if (scoringSongs.Contains(nps.songID))
            {
                nps.RpcAddScore(500, (int)Score.WasGuessed);
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

            GUIManagerScript.HideClassicGameParent();
        }
    }
}
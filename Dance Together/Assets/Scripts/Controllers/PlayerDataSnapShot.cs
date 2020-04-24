using App.Utility;
using App.Networking;

namespace App.Data
{
    /// <summary>
    /// A data struct that will store common player data for local server use.
    /// Using this allows us to store data of a player, incase they go offline.
    /// </summary>
    public class PlayerDataSnapShot
    { 
        private int playerID;
        private IndexedColor playerColor;
        private int songID;
        private bool isLocalPlayer;

        public int PlayerID
        {
            get { return playerID; }
            set { playerID = value; }
        }
        public IndexedColor PlayerColor
        {
            get { return playerColor; }
            set { playerColor = value; }
        }
        public int SongID
        {
            get { return songID; }
            set { songID = value; }
        }
        public bool IsLocalPlayer
        {
            get { return isLocalPlayer; }
        }

        public PlayerDataSnapShot()
        {
            playerID = -1;
            playerColor = new IndexedColor();
            songID = -1;
            isLocalPlayer = false;
        }
        /// <summary>
        /// Use for creating new SnapsShots
        /// </summary>
        /// <param name="player"></param>
        public PlayerDataSnapShot(DanceTogetherPlayer player)
        {
            playerID = player.PlayerID;
            playerColor = player.playerColor;
            songID = player.SongID;
            isLocalPlayer = player.isLocalPlayer;
        }

        /// <summary>
        /// Use for Updating existing SnapShots.
        /// </summary>
        /// <param name="player"></param>
        public void UpdateSS(DanceTogetherPlayer player)
        {
            playerID = player.PlayerID;
            playerColor = player.playerColor;
            songID = player.SongID;
            isLocalPlayer = player.isLocalPlayer;
        }
    }
}
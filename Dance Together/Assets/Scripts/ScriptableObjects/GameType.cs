using UnityEngine;

namespace App.Data
{
    #region Parameter Struct
    [System.Serializable]
    public struct GameData
    {
        // private VARs
        [SerializeField, TextArea]
        private string gameDescription;
        [SerializeField]
        private int minPlayers;
        [SerializeField]
        private int maxPlayers;
        [SerializeField]
        private float gameTime;
        [SerializeField]
        private int groupSize;

        // public Vars
        public string GameDescription
        {
            get { return gameDescription; }
        }
        public int MinPlayers
        {
            get { return minPlayers; }
        }
        public int MaxPlayers
        {
            get { return maxPlayers; }
        }
        public float GameTime
        {
            get { return gameTime; }
        }
        public int GroupSize
        {
            get { return groupSize; }
        }

        // Constructor
        public GameData(string _gameDescription = "", int _minPlayers = 0, int _maxPlayers = 0, float _gameTime = 0f, int _groupSize = 2)
        {
            gameDescription = _gameDescription;
            minPlayers = _minPlayers;
            maxPlayers = _maxPlayers;
            gameTime = _gameTime;
            groupSize = _groupSize;
        }
    }
    #endregion

    #region Scriptable Object
    [CreateAssetMenu(menuName = "DanceTogether/Data/GameType", fileName = "New Game Type")]
    public class GameType : ScriptableObject
    {
        // private VARS
        [SerializeField]
        private GameData data;

        // public VARS
        public GameData Data
        {
            get { return data; }
        }
    }
    #endregion
}

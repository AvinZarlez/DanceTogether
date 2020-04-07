using System.Collections.Generic;
using UnityEngine;

namespace App.Events
{
    #region GameEvent Data Class
    [System.Serializable]
    public class GameEventPayLoad
    {
        // Game States
        public enum States {
            NoStateChange,
            Initialize,
            MainMenu,
            StandBy,
            SearchingForGame,
            Lobby,
            GameInitialize,
            GameActive,
            GameEnded,
            GamePost
        };

        // Data Types
        public string DataString;
        public int DataInt;
        public float DataFloat;
        public bool DataBool;
        public States CurrentState;

        public GameEventPayLoad(string _dataString = "", int _dataInt = 0, float _dataFloat = 0f, bool _dataBool = false, States _gameState = States.Initialize)
        {
            DataString = _dataString;
            DataInt = _dataInt;
            DataFloat = _dataFloat;
            DataBool = _dataBool;
            CurrentState = _gameState;
        }

        public GameEventPayLoad(GameEventPayLoad _payLoad)
        {
            DataString = _payLoad.DataString;
            DataInt = _payLoad.DataInt;
            DataFloat = _payLoad.DataFloat;
            DataBool = _payLoad.DataBool;
            CurrentState = _payLoad.CurrentState;
        }

        public void Clear()
        {
            DataString = "";
            DataInt = 0;
            DataFloat = 0f;
            DataBool = false;
            CurrentState = States.NoStateChange;
        }
    }
    #endregion

    #region GameEvent
    [CreateAssetMenu(menuName = "DanceTogether/Game Event", fileName ="New Game Event")]
    public class GameEvent : ScriptableObject
    {
        private List<EventListener> eventListeners = new List<EventListener>();

        [TextArea]
        public string Description;
        public GameEventPayLoad Data;

        // Basic Raise
        public void Raise()
        {        
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised(this);
            }
        }

        #region Raise OverLoads
        // Data Change Raise Types for Ease of use.
        public void Raise(GameEventPayLoad _data)
        {
            Data = _data;

            Raise();
        }
        public void Raise(string _stringData, int _intData, float _floatData, bool _boolData, GameEventPayLoad.States _currentState)
        {
            Data.DataString = _stringData;
            Data.DataInt = _intData;
            Data.DataFloat = _floatData;
            Data.DataBool = _boolData;
            Data.CurrentState = _currentState;


            Raise();
        }
        public void Raise(string _stringData)
        {
            Data.DataString = _stringData;

            Raise();
        }
        public void Raise(int _intData)
        {
            Data.DataInt = _intData;

            Raise();
        }
        public void Raise(float _floatData)
        {
            Data.DataFloat = _floatData;

            Raise();
        }
        public void Raise(bool _boolData)
        {
            Data.DataBool = _boolData;

            Raise();
        }

        public void Raise(GameEventPayLoad.States _currentState)
        {
            Data.CurrentState = _currentState;

            Raise();
        }
        #endregion

        public void Register(EventListener passedEvent)
        {
            if (!eventListeners.Contains(passedEvent))
            {
                eventListeners.Add(passedEvent);
            }

        }

        public void DeRegister(EventListener passedEvent)
        {
            if (eventListeners.Contains(passedEvent))
            {
                eventListeners.Remove(passedEvent);
            }

        }
        

    }
    #endregion
}
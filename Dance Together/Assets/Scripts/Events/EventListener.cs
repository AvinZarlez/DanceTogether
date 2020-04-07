using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace App.Events
{
    public class EventListener : MonoBehaviour
    {
        public List<EventAndResponse> eventAndResponses = new List<EventAndResponse>();

        private void OnEnable()
        {

            if (eventAndResponses.Count >= 1)
            {
                foreach (EventAndResponse eAndR in eventAndResponses)
                {
                    eAndR.GameEvent.Register(this);
                }
            }


        }
        private void OnDisable()
        {
            if (eventAndResponses.Count >= 1)
            {
                foreach (EventAndResponse eAndR in eventAndResponses)
                {
                    eAndR.GameEvent?.DeRegister(this);
                }
            }
        }

        [ContextMenu("Raise Events")]
        public void OnEventRaised(GameEvent passedEvent)
        {

            for (int i = eventAndResponses.Count - 1; i >= 0; i--)
            {
                // Check if the passed event is the correct one
                if (passedEvent == eventAndResponses[i].GameEvent)
                {
                    //Debug.Log("Called Event named: " + eventAndResponses[i].name + " on GameObject: " + gameObject.name);
                    eventAndResponses[i]?.EventRaised();
                }
            }

        }
    }

    #region EventResponse Class
    [System.Serializable]
    public class EventAndResponse
    {
        public string name;
        public GameEvent GameEvent;
        public UnityEvent Response;
        public ResponseWithString ResponseForSentString;
        public ResponseWithInt ResponseForSentInt;
        public ResponseWithFloat ResponseForSentFloat;
        public ResponseWithBool ResponseForSentBool;
        public ResponseWithGameState ResponseForSentGameState;

        public bool HasEventAndResponse
        {
            get
            {
                if (GameEvent != null)
                {
                    if (Response != null || ResponseForSentString != null || ResponseForSentInt != null || ResponseForSentFloat != null || ResponseForSentBool != null)
                    {
                        return true;
                    }
                    return false;
                }
                else { return false; }
            }
        }

        public void EventRaised()
        {
            // default/generic
            Response?.Invoke();

            // string
            ResponseForSentString?.Invoke(GameEvent.Data.DataString);

            // int
            ResponseForSentInt?.Invoke(GameEvent.Data.DataInt);

            // float
            ResponseForSentFloat?.Invoke(GameEvent.Data.DataFloat);

            // bool
            ResponseForSentBool?.Invoke(GameEvent.Data.DataBool);

            // Custom GameState 
            ResponseForSentGameState?.Invoke(GameEvent.Data.CurrentState);
        }
    }
    #endregion

    #region Event Data Types
    [System.Serializable]
    public class ResponseWithString : UnityEvent<string> { }

    [System.Serializable]
    public class ResponseWithInt : UnityEvent<int> { }

    [System.Serializable]
    public class ResponseWithFloat : UnityEvent<float> { }

    [System.Serializable]
    public class ResponseWithBool : UnityEvent<bool> { }

    // Custom Var Responses.
    [System.Serializable]
    public class ResponseWithGameState : UnityEvent<GameEventPayLoad.States> { }
    #endregion
}
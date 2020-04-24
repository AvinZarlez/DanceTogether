using UnityEngine;

namespace App.Events
{
    public class EventRaiser : MonoBehaviour
    {
        public EventRaiseData[] eventsToRaise;

        // General purpose raise all.
        public void RaiseAllEvents()
        {
            foreach(EventRaiseData item in eventsToRaise)
            {
                item.Event?.Raise(item.Data);
            }
        }

        // Intended to be used with UnityEvents. Idealy will live on object that sends the events.
        public void RaiseSpecificEvent(int _eventArrayNumber)
        {
            if (_eventArrayNumber < 0 && _eventArrayNumber > eventsToRaise.Length - 1)
            {
                Debug.Log(this.name + " : has had a request to Raise an event that exists outside of the available range : range = " + (eventsToRaise.Length - 1) + " : value requested = " + _eventArrayNumber);
            }
            else
            {
                EventRaiseData foundEvent = eventsToRaise[_eventArrayNumber];
                foundEvent.Event?.Raise(foundEvent.Data);
            }
        }
    }

    [System.Serializable]
    public class EventRaiseData
    {
        [Header("New Data To send with Event")]
        public GameEventPayLoad Data;
        [Header("Event Reference to raise.")]
        public GameEvent Event;
    }
}
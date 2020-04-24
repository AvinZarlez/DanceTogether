using UnityEngine;

namespace App.Audio
{
    [System.Serializable]
    public struct AudioSoundGroup
    {
        // vars to be set in Inspector
        [SerializeField]
        private AudioClip correct;
        [SerializeField]
        private AudioClip wrong;
        [SerializeField]
        private AudioClip roundComplete;
        [SerializeField]
        private AudioClip newRound;
        [SerializeField]
        private AudioClip findPartner;
        [SerializeField]
        private AudioClip rules;
        [SerializeField]
        private AudioClip mainMenu;
        [SerializeField]
        private AudioClip countDown;

        public AudioClip Correct
        { 
            get { return correct; }
        }
        public AudioClip Wrong
        { 
            get { return wrong; }
        }
        public AudioClip RoundComplete
        {
            get { return roundComplete; }
        }
        public AudioClip FindPartner
        {
            get { return findPartner; }
        }
        public AudioClip Rules
        {
            get { return rules; }
        }
        public AudioClip MainMenue
        {
            get { return mainMenu; }
        }
        public AudioClip CountDown
        {
            get { return countDown; }
        }
    }
    [CreateAssetMenu(fileName = "New GameSounds", menuName = "DanceTogether/Audio/GameSounds")]
    public class GameSounds : ScriptableObject
    {
        [SerializeField]
        private AudioSoundGroup sounds;
        public AudioSoundGroup Sounds
        {
            get { return sounds; }
        }
    }
}

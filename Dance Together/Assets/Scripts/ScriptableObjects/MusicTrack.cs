using UnityEngine;

namespace App.Audio
{
    /// <summary>
    /// Use to associate information with audio in DanceTogether
    /// </summary>
    [CreateAssetMenu(menuName = "DanceTogether/Audio/MusicTrack", fileName = "New Music Track")]
    public class MusicTrack : ScriptableObject
    {
        [Header("Audio Track Information")]
        [SerializeField]
        private string trackName = "Track Name";
        [SerializeField, TextArea]
        private string description = "A brief description of the Audio Track";
        [SerializeField]
        private AudioClip audio;
        [SerializeField]
        private SongGenre genre;

        public string TrackName
        {
            get { return trackName; }
        }
        public string Description
        {
            get { return description; }
        }
        public AudioClip Audio
        {
            get { return audio; }
        }
        public SongGenre Genre
        {
            get { return genre; }
        }
    }
}
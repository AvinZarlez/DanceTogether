using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Audio
{
    /// <summary>
    /// A list that is meant to be used to group MusicTracks together, primarily by genre.
    /// </summary>
    [CreateAssetMenu(menuName = "DanceTogether/Audio/TrackList", fileName = "New TrackList")]
    public class TrackList : ScriptableObject
    {
        [SerializeField, TextArea]
        private string description = "A brief description of the track list.";
        [SerializeField]
        private SongGenre genre;
        [SerializeField]
        private List<MusicTrack> musicTrackList;

        public string Description
        {
            get { return description; }
        }
        public SongGenre Genre
        {
            get { return genre; }
        }
        public List<MusicTrack> MusicTrackList
        {
            get { return musicTrackList; }
        }
    }
}

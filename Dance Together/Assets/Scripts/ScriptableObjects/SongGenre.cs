using UnityEngine;

namespace App.Audio
{
    /// <summary>
    /// Used as a type of enum to catagorize music Genre.
    /// </summary>
    [CreateAssetMenu(menuName = "DanceTogether/Audio/Genre", fileName = "New Audio Genre")]
    public class SongGenre : ScriptableObject
    {
        [SerializeField, TextArea]
        private string description = "A brief description of the genre";

        public string Description
        {
            get { return description; }
        }
    }
}
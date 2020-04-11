using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using App.Utility;

namespace App.Audio
{
    public class DanceTogetherAudioManager : Singleton<DanceTogetherAudioManager>
    {
        //private MainController controller;

        [Header("Audio Mixer References")]
        public AudioMixerSnapshot inMenu;
        public AudioMixerSnapshot inGamePost;
        public AudioMixerSnapshot inGamePlay;

        [Header("Audio Source References")]
        public AudioSource menuSource;
        public AudioSource gamePlaySource;
        public AudioSource sfxSource;

        [Header("Sounds Reference")]
        public GameSounds GameSounds;

        [Header("Game Player Vars for Viewing")]
        [SerializeField]
        private MusicTrack currentMusicTrack;

        private float m_TransitionIn;
        private float m_TransitionOut;

        #region Monobehaviour Methods
        protected override void Awake()
        {
            float quarterNote = 2f;
            m_TransitionIn = quarterNote;
            m_TransitionOut = quarterNote * 4;

            base.Awake();
        }
        #endregion



        #region Public Methods
        /// <summary>
        /// Init this from master Game controller
        /// </summary>
        /// 
        /*
        public void Init(MainController _controller)
        {
            controller = _controller;

            if(controller == null)
            {
                Debug.Log("Controller is null, and audio manager is not properly set up.");
                return;
            }
        }
        */
        public void BeginMenuMusic()
        {
            if (menuSource.clip == null)
            {
                Debug.LogWarning("Menu music called, but doesnt have a clip set.");
                return;
            }

            if (!menuSource.isPlaying)
                menuSource.Play();

            StopAllCoroutines();
            inMenu.TransitionTo(m_TransitionIn);
            StartCoroutine(DelayStopAudio(gamePlaySource, m_TransitionIn));
        }
        public void BeginLobbyMusic()
        {
            if(!menuSource.isPlaying)
                menuSource.Play();

            StopAllCoroutines();
            inGamePost.TransitionTo(m_TransitionIn);
            StartCoroutine(DelayStopAudio(gamePlaySource, m_TransitionIn));

        }
        public void BeginGameMusic(MusicTrack _track, float _delayedPlay = 0f)
        {
            if(_track.Audio == null)
            {
                Debug.LogWarning("Game play music called, but doesnt have a clip set.");
                return;
            }

            StopAllCoroutines();
            gamePlaySource.Stop();
            gamePlaySource.clip = _track.Audio;
            gamePlaySource.PlayDelayed(_delayedPlay);
            inGamePlay.TransitionTo(m_TransitionIn);
            StartCoroutine(DelayStopAudio(menuSource, m_TransitionIn));
        }

        public void RoundBegin()
        {
            inGamePlay.TransitionTo(3f);
        }

        public void EndGameMusic()
        {
            if (!menuSource.isPlaying)
                menuSource.Play();

            StopAllCoroutines();
            inGamePost.TransitionTo(m_TransitionOut);
            // stop aduio source to safe processor.
            StartCoroutine(DelayStopAudio(gamePlaySource, m_TransitionOut));
        }

        public void Reset()
        {
            BeginMenuMusic();
        }

        /// <summary>
        /// play Correct audio
        /// </summary>
        public void PlayCorrectSFX()
        {
            sfxSource.PlayOneShot(GameSounds.Sounds.Correct);
        }
        /// <summary>
        /// play Wrong audio
        /// </summary>
        public void PlayWrongSFX()
        {
            sfxSource.PlayOneShot(GameSounds.Sounds.Wrong);
        }
        /// <summary>
        /// play Countdown audio
        /// </summary>
        public void PlayCountdownSFX()
        {
            sfxSource.PlayOneShot(GameSounds.Sounds.CountDown);
        }

        /// <summary>
        /// play Rules audio
        /// </summary>
        public void PlayRulesSFX()
        {
            sfxSource.PlayOneShot(GameSounds.Sounds.Rules);
        }
        /// <summary>
        /// play Find Partner audio
        /// </summary>
        public void PlayFindSFX()
        {
            sfxSource.PlayOneShot(GameSounds.Sounds.FindPartner);
        }
        /// <summary>
        /// stop SFX audio.
        /// </summary>
        public void StopSFX()
        {
            sfxSource.Stop();
        }
        #endregion

        #region IEnumerators
        private IEnumerator DelayStopAudio(AudioSource _source, float _delayTime)
        {
            yield return new WaitForSeconds(_delayTime);
            _source.Stop();
        }
        #endregion
    }
}
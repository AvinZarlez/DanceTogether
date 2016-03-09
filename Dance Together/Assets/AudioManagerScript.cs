using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManagerScript : MonoBehaviour {
    
    public AudioMixerSnapshot inMenu;
    public AudioMixerSnapshot inGameStarted;
    public AudioMixerSnapshot inGameplay;
    
    public AudioSource gameplaySource;
    public float bpm = 120;

    private float m_TransitionIn;
    private float m_TransitionOut;
    private float m_QuarterNote;

    [HideInInspector]
    static public AudioManagerScript instance = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Use this for initialization
    void Start () {

        m_QuarterNote = 60 / bpm;
        m_TransitionIn = m_QuarterNote;
        m_TransitionOut = m_QuarterNote * 16;

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StartMenuMusic()
    {
        inMenu.TransitionTo(m_TransitionIn);
    }

    public void StartGame()
    {
        inGameStarted.TransitionTo(m_TransitionOut);
    }

    public void StartGameMusic()
    {
        gameplaySource.Stop();
        gameplaySource.Play();
        inGameplay.TransitionTo(m_TransitionIn);
    }
}

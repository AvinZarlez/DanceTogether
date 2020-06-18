using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(ParticleSystem))]
public class AudioParticleController : MonoBehaviour
{
    public AudioSource audioSourceOutput;

    ParticleSystem psEmitter;

    public void Awake()
    {
        psEmitter = GetComponent<ParticleSystem>();
    }

    public void Update()
    {
        var emission = psEmitter.emission;

        if (!audioSourceOutput.isPlaying || !psEmitter.isEmitting)
        {
            // check idle value so we dont spend cpu re-applying color info.
            emission.rateOverTime = 0;
            // return while audio is not playing.
            return;
        }
        float[] spectrum = new float[64];
        audioSourceOutput.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        emission.rateOverTime = spectrum.Max() * 200;

        var forceLifetime = psEmitter.forceOverLifetime;
        forceLifetime.y = spectrum.Max() * 10000;

        var main = psEmitter.main; // mark main as a var, as this is the only way to save back data.
        audioSourceOutput.outputAudioMixerGroup.audioMixer.GetFloat("GameMusicVolume", out float volOut); // get actual dB volume from mixer group.
        main.startSize = Mathf.InverseLerp(-80f, 0f, volOut) * 250f; // convert dB into a 0-1 range and multiply by size factor. 
    }
}

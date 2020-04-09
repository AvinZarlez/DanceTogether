using System.Linq;
using UnityEngine;

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

        var test = psEmitter.forceOverLifetime;
        test.y = spectrum.Max() * 10000;
    }
}

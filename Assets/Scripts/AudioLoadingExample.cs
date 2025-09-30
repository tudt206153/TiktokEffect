using UnityEngine;

public class AudioLoadingExample : MonoBehaviour
{
    public AudioSource audioSource;
    public string soundName;

    void Start()
    {
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.PreloadAudioClips();
        }
    }
    public void PlaySound()
    {
        if (SoundEffectManager.Instance != null && audioSource != null)
        {
            SoundEffectManager.Instance.PlaySound(soundName, audioSource);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SoundEffectManager.Instance.PreloadAudioClips();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SoundEffectManager.Instance.ClearAudioCache();
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class AudioLoadingExample : MonoBehaviour
{
    public AudioSource audioSource;
    public string soundName;
    public string clipPath;
    public Sprite favoriteIcon;
    public Sprite defaultIcon;
    public Image iconImage;

    void Start()
    {
        // if (SoundEffectManager.Instance != null)
        // {
        //     SoundEffectManager.Instance.PreloadAudioClips();
        // }
    }
    
    public void PlaySound()
    {
        if (SoundEffectManager.Instance != null && audioSource != null)
        {
            audioSource.Stop();
            SoundEffectManager.Instance.PlaySound(soundName, audioSource);
        }
    }
    public void AddFavoriteSound()
    {
        if (SoundEffectManager.Instance != null)
        {
            iconImage.sprite = favoriteIcon;
            SoundEffectManager.Instance.AddSoundToFavorites(soundName, clipPath);
            PlayerPrefs.SetInt("FavoriteSoundName" + soundName + clipPath, 1);
        }
    }
    public void RemoveFavoriteSound()
    {
        if (SoundEffectManager.Instance != null)
        {
            Destroy(gameObject);
            iconImage.sprite = defaultIcon;
            SoundEffectManager.Instance.RemoveFavoriteSound(soundName, clipPath);
            PlayerPrefs.SetInt("FavoriteSoundName" + soundName + clipPath, 0);
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
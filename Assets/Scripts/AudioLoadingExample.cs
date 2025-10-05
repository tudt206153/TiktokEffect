using UnityEngine;
using UnityEngine.UI;

public class AudioLoadingExample : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public string soundName;
    public string clipPath;
    
    [Header("UI Elements")]
    public Sprite favoriteIcon;
    public Sprite defaultIcon;
    public Image iconImage;
    
    // Cache reference to avoid repeated null checks
    private SoundEffectManager soundManager;
    private bool isFavorite;

    void Start()
    {
        // Cache the manager reference for better performance
        soundManager = SoundEffectManager.Instance;
        
        // Load favorite state from PlayerPrefs
        isFavorite = PlayerPrefs.GetInt("FavoriteSoundName" + soundName + clipPath, 0) == 1;
        UpdateFavoriteIcon();
    }
    
    private void UpdateFavoriteIcon()
    {
        if (iconImage != null)
        {
            iconImage.sprite = isFavorite ? favoriteIcon : defaultIcon;
        }
    }
    
    public void PlaySound()
    {
        if (soundManager != null && audioSource != null)
        {
            // Stop current audio to prevent memory buildup from overlapping sounds

            audioSource.Stop();
            soundManager.PlaySound(soundName, audioSource);
        }
    }
    
    public void ToggleFavoriteSound()
    {
        if (soundManager != null)
        {
            if (isFavorite)
            {
                RemoveFavoriteSound();
            }
            else
            {
                AddFavoriteSound();
            }
        }
    }
    
    public void AddFavoriteSound()
    {
        if (soundManager != null && !isFavorite)
        {
            isFavorite = true;
            UpdateFavoriteIcon();
            soundManager.AddSoundToFavorites(soundName, clipPath);
            PlayerPrefs.SetInt("FavoriteSoundName" + soundName + clipPath, 1);
            PlayerPrefs.Save(); // Ensure data is saved
        }
    }
    
    public void RemoveFavoriteSound()
    {
        if (soundManager != null && isFavorite)
        {
            isFavorite = false;
            UpdateFavoriteIcon();
            soundManager.RemoveFavoriteSound(soundName, clipPath);
            PlayerPrefs.SetInt("FavoriteSoundName" + soundName + clipPath, 0);
            PlayerPrefs.Save(); // Ensure data is saved
            
            // Only destroy if this is in the favorites tab
            if (soundManager.selectedAudioStyle == AudioStyle.Favorite)
            {
                Destroy(gameObject);
            }
        }
    }
    
    void Update()
    {
        // Cache null check to avoid repeated lookups
        if (soundManager == null)
        {
            soundManager = SoundEffectManager.Instance;
        }
        
        // Update volume only if manager exists and volume has changed
        if (soundManager != null && audioSource != null)
        {
            if (Mathf.Abs(audioSource.volume - soundManager.defaultVolume) > 0.001f)
            {
                audioSource.volume = soundManager.defaultVolume;
            }
        }
        
        // Debug keys (only check in debug builds to reduce overhead)
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.P))
        {
            soundManager?.PreloadAudioClips();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            soundManager?.ClearAudioCache();
        }
        #endif
    }
    
    void OnDestroy()
    {
        // Stop audio source to free up resources
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}
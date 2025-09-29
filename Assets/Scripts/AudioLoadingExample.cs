using UnityEngine;

public class AudioLoadingExample : MonoBehaviour
{
    public SoundEffectManager soundManager;
    
    void Start()
    {
        // Example of different ways to specify audio file paths
        
        // 1. Relative path from StreamingAssets folder
        // Place your audio files in Assets/StreamingAssets/Audio/
        // Then use paths like: "Audio/mySound.wav"
        
        // 2. Absolute path
        // Example: "C:/MyProject/Sounds/background.mp3"
        
        // 3. Relative path from project root
        // Example: "SoundEffect/NHAC/song.wav"
        
        // Preload all audio clips for better performance
        if (soundManager != null)
        {
            soundManager.PreloadAudioClips();
        }
    }
    
    void Update()
    {
        // Example usage
        if (Input.GetKeyDown(KeyCode.Space))
        {
            soundManager.PlaySound(soundManager.soundData.soundList[0].soundName);
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            soundManager.PreloadAudioClips();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            soundManager.ClearAudioCache();
        }
    }
}
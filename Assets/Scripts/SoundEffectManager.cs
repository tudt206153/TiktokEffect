using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class SoundEffectManager : MonoBehaviour
{
    public SoundData soundData;
    public AudioSource audioSource;
    
    // Cache for loaded audio clips to avoid reloading
    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
    
    // Start is called before the first frame update
    void Start()
    {
        PlaySound("Sound 1");
    }

    public void PlaySound(string soundName)
    {
        SoundVariable sound = soundData.soundList.Find(s => s.soundName == soundName);
        if (sound != null)
        {
            StartCoroutine(LoadAndPlayAudio(sound.clipPath));
        }
        else
        {
            Debug.LogWarning($"Sound not found: {soundName}");
        }
    }
    
    public void PlaySoundSync(string soundName)
    {
        SoundVariable sound = soundData.soundList.Find(s => s.soundName == soundName);
        if (sound != null)
        {
            // Check if already cached
            if (audioClipCache.ContainsKey(sound.clipPath))
            {
                audioSource.PlayOneShot(audioClipCache[sound.clipPath]);
                return;
            }
            
            // For synchronous loading, try to load from StreamingAssets
            string streamingPath = Path.Combine(Application.streamingAssetsPath, sound.clipPath);
            if (File.Exists(streamingPath))
            {
                StartCoroutine(LoadAndPlayAudio(streamingPath));
            }
            else
            {
                Debug.LogWarning($"Audio file not found at path: {streamingPath}");
            }
        }
        else
        {
            Debug.LogWarning($"Sound not found: {soundName}");
        }
    }
    
    private IEnumerator LoadAndPlayAudio(string filePath)
    {
        // Check cache first
        if (audioClipCache.ContainsKey(filePath))
        {
            audioSource.PlayOneShot(audioClipCache[filePath]);
            yield break;
        }
        
        string fullPath = filePath;
        
        // If path doesn't start with file://, http://, or https://, assume it's a local file
        if (!filePath.StartsWith("file://") && !filePath.StartsWith("http://") && !filePath.StartsWith("https://"))
        {
            // Check if it's an absolute path
            if (!Path.IsPathRooted(filePath))
            {
                // If relative path, try StreamingAssets first, then relative to project
                string streamingPath = Path.Combine(Application.streamingAssetsPath, filePath);
                if (File.Exists(streamingPath))
                {
                    fullPath = "file://" + streamingPath;
                }
                else
                {
                    // Try relative to project root
                    string projectPath = Path.Combine(Application.dataPath, "..", filePath);
                    if (File.Exists(projectPath))
                    {
                        fullPath = "file://" + Path.GetFullPath(projectPath);
                    }
                    else
                    {
                        Debug.LogWarning($"Audio file not found at path: {filePath}");
                        yield break;
                    }
                }
            }
            else
            {
                // Absolute path
                if (File.Exists(filePath))
                {
                    fullPath = "file://" + filePath;
                }
                else
                {
                    Debug.LogWarning($"Audio file not found at path: {filePath}");
                    yield break;
                }
            }
        }
        
        // Determine audio type based on file extension
        AudioType audioType = GetAudioType(fullPath);
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load audio clip: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    // Cache the clip
                    audioClipCache[filePath] = clip;
                    audioSource.PlayOneShot(clip);
                }
                else
                {
                    Debug.LogWarning($"Failed to create AudioClip from file: {fullPath}");
                }
            }
        }
    }
    
    private AudioType GetAudioType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        switch (extension)
        {
            case ".mp3":
                return AudioType.MPEG;
            case ".wav":
                return AudioType.WAV;
            case ".ogg":
                return AudioType.OGGVORBIS;
            case ".aiff":
            case ".aif":
                return AudioType.AIFF;
            default:
                Debug.LogWarning($"Unsupported audio format: {extension}. Defaulting to WAV.");
                return AudioType.WAV;
        }
    }
    
    // Method to preload audio clips for better performance
    public void PreloadAudioClips()
    {
        foreach (var sound in soundData.soundList)
        {
            if (!audioClipCache.ContainsKey(sound.clipPath))
            {
                StartCoroutine(PreloadAudio(sound.clipPath));
            }
        }
    }
    
    private IEnumerator PreloadAudio(string filePath)
    {
        yield return StartCoroutine(LoadAudioClip(filePath));
    }
    
    private IEnumerator LoadAudioClip(string filePath)
    {
        if (audioClipCache.ContainsKey(filePath))
        {
            yield break;
        }
        
        string fullPath = filePath;
        
        if (!filePath.StartsWith("file://") && !filePath.StartsWith("http://") && !filePath.StartsWith("https://"))
        {
            if (!Path.IsPathRooted(filePath))
            {
                string streamingPath = Path.Combine(Application.streamingAssetsPath, filePath);
                if (File.Exists(streamingPath))
                {
                    fullPath = "file://" + streamingPath;
                }
                else
                {
                    string projectPath = Path.Combine(Application.dataPath, "..", filePath);
                    if (File.Exists(projectPath))
                    {
                        fullPath = "file://" + Path.GetFullPath(projectPath);
                    }
                    else
                    {
                        Debug.LogWarning($"Audio file not found at path: {filePath}");
                        yield break;
                    }
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    fullPath = "file://" + filePath;
                }
                else
                {
                    Debug.LogWarning($"Audio file not found at path: {filePath}");
                    yield break;
                }
            }
        }
        
        AudioType audioType = GetAudioType(fullPath);
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load audio clip: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    audioClipCache[filePath] = clip;
                }
            }
        }
    }
    
    // Clear cache to free memory
    public void ClearAudioCache()
    {
        foreach (var clip in audioClipCache.Values)
        {
            if (clip != null)
            {
                DestroyImmediate(clip);
            }
        }
        audioClipCache.Clear();
    }
    
    void OnDestroy()
    {
        ClearAudioCache();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

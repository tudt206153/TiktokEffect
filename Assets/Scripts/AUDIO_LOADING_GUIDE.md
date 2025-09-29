# Audio Loading from File Paths

This updated SoundEffectManager allows you to load audio files from file paths instead of Unity's Resources system.

## Supported Audio Formats
- MP3 (.mp3)
- WAV (.wav)
- OGG Vorbis (.ogg)
- AIFF (.aiff, .aif)

## How to Use

### 1. Setting Up Audio Files

#### Option A: StreamingAssets Folder (Recommended)
1. Place your audio files in `Assets/StreamingAssets/Audio/`
2. In your SoundData ScriptableObject, set the clipPath to relative paths like:
   - "Audio/mysound.wav"
   - "Audio/Background/music.mp3"

#### Option B: Project Folder
1. Place audio files anywhere in your project (e.g., in the existing `SoundEffect/` folder)
2. Set clipPath to relative paths like:
   - "SoundEffect/NHAC/song.wav"
   - "SoundEffect/HIEU UNG/effect.mp3"

#### Option C: Absolute Paths
1. Use complete file paths:
   - "C:/MyProject/Audio/sound.wav"
   - "D:/Music/background.mp3"

### 2. Methods Available

#### PlaySound(string soundName)
- Loads and plays audio asynchronously
- Uses caching for better performance
- Recommended for most use cases

#### PlaySoundSync(string soundName)
- For synchronous loading (legacy support)
- Less efficient than PlaySound

#### PreloadAudioClips()
- Loads all audio clips into memory cache
- Call this at game start for better performance
- Recommended for frequently used sounds

#### ClearAudioCache()
- Clears all cached audio clips to free memory
- Call when changing levels or when memory is needed

### 3. Performance Tips

1. **Preload frequently used sounds**: Call `PreloadAudioClips()` at the start of your scene
2. **Use StreamingAssets**: Files in StreamingAssets are included in builds and easily accessible
3. **Clear cache when needed**: Use `ClearAudioCache()` to manage memory usage
4. **Supported formats**: Use compressed formats like MP3 or OGG for background music, WAV for short sound effects

### 4. Example Usage

```csharp
public class GameManager : MonoBehaviour
{
    public SoundEffectManager soundManager;
    
    void Start()
    {
        // Preload all sounds for better performance
        soundManager.PreloadAudioClips();
    }
    
    void OnLevelComplete()
    {
        soundManager.PlaySound("Victory Sound");
    }
    
    void OnGameExit()
    {
        // Clean up memory
        soundManager.ClearAudioCache();
    }
}
```

### 5. Migration from Resources

If you were previously using Resources.Load, you need to:

1. Move your audio files from `Assets/Resources/` to `Assets/StreamingAssets/Audio/`
2. Update the clipPath in your SoundData to reflect the new location
3. Remove the "Resources/" prefix from your paths

Example:
- Old path: "Sounds/Background/music"
- New path: "Audio/Sounds/Background/music.mp3"

### 6. File Path Examples

```
StreamingAssets Structure:
Assets/StreamingAssets/
├── Audio/
│   ├── Music/
│   │   ├── background.mp3
│   │   └── menu.wav
│   └── SFX/
│       ├── jump.wav
│       └── explosion.ogg

SoundData clipPath values:
- "Audio/Music/background.mp3"
- "Audio/Music/menu.wav"
- "Audio/SFX/jump.wav"
- "Audio/SFX/explosion.ogg"
```

This system provides better flexibility and doesn't require audio files to be in the Resources folder, making your project structure more organized and build sizes potentially smaller.
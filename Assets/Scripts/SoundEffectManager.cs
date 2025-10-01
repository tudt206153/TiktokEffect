using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
using System;
using System.Runtime.InteropServices;
[System.Serializable]
public class SoundVariable
{
    public string soundName;
    public string clipPath;
}

[System.Serializable]
public class SoundDataCollection
{
    public List<SoundDataWrapper> soundDataList = new List<SoundDataWrapper>();

    public SoundDataCollection()
    {
        // Initialize with 8 empty sound data wrappers for each AudioStyle
        for (int i = 0; i < 8; i++)
        {
            soundDataList.Add(new SoundDataWrapper());
        }
    }
}

[System.Serializable]
public class SoundDataWrapper
{
    public List<SoundVariable> soundList = new List<SoundVariable>();
}
public enum AudioStyle
{

    Other,
    Laugh,
    Kiss,
    Song_1,
    Song_2,
    PK1,
    PK2,
    Favorite
}
public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;
    public string jsonPath = "TiktokEffect/JsonData";
    // Windows API declarations for always on top functionality
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;

    private bool wasAlwaysOnTop = false;
    public Sprite favoriteIcon;
    public Sprite defaultIcon;

    void Awake()
    {
        Instance = this;
        wasAlwaysOnTop = alwaysOnTop;
        LoadSoundDataFromJson();

    }

    #region JSON_SAVE_LOAD
    private string GetSoundDataFilePath()
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string fullPath = Path.Combine(desktopPath, jsonPath);

        // Create directory if it doesn't exist
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return Path.Combine(fullPath, soundDataFileName);
    }

    private void SaveSoundDataToJson()
    {
        try
        {
            string jsonString = JsonUtility.ToJson(soundDataCollection, true);
            File.WriteAllText(GetSoundDataFilePath(), jsonString);
            Debug.Log($"Sound data saved to: {GetSoundDataFilePath()}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save sound data: {ex.Message}");
        }
    }

    private void LoadSoundDataFromJson()
    {
        string filePath = GetSoundDataFilePath();

        if (File.Exists(filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                soundDataCollection = JsonUtility.FromJson<SoundDataCollection>(jsonString);
                Debug.Log("Sound data loaded from JSON file");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load sound data: {ex.Message}");
                CreateDefaultSoundData();
            }
        }
        else
        {
            Debug.Log("No existing sound data file found, creating default data");
            CreateDefaultSoundData();
        }
    }

    private void CreateDefaultSoundData()
    {
        soundDataCollection = new SoundDataCollection();
        SaveSoundDataToJson();
    }

    // Public method to manually save sound data
    public void SaveData()
    {
        SaveSoundDataToJson();
    }

    // Method to clear all sound data (useful for reset functionality)
    public void ClearAllSoundData()
    {
        soundDataCollection = new SoundDataCollection();
        SaveSoundDataToJson();
        Debug.Log("All sound data cleared");
    }

    // Test method to verify path resolution
    public void TestPathResolution(string testPath)
    {
        Debug.Log($"=== Testing Path Resolution for: {testPath} ===");
        Debug.Log($"StreamingAssets path: {Application.streamingAssetsPath}");
        Debug.Log($"Data path: {Application.dataPath}");
        Debug.Log($"Persistent data path: {Application.persistentDataPath}");

        string streamingPath = Path.Combine(Application.streamingAssetsPath, testPath);
        Debug.Log($"StreamingAssets test: {streamingPath} - Exists: {File.Exists(streamingPath)}");

        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string desktopFilePath = Path.Combine(desktopPath, testPath);
        Debug.Log($"Desktop test: {desktopFilePath} - Exists: {File.Exists(desktopFilePath)}");

        string executablePath = Path.GetDirectoryName(Application.dataPath);
        string executableFilePath = Path.Combine(executablePath, testPath);
        Debug.Log($"Executable dir test: {executableFilePath} - Exists: {File.Exists(executableFilePath)}");
    }

    // Method to remove a sound from a specific AudioStyle
    public bool RemoveSound(AudioStyle audioStyle, string soundName)
    {
        int styleIndex = (int)audioStyle;
        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {audioStyle} is not assigned.");
            return false;
        }

        SoundVariable soundToRemove = soundDataCollection.soundDataList[styleIndex].soundList.Find(s => s.soundName == soundName);
        if (soundToRemove != null)
        {
            soundDataCollection.soundDataList[styleIndex].soundList.Remove(soundToRemove);
            SaveSoundDataToJson();
            Debug.Log($"Removed sound: {soundName} from {audioStyle} category");
            return true;
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in {audioStyle} category.");
            return false;
        }
    }
    #endregion
    public Toggle alwaysOnTopToggle;
    public bool alwaysOnTop = true;
    [Header("Volume Settings")]
    public float defaultVolume = 1.0f;
    public Slider volumeSlider;
    [Header("Sound Data")]
    private SoundDataCollection soundDataCollection;
    private readonly string soundDataFileName = "soundData.json";
    public List<GameObject> tabList = new List<GameObject>();
    #region ADD_NEW_SOUND
    [Header("Add New Sound Here")]
    public GameObject addSoundTab;
    public TMP_InputField soundNameInput;
    public TMP_InputField clipPathInput;

    public TMP_Dropdown audioStyleDropdown;
    public AudioStyle selectedAudioStyle;

    public void AddNewSound()
    {
        AudioStyle selectedStyle = (AudioStyle)audioStyleDropdown.value;
        int styleIndex = (int)selectedStyle;

        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {selectedStyle} is not assigned.");
            return;
        }
        selectedAudioStyle = selectedStyle;
        string newSoundName = soundNameInput.text.Trim();
        string newClipPath = clipPathInput.text.Trim();

        if (string.IsNullOrEmpty(newSoundName) || string.IsNullOrEmpty(newClipPath))
        {
            Debug.LogWarning("Sound Name and Clip Path cannot be empty.");
            return;
        }

        // Check for duplicates in the selected style's sound data
        if (soundDataCollection.soundDataList[styleIndex].soundList.Exists(s => s.soundName == newSoundName))
        {
            Debug.LogWarning($"A sound with the name '{newSoundName}' already exists in {selectedStyle} category.");
            return;
        }

        SoundVariable newSound = new SoundVariable
        {
            soundName = newSoundName,
            clipPath = newClipPath
        };

        soundDataCollection.soundDataList[styleIndex].soundList.Add(newSound);
        Debug.Log($"Added new sound: {newSoundName} with path: {newClipPath} to style: {selectedStyle}");

        // Save to JSON file
        SaveSoundDataToJson();

        // Clear input fields after adding
        soundNameInput.text = "";
        clipPathInput.text = "";
        audioStyleDropdown.value = 0; // Reset to first option
    }
    public void GetClipPath()
    {
#if UNITY_EDITOR
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Audio Clip", desktopPath, "wav,mp3,ogg");
        if (!string.IsNullOrEmpty(path))
        {
            string relativePath = path.Substring(desktopPath.Length);
            //check if first character is /
            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Substring(1);
            }
            clipPathInput.text = relativePath;
        }
#else
        // In built application, use Windows file dialog
        OpenFileDialog();
#endif
    }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int lStructSize = 0;
        public IntPtr hwndOwner = IntPtr.Zero;
        public IntPtr hInstance = IntPtr.Zero;
        public string lpstrFilter = null;
        public string lpstrCustomFilter = null;
        public int nMaxCustFilter = 0;
        public int nFilterIndex = 0;
        public string lpstrFile = null;
        public int nMaxFile = 0;
        public string lpstrFileTitle = null;
        public int nMaxFileTitle = 0;
        public string lpstrInitialDir = null;
        public string lpstrTitle = null;
        public int Flags = 0;
        public short nFileOffset = 0;
        public short nFileExtension = 0;
        public string lpstrDefExt = null;
        public IntPtr lCustData = IntPtr.Zero;
        public IntPtr lpfnHook = IntPtr.Zero;
        public string lpTemplateName = null;
        public IntPtr pvReserved = IntPtr.Zero;
        public int dwReserved = 0;
        public int flagsEx = 0;
    }

    private void OpenFileDialog()
    {
        OpenFileName ofn = new OpenFileName();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.lpstrFilter = "Audio Files\0*.wav;*.mp3;*.ogg\0All Files\0*.*\0";
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrInitialDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        ofn.lpstrTitle = "Select Audio Clip";
        ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008; // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_HIDEREADONLY

        if (GetOpenFileName(ofn))
        {
            string selectedPath = ofn.lpstrFile;
            if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
            {
                // Convert to relative path if possible
                string dataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                if (selectedPath.StartsWith(dataPath))
                {
                    string relativePath = selectedPath.Substring(dataPath.Length);
                    //check if first character is \
                    if (relativePath.StartsWith("\\"))
                    {
                        relativePath = relativePath.Substring(1);
                    }
                    relativePath = relativePath.Replace("\\", "/");
                    clipPathInput.text =  relativePath;
                }
                else
                {
                    // Use absolute path if not under Assets
                    if (selectedPath.StartsWith("\\"))
                    {
                        selectedPath = selectedPath.Substring(1);
                    }
                    selectedPath = selectedPath.Replace("\\", "/");
                    clipPathInput.text = selectedPath;
                }
            }
        }
    }
#else
    private void OpenFileDialog()
    {
        Debug.LogWarning("File dialog is only supported in Windows builds. Please manually enter the file path.");
        // You could implement alternative solutions here for other platforms
        // For example, showing an input field or using a web-based file picker
    }
#endif
    #endregion


    public Button playSoundButton;
    [SerializeField] private RectTransform content;
    public void SetTab(int index)
    {
        selectedAudioStyle = (AudioStyle)index;
        for (int i = 0; i < tabList.Count; i++)
        {
            if (tabList[i] != null)
            {
                tabList[i].GetComponent<CanvasGroup>().interactable = (i == index);
                tabList[i].GetComponent<CanvasGroup>().alpha = (i == index) ? 1f : 0f;
                tabList[i].GetComponent<CanvasGroup>().blocksRaycasts = (i == index);

                if (tabList[i].GetComponent<CanvasGroup>().interactable)
                {
                    content = tabList[i].transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                    for (int k = 0; k < content.childCount; k++)
                    {
                        Destroy(content.GetChild(k).gameObject);
                    }
                    //if (content.childCount <= 0)
                    //{
                    for (int j = 0; j < soundDataCollection.soundDataList[index].soundList.Count; j++)
                    {
                        GameObject soundButton = Instantiate(playSoundButton.gameObject, content);
                        if (i == 7) soundButton.transform.GetChild(2).gameObject.SetActive(true);
                        soundButton.GetComponent<AudioLoadingExample>().soundName = soundDataCollection.soundDataList[index].soundList[j].soundName;
                        soundButton.GetComponent<AudioLoadingExample>().clipPath = soundDataCollection.soundDataList[index].soundList[j].clipPath;
                        if (PlayerPrefs.GetInt("FavoriteSoundName" + soundButton.GetComponent<AudioLoadingExample>().soundName + soundButton.GetComponent<AudioLoadingExample>().clipPath, 0) == 1)
                        {
                            soundButton.GetComponent<AudioLoadingExample>().iconImage.sprite = soundButton.GetComponent<AudioLoadingExample>().favoriteIcon;
                        }
                        else
                        {
                            soundButton.GetComponent<AudioLoadingExample>().iconImage.sprite = soundButton.GetComponent<AudioLoadingExample>().defaultIcon;
                        }
                        soundButton.GetComponentInChildren<TextMeshProUGUI>().text = soundDataCollection.soundDataList[index].soundList[j].soundName;
                    }

                    if (content.childCount >= 30)
                        content.offsetMax = new Vector2(0, 205 * (content.childCount / 5));
                    content.anchoredPosition = Vector2.zero;
                    //}

                }

            }
        }
        if (addSoundTab != null)
        {
            addSoundTab.GetComponent<CanvasGroup>().interactable = false;
            addSoundTab.GetComponent<CanvasGroup>().alpha = 0;
            addSoundTab.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
    public void OpenAddTab()
    {
        for (int i = 0; i < tabList.Count; i++)
        {
            if (tabList[i] != null)
            {
                tabList[i].GetComponent<CanvasGroup>().interactable = false;
                tabList[i].GetComponent<CanvasGroup>().alpha = 0;
                tabList[i].GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }
        if (addSoundTab != null)
        {
            addSoundTab.GetComponent<CanvasGroup>().interactable = true;
            addSoundTab.GetComponent<CanvasGroup>().alpha = 1;
            addSoundTab.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }
    public void AddSoundToFavorites(string soundName, string clipPath)
    {
        AudioStyle selectedStyle = AudioStyle.Favorite;
        int styleIndex = (int)selectedStyle;

        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {selectedStyle} is not assigned.");
            return;
        }
        selectedAudioStyle = selectedStyle;
        string newSoundName = soundName.Trim();
        string newClipPath = clipPath.Trim();

        if (string.IsNullOrEmpty(newSoundName) || string.IsNullOrEmpty(newClipPath))
        {
            Debug.LogWarning("Sound Name and Clip Path cannot be empty.");
            return;
        }

        // Check for duplicates in the selected style's sound data
        if (soundDataCollection.soundDataList[styleIndex].soundList.Exists(s => s.soundName == newSoundName))
        {
            Debug.LogWarning($"A sound with the name '{newSoundName}' already exists in {selectedStyle} category.");
            return;
        }

        SoundVariable newSound = new SoundVariable
        {
            soundName = newSoundName,
            clipPath = newClipPath
        };

        soundDataCollection.soundDataList[styleIndex].soundList.Add(newSound);
        Debug.Log($"Added new sound: {newSoundName} with path: {newClipPath} to style: {selectedStyle}");
        // Save to JSON file
        SaveSoundDataToJson();
    }
    public void RemoveFavoriteSound(string soundName, string clipPath)
    {
        AudioStyle selectedStyle = AudioStyle.Favorite;
        int styleIndex = (int)selectedStyle;

        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {selectedStyle} is not assigned.");
            return;
        }

        string newSoundName = soundName.Trim();
        string newClipPath = clipPath.Trim();

        if (string.IsNullOrEmpty(newSoundName) || string.IsNullOrEmpty(newClipPath))
        {
            Debug.LogWarning("Sound Name and Clip Path cannot be empty.");
            return;
        }

        // Find and remove the sound from the selected style's sound data
        SoundVariable soundToRemove = soundDataCollection.soundDataList[styleIndex].soundList.Find(s => s.soundName == newSoundName && s.clipPath == newClipPath);
        if (soundToRemove != null)
        {
            soundDataCollection.soundDataList[styleIndex].soundList.Remove(soundToRemove);
            Debug.Log($"Removed sound: {newSoundName} with path: {newClipPath} from style: {selectedStyle}");
            // Save to JSON file
            SaveSoundDataToJson();
        }
        else
        {
            Debug.LogWarning($"Sound '{newSoundName}' not found in {selectedStyle} category.");
        }
    }
    // Cache for loaded audio clips to avoid reloading
    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

    // Helper method to find sound by name across all AudioStyles
    private SoundVariable FindSoundByName(string soundName)
    {
        foreach (var soundData in soundDataCollection.soundDataList)
        {
            if (soundData != null)
            {
                SoundVariable sound = soundData.soundList.Find(s => s.soundName == soundName);
                if (sound != null)
                {
                    return sound;
                }
            }
        }
        return null;
    }

    // Method to get all sounds from a specific AudioStyle
    public List<SoundVariable> GetSoundsByStyle()
    {
        int styleIndex = (int)selectedAudioStyle;
        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {selectedAudioStyle} is not assigned.");
            return new List<SoundVariable>();
        }

        return new List<SoundVariable>(soundDataCollection.soundDataList[styleIndex].soundList);
    }

    // Method to get sound names from a specific AudioStyle
    public List<string> GetSoundNamesByStyle()
    {
        int styleIndex = (int)selectedAudioStyle;
        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {selectedAudioStyle} is not assigned.");
            return new List<string>();
        }

        List<string> soundNames = new List<string>();
        foreach (var sound in soundDataCollection.soundDataList[styleIndex].soundList)
        {
            soundNames.Add(sound.soundName);
        }
        return soundNames;
    }

    // Start is called before the first frame update
    void Start()
    {
        PreloadAudioClips();
        SetTab(7);
    }


    // Overloaded method to play sound by AudioStyle and name
    public void PlaySound(string soundName, AudioSource audioSource = null)
    {
        int styleIndex = (int)selectedAudioStyle;
        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {selectedAudioStyle} is not assigned.");
            return;
        }

        SoundVariable sound = soundDataCollection.soundDataList[styleIndex].soundList.Find(s => s.soundName == soundName);
        if (sound != null)
        {
            Debug.Log($"Attempting to play sound: {soundName} with path: {sound.clipPath}");
            StartCoroutine(LoadAndPlayAudio(sound.clipPath, audioSource));
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in {selectedAudioStyle} category.");
        }
    }

    // Overloaded method to play sound synchronously by AudioStyle and name
    public void PlaySoundSync(string soundName, AudioSource audioSource = null)
    {
        int styleIndex = (int)selectedAudioStyle;
        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {selectedAudioStyle} is not assigned.");
            return;
        }

        SoundVariable sound = soundDataCollection.soundDataList[styleIndex].soundList.Find(s => s.soundName == soundName);
        if (sound != null)
        {
            // Check if already cached
            if (audioClipCache.ContainsKey(sound.clipPath))
            {
                audioSource.volume = defaultVolume;
                audioSource.PlayOneShot(audioClipCache[sound.clipPath]);
                return;
            }

            // For synchronous loading, try to load from StreamingAssets
            string streamingPath = Path.Combine(Application.streamingAssetsPath, sound.clipPath);
            if (File.Exists(streamingPath))
            {
                StartCoroutine(LoadAndPlayAudio(streamingPath, audioSource));
            }
            else
            {
                Debug.LogWarning($"Audio file not found at path: {streamingPath}");
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in {selectedAudioStyle} category.");
        }
    }

    private IEnumerator LoadAndPlayAudio(string filePath, AudioSource audioSource = null)
    {
        // Check cache first
        if (audioClipCache.ContainsKey(filePath))
        {
            audioSource.volume = defaultVolume;
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
                // If relative path, try StreamingAssets first
                string streamingPath = Path.Combine(Application.streamingAssetsPath, filePath);
                Debug.Log($"Trying StreamingAssets path: {streamingPath}");

                if (File.Exists(streamingPath))
                {
                    fullPath = "file://" + streamingPath.Replace("\\", "/");
                    Debug.Log($"Found file in StreamingAssets: {fullPath}");
                }
                else
                {
                    // Try relative to Desktop (for desktop-based files)
                    string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                    string desktopFilePath = Path.Combine(desktopPath, filePath);
                    Debug.Log($"Trying Desktop path: {desktopFilePath}");

                    if (File.Exists(desktopFilePath))
                    {
                        fullPath = "file://" + desktopFilePath.Replace("\\", "/");
                        Debug.Log($"Found file on Desktop: {fullPath}");
                    }
                    else
                    {
                        // Try relative to executable directory (build location)
                        string executablePath = Path.GetDirectoryName(Application.dataPath);
                        string executableFilePath = Path.Combine(executablePath, filePath);
                        Debug.Log($"Trying executable directory path: {executableFilePath}");

                        if (File.Exists(executableFilePath))
                        {
                            fullPath = "file://" + executableFilePath.Replace("\\", "/");
                            Debug.Log($"Found file near executable: {fullPath}");
                        }
                        else
                        {
                            Debug.LogError($"Audio file not found at any location. Tried:\n" +
                                         $"1. StreamingAssets: {streamingPath}\n" +
                                         $"2. Desktop: {desktopFilePath}\n" +
                                         $"3. Executable dir: {executableFilePath}");
                            yield break;
                        }
                    }
                }
            }
            else
            {
                // Absolute path
                if (File.Exists(filePath))
                {
                    fullPath = "file://" + filePath.Replace("\\", "/");
                    Debug.Log($"Using absolute path: {fullPath}");
                }
                else
                {
                    Debug.LogError($"Audio file not found at absolute path: {filePath}");
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
        foreach (var soundData in soundDataCollection.soundDataList)
        {
            if (soundData != null)
            {
                foreach (var sound in soundData.soundList)
                {
                    if (!audioClipCache.ContainsKey(sound.clipPath))
                    {
                        StartCoroutine(PreloadAudio(sound.clipPath));
                    }
                }
            }
        }
    }

    // Method to preload audio clips for a specific AudioStyle
    public void PreloadAudioClips(AudioStyle audioStyle)
    {
        int styleIndex = (int)audioStyle;
        if (styleIndex >= soundDataCollection.soundDataList.Count || soundDataCollection.soundDataList[styleIndex] == null)
        {
            Debug.LogError($"SoundData for AudioStyle {audioStyle} is not assigned.");
            return;
        }

        foreach (var sound in soundDataCollection.soundDataList[styleIndex].soundList)
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
                // Try StreamingAssets first
                string streamingPath = Path.Combine(Application.streamingAssetsPath, filePath);
                if (File.Exists(streamingPath))
                {
                    fullPath = "file://" + streamingPath.Replace("\\", "/");
                }
                else
                {
                    // Try relative to Desktop
                    string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                    string desktopFilePath = Path.Combine(desktopPath, filePath);
                    if (File.Exists(desktopFilePath))
                    {
                        fullPath = "file://" + desktopFilePath.Replace("\\", "/");
                    }
                    else
                    {
                        // Try relative to executable directory
                        string executablePath = Path.GetDirectoryName(Application.dataPath);
                        string executableFilePath = Path.Combine(executablePath, filePath);
                        if (File.Exists(executableFilePath))
                        {
                            fullPath = "file://" + executableFilePath.Replace("\\", "/");
                        }
                        else
                        {
                            Debug.LogWarning($"Audio file not found at path: {filePath}");
                            yield break;
                        }
                    }
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    fullPath = "file://" + filePath.Replace("\\", "/");
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

    /// <summary>
    /// Sets the Unity application window to always stay on top of other applications
    /// </summary>
    /// <param name="alwaysOnTop">True to keep window on top, false to allow normal behavior</param>
    private void SetWindowAlwaysOnTop(bool alwaysOnTop)
    {
        try
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            IntPtr windowHandle = GetActiveWindow();
            if (windowHandle != IntPtr.Zero)
            {
                IntPtr insertAfter = alwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST;
                bool result = SetWindowPos(windowHandle, insertAfter, 0, 0, 0, 0, 
                    SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                
                if (result)
                {
                    Debug.Log($"Window always on top set to: {alwaysOnTop}");
                }
                else
                {
                    Debug.LogWarning("Failed to set window always on top property");
                }
            }
            else
            {
                Debug.LogWarning("Could not get window handle");
            }
#else
            Debug.Log("Always on top functionality is only available in Windows standalone builds");
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error setting window always on top: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the alwaysOnTop setting has changed and applies the window behavior accordingly
    /// </summary>
    private void CheckAndApplyAlwaysOnTop()
    {
        if (alwaysOnTop != wasAlwaysOnTop)
        {
            SetWindowAlwaysOnTop(alwaysOnTop);
            wasAlwaysOnTop = alwaysOnTop;
        }
    }

    void OnDestroy()
    {
        // Reset window behavior before destroying
        if (alwaysOnTop)
        {
            SetWindowAlwaysOnTop(false);
        }

        ClearAudioCache();
        Instance = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (volumeSlider != null)
        {
            defaultVolume = volumeSlider.value;
        }
        alwaysOnTop = alwaysOnTopToggle.isOn;

        // Check and apply always on top behavior
        CheckAndApplyAlwaysOnTop();
    }
}

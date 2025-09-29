using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundVariable
{
    public string soundName;
    public string clipPath;
}

[CreateAssetMenu(fileName = "SoundData", menuName = "ScriptableObjects/SoundData", order = 1)]
public class SoundData : ScriptableObject
{
    public List<SoundVariable> soundList = new List<SoundVariable>();
}

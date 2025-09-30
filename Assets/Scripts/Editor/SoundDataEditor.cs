using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(SoundData))]
public class SoundDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SoundData soundData = (SoundData)target;

        EditorGUILayout.LabelField("Sound Data Configuration", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Custom Editor is Working!", EditorStyles.helpBox);

        // Test button
        if (GUILayout.Button("TEST BUTTON - Click Me!"))
        {
            Debug.Log("Custom editor is working!");
        }

        EditorGUILayout.Space();

        // Display and edit the soundList
        SerializedProperty soundListProperty = serializedObject.FindProperty("soundList");
        EditorGUI.BeginChangeCheck();

        // Show array size control
        int arraySize = soundListProperty.arraySize;
        int newSize = EditorGUILayout.IntField("Sound List Size", arraySize);

        if (newSize != arraySize)
        {
            soundListProperty.arraySize = newSize;
        }

        EditorGUILayout.Space();

        // Display each sound variable with browse button
        for (int i = 0; i < soundListProperty.arraySize; i++)
        {
            SerializedProperty soundElement = soundListProperty.GetArrayElementAtIndex(i);
            SerializedProperty soundNameProperty = soundElement.FindPropertyRelative("soundName");
            SerializedProperty clipPathProperty = soundElement.FindPropertyRelative("clipPath");

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"Sound {i + 1}", EditorStyles.boldLabel);

            // Sound name field
            soundNameProperty.stringValue = EditorGUILayout.TextField("Sound Name", soundNameProperty.stringValue);

            // Clip path field with browse button
            EditorGUILayout.BeginHorizontal();
            clipPathProperty.stringValue = EditorGUILayout.TextField("Clip Path", clipPathProperty.stringValue);

            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFilePanel("Select Audio File",
                    Application.dataPath, "mp3,wav,ogg");

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Convert absolute path to relative path from Assets folder
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    clipPathProperty.stringValue = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Show preview of audio clip if path is valid
            if (!string.IsNullOrEmpty(clipPathProperty.stringValue))
            {
                AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPathProperty.stringValue);
                if (audioClip != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Preview:", GUILayout.Width(50));
                    EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), false);
                    EditorGUILayout.EndHorizontal();
                }
                else if (File.Exists(clipPathProperty.stringValue))
                {
                    EditorGUILayout.HelpBox("Audio file found but not imported to Unity. Please import it to the Assets folder.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("Audio file not found at specified path.", MessageType.Error);
                }
            }

            // Remove button for this sound
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                soundListProperty.DeleteArrayElementAtIndex(i);
                break; // Break to avoid index issues
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Add new sound button
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add New Sound", GUILayout.Width(120)))
        {
            soundListProperty.arraySize++;
            SerializedProperty newElement = soundListProperty.GetArrayElementAtIndex(soundListProperty.arraySize - 1);
            SerializedProperty newSoundName = newElement.FindPropertyRelative("soundName");
            SerializedProperty newClipPath = newElement.FindPropertyRelative("clipPath");

            newSoundName.stringValue = $"Sound {soundListProperty.arraySize}";
            newClipPath.stringValue = "";
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(soundData);
        }
    }
}
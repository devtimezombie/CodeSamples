using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelSceneReferences))]
public class LevelSceneEditor : Editor
{
    LevelSceneReferences LSR;
    public EditorBuildSettingsScene[] EBSS;
    [SerializeField]
    SceneAsset sceneAssetSuggested;
    int t_newIndex;
    bool t_sceneIsGameplay, showScriptDeets;
    SceneInfo temp_info;
    GUIStyle invalidSceneWarning = new GUIStyle();

    SceneAsset unityScene;

    private void OnEnable()
    {
        LSR = (LevelSceneReferences)target;
        temp_info = new SceneInfo("");
        sceneAssetSuggested = null;

        for (int i = 0; i < LSR.GameplayScenes.Count; i++)
        {
            RefreshBuildIndex(LSR.GameplayScenes[i]);
        }
    }

    void RefreshAll()
    {
        RefreshBuildIndex(LSR.LoadingScene);
        RefreshBuildIndex(LSR.MenuScene);

        for (int i = 0; i < LSR.GameplayScenes.Count; i++)
        {
            RefreshBuildIndex(LSR.GameplayScenes[i]);
        }
    }

    public override void OnInspectorGUI()
    {
        EBSS = EditorBuildSettings.scenes;
        LSR.ScenesInBuildSettings.Clear();
        foreach (EditorBuildSettingsScene eb in EBSS)
        {
            string s = eb.path;
            s = s.Substring(s.LastIndexOf("/") + 1);
            LSR.ScenesInBuildSettings.Add(s.Remove(s.LastIndexOf(".")));
        }

        //Pull all scenes listed in the build settings menu, so we know what Unity expects us to use
        EditorGUILayout.LabelField(UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings + " Scenes in Build Settings", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Will not take into account unchecked levels", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scene Name");
        EditorGUILayout.LabelField("Build Index");
        EditorGUILayout.EndHorizontal();

        foreach (string s in LSR.ScenesInBuildSettings)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(" -" + s);
            EditorGUILayout.IntField(LSR.ScenesInBuildSettings.FindIndex(npcString => npcString == s));
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Refresh"))
            RefreshAll();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Important Scenes", EditorStyles.boldLabel);

        //index
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ToggleLeft("Index", LSR.ScenesInBuildSettings[0] != null);
        EditorGUILayout.LabelField(LSR.ScenesInBuildSettings[0] != null ? LSR.ScenesInBuildSettings[0] + ".unity" : "");
        EditorGUILayout.IntField(0);
        EditorGUILayout.EndHorizontal();
        //menu
        RefreshBuildIndex(LSR.MenuScene);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ToggleLeft("Menu Scene", LSR.MenuScene.BuildIndex != -1);
        EditorGUILayout.LabelField(LSR.MenuScene.SceneName + ".unity");
        EditorGUILayout.IntField(LSR.MenuScene.BuildIndex);
        if (sceneAssetSuggested is SceneAsset && GUILayout.Button("o"))
            FillSceneInfo(LSR.MenuScene);
        EditorGUILayout.EndHorizontal();
        //loading scene
        RefreshBuildIndex(LSR.LoadingScene);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ToggleLeft("Load Splash Scene", LSR.LoadingScene.BuildIndex != -1);
        EditorGUILayout.LabelField(LSR.LoadingScene.SceneName + ".unity");
        EditorGUILayout.IntField(LSR.LoadingScene.BuildIndex);
        if (sceneAssetSuggested is SceneAsset && GUILayout.Button("o"))
            FillSceneInfo(LSR.LoadingScene);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gameplay Scenes", EditorStyles.boldLabel);
        for (int i = 0; i < LSR.GameplayScenes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(i + ". Scene " + (i + 1), GUILayout.Width(100));
            EditorGUILayout.LabelField(LSR.GameplayScenes[i].SceneName + ".unity | " + LSR.GameplayScenes[i].BuildIndex);

            if (sceneAssetSuggested is SceneAsset && GUILayout.Button("o", GUILayout.Width(25)))
                FillSceneInfo(LSR.GameplayScenes[i], i);
            if (GUILayout.Button("x", GUILayout.Width(25)))
                LSR.GameplayScenes.RemoveAt(i);
            EditorGUILayout.EndHorizontal();
        }

        if (sceneAssetSuggested is SceneAsset)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(LSR.GameplayScenes.Count + ". New");
            if (GUILayout.Button("Add as gameplay scene"))
            {
                LSR.GameplayScenes.Add(new SceneInfo(""));
                FillSceneInfo(LSR.GameplayScenes[LSR.GameplayScenes.Count - 1], LSR.GameplayScenes.Count - 1);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fill values with scene asset", EditorStyles.boldLabel);
        sceneAssetSuggested = (SceneAsset)EditorGUILayout.ObjectField("Scene Asset to Use", sceneAssetSuggested, typeof(SceneAsset), false); //(UnityEditor.SceneAsset)
        if (sceneAssetSuggested is SceneAsset)
        {
            FillSceneInfo(temp_info);
            if (temp_info.BuildIndex == -1)
            {
                invalidSceneWarning.normal.textColor = Color.red;
                EditorGUILayout.LabelField(temp_info.SceneName + " has not added to the build scenes list", invalidSceneWarning);
            }
            else
            {
                invalidSceneWarning.normal.textColor = Color.grey;
                EditorGUILayout.LabelField(temp_info.SceneName + " is valid.", invalidSceneWarning);
            }


        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Modified by the LevelSceneEditor script");
        showScriptDeets = EditorGUILayout.ToggleLeft("DrawDefaultInspector", showScriptDeets);
        if (showScriptDeets)
        {
            DrawDefaultInspector();
        }
    }

    void FillSceneInfo(SceneInfo t_SI, int arrayIndex = -1)
    {
        t_SI.SceneName = sceneAssetSuggested.name;

        if (LSR.ScenesInBuildSettings.Contains(t_SI.SceneName))
            RefreshBuildIndex(t_SI);
        else
            t_SI.BuildIndex = -1;

        if (arrayIndex > -1)
            t_SI.ElementName = arrayIndex + ". " + t_SI.SceneName;
        else
            t_SI.ElementName = t_SI.SceneName;

        EditorUtility.SetDirty(target);
    }

    void RefreshBuildIndex(SceneInfo t_SI)
    {
        t_SI.BuildIndex = LSR.ScenesInBuildSettings.FindIndex(npcString => npcString == t_SI.SceneName);
    }
}

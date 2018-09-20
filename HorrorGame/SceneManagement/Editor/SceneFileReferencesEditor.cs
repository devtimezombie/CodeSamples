using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;

namespace devtimezombie
{
    [CustomEditor(typeof(SceneFileReferences))]
    public class SceneFileReferencesEditor : Editor
    {
        SceneFileReferences LSR;
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
            LSR = (SceneFileReferences)target;
            temp_info = new SceneInfo("");
            sceneAssetSuggested = null;

            RefreshAll();
        }

        void RefreshAll()
        {
            for (int i = 0; i < LSR.LoadingScenes.Count; i++)
            {
                RefreshBuildIndex(LSR.LoadingScenes[i]);
            }
            for (int i = 0; i < LSR.MenuScenes.Count; i++)
            {
                RefreshBuildIndex(LSR.MenuScenes[i]);
            }
            for (int i = 0; i < LSR.GameplayScenes.Count; i++)
            {
                RefreshBuildIndex(LSR.GameplayScenes[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            EBSS = EditorBuildSettings.scenes;

            if (EBSS.Length == 0)
            {
                EditorGUILayout.LabelField("0 Scenes found in build manager.", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Looks like you can't build the game yet.", EditorStyles.miniLabel);
                if (GUILayout.Button("Open Build Settings"))
                    EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
            else
            {
                LSR.ScenesInBuildSettings.Clear();

                foreach (EditorBuildSettingsScene eb in EBSS)
                {
                    string s = eb.path;
                    s = s.Substring(s.LastIndexOf("/") + 1);
                    LSR.ScenesInBuildSettings.Add(s.Remove(s.LastIndexOf(".")));
                }

                //Pull all scenes listed in the build settings menu, so we know what Unity expects us to use
                {
                    EditorGUILayout.LabelField(SceneManager.sceneCountInBuildSettings + " Scene" + (EBSS.Length == 1 ? "" : "s") + " in Build Settings", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Correct as of " + System.DateTime.Now, EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("Will not take into account unchecked levels", EditorStyles.miniLabel);
                    if (GUILayout.Button("Open Build Settings"))
                        EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Scene Name");
                    EditorGUILayout.LabelField("Build Index");
                    EditorGUILayout.EndHorizontal();

                    foreach (string s in LSR.ScenesInBuildSettings)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(" -" + s);
                        //EditorGUILayout.BeginToggleGroup();
                        EditorGUILayout.IntField(LSR.ScenesInBuildSettings.FindIndex(npcString => npcString == s));
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);
                if (GUILayout.Button("Refresh"))
                    RefreshAll();

                //

                LSR.sceneReferences.Clear();
                foreach (SceneInfo si in LSR.MenuScenes)
                {
                    LSR.sceneReferences.Add(si.SceneName, si);
                }
                foreach (SceneInfo si in LSR.LoadingScenes)
                {
                    LSR.sceneReferences.Add(si.SceneName, si);
                }
                foreach (SceneInfo si in LSR.GameplayScenes)
                {
                    LSR.sceneReferences.Add(si.SceneName, si);
                }
                foreach (SceneInfo si in LSR.UnusedScenes)
                {
                    LSR.sceneReferences.Add(si.SceneName, si);
                }

                //

                for (int i = 1; i < LSR.ScenesInBuildSettings.Count; i++)
                {




                    if (!LSR.sceneReferences.ContainsKey(LSR.ScenesInBuildSettings[i]))
                    {
                        Debug.LogWarning("new entry");
                        SceneInfo t_SI = new SceneInfo(LSR.ScenesInBuildSettings[i]);
                        LSR.sceneReferences.Add(t_SI.SceneName, t_SI);
                    }
                    else
                    {
                        Debug.LogWarning(LSR.sceneReferences[LSR.ScenesInBuildSettings[i]].SceneName);
                    }

                    SceneInfo si = LSR.sceneReferences[LSR.ScenesInBuildSettings[i]];
                    switch (si.role)
                    {
                        case (SceneInfo.SceneRoles.menu):
                            if (!LSR.MenuScenes.Contains(si))
                                LSR.MenuScenes.Add(si);

                            if (LSR.LoadingScenes.Contains(si))
                                LSR.LoadingScenes.Remove(si);
                            if (LSR.GameplayScenes.Contains(si))
                                LSR.GameplayScenes.Remove(si);
                            if (LSR.UnusedScenes.Contains(si))
                                LSR.UnusedScenes.Remove(si);
                            break;
                        case (SceneInfo.SceneRoles.load):
                            if (!LSR.LoadingScenes.Contains(si))
                                LSR.LoadingScenes.Add(si);

                            if (LSR.MenuScenes.Contains(si))
                                LSR.MenuScenes.Remove(si);
                            if (LSR.GameplayScenes.Contains(si))
                                LSR.GameplayScenes.Remove(si);
                            if (LSR.UnusedScenes.Contains(si))
                                LSR.UnusedScenes.Remove(si);
                            break;
                        case (SceneInfo.SceneRoles.game):
                            if (!LSR.GameplayScenes.Contains(si))
                                LSR.GameplayScenes.Add(si);

                            if (LSR.LoadingScenes.Contains(si))
                                LSR.LoadingScenes.Remove(si);
                            if (LSR.MenuScenes.Contains(si))
                                LSR.MenuScenes.Remove(si);
                            if (LSR.UnusedScenes.Contains(si))
                                LSR.UnusedScenes.Remove(si);
                            break;
                        case (SceneInfo.SceneRoles.none):
                            if (!LSR.UnusedScenes.Contains(si))
                                LSR.UnusedScenes.Add(si);

                            if (LSR.LoadingScenes.Contains(si))
                                LSR.LoadingScenes.Remove(si);
                            if (LSR.GameplayScenes.Contains(si))
                                LSR.GameplayScenes.Remove(si);
                            if (LSR.MenuScenes.Contains(si))
                                LSR.MenuScenes.Remove(si);
                            break;
                    }
                }

                //ReorderScenes();
                DrawSceneLists();
                /*if (LSR.ScenesInBuildSettings.Count > 0)
                    for (int i = 1; i < LSR.ScenesInBuildSettings.Count; i++)
                    {
                        if (i == LSR.GameplayScenes.Count)
                        {
                            
                        }
                    }*/





                /* //index
                 EditorGUILayout.BeginHorizontal();
                 EditorGUILayout.ToggleLeft("Index", LSR.ScenesInBuildSettings[0] != null);
                 EditorGUILayout.LabelField(LSR.ScenesInBuildSettings[0] != null ? LSR.ScenesInBuildSettings[0] + ".unity" : "");
                 EditorGUILayout.IntField(0);
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
                 //menu
                 RefreshBuildIndex(LSR.MenuScene);
                 EditorGUILayout.BeginHorizontal();
                 EditorGUILayout.ToggleLeft("Menu Scene", LSR.MenuScene.BuildIndex != -1);
                 EditorGUILayout.LabelField(LSR.MenuScene.SceneName + ".unity");
                 EditorGUILayout.IntField(LSR.MenuScene.BuildIndex);
                 if (sceneAssetSuggested is SceneAsset && GUILayout.Button("o"))
                     FillSceneInfo(LSR.MenuScene);
                 EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Gameplay Scenes", EditorStyles.boldLabel);




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
                */

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Modified by the LevelSceneEditor script", EditorStyles.miniLabel);
            showScriptDeets = EditorGUILayout.ToggleLeft("DrawDefaultInspector", showScriptDeets);
            if (showScriptDeets)
            {
                DrawDefaultInspector();
            }
        }

        void FillSceneInfo(SceneInfo t_SI, int arrayIndex = -1)
        {
            t_SI.SceneName = sceneAssetSuggested.name;

            //if (!LSR.ScenesAccountedFor.Contains(t_SI.SceneName))
            //    LSR.ScenesAccountedFor.Add(t_SI.SceneName);

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

        /*   void ReorderScenes()
           {
               SceneInfo si = null;

               //update load list
               int i = 0;
               while (i < LSR.LoadingScenes.Count)
               {
                   si = LSR.LoadingScenes[i];

                   if (si.role == SceneInfo.SceneRoles.mainMenu)
                   {
                       LSR.MenuScenes.Add(si);
                       LSR.LoadingScenes.Remove(si);
                       continue;
                   }

                   if (si.role == SceneInfo.SceneRoles.none)
                   {
                       LSR.GameplayScenes.Add(si);
                       LSR.LoadingScenes.Remove(si);
                       continue;
                   }

                   i++;
               }

               //update menu list
               i = 0;
               while (i < LSR.MenuScenes.Count)
               {
                   si = LSR.MenuScenes[i];

                   if (si.role == SceneInfo.SceneRoles.loadScreen)
                   {
                       LSR.LoadingScenes.Add(si);
                       LSR.MenuScenes.Remove(si);
                       continue;
                   }

                   if (si.role == SceneInfo.SceneRoles.none)
                   {
                       LSR.GameplayScenes.Add(si);
                       LSR.MenuScenes.Remove(si);
                       continue;
                   }

                   i++;
               }

               //repopulate
               i = 0;
               while (i < LSR.GameplayScenes.Count)
               {
                   si = LSR.GameplayScenes[i];

                   if (si.role == SceneInfo.SceneRoles.mainMenu)
                   {
                       LSR.MenuScenes.Add(si);
                       LSR.GameplayScenes.Remove(si);
                       continue;
                   }

                   if (si.role == SceneInfo.SceneRoles.loadScreen)
                   {
                       LSR.LoadingScenes.Add(si);
                       LSR.GameplayScenes.Remove(si);
                       continue;
                   }
                   i++;
               }
           }
           */

        #region drawSceneLists
        void DrawSceneLists()
        {
            DrawDivider("Menu Scenes");
            DrawMenuSceneList();
            DrawDivider("Loading Screens");
            DrawLoadSceneList();
            DrawDivider("Gameplay Scenes");
            DrawGameSceneList();
            DrawDivider("Unused Scenes");
            DrawUnusedSceneList();
            if(GUILayout.Button("Confirm"))
                EditorUtility.SetDirty(target);
        }

        void DrawMenuSceneList()
        {
            for (int i = 0; i < LSR.MenuScenes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i + ". " + LSR.MenuScenes[i].SceneName + ".unity | " + LSR.MenuScenes[i].BuildIndex, GUILayout.Width(100));
                LSR.MenuScenes[i].role = (SceneInfo.SceneRoles)EditorGUILayout.EnumPopup(LSR.MenuScenes[i].role);

                if (sceneAssetSuggested is SceneAsset && GUILayout.Button("o", GUILayout.Width(25)))
                    FillSceneInfo(LSR.MenuScenes[i], i);
                if (GUILayout.Button("x", GUILayout.Width(25)))
                    LSR.MenuScenes.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
            }
        }
        void DrawLoadSceneList()
        {
            for (int i = 0; i < LSR.LoadingScenes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i + ". " + LSR.LoadingScenes[i].SceneName + ".unity | " + LSR.LoadingScenes[i].BuildIndex, GUILayout.Width(100));
                LSR.LoadingScenes[i].role = (SceneInfo.SceneRoles)EditorGUILayout.EnumPopup(LSR.LoadingScenes[i].role);

                if (sceneAssetSuggested is SceneAsset && GUILayout.Button("o", GUILayout.Width(25)))
                    FillSceneInfo(LSR.LoadingScenes[i], i);
                if (GUILayout.Button("x", GUILayout.Width(25)))
                    LSR.LoadingScenes.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
            }
        }
        void DrawGameSceneList()
        {
            for (int i = 0; i < LSR.GameplayScenes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i + ". " + LSR.GameplayScenes[i].SceneName + ".unity | " + LSR.GameplayScenes[i].BuildIndex, GUILayout.Width(100));
                LSR.GameplayScenes[i].role = (SceneInfo.SceneRoles)EditorGUILayout.EnumPopup(LSR.GameplayScenes[i].role);

                if (sceneAssetSuggested is SceneAsset && GUILayout.Button("o", GUILayout.Width(25)))
                    FillSceneInfo(LSR.GameplayScenes[i], i);
                if (GUILayout.Button("x", GUILayout.Width(25)))
                    LSR.GameplayScenes.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawUnusedSceneList()
        {
            for (int i = 0; i < LSR.UnusedScenes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i + ". " + LSR.UnusedScenes[i].SceneName + ".unity | " + LSR.UnusedScenes[i].BuildIndex, GUILayout.Width(100));
                LSR.UnusedScenes[i].role = (SceneInfo.SceneRoles)EditorGUILayout.EnumPopup(LSR.UnusedScenes[i].role);

                if (sceneAssetSuggested is SceneAsset && GUILayout.Button("o", GUILayout.Width(25)))
                    FillSceneInfo(LSR.UnusedScenes[i], i);
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion
        void DrawDivider(string message = "")
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (message == "")
                GUILayout.Label("________________");
            else
                EditorGUILayout.LabelField(message, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}

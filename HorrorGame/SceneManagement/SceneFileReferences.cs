using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace devtimezombie
{
    [CreateAssetMenu(fileName = "SceneReferenceAsset", menuName = "Scriptable Objects/SceneReferenceAsset")]
    public class SceneFileReferences : ScriptableObject
    {
        public List<string> ScenesInBuildSettings = new List<string>();
        public List<string> ScenesUnaccountedFor = new List<string>();
        public List<SceneInfo> MenuScenes = new List<SceneInfo>();
        public List<SceneInfo> LoadingScenes = new List<SceneInfo>();
        public List<SceneInfo> GameplayScenes = new List<SceneInfo>();
        public List<SceneInfo> UnusedScenes = new List<SceneInfo>();
        public Dictionary<string, SceneInfo> sceneReferences = new Dictionary<string, SceneInfo>();

        public bool HasMenu { get { return MenuScenes.Count > 0; } }
        public bool HasLoadScreen { get { return LoadingScenes.Count > 0; } }

        SceneInfo menuScene, loadingScene;
        public SceneInfo MenuScene { get { return menuScene.BuildIndex > 0 ? menuScene : MenuScenes[0]; } set { menuScene = value; } }
        public SceneInfo LoadingScene { get { return loadingScene.BuildIndex > 0 ? loadingScene : LoadingScenes[0]; } set { loadingScene = value; } } 
    }

    [System.Serializable]
    public class SceneInfo
    {
        public enum SceneRoles
        {
            none,
            load,
            menu,
            game
        }

        [HideInInspector]
        public string ElementName;
        public string SceneName;
        public int BuildIndex;
        public SceneRoles role;

        public SceneInfo(string name)
        {
            SceneName = name;
            BuildIndex = -1;
        }
    }
}
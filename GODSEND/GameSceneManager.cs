using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GODSEND
{
    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager GSM; //loaded at the first game object, so it can load additional scenes on top

        [SerializeField]

        bool usingLoadingScreen;
        public MenuSceneScript.GameMenuCodeName StartingMenu = MenuSceneScript.GameMenuCodeName.MainMenu;
        public LevelSceneReferences LevelSceneAsset;
        SceneInfo CurrentScene;
        List<int> LoadedSceneIndicies = new List<int>();
        int currentLoadedLevel;

        float startLoadTimestamp, minLoadTime = 2;

        public bool InGame { get; private set; }
        public static bool GamePaused { get; private set; }
        public static bool MainMenuShowing { get; private set; }
        public static bool GameOverScreenShowing, allowedToPause;
        public delegate void UIEvent();
        public static event UIEvent TogglePauseScreen;
        public static event UIEvent ToggleHelpScreen;
        public static event UIEvent BackoutOfMenus;
        public static event UIEvent MenuNav_Up, MenuNav_Down, MenuNav_Left, MenuNav_Right, MenuNav_Accept, MenuNav_Cancel;
        public static event UIEvent loadScreenPromptShown;
        public PlayerActions UINavActions { get; private set; }

        private void Awake()
        {
            if (GSM != null)
            {
                Debug.LogWarning("Duplicate level manager removed", gameObject);
                this.enabled = false;
            }
            else
            {
                GSM = this;
                DontDestroyOnLoad(this.transform);
                UINavActions = PlayerActions.CreateWithDefaultBindings();
                allowedToPause = false;
            }
        }

        private void Start()
        {
            if (LevelSceneAsset == null)
                Debug.LogError("Scene management asset not assigned to game scene manager.");

            CurrentScene = ValidateScene(LevelSceneAsset.GameplayScenes[0]);

            if (CurrentScene.BuildIndex < 1)
                for (int i = 0; i < LevelSceneAsset.GameplayScenes.Count; i++)
                {
                    if (LevelSceneAsset.GameplayScenes[i].BuildIndex > 0)
                    {
                        CurrentScene = ValidateScene(LevelSceneAsset.GameplayScenes[i]);
                        break;
                    }
                }

            SceneManager.LoadScene(LevelSceneAsset.MenuScene.SceneName);
            LoadedSceneIndicies.Add(SceneManager.GetSceneByName(LevelSceneAsset.MenuScene.SceneName).buildIndex);
        }

        SceneInfo ValidateScene(SceneInfo checkScene)
        {
            if (checkScene.BuildIndex > 0)
                return checkScene;
            else
                return LevelSceneAsset.MenuScene;
        }

        private void Update()
        {
            if (InGame)
            {
                if (UINavActions.Pause.WasPressed)
                {
                    if (MainMenuShowing)
                        CloseMainMenuInGame();
                    else
                    {
                        if (GamePaused)
                        {
                            Time.timeScale = 1;
                            GamePaused = false;
                            if (TogglePauseScreen != null)
                                TogglePauseScreen();
                        }
                        else if (allowedToPause && !GameOverScreenShowing)
                        {
                            Time.timeScale = 0;
                            GamePaused = true;
                            if (TogglePauseScreen != null)
                                TogglePauseScreen();
                        }
                    }
                }

                if (UINavActions.Help.WasPressed && !GamePaused && !GameOverScreenShowing)
                {
                    {
                        if (ToggleHelpScreen != null)
                            ToggleHelpScreen();
                    }
                }

                if (UINavActions.MoveRight.WasPressed)
                {
                    if (MenuNav_Right != null)
                        MenuNav_Right();
                }
                else if (UINavActions.MoveLeft.WasPressed)
                {
                    if (MenuNav_Left != null)
                        MenuNav_Left();
                }
                if (UINavActions.MoveUp.WasPressed)
                {
                    if (MenuNav_Up != null)
                        MenuNav_Up();
                }
                else if (UINavActions.MoveDown.WasPressed)
                {
                    if (MenuNav_Down != null)
                        MenuNav_Down();
                }

                /* if (HelpShowing)
                 {
                     if (Input.GetKeyDown(HelpKey)) //Hide help screen
                     {
                         Time.timeScale = 1;
                         HelpShowing = false;
                         if (ToggleHelpScreen != null)
                             ToggleHelpScreen();
                     }
                 }
                 else
                 {
                     if (Input.GetKeyDown(HelpKey)) //Show help screen
                     {
                         Time.timeScale = 0;
                         HelpShowing = true;
                         if (ToggleHelpScreen != null)
                             ToggleHelpScreen();
                     }
                 }*/
            }
            else
            {
                if (UINavActions.Pause.WasPressed) //backout of menus
                {
                    if (MenuNav_Cancel != null)
                        MenuNav_Cancel();
                }

                if (UINavActions.MenuNavDown.WasPressed)
                {
                    if (MenuNav_Down != null)
                        MenuNav_Down();
                }
                else if (UINavActions.MenuNavUp.WasPressed)
                {
                    if (MenuNav_Up != null)
                        MenuNav_Up();
                }
            }

            if (UINavActions.Accept.WasPressed)
            {
                if (MenuNav_Accept != null)
                    MenuNav_Accept();
            }
            if (UINavActions.Cancel.WasPressed)
            {
                if (InGame && MainMenuShowing)
                    CloseMainMenuInGame();
                else if (MenuNav_Cancel != null)
                    MenuNav_Cancel();
            }



        }

        public void ClosePauseMenuButton()
        {
            Time.timeScale = 1;
            GamePaused = false;
            if (TogglePauseScreen != null)
                TogglePauseScreen();
        }

        /*
        *
        * Load scenes
        * 
        */

        public void LoadSpecificGameplayLevel()
        {
            LoadSpecificGameplayLevel(0);
        }
        public void LoadSpecificGameplayLevel(int levelIndexInArray)
        {
            if (levelIndexInArray < -1 || levelIndexInArray > LevelSceneAsset.GameplayScenes.Count)
            {
                Debug.LogWarning("Cannot load that level, index out of bounds");
                return;
            }

            if (levelIndexInArray == -1)
                currentLoadedLevel++;
            else
                currentLoadedLevel = levelIndexInArray;

            CurrentScene = ValidateScene(LevelSceneAsset.GameplayScenes[currentLoadedLevel]);
            StartCoroutine(LoadYourAsyncSceneWithLoadScreen(CurrentScene.SceneName, true));
        }

        #region LoadScenesAsyncronously
        IEnumerator LoadYourAsyncSceneWithLoadScreen(string sceneName, bool loadingGameLevel)
        {
            //Coroutine sourced from http://blog.teamtreehouse.com/make-loading-screen-unity
            // The Application loads the Scene in the background at the same time as the current Scene.
            //This is particularly good for creating loading screens. You could also load the scene by build //number.

            //TODO add click to continue, with a minimum amount show time
            //display load screen and save reference
            //AsyncOperation async = SceneManager.LoadSceneAsync(LevelSceneAsset.LoadingScene.SceneName, LoadSceneMode.Additive);
            //async.allowSceneActivation = false;
            GameOverScreenShowing = false;
            GamePaused = false;
            Time.timeScale = 1;
            allowedToPause = false;

            yield return SceneManager.LoadSceneAsync(LevelSceneAsset.LoadingScene.SceneName, LoadSceneMode.Additive);
            LoadedSceneIndicies.Add(SceneManager.GetSceneByName(LevelSceneAsset.LoadingScene.SceneName).buildIndex);

            startLoadTimestamp = Time.time;

            //unload old scene and remove reference
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(LoadedSceneIndicies[0]));
            LoadedSceneIndicies.RemoveAt(0);

            //load new scene and add reference
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            async.allowSceneActivation = false;
            //yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (async.progress < 0.9f)
                yield return null;

            LoadedSceneIndicies.Add(SceneManager.GetSceneByName(sceneName).buildIndex);

            //show load for at least 2 sec
            while (Time.time < startLoadTimestamp + minLoadTime)
                yield return null;

            //require input to finish loading scene, feature dropped later in development

            /*if (loadingGameLevel)
            {
                //show button prompt
                if (loadScreenPromptShown != null)
                    loadScreenPromptShown();

                //wait for user input
                while (!UINavActions.Accept)
                    yield return null;
            }*/

            //trigger awake and start for scene
            async.allowSceneActivation = true;
            while (async.progress < 1)
                yield return null;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            //hide load screen and remove reference
            yield return SceneManager.UnloadSceneAsync(LevelSceneAsset.LoadingScene.SceneName);
            LoadedSceneIndicies.RemoveAt(0);

            InGame = loadingGameLevel;
            //TODO, make another variable to prevent movement until load screen is gone
        }

        //load scenes without a loading screen
        public void AddSceneDirect(string sceneName)
        {
            StartCoroutine(LoadYourAsyncScene(sceneName));
        }
        IEnumerator LoadYourAsyncScene(string sceneName)
        {
            //Coroutine sourced from http://blog.teamtreehouse.com/make-loading-screen-unity
            // The Application loads the Scene in the background at the same time as the current Scene.
            //This is particularly good for creating loading screens. You could also load the scene by build //number.
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            LoadedSceneIndicies.Add(SceneManager.GetSceneByName(sceneName).buildIndex);
        }
        #endregion

        /*
         *
         * Unload scenes
         * 
         */

        //out of the list of currently loaded scenes, unload the most recently loaded one
        public void UnloadLastScene()
        {
            if (LoadedSceneIndicies.Count > 0)
                DropScene(SceneManager.GetSceneByBuildIndex(LoadedSceneIndicies[LoadedSceneIndicies.Count - 1]));
            else
                Debug.Log("idk fam");
        }

        #region Unload Scenes Asyncronisously
        //start async load via coroutine (overload method for Unload Scene, so you can reference via Scene name instead of scene reference)
        public void DropScene(string sceneName)
        {
            DropScene(SceneManager.GetSceneByName(sceneName));
        }
        public void DropScene(Scene sceneName)
        {
            StartCoroutine(UnLoadYourAsyncScene(sceneName));
        }
        IEnumerator UnLoadYourAsyncScene(Scene sceneName)
        {
            //Coroutine sourced from http://blog.teamtreehouse.com/make-loading-screen-unity
            // The Application loads the Scene in the background at the same time as the current Scene.
            //This is particularly good for creating loading screens. You could also load the scene by build //number.
            yield return SceneManager.UnloadSceneAsync(sceneName);
            LoadedSceneIndicies.RemoveAt(LoadedSceneIndicies.Count - 1);
        }
        #endregion

        public void SetNavButton(GameObject button)
        {
            UnityEngine.EventSystems.EventSystem.current.firstSelectedGameObject = button;
        }

        public void ShowSettingsScreenInGame()
        {
            MainMenuShowing = true;
            StartingMenu = MenuSceneScript.GameMenuCodeName.SettingsMenu;
            AddSceneDirect(LevelSceneAsset.MenuScene.SceneName);
        }

        public void CloseMainMenuInGame()
        {
            if (InGame)
            {
                DropScene(LevelSceneAsset.MenuScene.SceneName);
                MainMenuShowing = false;
            }
        }

        public void RestartCurrentScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitToMenuInGame()
        {
            GamePaused = false;
            Time.timeScale = 1;
            StartingMenu = MenuSceneScript.GameMenuCodeName.MainMenu;
            CurrentScene = ValidateScene(LevelSceneAsset.MenuScene);
            StartCoroutine(LoadYourAsyncSceneWithLoadScreen(CurrentScene.SceneName, false));
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GODSEND
{
    /// <summary>
    /// Holds reference to UI relevant to game overlays
    /// 
    /// </summary>
    public class InGameUIListener : MonoBehaviour
    {
        public GameObject PauseScreen;
        public MenuButton[] pauseMenuButtons;
        public GameObject GameOverScreen;
        public MenuButton[] gameOverButtons;
        MenuButton currentMenuButton;
        public MenuButton CurrentMenuButton
        {
            get { return currentMenuButton; }
            set
            {
                if (currentMenuButton is MenuButton)
                {
                    currentMenuButton.NavFrom();
                }
                currentMenuButton = value;
                currentMenuButton.NavTo();
            }
        }
        int currentMenuButtonIndex;
        //i believe that for the tutorial, this shows relevant input hints
        public GameObject ControlHelp_KB, ControlHelp_XBOX;

        public void SetMenuButton(MenuButton button)
        {
            CurrentMenuButton = button;
            if (GameOverScreen.activeInHierarchy)
            {
                for (int i = 0; i < gameOverButtons.Length; i++)
                {
                    if (gameOverButtons[i] == CurrentMenuButton)
                    {
                        currentMenuButtonIndex = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < pauseMenuButtons.Length; i++)
                {
                    if (pauseMenuButtons[i] == CurrentMenuButton)
                    {
                        currentMenuButtonIndex = i;
                        break;
                    }
                }
            }
        }

        void ChangePromptsToController()
        {
            ControlHelp_KB.SetActive(false);
            ControlHelp_XBOX.SetActive(true);
        }

        void ChangePromptsToKeyboard()
        {
            ControlHelp_XBOX.SetActive(false);
            ControlHelp_KB.SetActive(true);
        }
        private void OnEnable()
        {
            GameSceneManager.ToggleHelpScreen += ToggleHelpOverlay;
            GameSceneManager.TogglePauseScreen += TogglePauseScreen;
            GameSceneManager.MenuNav_Left += NavLeft;
            GameSceneManager.MenuNav_Right += NavRight;
            GameSceneManager.MenuNav_Accept += NavAccept;
            GameSceneManager.MenuNav_Cancel += NavCancel;
            CanvasManager.CM.StartedUsingController += ChangePromptsToController;
            CanvasManager.CM.StartedUsingKeyboard += ChangePromptsToKeyboard;
            PauseScreen.SetActive(false);
        }

        private void OnDisable()
        {
            GameSceneManager.ToggleHelpScreen -= ToggleHelpOverlay;
            GameSceneManager.TogglePauseScreen -= TogglePauseScreen;
            GameSceneManager.MenuNav_Left -= NavLeft;
            GameSceneManager.MenuNav_Right -= NavRight;
            GameSceneManager.MenuNav_Accept -= NavAccept;
            GameSceneManager.MenuNav_Cancel -= NavCancel;
            CanvasManager.CM.StartedUsingController -= ChangePromptsToController;
            CanvasManager.CM.StartedUsingKeyboard -= ChangePromptsToKeyboard;
        }

        void NavLeft()
        {
            if (!PauseScreen.activeInHierarchy && !GameOverScreen.activeInHierarchy)
                return;

            if (GameOverScreen.activeInHierarchy)
            {
                currentMenuButtonIndex = currentMenuButtonIndex <= 0 ? gameOverButtons.Length - 1 : currentMenuButtonIndex - 1;
                CurrentMenuButton = gameOverButtons[currentMenuButtonIndex];
            }
            else
            {
                currentMenuButtonIndex = currentMenuButtonIndex <= 0 ? pauseMenuButtons.Length - 1 : currentMenuButtonIndex - 1;
                CurrentMenuButton = pauseMenuButtons[currentMenuButtonIndex];
            }
        }
        void NavRight()
        {
            if (!PauseScreen.activeInHierarchy && !GameOverScreen.activeInHierarchy)
                return;

            //if the game has ended, allow player to either quit or retry
            //otherwise, navigate the pause screen
            if (GameOverScreen.activeInHierarchy)
            {
                currentMenuButtonIndex = currentMenuButtonIndex >= gameOverButtons.Length - 1 ? 0 : currentMenuButtonIndex + 1;
                CurrentMenuButton = gameOverButtons[currentMenuButtonIndex];
            }
            else
            {
                currentMenuButtonIndex = currentMenuButtonIndex >= pauseMenuButtons.Length - 1 ? 0 : currentMenuButtonIndex + 1;
                CurrentMenuButton = pauseMenuButtons[currentMenuButtonIndex];
            }
        }
        void NavAccept()
        {
            if (!PauseScreen.activeInHierarchy && !GameOverScreen.activeInHierarchy)
                return;
            CurrentMenuButton.Click();
        }
        void NavCancel()
        {
            if (!PauseScreen.activeInHierarchy && !GameOverScreen.activeInHierarchy)
                return;

            if (GameSceneManager.MainMenuShowing)
                GameSceneManager.GSM.CloseMainMenuInGame();
            else
            {
                if (GameSceneManager.GSM is GameSceneManager)
                    GameSceneManager.GSM.ClosePauseMenuButton();
            }
        }

        public void TogglePauseScreen() //click pause, show pause menu, whether in debug mode or not
        {
            if (PauseScreen != null)
            {
                if ((Application.isEditor && PauseScreen.activeInHierarchy) || (!Application.isEditor && !GameSceneManager.GamePaused))
                {
                    PauseScreen.SetActive(false);
                    if (Application.isEditor)
                        Time.timeScale = 1;
                    CanvasManager.CM.PreventAllPlayerInput = false;
                }
                else if (!GameSceneManager.GameOverScreenShowing)
                {
                    PauseScreen.SetActive(true);
                    if (pauseMenuButtons.Length > 0)
                        CurrentMenuButton = pauseMenuButtons[0];
                    CanvasManager.CM.PreventAllPlayerInput = true;
                    if (Application.isEditor)
                        Time.timeScale = 0;
                }

            }
        }

        //set ui input to relevant button
        public void ShowGameOverScreen()
        {
            SetMenuButton(gameOverButtons[0]);
        }

        //show hide ability notes
        public void ToggleHelpOverlay()
        {
            CanvasManager.CM.movesetUI.ToggleHelp();
        }

        public void CloseHelpScreenViaButton()
        {
            if (GameSceneManager.GSM is GameSceneManager)
                GameSceneManager.GSM.ClosePauseMenuButton();
        }

        public void RetryLevel() //activated through UI button
        {
            if (GameSceneManager.GSM is GameSceneManager)
            {
                GameSceneManager.GSM.ClosePauseMenuButton();
                GameSceneManager.GameOverScreenShowing = false;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToNextLevel()
        {
            if (GameSceneManager.GSM != null)
            {
                //-1 is normally invalid, so it's used here as a notice to load the next level
                GameSceneManager.GSM.LoadSpecificGameplayLevel(-1);
            }
            else
            {
                Debug.LogWarning("No game scene manager to change level!");
            }
        }

        public void InGame_Settings()
        {
            if (GameSceneManager.GSM is GameSceneManager)
            {
                if (!GameSceneManager.MainMenuShowing)
                {
                    GameSceneManager.GSM.ShowSettingsScreenInGame();
                }
            }
            else
            {
                Debug.LogWarning("Show Settings command requested, no GameSceneManager in scene to respond");
            }
        }

        public void InGame_Quit()
        {
            if (GameSceneManager.GSM is GameSceneManager)
            {
                GameSceneManager.GSM.QuitToMenuInGame();
            }
            else
            {
                Debug.LogWarning("Quit to Main Menu command requested, no GameSceneManager in scene to respond");
            }
        }
    }
}

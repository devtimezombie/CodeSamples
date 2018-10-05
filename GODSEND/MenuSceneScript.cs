using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GODSEND
{
    public class MenuSceneScript : MonoBehaviour
    {
        //allowed menu screens
        public enum GameMenuCodeName
        {
            MainMenu = 0,
            SaveMenu = 1,
            LoadMenu = 2,
            PauseMenu = 3,
            LvlSelectMenu = 4,
            SettingsMenu = 5,
            CreditsMenu = 6,
            QuitMenu = 7
        }

        //menu data. used to show parts of the main menu - e.g. level select, and move controller selection to relevant buttons
        [System.Serializable]
        public struct GameMenuInfo
        {
            public GameMenuCodeName MenuCodeName;
            public GameObject MenuObject;
            public MenuOption[] buttons;
            public int defaultButtonIndex, cancelButtonIndex;
            public bool horizontalNav;
        }

        MenuOption currentMenuButton;
        public MenuOption CurrentMenuButton
        {
            get { return currentMenuButton; }
            set
            {
                if (currentMenuButton is MenuOption)
                {
                    currentMenuButton.NavFrom();
                }
                currentMenuButton = value;
                currentMenuButton.NavTo();
            }
        }

        //Tracking for which menu screen is open
        [SerializeField]
        GameMenuInfo[] gameMenuList;
        GameMenuInfo currentMenu;
        int currentMenuButtonIndex;
        int previousIndex;

        //Reference the menu's game object based on the menu enum
        //currently rebuilt every time the menu loads, not sure if that should be changed;
        Dictionary<GameMenuCodeName, GameMenuInfo> gameMenuDictionary = new Dictionary<GameMenuCodeName, GameMenuInfo>();
        GameMenuCodeName previousMenu;

        [SerializeField]
        OptionsMenu optionsMenu;

        public void SetMenuButton(MenuButton button)
        {
            CurrentMenuButton = button;
            for (int i = 0; i < currentMenu.buttons.Length; i++)
            {
                if (currentMenu.buttons[i] == CurrentMenuButton)
                {
                    currentMenuButtonIndex = i;
                    break;
                }
            }
        }

        private void OnEnable() //attach controller inputs ant UI events
        {
            GameSceneManager.BackoutOfMenus += HideAllMenus;
            GameSceneManager.MenuNav_Up += NavUp;
            GameSceneManager.MenuNav_Down += NavDown;
            GameSceneManager.MenuNav_Left += NavLeft;
            GameSceneManager.MenuNav_Right += NavRight;
            GameSceneManager.MenuNav_Accept += NavAccept;
            GameSceneManager.MenuNav_Cancel += NavCancel;
        }

        private void OnDisable()
        {
            GameSceneManager.BackoutOfMenus -= HideAllMenus;
            GameSceneManager.MenuNav_Up -= NavUp;
            GameSceneManager.MenuNav_Down -= NavDown;
            GameSceneManager.MenuNav_Left -= NavLeft;
            GameSceneManager.MenuNav_Right -= NavRight;
            GameSceneManager.MenuNav_Accept -= NavAccept;
            GameSceneManager.MenuNav_Cancel -= NavCancel;
        }

        void NavUp()
        {
            if (!currentMenu.horizontalNav)
                NavListDecrease();
        }
        void NavDown()
        {
            if (!currentMenu.horizontalNav)
                NavListIncrease();
        }
        void NavLeft()
        {
            if (currentMenu.horizontalNav)
                NavListDecrease();
        }
        void NavRight()
        {
            if (currentMenu.horizontalNav)
                NavListIncrease();
        }
        void NavListDecrease()
        {
            currentMenuButtonIndex = currentMenuButtonIndex <= 0 ? currentMenu.buttons.Length - 1 : currentMenuButtonIndex - 1;
            CurrentMenuButton = currentMenu.buttons[currentMenuButtonIndex];
        }
        void NavListIncrease()
        {
            currentMenuButtonIndex = currentMenuButtonIndex >= currentMenu.buttons.Length - 1 ? 0 : currentMenuButtonIndex + 1;
            CurrentMenuButton = currentMenu.buttons[currentMenuButtonIndex];
        }

        void NavAccept()
        {
            AkSoundEngine.PostEvent("Accept", gameObject);
            CurrentMenuButton.Click();
        }

        void NavCancel()
        {
            if (currentMenu.buttons[currentMenu.cancelButtonIndex] is MenuOption)
            {
                AkSoundEngine.PostEvent("Back", gameObject);
                currentMenu.buttons[currentMenu.cancelButtonIndex].Click();
            }
            else
            {
                Debug.LogWarning(currentMenu.MenuObject.name + " does not have a cancel button assigned", gameObject);
            }
        }

        void Start()
        {
            for (int i = 0; i < gameMenuList.Length; i++)
            {
                gameMenuDictionary.Add(gameMenuList[i].MenuCodeName, gameMenuList[i]);
            }

            //HideAllMenus();
            ShowMenu(GameSceneManager.GSM.StartingMenu);
        }

        //Hide all menu objects so the canvas shows nothing
        public void HideAllMenus()
        {
            for (int i = 1; i < gameMenuList.Length; i++)
            {
                gameMenuList[i].MenuObject.SetActive(false);
            }
        }

        
        /// <summary>
        /// show the selected major menu (level selection, options menu, etc)
        /// </summary>
        public void ShowMenu(GameMenuCodeName codeName)
        {
            HideAllMenus();
            currentMenu = gameMenuDictionary[codeName];
            currentMenu.MenuObject.SetActive(true);

            if (currentMenu.MenuCodeName == GameMenuCodeName.SettingsMenu && optionsMenu is OptionsMenu)
                optionsMenu.ShowOptionsMenu();
            else if (currentMenu.MenuCodeName == GameMenuCodeName.MainMenu)
                Debug.Log("startup animation");

            StartAtDefaultButton();
        }

        /// <summary>
        /// set navigation while a major menu is already open
        /// usually for confirmation dialogs
        /// </summary>
        public void ShowSubMenu(int index)
        {
            if (index >= 0 && index < gameMenuList.Length)
            {
                currentMenu = gameMenuList[index];
                currentMenu.MenuObject.SetActive(true);
                if (currentMenu.MenuCodeName == GameMenuCodeName.SettingsMenu && optionsMenu is OptionsMenu)
                    optionsMenu.ShowOptionsMenu();

                if (index == 0)
                {
                    currentMenuButtonIndex = previousIndex;
                    previousIndex = currentMenu.defaultButtonIndex;
                    CurrentMenuButton = currentMenu.buttons[currentMenuButtonIndex];
                }
                else
                {
                    previousIndex = currentMenuButtonIndex;
                    StartAtDefaultButton();
                }
            }
            else
            {
                Debug.LogWarning("not a valid sub menu index number");
            }
        }

        //move controller navigation to submenu options
        void StartAtDefaultButton()
        {
            if (currentMenu.buttons.Length > 0)
            {
                if (currentMenu.buttons[currentMenu.defaultButtonIndex] is MenuOption)
                    currentMenuButtonIndex = currentMenu.defaultButtonIndex;
                else
                    currentMenuButtonIndex = 0;
                CurrentMenuButton = currentMenu.buttons[currentMenuButtonIndex];
            }
            else
            {
                Debug.LogWarning("Menu has no list of buttons to navigate");
            }
        }

        //load the first game scene from the list
        public void LoadMainGame()
        {
            GameSceneManager.GSM.LoadSpecificGameplayLevel();
        }

        public void QuitGame()
        {
            Debug.Break();
            Application.Quit();
        }

        //for level selection, UI buttons would set a particular level number and load that one
        public void LoadGameplayLevelByIndex(int numberInArray)
        {
            GameSceneManager.GSM.LoadSpecificGameplayLevel(numberInArray);
        }

        public void CloseMainMenuInGame()
        {
            if (!GameSceneManager.GSM.InGame)
            {
                currentMenu.MenuObject.SetActive(false);
                ShowSubMenu(0);
            }
            else
            {
                GameSceneManager.GSM.CloseMainMenuInGame();
            }
        }
    }
}

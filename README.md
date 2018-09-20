# CodeSamples Overview
A small collection of some of the scripts that I have written within the last year. All of this is game programming.

# Contents of repo
## GODSEND
Part of an 8-month capstone game created as part of school. I was responsible for AI and UI, mostly. 

* UI - Scene Management
  * GameSceneManager - manages async level (un)loading.
  * LevelSceneEditor - Inspector override editor script for setting up scene files for runtime accessing. v1.3 with better version in HorrorGame

* UI - Menus
  * InGameUIListener - Varient moderator interpretting inputs as menu navigation/requests
  * MenusSceneScript - Manages main menu screens (main menu, level select, options)
  * MenuOption - Base class for buttons/toggles/selectable UI elements

* UI - Gameplay
  * CanvasManager - central access point for anything UI related (e.g. points, hp/abilities, timers, etc)
  * ArenaManager - Controls the timer, and thus transitions at start and finish of game
  * Points Manager - Interprets user actions and assigns value economically. Queues points announcements
  * DeathScreen - YOU DIED overlay animation (a la Dark Souls)
  * GameOverScreen - Conclusion overlay for wins and losses 
  * MovesetUI - updates UI to show relevant abilities/health values

## HorrorGame
Proof of concept for a VR-styled point-and-click horror experience.

* Input Management/Visualization core
  * CameraInputMouse - Gathers mouse input for camera movement and item scrolling 
  * InteractionManager - Takes abstracted input from active input class, tells objects to interact and tells active visualizers what to display
  * VisualizeVia(Canvas) - Show/hide cursor, hints
  * TeleVisualizer - Have a line to teleport target location
* Gameplay
  * Inventory - Manages all held items, picking up/using items, and switching through them
  * PuzzleElement - A small interactable, vague enough to use locks and keys

* SceneManagement
  * BaseSceneManager - Loads scenes, restarts scenes, quits game
  * GameSceneManager - inherits from above, loads scenes async 
  * SceneFileReferences - serializable asset for referencing scene data at runtime
  * SceneFileReferences - v2.2 of LevelSceneEditor (from GODSEND). Much better of an interface that more tightly links with Unity's level manager

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseSceneManager : MonoBehaviour
{
    public virtual void ChangeScene() { }
    public virtual void AdvanceScene() { }
    public virtual void LoadLevelByName(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public virtual void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public virtual void QuitGameplay() { }
    public virtual void QuitApplication()
    {
        Application.Quit();
    }
}

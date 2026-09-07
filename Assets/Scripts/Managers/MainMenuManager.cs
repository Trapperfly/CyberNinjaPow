using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameState gameState;
    public void ContinueGame()
    {

    }
    public void StartNewGame()
    {
        SceneManager.LoadScene(1);
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void SetGameState(bool tutorial)
    {
        if (tutorial)
            gameState.SetToTutorialGame();
        else
            gameState.SetToRandomGame();
    }
}

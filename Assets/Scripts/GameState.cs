using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "GameState")]
[System.Serializable]
public class GameState : ScriptableObject
{
    public bool setSeed = true;
    public bool showTutorials = true;

    public void SetToRandomGame()
    {
        setSeed = false;
        showTutorials = false;
    }
    public void SetToTutorialGame()
    {
        setSeed = true;
        showTutorials = true;
    }
}

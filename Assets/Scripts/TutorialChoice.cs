using UnityEngine;

public class TutorialChoice : MonoBehaviour
{
    public Tutorials tutorial;

    public void ShowTutorial()
    {
        Manager.Instance.tutorialManager.ShowTutorial(tutorial, true);
    }
}

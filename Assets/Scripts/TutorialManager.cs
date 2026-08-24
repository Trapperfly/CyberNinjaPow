using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public bool doNotShowTutorials;
    public bool tutorialActive;

    public Tutorial currentTutorial;

    public Image background;
    public GameObject tutorialBox;
    public TMP_Text tutorialTitle;
    public TMP_Text tutorialInfo;

    public GameObject nextButton;
    public GameObject prevButton;
    public GameObject finishButton;

    public float tutorialScale;

    [Range(0,1)]public float backgroundFadeMax;

    [Range(0, 2)] public float fadeInTime, fadeOutTime;

    [Range(0, 1)] public float whenDuringFadeInShouldTutorialShowUp;

    [Range(0, 2)] public float tutorialInAnimationTime;
    public AnimationCurve tutorialInAnimation;

    [Range(0, 1)] public float whenDuringAnimationShouldBackgroundFadeOut;

    [Range(0, 2)] public float tutorialOutAnimationTime;
    public AnimationCurve tutorialOutAnimation;

    public List<Tutorial> tutorials;

    public List<Vector2Int> tutorialSizes;

    public void ToggleTutorials(bool toggle)
    {
        doNotShowTutorials = toggle;
    }

    public void ShowRandomTutorialForTesting()
    {
        currentTutorial = tutorials[0];
        StartCoroutine(IFadeIn());
    }
    public void ShowTutorial(Tutorials specificTutorial)
    {
        if (doNotShowTutorials) return;

        foreach (Tutorial tutorial in tutorials)
        {
            if (tutorial.shown) continue;
            if (tutorial.specificTutorial == specificTutorial)
            {
                currentTutorial = tutorial;
                StartCoroutine(IFadeIn());
            }
        }
    }
    public void HideTutorial()
    {
        StartCoroutine(IHideTutorial());
    }

    public IEnumerator IFadeIn()
    {
        background.gameObject.SetActive(true);
        float i = 0;
        while (i < fadeInTime)
        {
            i += Time.deltaTime;
            background.color = new(0, 0, 0, (i / fadeInTime) * backgroundFadeMax);

            if (i > fadeInTime * whenDuringFadeInShouldTutorialShowUp && !tutorialActive) StartCoroutine(IShowTutorial());
            yield return null;
        }
    }
    public IEnumerator IShowTutorial()
    {
        tutorialActive = true;
        float i = 0;
        tutorialTitle.text = currentTutorial.tutorialName;
        currentTutorial.currentStep = -1;
        NextTutorialSlide(1);
        //RectTransform tutorialBorder = (RectTransform)tutorialBox.transform.GetChild(0).transform;
        //tutorialBorder.sizeDelta = new Vector2Int(tutorialSizes[(int)tutorial.tutorialSize].x + 5, tutorialSizes[(int)tutorial.tutorialSize].y + 5);
        //RectTransform tutorialFill = (RectTransform)tutorialBox.transform.GetChild(1).transform;
        //tutorialFill.sizeDelta = new Vector2Int(tutorialSizes[(int)tutorial.tutorialSize].x, tutorialSizes[(int)tutorial.tutorialSize].y);
        //RectTransform tutorialTextBox = (RectTransform)tutorialInfo.transform;
        //tutorialTextBox.sizeDelta = new Vector2Int(tutorialSizes[(int)tutorial.tutorialSize].x - 55, tutorialSizes[(int)tutorial.tutorialSize].y - 35);

        currentTutorial.shown = true;

        while (i < tutorialInAnimationTime)
        {
            i += Time.deltaTime;
            tutorialScale = tutorialInAnimation.Evaluate(i / tutorialInAnimationTime);
            tutorialBox.transform.localScale = new(tutorialScale, tutorialScale);
            yield return null;
        }

        yield return null;
    }
    public void NextTutorialSlide(int direction)
    {
        currentTutorial.currentStep += direction;
        tutorialInfo.text = currentTutorial.steps[currentTutorial.currentStep].tutorialText;

        nextButton.SetActive(true);
        prevButton.SetActive(true);
        finishButton.SetActive(true);

        if (currentTutorial.currentStep == 0) //Is the first step
        {
            prevButton.SetActive(false);
        }
        
        if (currentTutorial.currentStep >= currentTutorial.steps.Count - 1) //On last step of tutorial
        {
            nextButton.SetActive(false);
        }
        else
        {
            finishButton.SetActive(false);
        }

        if (currentTutorial.steps.Count == 1) //Not single slide
        {
            prevButton.SetActive(false);
            nextButton.SetActive(false);
        }
    }
    public IEnumerator IHideTutorial()
    {
        float i = 0;
        while (i < tutorialOutAnimationTime)
        {
            i += Time.deltaTime;

            tutorialScale = tutorialOutAnimation.Evaluate(i / tutorialOutAnimationTime);
            tutorialBox.transform.localScale = new(tutorialScale, tutorialScale);

            if (i > tutorialOutAnimationTime * whenDuringAnimationShouldBackgroundFadeOut && tutorialActive) StartCoroutine(IFadeOut());
            yield return null;
        }
        currentTutorial.currentStep = 0;
    }

    public IEnumerator IFadeOut()
    {
        tutorialActive = false;
        float i = fadeOutTime;
        while (i > 0)
        {
            i -= Time.deltaTime;
            background.color = new(0, 0, 0, (i / fadeOutTime) * backgroundFadeMax);
            yield return null;
        }
        yield return null;
        background.gameObject.SetActive(false);
    }

    [System.Serializable]
    public class Tutorial
    {
        public Tutorials specificTutorial;
        public string tutorialName;
        public List<TutorialStep> steps;
        public int currentStep = 0;
        public bool shown;
    }
    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(5, 10)] public string tutorialText;
        public Vector2 pointAt;
    }
}
public enum Tutorials
{
    None,
    StartOfGame,
    DrawCards,
    WhenCardsAreDrawn,
    WhenCardIsHovered,
    WhenEnemyIsSpawned,
    WhenEnemyTakesDamage,
    WhenEnemyIsHovered,
    WhenEnemyIsInMeleeRange,
    WhenPlayerTakesDamage,
}

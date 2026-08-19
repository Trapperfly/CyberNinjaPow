using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public bool doNotShowTutorials;
    public bool tutorialActive;

    public Image background;
    public GameObject tutorialBox;
    public TMP_Text tutorialTitle;
    public TMP_Text tutorialInfo;

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
    public int currentTutorial = 0;

    public void ToggleTutorials(bool toggle)
    {
        doNotShowTutorials = toggle;
    }
    public void ShowTutorial()
    {
        if (doNotShowTutorials) return;

        StartCoroutine(IFadeIn(tutorials[currentTutorial]));
    }
    public void HideTutorial()
    {
        StartCoroutine(IHideTutorial());

        currentTutorial += 1;
    }

    public IEnumerator IFadeIn(Tutorial tutorial)
    {
        float i = 0;
        while (i < fadeInTime)
        {
            i += Time.deltaTime;
            background.color = new(0, 0, 0, (i / fadeInTime) * backgroundFadeMax);

            if (i > fadeInTime * whenDuringFadeInShouldTutorialShowUp && !tutorialActive) StartCoroutine(IShowTutorial(tutorial));
            yield return null;
        }
    }
    public IEnumerator IShowTutorial(Tutorial tutorial)
    {
        tutorialActive = true;
        float i = 0;
        tutorialTitle.text = tutorial.tutorialName;
        tutorialInfo.text = tutorial.tutorialText;
        while (i < tutorialInAnimationTime)
        {
            i += Time.deltaTime;
            tutorialScale = tutorialInAnimation.Evaluate(i / tutorialInAnimationTime);
            tutorialBox.transform.localScale = new(tutorialScale, tutorialScale);
            yield return null;
        }

        yield return null;
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
    }

    [System.Serializable]
    public class Tutorial
    {
        public string tutorialName;
        public string tutorialText;
        public TutorialSize tutorialSize;
    }

    public enum TutorialSize
    {
        Small,
        Medium,
        Large,
    }
}

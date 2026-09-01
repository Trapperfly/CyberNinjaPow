using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private List<Coroutine> infoCoroutines = new List<Coroutine>();

    [Space]

    public TMP_Text unitName;
    public TMP_Text unitMovement;
    public TMP_Text unitClass;
    public TMP_Text unitDamage;
    public Image spriteRenderer;

    [Space]

    public int score = 0;
    public int scoreMultiplier = 0;
    public TMP_Text threatText;
    public TMP_Text scoreText;
    public TMP_Text scoreMulti;

    [Space]

    public int gradeMax = 100;
    public int grade = 0;
    public TMP_Text gradeLetter;

    [Space]

    public int amount = 1;
    public int iterations = 1;
    public float textSpeed = 1;
    public float removeTextSpeed = 1;

    public bool removing;
    private void Start()
    {
        RemoveEnemyInfo();
        Threat(Manager.Instance.gameManager.maxThreat);
        Grade(0);
        Score(0);
    }

    public void Threat(float threat)
    {
        StartCoroutine(PrintWackyText(threatText, threat.ToString(), amount, iterations, textSpeed));
    }
    public void Grade(int gradeChange)
    {
        grade += gradeChange;

        if (grade < 0) { grade = 0; }
        if (grade > gradeMax) { grade = gradeMax; }

        if (grade < gradeMax * 0.25f && scoreMultiplier != 1)
        {
            scoreMultiplier = 1;
            StartCoroutine(RemoveWackyText(scoreMulti, removeTextSpeed));
            //StartCoroutine(PrintWackyText(scoreMulti, "X" + scoreMultiplier.ToString(), amount, iterations, textSpeed));
            StartCoroutine(PrintWackyText(gradeLetter, "F", amount, iterations, textSpeed));
        }
        else if (grade > gradeMax * 0.9f && scoreMultiplier != 20)
        {
            scoreMultiplier = 20;
            StartCoroutine(PrintWackyText(scoreMulti, "X" + scoreMultiplier.ToString(), amount, iterations, textSpeed));
            StartCoroutine(PrintWackyText(gradeLetter, "Z!", amount, iterations, textSpeed));
        }
        else if (grade > gradeMax * 0.8f && scoreMultiplier != 10)
        {
            scoreMultiplier = 10;
            StartCoroutine(PrintWackyText(scoreMulti, "X" + scoreMultiplier.ToString(), amount, iterations, textSpeed));
            StartCoroutine(PrintWackyText(gradeLetter, "A", amount, iterations, textSpeed));
        }
        else if (grade > gradeMax * 0.75f && scoreMultiplier != 6)
        {
            scoreMultiplier = 6;
            StartCoroutine(PrintWackyText(scoreMulti, "X" + scoreMultiplier.ToString(), amount, iterations, textSpeed));
            StartCoroutine(PrintWackyText(gradeLetter, "B", amount, iterations, textSpeed));
        }
        else if (grade > gradeMax * 0.6f && scoreMultiplier != 4)
        {
            scoreMultiplier = 4;
            StartCoroutine(PrintWackyText(scoreMulti, "X" + scoreMultiplier.ToString(), amount, iterations, textSpeed));
            StartCoroutine(PrintWackyText(gradeLetter, "C", amount, iterations, textSpeed));
        }
        else if (grade > gradeMax * 0.45f && scoreMultiplier != 3)
        {
            scoreMultiplier = 3;
            StartCoroutine(PrintWackyText(scoreMulti, "X" + scoreMultiplier.ToString(), amount, iterations, textSpeed));
            StartCoroutine(PrintWackyText(gradeLetter, "D", amount, iterations, textSpeed));
        }
        else if (grade > gradeMax * 0.25f && scoreMultiplier != 2)
        {
            scoreMultiplier = 2;
            StartCoroutine(PrintWackyText(scoreMulti, "X" + scoreMultiplier.ToString(), amount, iterations, textSpeed));
            StartCoroutine(PrintWackyText(gradeLetter, "E", amount, iterations, textSpeed));
        }
    }
    public void Score(int scoreChange)
    {
        score += scoreChange * scoreMultiplier;
        StartCoroutine(PrintWackyText(scoreText, score.ToString(), amount, iterations, textSpeed));
    }
    public void DisplayEnemyInfo(EnemyUnit unit)
    {
        //if (!removing) return;
        removing = false;
        foreach (Coroutine info in infoCoroutines) StopCoroutine(info);
        infoCoroutines.Add(StartCoroutine(PrintWackyText(unitName, unit.enemy.enemyName, amount, iterations, textSpeed)));
        infoCoroutines.Add(StartCoroutine(PrintWackyText(unitMovement, "Movement: " + Mathf.Abs(unit.enemy.movement).ToString() + (unit.enemy.movement < 0 ? " up" : " down"), amount, iterations, textSpeed)));
        infoCoroutines.Add(StartCoroutine(PrintWackyText(unitClass, "Class: " + unit.enemy.enemyClass.ToString(), amount, iterations, textSpeed)));
        infoCoroutines.Add(StartCoroutine(PrintWackyText(unitDamage, "Damage: " + unit.enemy.damage.ToString(), amount, iterations, textSpeed)));
        //unitName.text = unit.enemy.enemyName;
        spriteRenderer.sprite = unit.enemy.sprite[0];
        spriteRenderer.color = Color.white;
        //unitMovement.text = "Movement: " + Mathf.Abs(unit.enemy.movement).ToString() + (unit.enemy.movement < 0 ? " up" : " down");
        //unitClass.text = "Class: " + unit.enemy.enemyClass.ToString();
        //unitDamage.text = "Damage: " + unit.enemy.damage.ToString();
    }

    public void RemoveEnemyInfo()
    {
        if (removing) return;
        removing = true;
        foreach (Coroutine info in infoCoroutines) StopCoroutine(info);
        infoCoroutines.Add(StartCoroutine(RemoveWackyText(unitName, textSpeed)));
        infoCoroutines.Add(StartCoroutine(RemoveWackyText(unitMovement, textSpeed)));
        infoCoroutines.Add(StartCoroutine(RemoveWackyText(unitClass, textSpeed)));
        infoCoroutines.Add(StartCoroutine(RemoveWackyText(unitDamage, textSpeed)));
        //unitName.text = "";
        //unitMovement.text = "";
        //unitClass.text = "";
        //unitDamage.text = "";
        spriteRenderer.color = new(0, 0, 0, 0);
    }

    public IEnumerator PrintWackyText(TMP_Text where, string what, int amount, int iterations, float speed)
    {
        string result = "";
        string builder = "";
        int progress = 0;
        List<char> availableChars = new() { 'a', '/', '|', 'z', 'b', 'R', '-', 'v', 'Q', '4', '2', 'x', '^', '>', '@', '[' };
        //List<char> availableChars = new() { '_' };
        //for (int i = 0; i < progress; i++)
        //{
        //    result += what[i];
        //}
        foreach (char c in what)
        {
            for (int i = 0; i < amount; i++)
            {
                builder += availableChars[Random.Range(0, availableChars.Count)];
                //builder += availableChars[Random.Range(0, availableChars.Count)];
                where.text = result + builder;
                builder = "";
                yield return new WaitForSeconds(speed / amount);
            }
            result += what[progress];
            progress++;
            where.text = result;
            yield return new WaitForSeconds(speed / amount);
        }
        yield return null;
    }

    public IEnumerator RemoveWackyText(TMP_Text where, float speed)
    {
        string text = where.text;
        int progress = 0;
        //List<char> availableChars = new() { 'a', '/', '|', 'z', 'b', 'R', '-', 'v', 'Q', '4', '2', 'x', '^', '>', '@', '[' };
        //List<char> availableChars = new() { '_' };
        //for (int i = 0; i < progress; i++)
        //{
        //    result += what[i];
        //}
        foreach (char c in text)
        {
            string result = "";
            //for (int i = 0; i < progress; i++)
            //{
            //    result += " ";
            //}
            result += "_";
            for (int i = progress + 1; i < text.Length; i++)
            {
                result += text[i];
            }
            where.text = result;
            progress++;

            yield return new WaitForSeconds(removeTextSpeed);
        }
        where.text = "";
    }
}

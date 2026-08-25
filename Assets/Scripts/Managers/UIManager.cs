using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text unitName;
    public TMP_Text unitMovement;
    public TMP_Text unitClass;
    public TMP_Text unitDamage;
    public Image spriteRenderer;

    public int amount = 1;
    public int iterations = 1;
    public float textSpeed = 1;
    public float removeTextSpeed = 1;

    private void Start()
    {
        RemoveEnemyInfo();
    }
    public void DisplayEnemyInfo(EnemyUnit unit)
    {
        StopAllCoroutines();
        StartCoroutine(PrintWackyText(unitName, unit.enemy.enemyName, amount, iterations, textSpeed));
        StartCoroutine(PrintWackyText(unitMovement, "Movement: " + Mathf.Abs(unit.enemy.movement).ToString() + (unit.enemy.movement < 0 ? " up" : " down"), amount, iterations, textSpeed));
        StartCoroutine(PrintWackyText(unitClass, "Class: " + unit.enemy.enemyClass.ToString(), amount, iterations, textSpeed));
        StartCoroutine(PrintWackyText(unitDamage, "Damage: " + unit.enemy.damage.ToString(), amount, iterations, textSpeed));
        //unitName.text = unit.enemy.enemyName;
        spriteRenderer.sprite = unit.enemy.sprite[0];
        spriteRenderer.color = Color.white;
        //unitMovement.text = "Movement: " + Mathf.Abs(unit.enemy.movement).ToString() + (unit.enemy.movement < 0 ? " up" : " down");
        //unitClass.text = "Class: " + unit.enemy.enemyClass.ToString();
        //unitDamage.text = "Damage: " + unit.enemy.damage.ToString();
    }

    public void RemoveEnemyInfo()
    {
        if (unitName.text == "") return;
        StopAllCoroutines();
        StartCoroutine(RemoveWackyText(unitName, textSpeed));
        StartCoroutine(RemoveWackyText(unitMovement, textSpeed));
        StartCoroutine(RemoveWackyText(unitClass, textSpeed));
        StartCoroutine(RemoveWackyText(unitDamage, textSpeed));
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
        List<char> availableChars = new() { '_' };
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
            for (int i = progress; i < text.Length; i++)
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

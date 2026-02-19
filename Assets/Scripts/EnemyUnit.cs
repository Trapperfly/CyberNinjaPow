using UnityEngine;
using System.Collections.Generic;

public class EnemyUnit : MonoBehaviour
{
    public Vector2Int position;
    public Enemy enemy;
    public List<Intention> intentions;
    public int phase = 0;
    public int intention = 0;
    public int timer = 0;
    public int damageTaken = 0;
    public List<EffectInfo> effects;
    bool pong;

    public SpriteRenderer timerSpriteRenderer;
    public Sprite timerDot;
    public Sprite timerDanger;

    private void Start()
    {
        Manager.Instance.enemyManager.enemies.Add(this);
        intentions = enemy.enemyHealth[phase].intentions;

        Vector2 targetPosition = Manager.Instance.boardManager.spaces[position].transform.position;
        transform.localPosition = new Vector3(targetPosition.x, targetPosition.y, 0);
    }

    public void Timer()
    {
        int curTimer = enemy.enemyHealth[phase].intentions[intention].timer;
        if (curTimer > timer) {
            timer++;
            timerSpriteRenderer.size = new(curTimer - timer, 1);
            if (curTimer == timer)
            {
                timerSpriteRenderer.sprite = timerDanger;
                timerSpriteRenderer.size = new(1, 1);
            }
            else timerSpriteRenderer.sprite = timerDot;
            return; 
        }
        EffectOnTimer();
        Act();
        EffectOnAfterTimer();
        timer = 0;
        timerSpriteRenderer.sprite = timerDot;
        timerSpriteRenderer.size = new(curTimer - timer, 1);
    }

    public void Act()
    {
        EffectOnAct();
        Move();
        Attack();
        EffectOnAfterAct();
        switch (enemy.looping)
        {
            case IntentionLooping.none:
                if(intentions.Count - 1 > intention) intention += 1;
                break;
            case IntentionLooping.Loop:
                if (intentions.Count - 1 <= intention)
                    intention = 0;
                else intention += 1;
                break;
            case IntentionLooping.ReverseLoop:
                if (intention == 0)
                    intention = intentions.Count - 1;
                else intention -= 1;
                break;
            case IntentionLooping.PingPong:
                if (!pong)
                {
                    if (intentions.Count - 1 == intention)
                    {
                        pong = true;
                        intention -= 1;
                    }
                    else intention += 1;
                }
                else
                {
                    if (intention == 0)
                    {
                        pong = false;
                        intention += 1;
                    }
                    else intention -= 1;
                }
                break;
            case IntentionLooping.RepeatEnd:
                break;
            case IntentionLooping.Random:
                intention = Random.Range(0, intentions.Count);
                break;
            case IntentionLooping.RandomStart:
                break;
            default:
                break;
        }
    }

    public void EffectOnAct()
    {
        foreach (EffectInfo effect in effects)
        {
            effect.effect.OnAct(this);
        }
    }
    public void EffectOnAfterAct()
    {
        foreach (EffectInfo effect in effects)
        {
            effect.effect.OnAfterAct(this);
        }
    }
    public void EffectOnTimer()
    {
        foreach (EffectInfo effect in effects)
        {
            effect.effect.OnAct(this);
        }
    }
    public void EffectOnAfterTimer()
    {
        foreach (EffectInfo effect in effects)
        {
            effect.effect.OnAfterAct(this);
        }
    }

    public void Move()
    {
        if (Manager.Instance.enemyManager.CheckIfCellIsOccupied(position + enemy.enemyHealth[phase].intentions[intention].movement)) return;
        if (Manager.Instance.enemyManager.CheckIfCellIsOutsideOfBoard(position + enemy.enemyHealth[phase].intentions[intention].movement)) return;

        position += enemy.enemyHealth[phase].intentions[intention].movement;
        Vector2 targetPosition = Manager.Instance.boardManager.spaces[position].transform.position;
        transform.localPosition = new Vector3(targetPosition.x, targetPosition.y, 0);
    }

    public void Attack()
    {

    }
}

using UnityEngine;
using System.Collections.Generic;

public class EnemyUnit : MonoBehaviour
{
    public Vector2Int position;
    public Enemy enemy;
    public List<Intention> intentions;
    public int phase = 0;
    public int intention = 0;
    public int damageTaken = 0;
    public List<EffectInfo> effects;
    bool pong;

    private void Start()
    {
        Manager.Instance.enemyManager.enemies.Add(this);
        intentions = enemy.enemyHealth[phase].intentions;
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
                intention += 1;
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

    public void Move()
    {
        if (Manager.Instance.enemyManager.CheckIfCellIsOccupied(position + enemy.enemyHealth[phase].intentions[intention].movement)) return;

        position += enemy.enemyHealth[phase].intentions[intention].movement;
        transform.localPosition = new Vector3(position.x, position.y, 0);
    }

    public void Attack()
    {

    }
}

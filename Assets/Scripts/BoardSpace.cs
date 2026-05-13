using TMPro;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    public Vector2Int position;

    public GameObject damage;

    public GameObject cardTargeting;
    public GameObject cardAvailableTargeting;
    public GameObject enemyTargeting;
    public GameObject projectileTargeting;
    public void Colorize(GridSpaceSelection selectionMethod, int damageOnTile = 0)
    {
        switch (selectionMethod)
        {
            case GridSpaceSelection.None:
                cardTargeting.SetActive(false);
                enemyTargeting.SetActive(false);
                cardAvailableTargeting.SetActive(false);
                foreach (Transform child in projectileTargeting.transform)
                {
                    child.gameObject.SetActive(false);
                }
                Clear();
                break;

            case GridSpaceSelection.CardTargeting:
                cardTargeting.SetActive(true);

                if (damage != null)
                {
                    //Destroy(damage);
                    damage.GetComponent<TMP_Text>().text += "+" + damageOnTile.ToString();
                    return;
                }
                if (damageOnTile == 0) return;
                damage = Instantiate(Manager.Instance.boardManager.damageNumberForTilePrefab, Manager.Instance.boardManager.boardInformation);
                damage.transform.position = Camera.main.WorldToScreenPoint(transform.position) + (Vector3)Manager.Instance.boardManager.damageNumberOffset;

                damage.GetComponent<TMP_Text>().text = damageOnTile.ToString();
                break;
            case GridSpaceSelection.EnemyAttack:
                enemyTargeting.SetActive(true);

                if (damage != null)
                {
                    //Destroy(damage);
                    damage.GetComponent<TMP_Text>().text += "+" + damageOnTile.ToString();
                    return;
                }
                if (damageOnTile == 0) return;
                damage = Instantiate(Manager.Instance.boardManager.damageNumberForTilePrefab, Manager.Instance.boardManager.boardInformation);
                damage.transform.position = Camera.main.WorldToScreenPoint(transform.position) + (Vector3)Manager.Instance.boardManager.damageNumberOffset;

                damage.GetComponent<TMP_Text>().text = damageOnTile.ToString();
                break;
            case GridSpaceSelection.CardAvailableTargeting:
                cardAvailableTargeting.SetActive(true);
                break;
            case GridSpaceSelection.EnemyProjectileVertical:
                projectileTargeting.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case GridSpaceSelection.EnemyProjectileHorizontal:
                projectileTargeting.transform.GetChild(1).gameObject.SetActive(true);
                break;
            case GridSpaceSelection.EnemyProjectileDiagonal:
                projectileTargeting.transform.GetChild(2).gameObject.SetActive(true);
                break;
            case GridSpaceSelection.PlayerProjectileVertical:
                projectileTargeting.transform.GetChild(3).gameObject.SetActive(true);
                break;
            case GridSpaceSelection.PlayerProjectileHorizontal:
                projectileTargeting.transform.GetChild(4).gameObject.SetActive(true);
                break;
            case GridSpaceSelection.PlayerProjectileDiagonal:
                projectileTargeting.transform.GetChild(5).gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void Clear()
    {
        Destroy(damage);
    }
}
public enum GridSpaceSelection
{
    None,
    CardTargeting,
    EnemyAttack,
    CardAvailableTargeting,
    EnemyProjectileVertical,
    EnemyProjectileHorizontal,
    EnemyProjectileDiagonal,
    PlayerProjectileVertical,
    PlayerProjectileHorizontal,
    PlayerProjectileDiagonal,
}
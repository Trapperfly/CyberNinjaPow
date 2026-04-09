using TMPro;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    public Vector2Int position;

    public GameObject damage;

    public GameObject cardTargeting;
    public GameObject enemyTargeting;
    public void Colorize(GridSpaceSelection selectionMethod, int damageOnTile = 0)
    {
        switch (selectionMethod)
        {
            case GridSpaceSelection.None:
                cardTargeting.SetActive(false);
                enemyTargeting.SetActive(false);
                Clear();
                break;
            case GridSpaceSelection.CardTargeting:
                cardTargeting.SetActive(true);

                if (damage != null) Destroy(damage);
                damage = Instantiate(Manager.Instance.boardManager.damageNumberForTilePrefab, Manager.Instance.boardManager.boardInformation);
                damage.transform.position = Camera.main.WorldToScreenPoint(transform.position) + (Vector3)Manager.Instance.boardManager.damageNumberOffset;

                damage.GetComponent<TMP_Text>().text = damageOnTile.ToString();
                break;
            case GridSpaceSelection.EnemyAttack:
                enemyTargeting.SetActive(true);

                if (damage != null) Destroy(damage);
                damage = Instantiate(Manager.Instance.boardManager.damageNumberForTilePrefab, Manager.Instance.boardManager.boardInformation);
                damage.transform.position = Camera.main.WorldToScreenPoint(transform.position) + (Vector3)Manager.Instance.boardManager.damageNumberOffset;

                damage.GetComponent<TMP_Text>().text = damageOnTile.ToString();
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

}
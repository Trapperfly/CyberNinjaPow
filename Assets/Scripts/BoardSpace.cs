using TMPro;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    [HideInInspector] public SpriteRenderer spriteRenderer;
    public Vector2Int position;

    public GameObject damage;
    //public Hazard hazard;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Colorize(GridSpaceSelection selectionMethod, int damageOnTile = 0)
    {
        switch (selectionMethod)
        {
            case GridSpaceSelection.None:
                spriteRenderer.color = Color.white;
                Clear();
                break;
            case GridSpaceSelection.CardTargeting:
                spriteRenderer.color = Color.green;
                if (damage != null) Destroy(damage);
                damage = Instantiate(Manager.Instance.boardManager.damageNumberForTilePrefab, Manager.Instance.boardManager.boardInformation);
                damage.transform.position = Camera.main.WorldToScreenPoint(transform.position) + (Vector3)Manager.Instance.boardManager.damageNumberOffset;

                damage.GetComponent<TMP_Text>().text = damageOnTile.ToString();
                break;
            case GridSpaceSelection.EnemyAttack:
                spriteRenderer.color = Color.red;
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
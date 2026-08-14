using TMPro;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    public Vector2Int position;

    public GameObject damage;

    public GameObject targetDisplay;
    public GameObject projectileTargeting;
    public void Colorize(GridSpaceSelection selectionMethod, int damageOnTile = 0)
    {
        SpriteRenderer renderer = targetDisplay.transform.GetChild(0).GetComponent<SpriteRenderer>();
        switch (selectionMethod)
        {
            case GridSpaceSelection.None:
                targetDisplay.SetActive(false);
                projectileTargeting.SetActive(false);
                Clear();
                break;

            case GridSpaceSelection.CardTargeting:
                renderer.sprite = Manager.Instance.boardManager.targetingSprites[0];

                targetDisplay.SetActive(true);

                AddDamageNumber(damageOnTile);
                break;
            case GridSpaceSelection.EnemyAttack:
                renderer.sprite = Manager.Instance.boardManager.targetingSprites[1];

                targetDisplay.SetActive(true);

                //AddDamageNumber(damageOnTile);
                break;
            case GridSpaceSelection.AllyAttack:
                renderer.sprite = Manager.Instance.boardManager.targetingSprites[2];

                targetDisplay.SetActive(true);

                //AddDamageNumber(damageOnTile);
                break;
            case GridSpaceSelection.CardAvailableTargeting:
                renderer.sprite = Manager.Instance.boardManager.targetingSprites[3];

                targetDisplay.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void AddDamageNumber(int damageOnTile)
    {
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
    }

    public void RangedColorize(Vector2Int rangedDirection, GridSpaceSelection source, int damageOnTile = 0)
    {
        SpriteRenderer renderer = projectileTargeting.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (source == GridSpaceSelection.EnemyAttack)
        {
            if (rangedDirection.x == 0 && rangedDirection.y != 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[0];
            if (rangedDirection.x != 0 && rangedDirection.y == 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[1];
            if (rangedDirection.x != 0 && rangedDirection.y != 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[2];

        }
        if (source == GridSpaceSelection.CardTargeting)
        {
            if (rangedDirection.x == 0 && rangedDirection.y != 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[3];
            if (rangedDirection.x != 0 && rangedDirection.y == 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[4];
            if (rangedDirection.x != 0 && rangedDirection.y != 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[5];
        }
        if (source == GridSpaceSelection.AllyAttack)
        {
            if (rangedDirection.x == 0 && rangedDirection.y != 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[6];
            if (rangedDirection.x != 0 && rangedDirection.y == 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[7];
            if (rangedDirection.x != 0 && rangedDirection.y != 0) renderer.sprite = Manager.Instance.boardManager.projectileDisplays[8];
        }
        if ((rangedDirection.x < 0 && rangedDirection.y < 0) || (rangedDirection.x > 0 && rangedDirection.y > 0))
            renderer.flipX = false;
        else
            renderer.flipX = true;
        projectileTargeting.SetActive(true);
    }

    public void Clear()
    {
        if (!damage) return;
        damage.SetActive(false);
        Destroy(damage);
        damage = null;
    }
}
public enum GridSpaceSelection
{
    None,
    CardTargeting,
    EnemyAttack,
    AllyAttack,
    CardAvailableTargeting,
}
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    [HideInInspector] public SpriteRenderer spriteRenderer;
    public Vector2Int position;
    //public Hazard hazard;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Colorize(GridSpaceSelection selectionMethod)
    {
        switch (selectionMethod)
        {
            case GridSpaceSelection.None:
                spriteRenderer.color = Color.white;
                break;
            case GridSpaceSelection.CardTargeting:
                spriteRenderer.color = Color.green;
                break;
            case GridSpaceSelection.EnemyAttack:
                spriteRenderer.color = Color.red;
                break;
            default:
                break;
        }
    }
}
public enum GridSpaceSelection
{
    None,
    CardTargeting,
    EnemyAttack,

}
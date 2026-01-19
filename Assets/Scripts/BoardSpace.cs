using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    [HideInInspector] public SpriteRenderer spriteRenderer;
    public Vector2Int position;
    public Enemy enemy;
    //public Hazard hazard;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Colorize(bool target)
    {
        if (target) spriteRenderer.color = Color.green;
        if (!target) spriteRenderer.color = Color.white;
    }
}

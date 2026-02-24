using UnityEngine;

public class Manager : MonoBehaviour
{
    #region Singleton
    private static Manager _instance;
    public static Manager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        gameManager = GetComponent<GameManager>();
        boardManager = GetComponent<BoardManager>();
        enemyManager = GetComponent<EnemyManager>();
        deckManager = GetComponent<DeckManager>();
    }
    #endregion
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public BoardManager boardManager;
    [HideInInspector] public DeckManager deckManager;
    [HideInInspector] public EnemyManager enemyManager;

    public bool busy = false;
}

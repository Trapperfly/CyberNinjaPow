using UnityEngine;

public class Manager : MonoBehaviour
{
    #region Singleton
    private static Manager _instance;
    public static Manager Instance { get { return _instance; } }

    private void Awake()
    {
        //Random.InitState(10);
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
        shopManager = GetComponent<ShopManager>();
        itemManager = GetComponent<ItemManager>();
        playerManager = GetComponent<PlayerManager>();
    }
    #endregion
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public BoardManager boardManager;
    [HideInInspector] public DeckManager deckManager;
    [HideInInspector] public EnemyManager enemyManager;
    [HideInInspector] public ShopManager shopManager;
    [HideInInspector] public ItemManager itemManager;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public TutorialManager tutorialManager;

    public bool busy = false;
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Button deployButton;
    public int maxCards = 5;
    public float cardRespawnInterval = 10f; // 卡牌重新生成间隔
    public float deployCooldown = 1f; // 部署冷却时间

    private List<PlayerCard> availableCards = new List<PlayerCard>();
    private int totalCardsCreated = 0;
    private Coroutine respawnCoroutine;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        // 初始生成一些卡牌
        InitializeInitialCards();

        if (deployButton != null)
        {
            deployButton.onClick.AddListener(OnDeployButtonClicked);
            UpdateButtonState();
        }

        // 开始卡牌重生协程
        respawnCoroutine = StartCoroutine(CardRespawnRoutine());
    }

    void InitializeInitialCards()
    {
        int initialCards = Mathf.Min(3, maxCards); // 初始生成3张卡牌或最大数量
        for (int i = 0; i < initialCards; i++)
        {
            CreateNewCard();
        }
    }

    IEnumerator CardRespawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cardRespawnInterval);

            // 检查是否需要生成新卡牌
            if (availableCards.Count < maxCards)
            {
                CreateNewCard();
                Debug.Log($"生成新卡牌，当前可用卡牌: {availableCards.Count}/{maxCards}");
                UpdateButtonState();
            }
        }
    }

    void Update()
    {
        // 处理冷却时间
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                UpdateButtonState();
            }
        }

        // 更新按钮状态
        UpdateButtonState();
    }

    void CreateNewCard()
    {
        if (totalCardsCreated >= maxCards && maxCards > 0) return;

        GameObject cardObj = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        PlayerCard card = cardObj.GetComponent<PlayerCard>();

        // 设置卡牌管理器引用，以便卡牌死亡时通知管理器
        card.SetCardManager(this);

        card.GetComponent<SpriteRenderer>().enabled = false;
        availableCards.Add(card);
        totalCardsCreated++;
    }

    // 当卡牌被部署或死亡时调用
    public void OnCardStateChanged(PlayerCard card)
    {
        if (card.currentState == PlayerCard.CardState.Dead ||
            card.currentState == PlayerCard.CardState.Deployed)
        {
            // 从可用卡牌列表中移除已部署或死亡的卡牌
            if (availableCards.Contains(card))
            {
                availableCards.Remove(card);
            }

            UpdateButtonState();

            // 如果卡牌死亡，可以在这里触发重生计时器或立即生成新卡牌
            if (card.currentState == PlayerCard.CardState.Dead)
            {
                // 可以添加死亡特效或其他处理
                Debug.Log("卡牌死亡，将从池中移除");
            }
        }
    }

    void OnDeployButtonClicked()
    {
        if (!isOnCooldown && HasAvailableCards())
        {
            SelectCardForDeployment();
        }
    }

    void SelectCardForDeployment()
    {
        foreach (PlayerCard card in availableCards)
        {
            if (card.currentState == PlayerCard.CardState.ReadyToSpawn)
            {
                card.SpawnAtCenter();

                // 启动冷却时间
                StartCooldown();

                UpdateButtonState();
                break;
            }
        }
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = deployCooldown;
    }

    void UpdateButtonState()
    {
        if (deployButton == null) return;

        Text buttonText = deployButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            if (isOnCooldown)
            {
                buttonText.text = $"冷却中 ({cooldownTimer:F1}s)";
                deployButton.interactable = false;
            }
            else
            {
                buttonText.text = $"生成卡牌 ({availableCards.Count}/{maxCards})";
                deployButton.interactable = HasAvailableCards();
            }
        }
    }

    bool HasAvailableCards()
    {
        foreach (PlayerCard card in availableCards)
        {
            if (card.currentState == PlayerCard.CardState.ReadyToSpawn)
            {
                return true;
            }
        }
        return false;
    }

    // 公共方法，允许其他代码修改生成参数
    public float CardRespawnInterval
    {
        get => cardRespawnInterval;
        set
        {
            cardRespawnInterval = Mathf.Max(1f, value);
            // 重启协程以应用新的间隔
            if (respawnCoroutine != null)
                StopCoroutine(respawnCoroutine);
            respawnCoroutine = StartCoroutine(CardRespawnRoutine());
        }
    }

    public int MaxCards
    {
        get => maxCards;
        set => maxCards = Mathf.Max(1, value);
    }

    public float DeployCooldown
    {
        get => deployCooldown;
        set => deployCooldown = Mathf.Max(0f, value);
    }

    void OnDestroy()
    {
        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);
    }
}
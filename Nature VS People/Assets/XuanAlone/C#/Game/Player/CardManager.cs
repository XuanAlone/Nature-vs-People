using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Button deployButton;
    public int maxCards = 5;
    public float cardRespawnInterval = 10f; // �����������ɼ��
    public float deployCooldown = 1f; // ������ȴʱ��

    private List<PlayerCard> availableCards = new List<PlayerCard>();
    private int totalCardsCreated = 0;
    private Coroutine respawnCoroutine;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        // ��ʼ����һЩ����
        InitializeInitialCards();

        if (deployButton != null)
        {
            deployButton.onClick.AddListener(OnDeployButtonClicked);
            UpdateButtonState();
        }

        // ��ʼ��������Э��
        respawnCoroutine = StartCoroutine(CardRespawnRoutine());
    }

    void InitializeInitialCards()
    {
        int initialCards = Mathf.Min(3, maxCards); // ��ʼ����3�ſ��ƻ��������
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

            // ����Ƿ���Ҫ�����¿���
            if (availableCards.Count < maxCards)
            {
                CreateNewCard();
                Debug.Log($"�����¿��ƣ���ǰ���ÿ���: {availableCards.Count}/{maxCards}");
                UpdateButtonState();
            }
        }
    }

    void Update()
    {
        // ������ȴʱ��
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                UpdateButtonState();
            }
        }

        // ���°�ť״̬
        UpdateButtonState();
    }

    void CreateNewCard()
    {
        if (totalCardsCreated >= maxCards && maxCards > 0) return;

        GameObject cardObj = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        PlayerCard card = cardObj.GetComponent<PlayerCard>();

        // ���ÿ��ƹ��������ã��Ա㿨������ʱ֪ͨ������
        card.SetCardManager(this);

        card.GetComponent<SpriteRenderer>().enabled = false;
        availableCards.Add(card);
        totalCardsCreated++;
    }

    // �����Ʊ����������ʱ����
    public void OnCardStateChanged(PlayerCard card)
    {
        if (card.currentState == PlayerCard.CardState.Dead ||
            card.currentState == PlayerCard.CardState.Deployed)
        {
            // �ӿ��ÿ����б����Ƴ��Ѳ���������Ŀ���
            if (availableCards.Contains(card))
            {
                availableCards.Remove(card);
            }

            UpdateButtonState();

            // ����������������������ﴥ��������ʱ�������������¿���
            if (card.currentState == PlayerCard.CardState.Dead)
            {
                // �������������Ч����������
                Debug.Log("�������������ӳ����Ƴ�");
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

                // ������ȴʱ��
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
                buttonText.text = $"��ȴ�� ({cooldownTimer:F1}s)";
                deployButton.interactable = false;
            }
            else
            {
                buttonText.text = $"���ɿ��� ({availableCards.Count}/{maxCards})";
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

    // �����������������������޸����ɲ���
    public float CardRespawnInterval
    {
        get => cardRespawnInterval;
        set
        {
            cardRespawnInterval = Mathf.Max(1f, value);
            // ����Э����Ӧ���µļ��
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
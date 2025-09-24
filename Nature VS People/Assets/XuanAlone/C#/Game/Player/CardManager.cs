using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Button deployButton;
    public int maxCards = 5;

    private List<PlayerCard> availableCards = new List<PlayerCard>();
    private PlayerCard currentSelectedCard;

    void Start()
    {
        InitializeCardPool();

        if (deployButton != null)
        {
            deployButton.onClick.AddListener(OnDeployButtonClicked);
            UpdateButtonState();
        }
    }

    void InitializeCardPool()
    {
        for (int i = 0; i < maxCards; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
            PlayerCard card = cardObj.GetComponent<PlayerCard>();
            card.GetComponent<SpriteRenderer>().enabled = false;
            availableCards.Add(card);
        }
    }

    void OnDeployButtonClicked()
    {
        if (currentSelectedCard == null)
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
                currentSelectedCard = card;
                card.SpawnAtCenter();

                // 卡牌生成后，禁用按钮直到卡牌被部署或销毁
                if (deployButton != null)
                {
                    deployButton.interactable = false;
                }

                UpdateButtonState();
                break;
            }
        }
    }

    void Update()
    {
        // 监听卡牌状态变化，当卡牌被部署或销毁后重新激活按钮
        if (currentSelectedCard != null &&
            (currentSelectedCard.currentState == PlayerCard.CardState.Deployed ||
             currentSelectedCard.currentState == PlayerCard.CardState.Dead))
        {
            currentSelectedCard = null;
            if (deployButton != null)
            {
                deployButton.interactable = true;
            }
            UpdateButtonState();
        }

        // 更新按钮状态
        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        if (deployButton == null) return;

        Text buttonText = deployButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            if (currentSelectedCard == null)
            {
                buttonText.text = "生成卡牌";
                // 检查是否还有可用的卡牌
                deployButton.interactable = HasAvailableCards();
            }
            else
            {
                buttonText.text = "卡牌已生成 - 点击屏幕部署";
                deployButton.interactable = false;
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
}
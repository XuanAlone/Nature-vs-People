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
        // 初始化卡牌池
        InitializeCardPool();

        if (deployButton != null)
        {
            deployButton.onClick.AddListener(OnDeployButtonClicked);
            UpdateButtonText();
        }
    }

    void InitializeCardPool()
    {
        for (int i = 0; i < maxCards; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
            PlayerCard card = cardObj.GetComponent<PlayerCard>();
            card.GetComponent<SpriteRenderer>().enabled = false; // 初始隐藏
            availableCards.Add(card);
        }
    }

    void OnDeployButtonClicked()
    {
        if (currentSelectedCard == null)
        {
            // 选择一张卡牌生成在中心
            SelectCardForDeployment();
        }
        else
        {
            // 部署已生成的卡牌
            DeploySelectedCard();
        }
    }

    void SelectCardForDeployment()
    {
        // 查找可用的卡牌
        foreach (PlayerCard card in availableCards)
        {
            if (card.currentState == PlayerCard.CardState.ReadyToSpawn)
            {
                currentSelectedCard = card;
                card.SpawnAtCenter();
                UpdateButtonText();
                break;
            }
        }
    }

    void DeploySelectedCard()
    {
        if (currentSelectedCard != null && currentSelectedCard.currentState == PlayerCard.CardState.Spawned)
        {
            currentSelectedCard.DeployCard();
            currentSelectedCard = null;
            UpdateButtonText();
        }
    }

    void UpdateButtonText()
    {
        if (deployButton != null)
        {
            Text buttonText = deployButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                if (currentSelectedCard == null)
                {
                    buttonText.text = "生成卡牌";
                }
                else
                {
                    buttonText.text = "部署卡牌";
                }
            }
        }
    }
}
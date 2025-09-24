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
        // ��ʼ�����Ƴ�
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
            card.GetComponent<SpriteRenderer>().enabled = false; // ��ʼ����
            availableCards.Add(card);
        }
    }

    void OnDeployButtonClicked()
    {
        if (currentSelectedCard == null)
        {
            // ѡ��һ�ſ�������������
            SelectCardForDeployment();
        }
        else
        {
            // ���������ɵĿ���
            DeploySelectedCard();
        }
    }

    void SelectCardForDeployment()
    {
        // ���ҿ��õĿ���
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
                    buttonText.text = "���ɿ���";
                }
                else
                {
                    buttonText.text = "������";
                }
            }
        }
    }
}
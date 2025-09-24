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

                // �������ɺ󣬽��ð�ťֱ�����Ʊ����������
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
        // ��������״̬�仯�������Ʊ���������ٺ����¼��ť
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

        // ���°�ť״̬
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
                buttonText.text = "���ɿ���";
                // ����Ƿ��п��õĿ���
                deployButton.interactable = HasAvailableCards();
            }
            else
            {
                buttonText.text = "���������� - �����Ļ����";
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
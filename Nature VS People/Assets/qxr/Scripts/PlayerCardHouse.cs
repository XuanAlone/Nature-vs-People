using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardHouse : MonoBehaviour
{
    public List<CardBase> cardList = new List<CardBase>();//牌列表

    public GameObject cardPrefab;

    public Transform HousePanel;

    public List<GameObject> displayedCards = new List<GameObject>();//正在显示的牌列表


    //public event System.Action<CardBase> OnCardDrawn;//抽卡事件


    //public Transform HandCard;

    public void BtnToAddWind()
    {
        CardWind cardWind = new CardWind();
        cardList.Add(cardWind);
        UpdateCardDisplay();
    }

    public void BtnToAddWater()
    {
        CardWater cardWater = new CardWater();
        cardList.Add(cardWater);
        UpdateCardDisplay();
    }

    public void BtnToCardDian()
    {
        CardDian cardDian = new CardDian();
        cardList.Add(cardDian);
        UpdateCardDisplay();
    }

    public void BtnToCardSoil()
    { 
        CardSoil cardSoil = new CardSoil();
        cardList.Add(cardSoil);
        UpdateCardDisplay();
    }

    public void BenToCardSun()
    {
        CardSun cardSun = new CardSun();
        cardList.Add(cardSun);
        UpdateCardDisplay();
    }

    public void BtnToCardMouse()
    {
        CardMouse cardMouse = new CardMouse();
        cardList.Add(cardMouse);
        UpdateCardDisplay();
    }

    public void BtnToCardTiger()
    {
        CardTiger cardTiger = new CardTiger();
        cardList.Add(cardTiger);
        UpdateCardDisplay();
    }


    public void ShowCard()
    {


        foreach (var card in cardList)
        {
            GameObject thisCard=GameObject.Instantiate(cardPrefab,HousePanel);
            
            if (card is CardWind)
            {
                var theCard = card as CardWind;
                thisCard.GetComponent<CardDisplay>().card=theCard;
                displayedCards.Add(thisCard);
            }
            if (card is CardWater)
            {
                var theCard = card as CardWater;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                displayedCards.Add(thisCard);
            }
            if (card is CardDian)
            {
                var theCard = card as CardDian;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                displayedCards.Add(thisCard);
            }
            if (card is CardSoil)
            {
                var theCard = card as CardSoil;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                displayedCards.Add(thisCard);
            }
            if (card is CardSun)
            {
                var theCard = card as CardSun;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                displayedCards.Add(thisCard);
            }
            if (card is CardMouse)
            {
                var theCard = card as CardMouse;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                displayedCards.Add(thisCard);
            }
            if (card is CardTiger)
            {
                var theCard = card as CardTiger;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                displayedCards.Add(thisCard);
            }
        }
    }

    public void ClearDisplayedCards()
    {
        foreach (GameObject cardObj in displayedCards)
        {
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
        }
        displayedCards.Clear();
    }


    public void UpdateCardDisplay()
    {
        // 先清除所有已显示的卡牌
        ClearDisplayedCards();

        // 重新显示所有卡牌
        ShowCard();
    }

//按索引删除某个卡牌并更新显示
    public void RemoveCard(int index)
    {
        if (index >= 0 && index < cardList.Count)
        {
            cardList.RemoveAt(index);
            UpdateCardDisplay();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        BtnToAddWater();
        

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //随机抽取一张卡牌
    public CardBase NewCardToHand()
    {
        if (cardList.Count == 0)
        {
            Debug.LogWarning("卡牌列表为空，无法抽取");
            return null;
        }

        int randomIndex = Random.Range(0, cardList.Count);
        CardBase drawnCard = cardList[randomIndex];
        cardList.RemoveAt(randomIndex);

        // 更新显示
        UpdateCardDisplay();

        Debug.Log($"抽取并移除卡牌: {drawnCard.GetType().Name}，剩余卡牌: {cardList.Count}");
        return drawnCard;
    }
}

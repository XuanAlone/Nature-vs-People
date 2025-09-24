using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardWareHouse : MonoBehaviour
{
    public TextAsset cardData;
    public List<CardBase> cardList = new List<CardBase>();

    //public GameObject prefer;
    // Start is called before the first frame update
    void Start()
    {
        LoadCardData();
    }

    // Update is called once per frame
    void Update()
    {

    }


    //public void AddCardToWareHouse(CardBase card)
    //{

    //    if (card is CardWind)
    //    {
    //        GameObject newCard = GameObject.Instantiate(prefer);
    //        newCard.GetComponent<CardDisplay>().card = card as CardWind;

    //        cardList.Add(newCard);

    //    }
    //    if (card is CardWater)
    //    {
    //        var theCard = card as CardWater;
    //        cardList.Add(theCard);
    //    }
    //    if (card is CardDian)
    //    {
    //        var theCard = card as CardDian;
    //        cardList.Add(theCard);
    //    }
    //    if (card is CardSoil)
    //    {
    //        var theCard = card as CardSoil;
    //        cardList.Add(theCard);
    //    }
    //    if (card is CardSun)
    //    {
    //        var theCard = card as CardSun;
    //        cardList.Add(theCard);
    //    }
    //    if (card is CardMouse)
    //    {
    //        var theCard = card as CardMouse;
    //        cardList.Add(theCard);
    //    }
    //    if (card is CardTiger)
    //    {
    //        var theCard = card as CardTiger;
    //        cardList.Add(theCard);
    //    }
    //}

    //从CSV加载数据并分类型实例化牌
    public void LoadCardData()
    {
        string[] dataRow=cardData.text.Split('\n');
        foreach (var row in dataRow)
        {
            string[] rowArray = row.Split(',');
            if (rowArray[0] == "#")
            { continue; }
            else if (rowArray[0]=="风")
            {
                int id=int.Parse(rowArray[1]);
                int energy= int.Parse(rowArray[3]);

                CardWind cardWind=new CardWind();
                cardList.Add(cardWind);

                Debug.Log("读取到风" + cardWind.cardName);
            }
            else if (rowArray[0] == "水")
            {
                int id = int.Parse(rowArray[1]);
                int energy = int.Parse(rowArray[3]);

                CardWater cardWater=new CardWater();
                cardList.Add(cardWater);

            }
            else if (rowArray[0] == "雷")
            {
                int id = int.Parse(rowArray[1]);
                int energy = int.Parse(rowArray[3]);

                CardDian cardDian=new CardDian();
                cardList.Add(cardDian);
            }
            else if (rowArray[0] == "土")
            {
                int id = int.Parse(rowArray[1]);
                int energy = int.Parse(rowArray[3]);

                CardSoil cardSoil=new CardSoil();
                cardList.Add(cardSoil);
            }
            else if (rowArray[0] == "阳光")
            {
                int id = int.Parse(rowArray[1]);
                int energy = int.Parse(rowArray[3]);

                CardSun cardSun=new CardSun();
                cardList.Add(cardSun);
            }
            else if (rowArray[0] == "老鼠")
            {
                CardMouse cardMouse=new CardMouse();
                cardList.Add(cardMouse);
            }
            else if (rowArray[0] == "老虎")
            {
                CardTiger cardTiger=new CardTiger();
                cardList.Add(cardTiger);
            }
        }
    }

    public CardBase RandomCard()
    {
        CardBase card = cardList[Random.Range(0,cardList.Count)];

        return card;
    }
    
}

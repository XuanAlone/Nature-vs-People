using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewHandPanelCard : MonoBehaviour
{
    public List<CardBase> handCardList=new List<CardBase>();

    public CardBase currentDrawnCard;
    public PlayerCardHouse cardHouse;

    public Transform HandPanel;
    public GameObject prefer;
    // Start is called before the first frame update
    void Start()
    {
        cardHouse = FindObjectOfType<PlayerCardHouse>();



        // 检查是否成功找到引用
        if (cardHouse == null)
        {
            Debug.LogError("未找到PlayerCardHouse组件！");
        }
        else
        {
            Debug.Log("成功连接到卡牌仓库");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NewCard();
        }
    }
    public void NewCard()
    {
        if (cardHouse != null)
        {
            // 直接调用PlayerCardHouse的抽卡方法
            CardBase card = cardHouse.NewCardToHand();

            if (card != null)
            {
                GameObject thisCard = GameObject.Instantiate(prefer, HandPanel);
                if (card is CardWind)
                {
                    var theCard = card as CardWind;
                    thisCard.GetComponent<CardDisplay>().card = theCard;
                    
                }
                if (card is CardWater)
                {
                    var theCard = card as CardWater;
                    thisCard.GetComponent<CardDisplay>().card = theCard;
                    
                }
                if (card is CardDian)
                {
                    var theCard = card as CardDian;
                    thisCard.GetComponent<CardDisplay>().card = theCard;
                    
                }
                if (card is CardSoil)
                {
                    var theCard = card as CardSoil;
                    thisCard.GetComponent<CardDisplay>().card = theCard;
                    
                }
                if (card is CardSun)
                {
                    var theCard = card as CardSun;
                    thisCard.GetComponent<CardDisplay>().card = theCard;
                    
                }
                if (card is CardMouse)
                {
                    var theCard = card as CardMouse;
                    thisCard.GetComponent<CardDisplay>().card = theCard;
                    
                }
                if (card is CardTiger)
                {
                    var theCard = card as CardTiger;
                    thisCard.GetComponent<CardDisplay>().card = theCard;
                    
                }
            }
            else
            {
                Debug.LogWarning("抽卡失败，卡牌库可能为空");
            }
        }
        else
        {
            Debug.LogError("卡牌仓库未初始化");
        }
    }
    
}

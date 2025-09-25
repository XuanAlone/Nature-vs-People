using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Button LogeButton;
    public ImprovedLineSegmentController improvedLineSegmentController;
    public GameObject CDImage;
    public GameObject PlayButton;
    public Button  Button1;
    public GameObject DestoryCard;
    public GameObject CDreward;

    public TMP_Text cardName;
    public TMP_Text cardStyle;
    public TMP_Text cardEnergy;

    public Image cardImage;

    private Image cardImageCD;
    //public Image cardStyleBack;

    public GameObject cardImageSun;
    public GameObject cardImageWind;
    public GameObject cardImageWater;
   // public GameObject cardImageFire;
    public GameObject cardImageSoil;
    public GameObject cardImageDian;
    public GameObject cardImageMouse;
    public GameObject cardImageTiger;

    //public Image cardImageSunCD;
    //public Image cardImageWindCD;
    //public Image cardImageWaterCD;
    //public Image cardImageFireCD;
    //public Image cardImageSoilCD;
    //public Image cardImageDianCD;
    //public Image cardImageMouseCD;
    //public Image cardImageTigerCD;

    public CardBase card;
    // Start is called before the first frame update
    void Start()
    {
        improvedLineSegmentController=FindObjectOfType<ImprovedLineSegmentController>();
        
        
        ShowCard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //切换状态时调用该函数改变卡牌图像为CD状
    public void CardImageToCD()
    {
        cardImage = cardImageCD;
    }

    //根据该脚本中card字段的种类改变显示图像与cd图像
    public void ShowCard()
    {
        

        if (card is CardWind)
        {
            var theCard = card as CardWind;
            cardImageSun.SetActive(false);
            cardImageWind.SetActive(true);//
            cardImageWater.SetActive(false);
            cardImageDian.SetActive(false);
            cardImageSoil.SetActive(false);
            cardImageTiger.SetActive(false);
            cardImageMouse.SetActive(false);

            cardName.text = card.cardName;
            cardStyle.text = card.cardStyle;
            cardEnergy.text = card.cardEnergy;
            // 当按钮A被点击时，模拟按钮B也被点击
            Button1.onClick.AddListener(() => {
                // 这一行就是最直接的"按下按钮B"的代码
                improvedLineSegmentController.createSegmentButton.onClick.Invoke();
            });


        }
        if (card is CardWater)
        {
            var theCard = card as CardWater;
            cardImageSun.SetActive(false);
            cardImageWind.SetActive(false);
            cardImageWater.SetActive(true);//
            cardImageDian.SetActive(false);
            cardImageSoil.SetActive(false);
            cardImageTiger.SetActive(false);
            cardImageMouse.SetActive(false);

            cardName.text = card.cardName;
            cardStyle.text = card.cardStyle;
            cardEnergy.text = card.cardEnergy;
        }
        if (card is CardDian)
        {
            var theCard = card as CardDian;
            cardImageSun.SetActive(false);
            cardImageWind.SetActive(false);
            cardImageWater.SetActive(false);
            cardImageDian.SetActive(true);//
            cardImageSoil.SetActive(false);
            cardImageTiger.SetActive(false);
            cardImageMouse.SetActive(false);

            cardName.text = card.cardName;
            cardStyle.text = card.cardStyle;
            cardEnergy.text = card.cardEnergy;
        }
        if (card is CardSoil)
        {
            var theCard = card as CardSoil;
            cardImageSun.SetActive(false);
            cardImageWind.SetActive(false);
            cardImageWater.SetActive(false);
            cardImageDian.SetActive(false);
            cardImageSoil.SetActive(true);//
            cardImageTiger.SetActive(false);
            cardImageMouse.SetActive(false);

            cardName.text = card.cardName;
            cardStyle.text = card.cardStyle;
            cardEnergy.text = card.cardEnergy;

            // 当按钮A被点击时，模拟按钮B也被点击
            Button1.onClick.AddListener(() => {
                // 这一行就是最直接的"按下按钮B"的代码
                improvedLineSegmentController.createSegmentButton.onClick.Invoke();
            });


        }
        if (card is CardSun)
        {
            var theCard = card as CardSun;
            cardImageSun.SetActive(true);//
            cardImageWind.SetActive(false);
            cardImageWater.SetActive(false);
            cardImageDian.SetActive(false);
            cardImageSoil.SetActive(false);
            cardImageTiger.SetActive(false);
            cardImageMouse.SetActive(false);

            cardName.text = card.cardName;
            cardStyle.text = card.cardStyle;
            cardEnergy.text = card.cardEnergy;
        }
        if (card is CardMouse)
        {
            var theCard = card as CardMouse;
            cardImageSun.SetActive(false);
            cardImageWind.SetActive(false);
            cardImageWater.SetActive(false);
            cardImageDian.SetActive(false);
            cardImageSoil.SetActive(false);
            cardImageTiger.SetActive(false);
            cardImageMouse.SetActive(true);

            cardName.text = card.cardName;
            cardStyle.text = card.cardStyle;
            cardEnergy.text = card.cardEnergy;
        }
        if (card is CardTiger)
        {
            var theCard = card as CardTiger;
            cardImageSun.SetActive(false);
            cardImageWind.SetActive(false);
            cardImageWater.SetActive(false);
            cardImageDian.SetActive(false);
            cardImageSoil.SetActive(false);
            cardImageTiger.SetActive(true);
            cardImageMouse.SetActive(false);

            cardName.text = card.cardName;
            cardStyle.text = card.cardStyle;
            cardEnergy.text = card.cardEnergy;
        }

    }

    public void CdReward()
    {
        CDreward.SetActive(true);
        Invoke("CdRewardDestory", 2f);
    }

    public void CdRewardDestory()
    {
        CDreward.SetActive(false);
    }
    //执行卡牌逻辑
    public void BtnDoCard()
    {
        HandToCDManager.Instance.count += 1;
        if (HandToCDManager.Instance.count >= 5)
        { 
            CdReward();
            HandToCDManager.Instance.count -= 1;
            return;

        }
        if (card is CardWind)
        {
            var theCard = card as CardWind;


            //逻辑
            if (LogeButton is Button)
            {
                ButtonList.Instance.buttons.Add(LogeButton);
            }

            HandToCDManager.Instance.CDcards.Add(theCard);
            Debug.Log("jiaru");
            Destroy(DestoryCard);


            
        }
        if (card is CardWater)
        {
            var theCard = card as CardWater;
            HandToCDManager.Instance.CDcards.Add(theCard);
            Debug.Log("jiaru");
            Destroy(DestoryCard);
        }
        if (card is CardDian)
        {
            var theCard = card as CardDian;
            HandToCDManager.Instance.CDcards.Add(theCard);
            Destroy(DestoryCard);
        }
        if (card is CardSoil)
        {
            var theCard = card as CardSoil;
            HandToCDManager.Instance.CDcards.Add(theCard);
            Destroy(DestoryCard);
        }
        if (card is CardSun)
        {
            var theCard = card as CardSun;
            HandToCDManager.Instance.CDcards.Add(theCard);
            Destroy(DestoryCard);
        }
        if (card is CardMouse)
        {
            var theCard = card as CardMouse;
            HandToCDManager.Instance.CDcards.Add(theCard);
            Destroy(DestoryCard);
        }
        if (card is CardTiger)
        {
            var theCard = card as CardTiger;
            HandToCDManager.Instance.CDcards.Add(theCard);
            Destroy(DestoryCard);
        }
    }
}

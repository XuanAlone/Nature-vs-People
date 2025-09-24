using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDcard : MonoBehaviour
{
    public Transform Panel;
    public GameObject cardPre;


    public GameObject CD10;
    public GameObject CD5;

    //public void Cdfinish(thisCard)
    //{//
    //    thisCard.GetComponent<CardDisplay>().PlayButton.SetActive(true);
    //    thisCard.GetComponent<CardDisplay>().CDImage.SetActive(false);
    //}
    public void CDCard()

    {
        foreach (var card in HandToCDManager.Instance.CDcards)
        {
            GameObject thisCard = GameObject.Instantiate(cardPre, Panel);

            if (card is CardWind)
            {
                var theCard = card as CardWind;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                thisCard.GetComponent<CardDisplay>().PlayButton.SetActive(false);
                thisCard.GetComponent<CardDisplay>().CDImage.SetActive(true);
                //invoke(cdFinish)
                
            }
            if (card is CardWater)
            {
                var theCard = card as CardWater;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                //thisCard.GetComponent<CardDisplay>().PlayButton.SetActive(false);
                //thisCard.GetComponent<CardDisplay>().CDImage.SetActive(true);
            }
            if (card is CardDian)
            {
                var theCard = card as CardDian;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                thisCard.GetComponent<CardDisplay>().PlayButton.SetActive(false);
                thisCard.GetComponent<CardDisplay>().CDImage.SetActive(true);
            }
            if (card is CardSoil)
            {
                var theCard = card as CardSoil;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                thisCard.GetComponent<CardDisplay>().PlayButton.SetActive(false);
                thisCard.GetComponent<CardDisplay>().CDImage.SetActive(true);
            }
            if (card is CardSun)
            {
                var theCard = card as CardSun;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                thisCard.GetComponent<CardDisplay>().PlayButton.SetActive(false);
                thisCard.GetComponent<CardDisplay>().CDImage.SetActive(true);
            }
            if (card is CardMouse)
            {
                var theCard = card as CardMouse;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                thisCard.GetComponent<CardDisplay>().PlayButton.SetActive(false);
                thisCard.GetComponent<CardDisplay>().CDImage.SetActive(true);
            }
            if (card is CardTiger)
            {
                var theCard = card as CardTiger;
                thisCard.GetComponent<CardDisplay>().card = theCard;
                thisCard.GetComponent<CardDisplay>().PlayButton.SetActive(false);
                thisCard.GetComponent<CardDisplay>().CDImage.SetActive(true);
            }
        }
    }
}

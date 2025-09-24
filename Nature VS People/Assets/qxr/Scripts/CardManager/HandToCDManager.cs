using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandToCDManager 
{
    private static HandToCDManager instance;
    public static HandToCDManager Instance
    {
        get
        {
            if (instance == null)
                instance = new HandToCDManager();
            return instance;
         }
    }

    private HandToCDManager() { }



    //CD的手牌列表
    public  List<CardBase> CDcards=new List<CardBase>();
    //卡槽计数
    public int count=0;
}

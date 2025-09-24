using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardBase
{
    public string cardStyle;
    public string cardName;
    public string cardEnergy;

    
}


public class CardWind : CardBase
{
    public CardWind() 
    {
        this.cardStyle = "环境";
        this.cardName = "风";
        this.cardEnergy = "5";
    }
}

public class CardDian : CardBase
{
    public CardDian()
    {
        this.cardStyle = "环境";
        this.cardName = "雷电";
        this.cardEnergy = "5";
    }
}

public class CardWater : CardBase
{
    public CardWater()
    {
        this.cardStyle = "环境";
        this.cardName = "雨水";
        this.cardEnergy = "5";
    }
}

public class CardSoil : CardBase
{
    public CardSoil()
    {
        this.cardStyle = "环境";
        this.cardName = "沙地";
        this.cardEnergy = "5";
    }
}

public class CardSun : CardBase
{
    public CardSun()
    {
        this.cardStyle = "环境";
        this.cardName = "阳光";
        this.cardEnergy = "5";
    }
}

public class CardMouse : CardBase
{
    public    CardMouse()
    {
        this.cardStyle = "动物";
        this.cardName = "老鼠";
        this.cardEnergy = "2";
    }
}

public class CardTiger : CardBase
{
     public   CardTiger()
    {
        this.cardStyle = "动物";
        this.cardName = "老虎";
        this.cardEnergy = "2";
    }
}





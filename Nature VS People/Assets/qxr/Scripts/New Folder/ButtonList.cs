using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonList 
{
    private ButtonList()
    { }
    private static ButtonList instance;
    public static ButtonList Instance
    {
        get {
            return instance;
        }
    }


    public List<Button> buttons = new List<Button>();
}

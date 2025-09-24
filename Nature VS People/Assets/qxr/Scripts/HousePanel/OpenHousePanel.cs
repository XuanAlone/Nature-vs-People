using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenHousePanel : MonoBehaviour
{

    public GameObject house;
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BtnToShowHouse()
    {
        house.transform.position = target.transform.position;
    }
}

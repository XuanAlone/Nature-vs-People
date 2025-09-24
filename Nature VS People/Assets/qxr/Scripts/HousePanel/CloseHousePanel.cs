using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseHousePanel : MonoBehaviour
{
    public GameObject house;
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        house.transform.position = target.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void BtnToCloseHouse()
    {
        house.transform.position=target.transform.position;
    }
}

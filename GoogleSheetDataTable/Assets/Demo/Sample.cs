using System.Collections;
using System.Collections.Generic;
using DataTables;
using UnityEngine;

public class Sample : MonoBehaviour
{
    void Start()
    {
        // Access by Unique Key
        var firstFood = FoodsTable.Instance.GetDataById(0); 
        
        Debug.Log(firstFood.Name);
        Debug.Log(firstFood.CanBuy);
    }
}

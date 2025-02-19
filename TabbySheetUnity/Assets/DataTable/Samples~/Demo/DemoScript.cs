using TabbySheet;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    void Start()
    {
        TabbyDataSheet.Init();
        
        var foodsTable = DataSheet.Load<FoodsTable>();
        
        foreach (var data in foodsTable)
            Debug.Log(data.Name);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThangChau : ThangCon
{
    private void Start()
    {
        txt = "thang chau";
        Test2();
    }

    protected override void Test()
    {
        base.Test();
        Debug.Log("Thang Chau");
    }
}

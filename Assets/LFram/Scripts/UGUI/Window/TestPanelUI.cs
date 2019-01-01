using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPanelUI : PanelBase 
{
    private TestPanel testPanel;
    public override void OnInit(Hashtable param)
    {
        base.OnInit(param);

        testPanel = PanelObj.GetComponent<TestPanel>();
        AddButtonClickListener(testPanel.btn1, OnBtn1Click);
        AddButtonClickListener(testPanel.btn2, OnBtn2Click);
        AddButtonClickListener(testPanel.btn3, OnBtn3Click);
    }
	

    void OnBtn1Click()
    {
        Debug.LogError("按钮1被点击");
    }
    void OnBtn2Click()
    {
        Debug.LogError("按钮2被点击");
    }
    void OnBtn3Click()
    {
        Debug.LogError("按钮3被点击");
    }
}

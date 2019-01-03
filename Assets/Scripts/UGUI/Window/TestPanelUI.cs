using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        GameObject go = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab",true);
        ObjectManager.Instance.DisposeObject(go);
        ObjectManager.Instance.ClearCache();

        string imagePath1 = "Assets/GameData/Texture/test1.png";
        string imagePath2 = "Assets/GameData/Texture/test2.png";

        ResourceManager.Instance.AsyncLoadResource(imagePath1, OnLoadSpriteTest1, LoadResPriority.RES_SLOW,isSprite:true);
        ResourceManager.Instance.AsyncLoadResource(imagePath2, OnLoadSpriteTest2, LoadResPriority.RES_MIDDLE,isSprite:true);
    }


   

    //异步加载图片完成回调
	void OnLoadSpriteTest1(string path, UnityEngine.Object obj, Hashtable param)
	{
		if(obj != null)
		{
			Sprite sp = obj as Sprite;
			testPanel.test1.sprite = sp;
		}
	}

    //异步加载图片完成回调
	void OnLoadSpriteTest2(string path, UnityEngine.Object obj, Hashtable param)
	{
		if(obj != null)
		{
			Sprite sp = obj as Sprite;
			testPanel.test2.sprite = sp;
		}
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

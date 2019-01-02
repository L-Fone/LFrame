using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : PanelBase 
{

	private LoadingPanel mainPanel;
	private string mSceneName;

	public override void OnInit(Hashtable param)
	{
		base.OnInit(param);

		mainPanel = PanelObj.GetComponent<LoadingPanel>();

		mSceneName = (string)param["curscene"];
	}


	public override void OnUpdate()
	{
		base.OnUpdate();

		if(mainPanel == null)
		{
			return;
		}
		mainPanel.slider.value = GameMapManager.LoadingProgress*1f / 100.0f;
		mainPanel.text.text = string.Format("{0}%", GameMapManager.LoadingProgress);

		if(GameMapManager.LoadingProgress >= 100)
		{
			LoadOtherScene();
		}
	}


	/// <summary>
	/// 切换其他场景 加载第一个UI
	/// </summary>
	public void LoadOtherScene()
	{	
		//根据场景名字打开对应场景第一个界面

		if(mSceneName == GameConst.SCENE_MENU)
		{
			UIMgr.Instance.OpenWindow(GameConst.UI_TEST);
		}

		UIMgr.Instance.ClosePanel(GameConst.UI_LOADING);
	}
	
}

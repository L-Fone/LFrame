using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI管理器
/// </summary>
public class UIMgr : Singleton<UIMgr> 
{
	//UI节点
	private RectTransform uiRoot;
	//窗口节点
	private RectTransform windowRoot;
	//UI摄像机
	private Camera uiCamera;
	//EventSystem
	private EventSystem uiEventSystem;
	//屏幕分辨率[宽高比]
	private float canvasRate = 0;


	//注册所有的UI面板
	private Dictionary<string, System.Type> RegisterPanelDict = new Dictionary<string, System.Type>();
	//所有打开的窗口
	private Dictionary<string, PanelBase> mOpenPanelDict = new Dictionary<string, PanelBase>();
	//缓存窗口
	private List<PanelBase> mPanelList = new List<PanelBase>();

	/* ------------------------------------------初始化-------------------------------------------------------- */

	/// <summary>
	/// 初始化
	/// </summary>
	public void Init(RectTransform uiroot, RectTransform windowroot, Camera uicamera, EventSystem eventsys)
	{
		this.uiRoot = uiroot;
		this.windowRoot = windowroot;
		this.uiCamera = uicamera;
		this.uiEventSystem = eventsys;
		this.canvasRate = Screen.height / (uicamera.orthographicSize * 2);

		RegisterUI();
	}	

	/* ------------------------------------------注册UI-------------------------------------------------------- */

	/// <summary>
	/// 注册UI
	/// </summary>
	public void RegisterUI()
	{
		Register<TestPanelUI>(GameConst.UI_TEST);
		Register<LoadingUI>(GameConst.UI_LOADING);



		Debug.LogError("RegisterUI Done!!");
	}

 
	/* ------------------------------------------功能 API-------------------------------------------------------- */

	/// <summary>
	/// 打开窗口
	/// </summary>
	/// <param name="uiName">UIPanelName</param>
	/// <param name="top">是否要在最顶层</param>
	/// <param name="param">传入的参数</param>
	/// <returns></returns>
	public PanelBase OpenWindow(string uiName, bool top = true, Hashtable param = null)
	{
		PanelBase panel = FindPanelByName<PanelBase>(uiName);
		if(panel == null)
		{
			//加载
			System.Type tp = null;
			if(RegisterPanelDict.TryGetValue(uiName, out tp))
			{
				//当查找到了则创建出类
				panel = System.Activator.CreateInstance(tp) as PanelBase;
			}
			else
			{
				Debug.LogError("找不到窗口对应的脚本，窗口名：" + uiName);		
				return null;		
			}

			//同步实例化出UI
			GameObject panelObj = ObjectManager.Instance.InstantiateObject(PathConst.UI_PANEL_PATH+uiName, false, false);
			if(panelObj == null)
			{
				Debug.LogError("创建窗口Prefab失败：" + uiName);
			}

			if(!mOpenPanelDict.ContainsKey(uiName))
			{
				mPanelList.Add(panel);
				mOpenPanelDict.Add(uiName, panel);
			}	

			panel.PanelObj = panelObj;
			panel.PanelTrans = panelObj.transform;
			panel.PanelName = uiName;
			panel.OnInit(param);
			panelObj.transform.SetParent(windowRoot, false);

			//设置层级
			if(top)
			{
				panelObj.transform.SetAsLastSibling();
			}

			panel.OnShow(param);
		}
		else
		{
			ShowPanel(uiName, top, param);
		}

		return panel;
	}

	/// <summary>
	/// 根据窗口名字显示窗口
	/// </summary>
	/// <param name="name"></param>
	/// <param name="param"></param>
	private void ShowPanel(string name, bool top = true, Hashtable param = null)
	{
		PanelBase panel = FindPanelByName<PanelBase>(name);
		ShowPanel(panel, top, param);
	}

	/// <summary>
	/// 根据窗口对象显示窗口
	/// </summary>
	/// <param name="name"></param>
	/// <param name="param"></param>
	private void ShowPanel(PanelBase panel, bool top = true, Hashtable param = null)
	{
		if(panel != null)
		{
			if(panel.PanelObj != null && !panel.PanelObj.activeSelf)
			{
				panel.PanelObj.SetActive(true);
			}
			//设置层级
			if(top)
			{
				panel.PanelObj.transform.SetAsLastSibling();
			}
			panel.OnShow(param);
		}
	}

	/// <summary>
	/// 根据名字关闭窗口
	/// </summary>
	/// <param name="uiName"></param>
	/// <param name="destroy"></param>
	public void ClosePanel(string uiName, bool destroy = false)
	{
		PanelBase panel = FindPanelByName<PanelBase>(uiName);
		ClosePanel(panel, destroy);
	}

	/// <summary>
	/// 根据对象关闭窗口
	/// </summary>
	/// <param name="panel"></param>
	/// <param name="destroy"></param>
	public void ClosePanel(PanelBase panel, bool destroy = false)
	{
		if(panel != null)
		{
			panel.OnDisable();
			panel.OnClose();
			if(mOpenPanelDict.ContainsKey(panel.PanelName))
			{
				mOpenPanelDict.Remove(panel.PanelName);
				mPanelList.Remove(panel);
			}

			if(destroy)
			{
				ObjectManager.Instance.DisposeObject(panel.PanelObj, 0, true);
			}
			else
			{
				ObjectManager.Instance.DisposeObject(panel.PanelObj, recyclaParent:false);
			}
			panel.PanelObj = null;
			panel = null;
		}
	}


	/// <summary>
	/// 关闭所有UI
	/// </summary>
	public void CloseAllPanel()
	{
		for (int i = mPanelList.Count - 1; i >= 0 ; i--)
		{
			ClosePanel(mPanelList[i]);
		}
	}

	/// <summary>
	/// 切换到唯一窗口
	/// </summary>
	public void SwitchStateByName(string uiName, bool top, Hashtable param)
	{
		CloseAllPanel();
		OpenWindow(uiName, top, param);
	}

	/// <summary>
	/// 隐藏窗口面板
	/// </summary>
	/// <param name="uiName"></param>
	public void HidePanel(string uiName)
	{
		PanelBase panel = FindPanelByName<PanelBase>(uiName);
		HidePanel(panel);
	}

	/// <summary>
	/// 隐藏窗口
	/// </summary>
	/// <param name="panel"></param>
	public void HidePanel(PanelBase panel)
	{
		if(panel != null)
		{
			panel.PanelObj.SetActive(false);
			panel.OnDisable();
		}
	}

	/// <summary>
	/// 显示或者隐藏所有UI
	/// </summary>
	public void ShowOrHideUI(bool show)
	{
		if(uiRoot != null)
		{
			uiRoot.gameObject.SetActive(show);
		}
	}

	/// <summary>
	/// 设置默认选择的UI对象
	/// </summary>
	/// <param name="obj"></param>
	public void SetNormalSelectObj(GameObject obj)
	{
		if(uiEventSystem == null)
		{
			uiEventSystem = EventSystem.current;
		}
		uiEventSystem.firstSelectedGameObject = obj;
	}

	/// <summary>
	/// 更新
	/// </summary>
	public void OnUpdate()
	{
		for (int i = 0; i < mPanelList.Count; i++)
		{
			if(mPanelList[i] != null)
			{
				mPanelList[i].OnUpdate();
			}
		}
	}

	/// <summary>
	/// 给UI发送消息
	/// </summary>
	/// <param name="uiName">窗口名</param>
	/// <param name="msgID">消息ID</param>
	/// <param name="param">传入的参数</param>
	/// <returns></returns>
	public bool SendMessageToUI(string uiName, UIMsgID msgID = 0, Hashtable param = null)
	{
		PanelBase panel = FindPanelByName<PanelBase>(uiName);
		if(panel != null)
		{
			return panel.OnMessage(msgID,param);
		}
		return false;
	}
	

	/* ------------------------------------------泛型方法-------------------------------------------------------- */


	/// <summary>
	/// 窗口注册方法
	/// </summary>
	/// <param name="name">窗口名</param>
	/// <typeparam name="T">窗口泛型类</typeparam>
	public void Register<T>(string name) where T : PanelBase
	{
		RegisterPanelDict[name] = typeof(T);
	}

	/// <summary>
	/// 通过名字查找到UIPanel
	/// </summary>
	/// <param name="name">UI面板名字</param>
	/// <typeparam name="T">返回出去的UI面板</typeparam>
	/// <returns></returns>
	public T FindPanelByName<T>(string name) where T : PanelBase
	{
		PanelBase pb = null;
		if(mOpenPanelDict.TryGetValue(name, out pb))
		{
			return (T)pb;
		}
		return null;
	}
}

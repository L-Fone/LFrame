using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI面板类
/// </summary>
public class PanelBase 
{
	//名字
	public string PanelName{ get; set; }
	//引用GameObject
	public GameObject PanelObj{ get; set; }
	//引用Transform
	public Transform PanelTrans{ get; set;}
	//所有的按钮
	protected List<Button> AllButton = new List<Button>();
	protected List<Toggle> AllToggle = new List<Toggle>();

	//UI接受消息
	public virtual bool OnMessage(UIMsgID msgID, Hashtable param)
	{
		return true;
	}

	//生命周期函数
	public virtual void OnInit(Hashtable param){}
	public virtual void OnShow(Hashtable param){}
	public virtual void OnUpdate(){}
	public virtual void OnDisable(){}

	public virtual void OnClose()
	{
		RemoveAllButtonListener();
		RemoveAllToggleListener();
		AllButton.Clear();
		AllToggle.Clear();
	}

	/* -----------------------------------事件相关----------------------------------------------- */

	/// <summary>
	/// 移除所有按钮的事件监听
	/// </summary>
	public void RemoveAllButtonListener()
	{
		foreach (Button btn in AllButton)
		{
			btn.onClick.RemoveAllListeners();
		}
	}

	/// <summary>
	/// 移除所有Toggle事件
	/// </summary>
	public void RemoveAllToggleListener()
	{
		foreach (Toggle btn in AllToggle)
		{
			btn.onValueChanged.RemoveAllListeners();
		}
	}

	/// <summary>
	/// 添加按钮点击事件
	/// </summary>
	public void AddButtonClickListener(Button btn, UnityEngine.Events.UnityAction action)
	{
		if(btn != null)
		{
			if(!AllButton.Contains(btn))
			{
				AllButton.Add(btn);
			}
			btn.onClick.RemoveAllListeners();
			btn.onClick.AddListener(action);			
			btn.onClick.AddListener(ButtonPlaySound);
		}
	}

	/// <summary>
	/// 添加Toggle点击事件
	/// </summary>
	public void AddToggleClickListener(Toggle toggle, UnityEngine.Events.UnityAction<bool> action)
	{
		if(toggle != null)
		{
			if(!AllToggle.Contains(toggle))
			{
				AllToggle.Add(toggle);
			}
			toggle.onValueChanged.RemoveAllListeners();
			toggle.onValueChanged.AddListener(action);			
			toggle.onValueChanged.AddListener(TogglePlaySound);
		}
	}

	/* --------------------------------------常用API------------------------------------------------ */

	/// <summary>
	/// 同步替换图片
	/// </summary>
	/// <param name="path"></param>
	/// <param name="image"></param>
	/// <param name="setNativeSize"></param>
	public bool ChangeImageSprite(string path, Image image, bool setNativeSize = false)
	{
		if(image == null)
		{
			return false;
		}
		Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
		if(sp != null)
		{
			if(image.sprite != null)
			{
				image.sprite = null;
			}
			image.sprite = sp;
			if(setNativeSize)
			{
				image.SetNativeSize();
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// 异步替换图片
	/// </summary>
	/// <param name="path"></param>
	/// <param name="image"></param>
	/// <param name="setNativeSize"></param>
	/// <returns></returns>
	public void ChangeImageSpriteAsync(string path, Image image, bool setNativeSize = false)
	{
		if(image == null)
		{
			return;
		}
		Hashtable param = new Hashtable();
		param["image"] = image;
		param["native"] = setNativeSize;
		ResourceManager.Instance.AsyncLoadResource(path, OnLoadSpriteFinish, LoadResPriority.RES_MIDDLE, param);
		
	}

	//异步加载图片完成回调
	void OnLoadSpriteFinish(string path, Object obj, Hashtable param)
	{
		if(obj != null)
		{
			Sprite sp = obj as Sprite;
			Image image = (Image)param["image"];
			bool setNativeSize = (bool)param["native"];

			if(image.sprite != null)
			{
				image.sprite = null;
			}
			image.sprite = sp;
			if(setNativeSize)
			{
				image.SetNativeSize();
			}
		}
	}

	/* --------------------------------------按钮声音事件--------------------------------------------- */

	/// <summary>
	/// 播放Button点击声音
	/// </summary>
	void ButtonPlaySound()
	{

	}

	/// <summary>
	/// 播放toggle声音
	/// </summary>
	/// <param name="state"></param>
	void TogglePlaySound(bool state)
	{

	}
}


/// <summary>
/// 发送消息给UI
/// </summary>
public enum UIMsgID
{
	None = 0,
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameStart : MonoBehaviour 
{

	public AudioSource audio;
	AudioClip clip;

	private GameObject obj = null;

	private void Awake() 
	{
		DontDestroyOnLoad(this);
		//加载配置表
		AssetBundleManager.Instance.LoadAssetBundleConfig();
		ResourceManager.Instance.Init(this);
		//初始化场景Manager
		GameMapManager.Instance.Init(this);
		Transform recycla = this.transform.Find("RecyclaPool").transform;
		Transform sceneTrs = this.transform.Find("SceneTrs").transform;
		ObjectManager.Instance.Init(recycla, sceneTrs);

		

		//初始化UI
		RectTransform uiRoot = this.transform.Find("UIRoot") as RectTransform;
		RectTransform windowRoot = uiRoot.transform.Find("Window") as RectTransform;
		Camera uiCamera = uiRoot.transform.Find("UICamera").GetComponent<Camera>();
		EventSystem evets = uiRoot.transform.Find("EventSystem").GetComponent<EventSystem>();
		UIMgr.Instance.Init(uiRoot, windowRoot, uiCamera, evets);

		RegisterUI();
	}	

	/* ------------------------------------------注册UI-------------------------------------------------------- */

	/// <summary>
	/// 注册UI
	/// </summary>
	public void RegisterUI()
	{
		UIMgr.Instance.Register<TestPanelUI>(GameConst.UI_TEST);
		UIMgr.Instance.Register<LoadingUI>(GameConst.UI_LOADING);



		Debug.Log("Register All UI Done!!");
	}

	void Start () 
	{
		//资源同步加载
		// clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
		// audio.clip = clip;
		// audio.Play();

		/* ----------------- */

		//资源异步加载
		//ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/menusound.mp3", OnLoadFinish, LoadResPriority.RES_MIDDLE);

		//同步实例化资源
		//obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);

		//异步实例化资源
		//ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/Attack.prefab", OnInstantiateFinish, LoadResPriority.RES_HIGHT,null,true);

		//预加载[实例化]资源
		//ObjectManager.Instance.PreLoadGameObject("Assets/GameData/Prefabs/Attack.prefab", 10);

		//加载UI
		
	}

	//资源异步加载回调
	// void OnLoadFinish(string path, Object obj, Hashtable hh)
	// {
	// 	audio.clip = obj as AudioClip;
	// 	audio.Play();
	// }

	//异步实例化回调
	void OnInstantiateFinish(string path, Object obj, Hashtable param = null)
	{
		this.obj = obj as GameObject;
	}

	private void Update() 
	{
		//UI的Update
		UIMgr.Instance.OnUpdate();

		if(Input.GetKeyDown(KeyCode.A))
		{
			// audio.Stop();
			// audio.clip = null;
			// //资源卸载
			// ResourceManager.Instance.DisposeResource(clip,true);

			//卸载实例化的资源
			//ObjectManager.Instance.DisposeObject(obj);

			//显示UI
			//UIMgr.Instance.OpenWindow("TestPanel.prefab",true);

			// GameObject go = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true, true);
			// ObjectManager.Instance.DisposeObject(go);
			// go = null;

			//预加载
			ObjectManager.Instance.PreLoadGameObject("Assets/GameData/Prefabs/Attack.prefab",5);

			//加载场景
			GameMapManager.Instance.LoadScene(GameConst.SCENE_MENU);
		}
		else if(Input.GetKeyDown(KeyCode.C))
		{
			//同步实例化资源
			//obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);

			//异步实例化资源
			//ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/Attack.prefab", OnInstantiateFinish, LoadResPriority.RES_HIGHT,null,true);

			//隐藏UI
			//UIMgr.Instance.HidePanel("TestPanel.prefab");
		}
		else if(Input.GetKeyDown(KeyCode.Q))
		{
			//卸载实例化的资源
			//ObjectManager.Instance.DisposeObject(obj,0,true);
			//obj  = null;
		}
	}

	//清空电脑缓存
	private void OnApplicationQuit() 
	{
#if UNITY_EDITOR
		ResourceManager.Instance.ClearCache(true);
		Resources.UnloadUnusedAssets();
#endif
	}

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏场景管理器【管理 跳转】
/// </summary>
public class GameMapManager : Singleton<GameMapManager> 
{

	/// <summary>
	/// 当前场景名
	/// </summary>
	public string CurSceneName{ get; set; }

	/// <summary>
	/// 场景是否加载完成
	/// </summary>
	public bool AlreadyLoadScene{get; set;}

	/// <summary>
	/// 开始加载场景回调
	/// </summary>
	public Action LoadSceneEnterCallback;

	/// <summary>
	/// 加载场景完成回调
	/// </summary>
	public Action LoadSceneFinishCallback;
	
	//切换场景进度条
	public static int LoadingProgress = 0;

	private MonoBehaviour Mono;

	/// <summary>
	/// 初始化
	/// </summary>
	public void Init(MonoBehaviour mono)
	{
		this.Mono = mono;
	}


	/// <summary>
	/// 设置场景基本环境
	/// </summary>
	/// <param name="name"></param>
	void SetSceneSetting(string name)
	{
		//设置各种场景环境  根据配表控制  TODO

	}


	/// <summary>
	/// 加载场景
	/// </summary>
	/// <param name="name">加载的场景名</param>
	public void LoadScene(string name)
	{
		LoadingProgress = 0;
		Mono.StartCoroutine(AsyncLoadScene(name));
		Hashtable param = new Hashtable();
		param["curscene"] = name;
		UIMgr.Instance.OpenWindow(GameConst.UI_LOADING,true,param);
	}

	//异步加载场景
	IEnumerator AsyncLoadScene(string name)
	{
		//开始加载场景回调
		if(LoadSceneEnterCallback!=null)
		{
			LoadSceneEnterCallback();
		}

		ClearCache();
		AlreadyLoadScene = false;
		//置空场景
		AsyncOperation unLoadScene = SceneManager.LoadSceneAsync(GameConst.SCENE_EMPTY, LoadSceneMode.Single);
		while (unLoadScene != null && !unLoadScene.isDone)
		{
			//每帧等待
			yield return new WaitForEndOfFrame();
		}

		LoadingProgress = 0;
		int targetPropress = 0;

		//加载场景
		AsyncOperation asyncScene = SceneManager.LoadSceneAsync(name);
		if(asyncScene != null && !asyncScene.isDone)
		{
			//未加载完不显示
			asyncScene.allowSceneActivation = false;
			while (asyncScene.progress < 0.9f)
			{
				targetPropress = (int)(asyncScene.progress * 100);
				yield return new WaitForEndOfFrame();

				//平滑过度
				while (LoadingProgress < targetPropress)
				{
					++LoadingProgress;
					yield return new WaitForEndOfFrame();
				}
			}

			CurSceneName = name;

			//加载场景基本环境
			SetSceneSetting(name);

			//自行加载剩余的10%
			targetPropress = 100;
			while (LoadingProgress < targetPropress - 2)
			{
				++LoadingProgress;
				yield return new WaitForEndOfFrame();
			}
			LoadingProgress = 100;
			//完成显示场景
			asyncScene.allowSceneActivation = true;
			AlreadyLoadScene = true;

			//加载完成回调
			if(LoadSceneFinishCallback!=null)
			{
				LoadSceneFinishCallback();
			}
		}
		yield return null;
	}

	/// <summary>
	/// 跳场景 清空缓存等
	/// </summary>
	private void ClearCache()
	{
		ObjectManager.Instance.ClearCache();
		ResourceManager.Instance.ClearCache();
	}

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour 
{

	public AudioSource audio;
	AudioClip clip;

	private void Awake() 
	{
		DontDestroyOnLoad(this);
		//加载配置表
		AssetBundleManager.Instance.LoadAssetBundleConfig();
		ResourceManager.Instance.Init(this);
		Transform recycla = this.transform.Find("RecyclaPool").transform;
		Transform sceneTrs = this.transform.Find("SceneTrs").transform;
		ObjectManager.Instance.Init(recycla, sceneTrs);

	}

	void Start () 
	{
		//资源同步加载
		// clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
		// audio.clip = clip;
		// audio.Play();

		/* ----------------- */

		//资源异步加载
		ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/menusound.mp3", OnLoadFinish, LoadResPriority.RES_MIDDLE);
	}

	//资源异步加载回调
	void OnLoadFinish(string path, Object obj, Hashtable hh)
	{
		audio.clip = obj as AudioClip;
		audio.Play();
	}

	private void Update() 
	{
		if(Input.GetKeyDown(KeyCode.A))
		{
			audio.Stop();
			audio.clip = null;
			//资源卸载
			ResourceManager.Instance.DisposeResource(clip,true);
		}
	}

	//清空电脑缓存
	private void OnApplicationQuit() 
	{
#if UNITY_EDITOR
		ResourceManager.Instance.ClearCache();
		Resources.UnloadUnusedAssets();
#endif
	}

}

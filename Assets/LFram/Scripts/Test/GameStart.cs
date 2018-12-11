using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour 
{

	public AudioSource audio;
	AudioClip clip;

	private void Awake() 
	{
		//加载配置表
		AssetBundleManager.Instance.LoadAssetBundleConfig();
	}

	void Start () 
	{
		//资源加载
		clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
		audio.clip = clip;
		audio.Play();
	}

	private void Update() 
	{
		if(Input.GetKeyDown(KeyCode.A))
		{
			audio.Stop();
			audio.clip = null;
			//资源卸载
			ResourceManager.Instance.DisposeResource(clip);
		}
	}

}

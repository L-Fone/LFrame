using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour 
{

	void Start () 
	{
		AssetBundleManager.Instance.LoadAssetBundleConfig();
	}

}

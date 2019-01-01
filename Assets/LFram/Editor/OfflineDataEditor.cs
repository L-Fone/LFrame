using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OfflineDataEditor 
{

	//右键操作生成普通预设的离线数据
	[MenuItem("Assets/生成离线数据")]
	public static void AssetCreateOfflineData()
	{
		GameObject[] objs = Selection.gameObjects;
		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayCancelableProgressBar("添加离线数据","正在修改：" + objs[i] + "......", 1.0f / objs.Length * i);
			CreateOfflineData(objs[i]);
		}
		EditorUtility.ClearProgressBar();
	}

	//生成模型的离线数据
	public static void CreateOfflineData(GameObject obj)
	{
		OfflineData offlineData = obj.GetComponent<OfflineData>();
		if(offlineData == null)
		{
			offlineData = obj.AddComponent<OfflineData>();
		}

		offlineData.BindData();
		EditorUtility.SetDirty(obj);
		Debug.Log("修改了 ==> " + obj.name + " prefab!");
		Resources.UnloadUnusedAssets();
		AssetDatabase.Refresh();
	}

	/* ----------------------------------------------------------------------------------- */

	[MenuItem("离线数据/生成所有UI Prefab离线数据")]
	public static void AllCreateAllUIData()
	{
		string path = PathConst.UI_PATH;
		//获取文件夹下所有prefab
		string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[]{ path } );
		for (int i = 0; i < allStr.Length; i++)
		{
			//路径转换
			string itemPath = AssetDatabase.GUIDToAssetPath(allStr[i]);

			EditorUtility.DisplayCancelableProgressBar("添加UI离线数据","正在扫描路径：" + itemPath + "......", 1.0f / allStr.Length * i);
			GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(itemPath);
			if(go == null)
			{
				continue;
			}
			CreateUIOfflineData(go);
		}
		Debug.Log("UI离线数据全部生成完毕！！");
		EditorUtility.ClearProgressBar();
	}


	//右键操作生成UI预设的离线数据
	[MenuItem("Assets/生成UI离线数据")]
	public static void AssetCreateUIOfflineData()
	{
		GameObject[] objs = Selection.gameObjects;
		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayCancelableProgressBar("添加UI离线数据","正在修改：" + objs[i] + "......", 1.0f / objs.Length * i);
			CreateUIOfflineData(objs[i]);
		}
		EditorUtility.ClearProgressBar();
	}


	//生成UI离线数据
	public static void CreateUIOfflineData(GameObject obj)
	{
		obj.layer = LayerMask.NameToLayer("UI");

		UIOfflineData uiData = obj.GetComponent<UIOfflineData>();
		if(uiData == null)
		{
			uiData = obj.AddComponent<UIOfflineData>();
		}

		uiData.BindData();
		EditorUtility.SetDirty(obj);
		Debug.Log("修改了 ==> " + obj.name + "  UI prefab!");
		Resources.UnloadUnusedAssets();
		AssetDatabase.Refresh();

	}


	/* ----------------------------------------------------------------------------------- */


	[MenuItem("离线数据/生成所有特效 Prefab离线数据")]
	public static void AllCreateAllEffectData()
	{
		string path = PathConst.EFFECT_PATH;
		//获取文件夹下所有prefab
		string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[]{ path } );
		for (int i = 0; i < allStr.Length; i++)
		{
			//路径转换
			string itemPath = AssetDatabase.GUIDToAssetPath(allStr[i]);

			EditorUtility.DisplayCancelableProgressBar("添加特效离线数据","正在扫描路径：" + itemPath + "......", 1.0f / allStr.Length * i);
			GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(itemPath);
			if(go == null)
			{
				continue;
			}
			CreateEffectOfflineData(go);
		}
		Debug.Log("特效离线数据全部生成完毕！！");
		EditorUtility.ClearProgressBar();
	}


	//右键操作生成特效预设的离线数据
	[MenuItem("Assets/生成特效离线数据")]
	public static void AssetCreateEffectOfflineData()
	{
		GameObject[] objs = Selection.gameObjects;
		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayCancelableProgressBar("添加特效离线数据","正在修改：" + objs[i] + "......", 1.0f / objs.Length * i);
			CreateEffectOfflineData(objs[i]);
		}
		EditorUtility.ClearProgressBar();
	}



	//生成特效离线数据
	public static void CreateEffectOfflineData(GameObject obj)
	{
		obj.layer = LayerMask.NameToLayer("UI");

		EffectOfflineData uiData = obj.GetComponent<EffectOfflineData>();
		if(uiData == null)
		{
			uiData = obj.AddComponent<EffectOfflineData>();
		}

		uiData.BindData();
		EditorUtility.SetDirty(obj);
		Debug.Log("修改了 ==> " + obj.name + "  Effect prefab!");
		Resources.UnloadUnusedAssets();
		AssetDatabase.Refresh();
	}

}

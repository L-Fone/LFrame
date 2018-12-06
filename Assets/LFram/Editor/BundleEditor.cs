using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
	/* 所有文件夹AB包Dict; key = ABName; value = path */
	public static Dictionary<string,string> m_AllFileDir = new Dictionary<string, string>();

	/* 路径过滤List */
	public static List<string> m_AllFileAB = new List<string>();

	/* 单个Prefab的AB包; key = prefab名字; value = prefab所有依赖项路径列表*/
	public static Dictionary<string,List<string>> m_allPrefabDir = new Dictionary<string, List<string>>();


	//初始化数据
	private static void InitData()
	{
		m_AllFileDir.Clear();
		m_AllFileAB.Clear();
		m_allPrefabDir.Clear();
	}

	[MenuItem("Tools/打包")]
	public static void Build()
	{
		InitData();
	
		ABConfig abConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<ABConfig>(PathConst.ABCONFIG_PATH);

		/* ------------------------------------按文件夹全部打包-----------------------------------------------  */

		foreach (ABConfig.FileDirName item in abConfig.m_allFileDirAB)
		{
			if(m_AllFileDir.ContainsKey(item.ABName))
			{
				Debug.LogError("AB包名配置名字重复，请检查");
			}
			else
			{
				m_AllFileDir.Add(item.ABName, item.Path);
				m_AllFileAB.Add(item.Path);
			}
		}

		/* ------------------------------------按文件[Prefab]全部打包-----------------------------------------------  */
		
		//找到[abConfig.m_allPrefabPaht]路径下所有prefab的唯一标识 GUID
		string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_allPrefabPaht.ToArray());
		for (int i = 0; i < allStr.Length; i++)
		{
			//通过唯一标识GUID得到路径
			string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
			//Debug.Log(path);
			
			//加一个进度条
			EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i*1.0f/allStr.Length);

			//非过滤的路径才添加
			if(!ContainsAllfileAB(path))
			{
				//获取Prefab依赖项
				GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				string[] allDepend = AssetDatabase.GetDependencies(path);

				//所有依赖项路径列表
				List<string> allDependPath = new List<string>();
				for (int j = 0; j < allDepend.Length; j++)
				{
					Debug.Log(allDepend[j]);
					//该依赖项不在过滤的路径里面 并且依赖项不是脚本
					if(!ContainsAllfileAB(allDepend[i]) && !allDepend[i].EndsWith(".cs"))
					{
						m_AllFileAB.Add(allDepend[i]);//加入过滤列表
						allDependPath.Add(allDepend[i]);//加入临时列表
					}

					//添加每个prefab的所有依赖路径
					if(m_allPrefabDir.ContainsKey(obj.name))
					{
						Debug.LogError("存在相同名字的Prefab,请检查：name = " + obj.name);
					}
					else
					{
						m_allPrefabDir.Add(obj.name, allDependPath);
					}
				}
			}
		}

		//清除进度条
		EditorUtility.ClearProgressBar();




		//打包AB包
		// BuildPipeline.BuildAssetBundles
		// (
		// 	Application.streamingAssetsPath,//打包存储路径
		// 	BuildAssetBundleOptions.ChunkBasedCompression,//常用压缩方式
		// 	EditorUserBuildSettings.activeBuildTarget//打包平台(此处暂填编辑器使用的平台)
		// );
		// //刷新编辑器
		// AssetDatabase.Refresh();
	}

	///<summary>
	///	判断路径是否已经在路径过滤列表里
	///</summary>
	static bool ContainsAllfileAB(string path)
	{
		for (int i = 0; i < m_AllFileAB.Count; i++)
		{
			if(path == m_AllFileAB[i] || path.Contains(m_AllFileAB[i]))
			{
				return true;
			}
		}
		return false;
	}
}

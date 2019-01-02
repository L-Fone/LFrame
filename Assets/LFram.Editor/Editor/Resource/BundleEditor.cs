using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class BundleEditor
{
	/* 所有文件夹AB包Dict; key = ABName; value = path */
	private static Dictionary<string,string> m_AllFileDir = new Dictionary<string, string>();

	/* 路径过滤List */
	private static List<string> m_AllFileAB = new List<string>();

	/* 单个Prefab的AB包; key = prefab名字; value = prefab所有依赖项路径列表*/
	private static Dictionary<string,List<string>> m_allPrefabDir = new Dictionary<string, List<string>>();

	//[过滤]存储打包所需要的(有效的)文件路径的List
	private static List<string> m_ConfigFile = new List<string>();


	//初始化数据
	private static void InitData()
	{
		m_AllFileDir.Clear();
		m_AllFileAB.Clear();
		m_allPrefabDir.Clear();
		m_ConfigFile.Clear();
	}

	[MenuItem("Tools/打包")]
	public static void Build()
	{
		InitData();

		//读取AB打包路径配置表
		ABConfig abConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<ABConfig>(PathConst.ABCONFIG_PATH);

		/* ------------------------------------按文件夹[材质、图片等资源]存储全部打包路径-----------------------------------------------  */

		foreach (ABConfig.FileDirName fileDir in abConfig.m_allFileDirAB)
		{
			if(m_AllFileDir.ContainsKey(fileDir.ABName))
			{
				Debug.LogError("AB包名配置名字重复，请检查");
			}
			else
			{
				m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
				m_AllFileAB.Add(fileDir.Path);
				m_ConfigFile.Add(fileDir.Path);
			}
		}

		/* ------------------------------------按文件[Prefab]存储全部打包历经-----------------------------------------------  */
		
		//找到[abConfig.m_allPrefabPaht]路径下所有prefab的唯一标识 GUID
		string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_allPrefabPath.ToArray());
		for (int i = 0; i < allStr.Length; i++)
		{
			//通过唯一标识GUID得到路径
			string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
			//Debug.Log(path);
			
			//加一个进度条
			EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i*1.0f/allStr.Length);

			m_ConfigFile.Add(path);

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
					//Debug.Log(allDepend[j]);
					//该依赖项不在过滤的路径里面 并且依赖项不是脚本
					if(!ContainsAllfileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs"))
					{
						m_AllFileAB.Add(allDepend[j]);//加入过滤列表
						allDependPath.Add(allDepend[j]);//加入临时列表
					}
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

		/* ------------------------------------设置AB包名称-----------------------------------------------  */

		//设置[材质、图片、模型等]AB包
		foreach (string name in m_AllFileDir.Keys)
		{
			//遍历所有文件夹AB包Dict
			SetABName(name, m_AllFileDir[name]);
		}

		//设置[Prefab]AB包
		foreach (string name in m_allPrefabDir.Keys)
		{
			SetABName(name, m_allPrefabDir[name]);
		}

		/* ------------------------------------打包AB包 生成配置表-----------------------------------------------  */

		//打AB包
		BuildAssetBundle();


		/* ------------------------------------清除设置过的AB包名称-----------------------------------------------  */

		//清除AB包名称
		string[] oldABName = AssetDatabase.GetAllAssetBundleNames();
		for (int i = 0; i < oldABName.Length; i++)
		{
			//强制删除
			AssetDatabase.RemoveAssetBundleName(oldABName[i],true);
			//加个进度条
			EditorUtility.DisplayProgressBar("清除AB包名", "name:" + oldABName[i], i*1.0f/oldABName.Length);
		}


		/* ------------------------------------刷新编辑器界面 结束-----------------------------------------------  */

		//刷新编辑器
		AssetDatabase.Refresh();
		//清除进度条
		EditorUtility.ClearProgressBar();

	}

	///<summary>
	///	设置AB包
	///</summary>
	static void SetABName(string name, string path)
	{
		AssetImporter assetImporter = AssetImporter.GetAtPath(path);
		if(assetImporter == null)
		{
			Debug.LogError("不存在此文件路径，path : " + path);
			return;
		}
		//设置AB包名
		assetImporter.assetBundleName = name;
	}

	static void SetABName(string name, List<string> paths)
	{
		for (int i = 0; i < paths.Count; i++)
		{
			SetABName(name,paths[i]);
		}
	}

	///<summary>
	///	判断路径是否已经在路径过滤列表里
	///</summary>
	static void BuildAssetBundle()
	{
		//获取已经设置的所有AB包名字
		string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
		//key = 路径 value = AB包名字
		Dictionary<string, string> resPathDict = new Dictionary<string, string>();
		for (int i = 0; i < allBundles.Length; i++)
		{
			//通过包名获取全路径
			//此AB包下面包含的资源的所有路径
			string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
			for (int j = 0; j < allBundlePath.Length; j++)
			{
				//Debug.Log(allBundlePath[j]);
				//排除不需要打包的路径
				if(allBundlePath[j].EndsWith(".cs"))
				{ 
					continue; 
				}
				Debug.Log("此AB包：" + allBundles[i] + "下面包含的资源文件路径：" + allBundlePath[j]);
				if(ValidPath(allBundlePath[j]))
				{
					resPathDict.Add(allBundlePath[j], allBundles[i]);
				}
			}
		}

		//删除没用的AB包
		DeleteAB();

		//生成AB包配置表
		BuildABConfigToXML(resPathDict);

		//打包AB包
		BuildPipeline.BuildAssetBundles
		(
			PathConst.BUNDLE_TARGET_PATH,//打包存储路径
			BuildAssetBundleOptions.ChunkBasedCompression,//常用压缩方式
			EditorUserBuildSettings.activeBuildTarget//打包平台(此处暂填编辑器使用的平台)
		);
	}

	///<summary>
	///	生成AB包配置表
	///</summary>
	static void BuildABConfigToXML(Dictionary<string, string> resPathDict)
	{
		AssetBundleConfig config = new AssetBundleConfig();
		config.ABList = new List<ABBase>();
		
		foreach (string path in resPathDict.Keys)
		{
			ABBase abBase = new ABBase();
			abBase.Path = path;
			abBase.Crc = Crc32.GetCrc32(path);
			abBase.ABName = resPathDict[path];
			abBase.AssetName = path.Remove(0,path.LastIndexOf('/') + 1);
			abBase.ABDependce = new List<string>();

			//获取所有依赖项 并且过滤
			string[] resDePendce = AssetDatabase.GetDependencies(path);
			for (int i = 0; i < resDePendce.Length; i++)
			{
				string tempPath = resDePendce[i];
				//如果是自身 或者 脚本文件 就过滤掉
				if(tempPath == path || path.EndsWith(".cs"))
				{
					continue;
				}

				//通过路径充缓存里面取出AB包名字
				string abName = "";
				if(resPathDict.TryGetValue(tempPath, out abName))
				{
					//如果路径和自己一样[说明在同一个AB包里面] 则过滤掉
					if(abName == resPathDict[path])
					{ 
						continue; 
					}
					//如果依赖列表里面没有包含 则添加
					if(!abBase.ABDependce.Contains(abName))
					{
						abBase.ABDependce.Add(abName);
					}
				}
			}
			config.ABList.Add(abBase);
		}

		//写入XML
		string xmlPath = PathConst.ABCONFIG_XML_PATH;
		if(File.Exists(xmlPath)){ File.Delete(xmlPath); }
		FileStream fileStream = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
		//序列化，传入写入类型
		XmlSerializer xs = new XmlSerializer(config.GetType());
		xs.Serialize(sw, config);
		sw.Close();		
		fileStream.Close();

		//二进制文件不需要记录路径
		foreach (ABBase abBase in config.ABList)
		{
			abBase.Path = "";	
		}

		//写入二进制	
		string bytePath = PathConst.ABCONFIG_BYTES_PATH;
		FileStream fs = new FileStream(bytePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
		//清空
		fs.Seek(0, SeekOrigin.Begin);
		fs.SetLength(0);
		//写入
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(fs, config);
		fs.Close();
		
		//刷新
		AssetDatabase.Refresh();

		//把配置表打包成二进制文件
		SetABName(PathConst.ABCONFIG_ABNAME, PathConst.ABCONFIG_BYTES_PATH);

		

	}


	///<summary>
	///	删除本地多余的或者被改名的AB包文件
	///</summary>
	static void DeleteAB()
	{
		//获取所有的AB包名字
		string[] allBundleName = AssetDatabase.GetAllAssetBundleNames();
		DirectoryInfo direction = null;
		//通过路径获取文件夹信息
		if(!Directory.Exists(PathConst.BUNDLE_TARGET_PATH))
		{
			direction = Directory.CreateDirectory(PathConst.BUNDLE_TARGET_PATH);
		}
		else
		{
			direction = new DirectoryInfo(PathConst.BUNDLE_TARGET_PATH);
		}
		//获取文件夹里面的所有文件
		FileInfo[] files = direction.GetFiles("*",SearchOption.AllDirectories);

		for (int i = 0; i < files.Length; i++)
		{
			//判断如果本地文件名在要打AB包的列表里则保留 否则删除[以前遗留]无用的AB包
			if(ContainsABName(files[i].Name, allBundleName) || files[i].Name.EndsWith(".meta") || 
			files[i].Name.EndsWith(".manifest") || files[i].Name.Equals(PathConst.ABCONFIG_ABNAME))
			{
				continue;
			}
			//删除以前遗留的AB包
			else
			{
				//Debug.Log("此AB包被删除或者改名了，name = " + files[i].Name);
				if(File.Exists(files[i].FullName))
				{
					//删除多余AB包
					File.Delete(files[i].FullName);
				}
			}
		}
	}

	///<summary>
	///	遍历本地文件是否需要删除
	///</summary>
	static bool ContainsABName(string name, string[] strs)
	{
		for (int i = 0; i < strs.Length; i++)
		{
			if(name == strs[i])
			{
				return true;
			}
		}
		return false;
	}


	///<summary>
	///	判断路径是否已经在路径过滤列表里
	/// 是否已包含存在的AB包里，用来做AB包冗余剔除
	///</summary>
	static bool ContainsAllfileAB(string path)
	{
		for (int i = 0; i < m_AllFileAB.Count; i++)
		{
			if(path == m_AllFileAB[i] || (path.Contains(m_AllFileAB[i]) && (path.Replace(m_AllFileAB[i],"")[0] == '/')))
			{
				return true;
			}
		}
		return false;
	}

	///<summary>
	///	判断是否是有效路径
	///</summary>
	///<param name="path"></param>
	///<returns></returns>
	static bool ValidPath(string path)
	{
		for (int i = 0; i < m_ConfigFile.Count; i++)
		{
			if(path.Contains(m_ConfigFile[i]))
			{
				return true;
			}
		}
		return false;
	}
}

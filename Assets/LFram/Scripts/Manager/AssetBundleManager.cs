using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// AB包管理
/// </summary>
public class AssetBundleManager : Singleton<AssetBundleManager> 
{
	//资源关系依赖配置表，可以根据CRC来找到对应的资源块
	protected Dictionary<uint, ResourceItem> m_ResourceItemDict = new Dictionary<uint, ResourceItem>();

	//储存已加载的AB包，key = crc; value = 已加载的assetBundle
	protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDict = new Dictionary<uint, AssetBundleItem>();

	//AssetBundleItem 的类对象池
	protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = new ClassObjectPool<AssetBundleItem>(1000);


	/// <summary>
	/// 加载AB配置表
	/// </summary>
	/// <returns></returns>
	public bool LoadAssetBundleConfig()
	{
		m_ResourceItemDict.Clear();

		//通过路径加载AB配置表
		string configPath = PathConst.ABCONFIG_LOAD_PATH + PathConst.ABCONFIG_ABNAME;
		AssetBundle config = AssetBundle.LoadFromFile(configPath);
		TextAsset textAsset = config.LoadAsset<TextAsset>(PathConst.ABCONFIG_NAME);
		if(textAsset == null)
		{
			Debug.LogError("AssetBundleConfig 不存在，请检查配置路径！！！");
			return false;
		}
		
		//反序列化
		MemoryStream stream = new MemoryStream(textAsset.bytes);
		BinaryFormatter bf = new BinaryFormatter();
		AssetBundleConfig abConfig = (AssetBundleConfig)bf.Deserialize(stream);
		stream.Close();

		//
		for (int i = 0; i < abConfig.ABList.Count; i++)
		{
			ABBase abBase = abConfig.ABList[i];
			ResourceItem item = new ResourceItem();
			item.Crc = abBase.Crc;
			item.AssetName = abBase.AssetName;
			item.BundleName = abBase.ABName;
			item.DependBundle = abBase.ABDependce;
			
			if(m_ResourceItemDict.ContainsKey(item.Crc))
			{
				Debug.LogError("存在重复的Crc, 资源名 : " + item.AssetName + "__ab包名 ： " + item.BundleName);
			}
			else
			{
				m_ResourceItemDict.Add(item.Crc, item);
			}
		}

		return true;
	}


	/// <summary>
	/// 根据路径的Crc加载AB包中间类
	/// </summary>
	/// <param name="crc"></param>
	/// <returns></returns>
	public ResourceItem LoadReouseAssetBundle(uint crc)
	{
		ResourceItem item = null;

		if(!m_ResourceItemDict.TryGetValue(crc, out item) || item == null)
		{
			Debug.LogError("在AssetBundleConfig中 没有找到对应的 Crc ：" + crc.ToString());
			return item;			
		}

		if(item.CurBundle != null)
		{
			return item;
		}

		//加载AB包
		item.CurBundle = LoadAssetBundle(item.BundleName);

		//加载依赖项
		if(item.DependBundle != null)
		{
			for (int i = 0; i < item.DependBundle.Count; i++)
			{
				LoadAssetBundle(item.DependBundle[i]);
			}	
		}
	
		return item;
	}

	/// <summary>
	/// 根据名字加载单个AssetBundle
	/// </summary>
	/// <returns></returns>
	private AssetBundle LoadAssetBundle(string name)
	{
		AssetBundleItem item = null;
		uint crc = Crc32.GetCrc32(name);

		if(!m_AssetBundleItemDict.TryGetValue(crc, out item))
		{
			AssetBundle bundle = null;
			string fullPath = PathConst.ABCONFIG_LOAD_PATH + name;
			if(File.Exists(fullPath))
			{
				bundle = AssetBundle.LoadFromFile(fullPath);
			}

			if(bundle == null)
			{
				Debug.LogError("加载 AssetBundle包失败，包名为：" + name + "__请检查路径：" + fullPath);
			}

			item = m_AssetBundleItemPool.Spawn();
			item.assetBundle = bundle;
			item.RefCount ++;
			m_AssetBundleItemDict.Add(crc, item);
		}
		else
		{
			item.RefCount ++;
		}
		
		return item.assetBundle;
	}

	/// <summary>
	/// 卸载资源
	/// </summary>
	/// <param name="item"></param>
	public void DisposeAsset(ResourceItem item)
	{
		if(item == null)
		{
			return;
		}

		//先卸载依赖项 再卸载 自己
		if(item.DependBundle != null && item.DependBundle.Count > 0)
		{
			for (int i = 0; i < item.DependBundle.Count; i++)
			{
				UnLoadAssetBundle(item.DependBundle[i]);
			}
		}
		UnLoadAssetBundle(item.AssetName);
	}

	/// <summary>
	/// 卸载AB包
	/// </summary>
	/// <param name="name"></param>
	public void UnLoadAssetBundle(string name)
	{
		uint crc = Crc32.GetCrc32(name);
		AssetBundleItem item = null;
		if(m_AssetBundleItemDict.TryGetValue(crc, out item) && item != null)
		{
			item.RefCount--;
			if(item.RefCount <= 0 && item.assetBundle != null)
			{
				item.assetBundle.Unload(true);
				item.Reset();
				m_AssetBundleItemPool.Recycle(item);
				m_AssetBundleItemDict.Remove(crc);
			}
		}
	}

	/// <summary>
	/// 根据Crc 查找ResouceItem
	/// </summary>
	/// <param name="crc"></param>
	/// <returns></returns>
	public ResourceItem FindResouceItem(uint crc)
	{
		return m_ResourceItemDict[crc];
	}
}



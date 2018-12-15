using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 对象池管理
/// </summary>
public class ObjectManager : Singleton<ObjectManager> 
{

	//隐藏状态的游戏物体对象池[存储所有要回收的游戏对象][对象池节点]
	public Transform RecyclaPoolTrs;
	//[实例化的游戏物体对象池]
	public Transform SceneTrs;
	//游戏对象列表对象池
	protected Dictionary<uint, List<ResourceObject>> ObjectPool = new Dictionary<uint, List<ResourceObject>>();
	//[ResourceObject]类对象池
	protected ClassObjectPool<ResourceObject> ResourceObjectPool = ObjectManager.Instance.GetOrCreatClassPool<ResourceObject>(1000);

	//暂存ResourceObject的Dict key = GUID value = ResourceObject
	protected Dictionary<int, ResourceObject> ResourceObjectDict = new Dictionary<int, ResourceObject>();


	/* ---------------------------------------资源同步加载-------------------------------------------------- */

#region 同步资源[游戏物体]加载

	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="recyclaTrs">回收节点</param>
	/// <param name="sceneTrs">场景默认节点</param>
	public void Init(Transform recyclaTrs, Transform sceneTrs)
	{
		RecyclaPoolTrs = recyclaTrs;
		SceneTrs = sceneTrs;
	}
	
	/// <summary>
	/// [同步资源加载]实例化资源
	/// </summary>
	/// <param name="path">路径</param>
	/// <param name="setScenceObj">实例化后的默认放置点</param>
	/// <param name="clearCache">跳场景是否清除缓存</param>
	/// <returns></returns>
	public GameObject InstantiateObject(string path, bool setScenceObj = false, bool clearCache = true)
	{
		uint crc = Crc32.GetCrc32(path);
		ResourceObject resobj = GetObjectFromPool(crc);
		if(resobj == null)
		{
			resobj = ResourceObjectPool.Spawn();
			resobj.Crc = crc;
			resobj.Clear = clearCache;

			//ResourceManager提供加载resourceItem方法
			resobj = ResourceManager.Instance.LoadResource(path, resobj);

			//实例化
			if(resobj.ResItem.obj != null)
			{
				resobj.CloneObj = GameObject.Instantiate(resobj.ResItem.obj) as GameObject;
			}
		}

		//如果设置到默认节点下
		if(setScenceObj)
		{
			resobj.CloneObj.transform.SetParent(SceneTrs, false);
		}

		int tempGUID = resobj.CloneObj.GetInstanceID();
		if(!ResourceObjectDict.ContainsKey(tempGUID))
		{
			ResourceObjectDict.Add(tempGUID, resobj);
		}

		return resobj.CloneObj;
	}

	/// <summary>
	/// 从对象池里面取一个游戏物体
	/// </summary>
	/// <param name="crc"></param>
	/// <returns></returns>
	private ResourceObject GetObjectFromPool(uint crc)
	{
		List<ResourceObject> list = null;
		if(ObjectPool.TryGetValue(crc, out list) && list != null && list.Count > 0)
		{
			//ResourceManager的引用计数

			ResourceObject item = list[0];
			list.RemoveAt(0);
			GameObject obj = item.CloneObj;
			//编辑器下进行改名
			//判空 比 obj == null 的效率要高
			if(System.Object.ReferenceEquals(obj,null))
			{

				item.Already = false;

#if UNITY_EDITOR
				if(obj.name.EndsWith("(Recycla)"))
				{
					obj.name = obj.name.Replace("(Recycla)","");
				}
#endif
			}
			return item;
		}
		return null;
	}

#endregion

	/* ---------------------------------------资源卸载-------------------------------------------------- */

	/// <summary>
	/// 回收游戏物体
	/// </summary>
	/// <param name="obj">游戏物体对象</param>
	/// <param name="maxCacheCount">最大缓存个数：-1为不限</param>
	/// <param name="destroyCache">是否清除缓存</param>
	/// <param name="recyclaParent">是否回收到对象回收池节点下</param>
	public void DisposeObject(GameObject obj, int maxCacheCount = -1,bool destroyCache = false, bool recyclaParent = false)
	{
		if(obj == null)
		{
			return;
		}

		ResourceObject resobj = null;
		int guid = obj.GetInstanceID();
		if(!ResourceObjectDict.TryGetValue(guid, out resobj))
		{
			Debug.LogWarning("该对象不是通过ObjectManager创建的，请查看：" + obj.name);
			return;
		}

		if(resobj == null)
		{
			Debug.LogError("缓存的 ResourceObject 为空，请检查：" + obj.name);
			return;
		}

		if(resobj.Already)
		{
			Debug.LogError("该对象已经回收进对象池, 请确认是否清空了引用, 游戏物体对象：" + obj.name);
			return;
		}

		//进行回收释放

#if UNITY_EDITOR
		obj.name += "(Recycla)";
#endif

		List<ResourceObject> tempList = null;

		//等于0为不缓存 -1为无限缓存
		if(maxCacheCount == 0)
		{
			ResourceObjectDict.Remove(guid);
			ResourceManager.Instance.DisposeResource(resobj, destroyCache);
			resobj.Reset();
			ResourceObjectPool.Recycle(resobj);
		}

		//回收到对象池
		else
		{			
			//判断池内有没有这样一个东西

			if(!ObjectPool.TryGetValue(resobj.Crc, out tempList) || tempList == null)
			{
				tempList = new List<ResourceObject>();
				ObjectPool.Add(resobj.Crc , tempList);
			}
			
			if(resobj.CloneObj != null)
			{
				//回收到父节点
				if(recyclaParent)
				{
					resobj.CloneObj.transform.SetParent(RecyclaPoolTrs);
				}
				else
				{
					resobj.CloneObj.SetActive(false);
				}
			}

			//如果缓存个数小于0
			if(maxCacheCount < 0 || tempList.Count < maxCacheCount)
			{
				tempList.Add(resobj);
				resobj.Already = true;
				
				//ResourceManager 做引用计数

			}

			//达到了最大缓存个数
			else
			{
				ResourceObjectDict.Remove(guid);
				ResourceManager.Instance.DisposeResource(resobj, destroyCache);
				resobj.Reset();
				ResourceObjectPool.Recycle(resobj);
			}
		}


	}


	/* ---------------------------------------类对象池的使用-------------------------------------------------- */

#region 类对象池的使用

	// 根据类型存储对应类对象池	
	protected Dictionary<Type, object> m_ClassPoolDict = new Dictionary<Type, object>();

	/// <summary>
	/// 创建类对象池
	/// 创建完成后可以调用 Spawn 和 Recycle 进行创建和回收
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public ClassObjectPool<T> GetOrCreatClassPool<T> (int maxCount) where T : class,new()
	{
		Type type = typeof(T);
		object outObj = null;
		if(!m_ClassPoolDict.TryGetValue(type, out outObj) || outObj == null)
		{
			ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
			m_ClassPoolDict.Add(type, newPool);
			return newPool;
		}
		return outObj as ClassObjectPool<T>;
	}

#endregion	
	


}

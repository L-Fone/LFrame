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
	protected Dictionary<uint, List<ResourceObject>> ObjectPoolDict = new Dictionary<uint, List<ResourceObject>>();
	//[ResourceObject]类对象池
	protected ClassObjectPool<ResourceObject> ResourceObjectPool = null;

	//暂存ResourceObject的Dict key = GUID value = ResourceObject
	protected Dictionary<int, ResourceObject> ResourceObjectDict = new Dictionary<int, ResourceObject>();

	//根据异步的GUID储存ResourceObject用来判断是否正在异步加载
	protected Dictionary<long, ResourceObject> AsyncResObjsDict = new Dictionary<long, ResourceObject>();


	/* ---------------------------------------初始化-------------------------------------------------- */

	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="recyclaTrs">回收节点</param>
	/// <param name="sceneTrs">场景默认节点</param>
	public void Init(Transform recyclaTrs, Transform sceneTrs)
	{
		RecyclaPoolTrs = recyclaTrs;
		SceneTrs = sceneTrs;
		ResourceObjectPool = ObjectManager.Instance.GetOrCreatClassPool<ResourceObject>(1000);
	}

	/* ---------------------------------------判断和清空-------------------------------------------------- */

	/// <summary>
	/// 判断是否正在异步实例化
	/// </summary>
	/// <param name="guid"></param>
	/// <returns></returns>
	public bool IsingAsyncLoad(long guid)
	{
		if(!AsyncResObjsDict.ContainsKey(guid))
		{
			return false;
		}
		return AsyncResObjsDict[guid] != null;
	}

	/// <summary>
	/// 该对象是否是对象池创建的
	/// </summary>
	/// <returns></returns>
	public bool IsObjectMgrCreat(GameObject obj)
	{
		int guid = obj.GetInstanceID();
		if(!ResourceObjectDict.ContainsKey(guid))
		{
			return false;
		}
		ResourceObject resobj = ResourceObjectDict[guid];
		return resobj != null;
	}

	/// <summary>
	/// 清空对象池
	/// </summary>
	public void ClearCache()
	{
		List<uint> tempList = new List<uint>();
		foreach (uint key in ObjectPoolDict.Keys)
		{
			List<ResourceObject> st = ObjectPoolDict[key];
			for (int i = st.Count - 1; i >= 0; i--)
			{
				ResourceObject resobj = st[i];
				if(!System.Object.ReferenceEquals(resobj.CloneObj, null) && resobj.Clear)
				{
					GameObject.Destroy(resobj.CloneObj);
					ResourceObjectDict.Remove(resobj.CloneObj.GetInstanceID());
					ResourceObjectPool.Recycle(resobj);
				}
			}

			//记录全部被清的
			if(st.Count<=0)
			{
				tempList.Add(key);
			}
		}

		for (int i = 0; i < tempList.Count; i++)
		{
			uint key = tempList[i];	
			if(ObjectPoolDict.ContainsKey(key))
			{
				ObjectPoolDict.Remove(key);
			}
		}

		tempList.Clear();
	}

	/// <summary>
	/// 清除某个资源在对象池中所有的资源对象
	/// </summary>
	/// <param name="crc"></param>
	public void ClearPoolObject(uint crc)
	{
		List<ResourceObject> st = null;
		if(!ObjectPoolDict.TryGetValue(crc, out st) || st == null)
		{
			return;
		}

		for (int i = st.Count - 1; i >= 0; i--)
		{
			ResourceObject resobj = st[i];
			if(resobj.Clear)
			{
				st.Remove(resobj);
				int tempID = resobj.CloneObj.GetInstanceID();
				GameObject.Destroy(resobj.CloneObj);
				resobj.Reset();
				ResourceObjectDict.Remove(tempID);
				ResourceObjectPool.Recycle(resobj);
			}
		}

		if(st.Count <= 0)
		{
			ObjectPoolDict.Remove(crc);
		}
	}

	/* ---------------------------------------资源预加载-------------------------------------------------- */

	/// <summary>
	/// 预加载GameObject[实例化]
	/// </summary>
	/// <param name="path">路径</param>
	/// <param name="count">预加载个数</param>
	/// <param name="clear">跳转场景是否清除</param>
	public void PreLoadGameObject(string path, int count = 1, bool clear = false)
	{
		List<GameObject> tempGomeObjectList = new List<GameObject>();
		//加载
		for (int i = 0; i < count; i++)
		{
			GameObject go = InstantiateObject(path, false, clear);
			tempGomeObjectList.Add(go);
		}
		//清除
		for (int i = 0; i < count; i++)
		{
			GameObject go = tempGomeObjectList[i];
			DisposeObject(go);
			go = null;
		}
		tempGomeObjectList.Clear();
	}

	/* ---------------------------------------资源同步加载-------------------------------------------------- */

#region 同步资源[游戏物体]加载
	
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
		if(ObjectPoolDict.TryGetValue(crc, out list) && list != null && list.Count > 0)
		{
			//ResourceManager的引用计数
			ResourceManager.Instance.IncreaseResourceRef(crc);

			ResourceObject item = list[0];
			list.RemoveAt(0);
			GameObject obj = item.CloneObj;
			//编辑器下进行改名
			//判空 比 obj == null 的效率要高
			if(!System.Object.ReferenceEquals(obj,null))
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

	/* ---------------------------------------资源异步实例化加载-------------------------------------------------- */

	/// <summary>
	/// 资源异步实例化对象加载
	/// </summary>
	/// <param name="path">资源路径</param>
	/// <param name="dealfinish">完成回调</param>
	/// <param name="priority">优先级</param>
	/// <param name="param">传递的参数</param>
	/// <param name="setScenceObj">是否放在默认节点下</param>
	/// <param name="bClear">跳转场景是否清除缓存</param>
	public long InstantiateObjectAsync(string path, OnAsyncObjFinish dealfinish, LoadResPriority priority, Hashtable param = null, bool setScenceObj = false, bool bClear = true)
	{
		if(string.IsNullOrEmpty(path))
		{
			return 0;
		}
		//先从缓存池里取
		uint crc = Crc32.GetCrc32(path);
		ResourceObject resobj = GetObjectFromPool(crc);
		if(resobj != null)
		{
			if(setScenceObj)
			{
				resobj.CloneObj.transform.SetParent(SceneTrs, false);
			}

			if(dealfinish != null)
			{
				dealfinish(path, resobj.CloneObj, param);
			}

			return resobj.GUID;
		}

		//创建异步加载对应的GUID(唯一标识，用于取消异步加载)
		long guid = ResourceManager.Instance.CreatGUID();

		//如果缓存池里没有该对象 则加载
		resobj = ResourceObjectPool.Spawn(true);
		resobj.Crc = crc;
		resobj.SetSceneParent = setScenceObj;
		resobj.Clear = bClear;
		resobj.DealFinish = dealfinish;
		resobj.Param = param;

		//调用ResourceManager的异步加载接口
		ResourceManager.Instance.AsyncLoadResource(path, resobj, OnLoadResourceObjectFinish, priority);

		return guid;
	}

	/// <summary>
	/// 异步资源加载的回调
	/// </summary>
	/// <param name="path">路径</param>
	/// <param name="resobj">中间类</param>
	/// <param name="param">参数</param>
	void OnLoadResourceObjectFinish(string path, ResourceObject resobj, Hashtable param = null)
	{
		if(resobj == null)
		{
			return;
		}
		if(resobj.ResItem.obj == null)
		{
#if UNITY_EDITOR
			Debug.LogError("异步资源加载的资源为空 "  + path);
#endif
		}
		else
		{
			resobj.CloneObj = GameObject.Instantiate(resobj.ResItem.obj) as GameObject;
		}

		//加载完成 从正在加载的异步中移除记录
		if(AsyncResObjsDict.ContainsKey(resobj.GUID))
		{
			AsyncResObjsDict.Remove(resobj.GUID);
		}

		if(resobj.CloneObj != null && resobj.SetSceneParent)
		{
			resobj.CloneObj.transform.SetParent(SceneTrs, false);
		}

		//执行回调
		if(resobj.DealFinish != null)
		{
			int tempID = resobj.CloneObj.GetInstanceID();
			if(!ResourceObjectDict.ContainsKey(tempID))
			{
				ResourceObjectDict.Add(tempID, resobj);
			}

			resobj.DealFinish(path, resobj.CloneObj, resobj.Param);
		}
	}

	/* ---------------------------------------取消异步实例化-------------------------------------------------- */

	/// <summary>
	/// 通过唯一标识取消异步实例化加载
	/// </summary>
	/// <param name="guid">唯一标识</param>
	public void CancelLoad(long guid)
	{
		ResourceObject resobj = null;
		//取消加载
		if(AsyncResObjsDict.TryGetValue(guid, out resobj) && ResourceManager.Instance.CancelLoad(resobj))
		{
			//清除缓存队列
			AsyncResObjsDict.Remove(guid);
			resobj.Reset();
			ResourceObjectPool.Recycle(resobj);
		}
	}


	/* ---------------------------------------资源卸载-------------------------------------------------- */

	/// <summary>
	/// 回收游戏物体
	/// </summary>
	/// <param name="obj">游戏物体对象</param>
	/// <param name="maxCacheCount">最大缓存个数：-1为不限</param>
	/// <param name="destroyCache">是否清除缓存</param>
	/// <param name="recyclaParent">是否回收到对象回收池节点下</param>
	public void DisposeObject(GameObject obj, int maxCacheCount = -1,bool destroyCache = false, bool recyclaParent = true)
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

			if(!ObjectPoolDict.TryGetValue(resobj.Crc, out tempList) || tempList == null)
			{
				tempList = new List<ResourceObject>();
				ObjectPoolDict.Add(resobj.Crc , tempList);
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
				ResourceManager.Instance.DecreaseResourceRef(resobj);
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

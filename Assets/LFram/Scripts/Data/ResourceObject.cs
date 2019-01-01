using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameObject资源中间类
/// </summary>
public class ResourceObject
{
	/// <summary>
	/// 路径对应的Crc
	/// </summary>
	public uint Crc;

	/// <summary>
	/// 未实例化的资源Item
	/// </summary>
	public ResourceItem ResItem = null;

	/// <summary>
	/// 实例化出来的游戏物体
	/// </summary>
	public GameObject CloneObj = null;
	
	/// <summary>
	/// 是否跳场景清除
	/// </summary>
	public bool Clear = true;

	/// <summary>
	/// 是否已经放回对象池
	/// </summary>
	public bool Already = false;

	/* -----------------异步实例化------------------------- */

	/// <summary>
	/// [异步实例化]是否放到场景节点下面
	/// </summary>
	public bool SetSceneParent = false;

	/// <summary>
	/// [异步实例化]实例化资源加载完成回调
	/// </summary>
	public OnAsyncObjFinish DealFinish = null;

	/// <summary>
	/// [异步实例化]实例化资源加载传递的参数
	/// </summary>
	public Hashtable Param = null;


	/* -----------------异步实例化------------------------- */

	/// <summary>
	/// 离线数据[用于还原状态]
	/// </summary>
	public OfflineData offlineData = null;

	/// <summary>
	/// 储存的GUID
	/// </summary>
	public int GUID = 0;

	/// <summary>
	/// 还原
	/// </summary>
	public void Reset() 
	{
		Crc = 0;
		ResItem = null;
		CloneObj = null;
		Clear = true;
		Already = false;
		GUID = 0;
		SetSceneParent = false;
		DealFinish = null;
		Param = null;
		offlineData = null;
	}
}

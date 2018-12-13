using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源块
/// </summary>
public class ResourceItem
{
	/// <summary>
	/// 资源路径的Crc
	/// </summary>
	public uint Crc = 0;

	/// <summary>
	/// 资源文件名
	/// </summary>
	public string AssetName = string.Empty;

	/// <summary>
	/// 资源所在的AB包名称
	/// </summary>
	public string BundleName = string.Empty;

	/// <summary>
	/// 该资源所依赖的AB包
	/// </summary>
	public List<string> DependBundle = null;

	/// <summary>
	/// 该资源所加载完成的AB包
	/// </summary>
	public AssetBundle CurBundle = null;

	/* ------------------------------------------------------ */

	/// <summary>
	/// 资源对象
	/// </summary>
	public Object obj = null;

	/// <summary>
	/// 最后使用时间
	/// </summary>
	public float LastUseTime = 0.0f;

	/// <summary>
	/// 资源唯一标识
	/// </summary>
	public int GUID = 0;

	/// <summary>
	/// 跳场景时否需要清掉
	/// </summary>
	public bool Clear = true;

	protected int m_RefCount = 0;

	/// <summary>
	/// 引用计数
	/// </summary>
	public int RefCount
	{
		get{ return m_RefCount; }
		set
		{ 
			m_RefCount = value;

			if(m_RefCount < 0)
			{
				Debug.LogError("引用计数小于0，Count = " + m_RefCount + (obj != null ? obj.name : "未知名字"));
			}
		}
	}
}

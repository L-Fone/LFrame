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

}

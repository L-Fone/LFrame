using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Excel数据类
/// </summary>
[System.Serializable]
public class ExcelBase
{
	/// <summary>
	/// 编辑器下初始类转XML文件
	/// </summary>
	public virtual void Construction(){}

	/// <summary>
	/// 数据初始化
	/// </summary>
	public virtual void Init(){}
}

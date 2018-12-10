using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AB包资源
/// </summary>
public class AssetBundleItem
{
    /// <summary>
    /// AB包
    /// </summary>
    public AssetBundle assetBundle = null;

    /// <summary>
    /// 引用计数
    /// </summary>
    public int RefCount = 0;

    /// <summary>
    /// 还原数据
    /// </summary>
    public void Reset() 
    {
        assetBundle = null;
        RefCount = 0;
    }

}
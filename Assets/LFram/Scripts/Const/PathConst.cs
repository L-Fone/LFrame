using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathConst
{
	//AB配置表名字
	public static string ABCONFIG_NAME = "AssetBundleConfig";
	//ABConfig表所在的AB包名字
	public static string ABCONFIG_ABNAME = "abconfig";

	//[AB包配置路径表]路径
	public static string ABCONFIG_PATH = "Assets/LFram/Editor/ABConfig.asset";
	
	//[AB包]打包出来的路径
	public static string BUNDLE_TARGET_PATH = Application.streamingAssetsPath;

	//[ABConfig.XML] 生成路径
	public static string ABCONFIG_XML_PATH = Application.dataPath + "/AssetBundleConfig.xml"; 

	//[ABConfig.bytes] 生成路径
	public static string ABCONFIG_BYTES_PATH = Application.dataPath + "/LFram/ABData/AssetBundleConfig.bytes"; 

	//[ABConfig.ab] config包的加载路径
	public static string ABCONFIG_LOAD_PATH = Application.streamingAssetsPath + "/";

	//是否从AssetBundle进行加载
	public static bool LoadFromAssetBundle = false;

	//异步资源加载 分帧延时[单位微秒]
	public static long MAX_LOADRESOURCE_TIME = 200000;
}



/// <summary>
/// 资源加载优先级
/// </summary>
public enum LoadResPriority
{
	RES_HIGHT = 0,	// 最高优先级	
	RES_MIDDLE,		// 中优先级
	RES_SLOW,		// 低优先级
	RES_NUM			// 优先级个数
}

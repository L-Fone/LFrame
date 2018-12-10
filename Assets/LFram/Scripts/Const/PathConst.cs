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
}

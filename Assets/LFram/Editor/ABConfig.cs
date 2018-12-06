using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABConfig",menuName = "CreatABConfig", order = 0)]
public class ABConfig : ScriptableObject
{
	//单个文件所在文件夹路径，会遍历这个文件夹下所有的prefab,所有的prefab名字不能重复，确保唯一性
	public List<string> m_allPrefabPaht = new List<string>();
	public List<FileDirName> m_allFileDirAB = new List<FileDirName>();
	
	//文件夹的AB包
	[System.Serializable]
	public struct FileDirName
	{
		public string ABName;
		public string Path;
	}	


}

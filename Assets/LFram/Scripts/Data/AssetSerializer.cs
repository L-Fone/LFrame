using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//继承ScriptableObject 才能Assets序列化
//[CreateAssetMenu(fileName = "TestAsset",menuName = "CreatAssets", order = 0)]
public class AssetSerializer : ScriptableObject
{
	public int ID;
	public string Name;
	public List<int> TestList;
}

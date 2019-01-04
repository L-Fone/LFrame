using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class MonsterData : ExcelBase 
{
	/// <summary>
	/// 编辑器下初始类转XML文件
	/// </summary>
	public override void Construction()
	{
		AllMonster = new List<MonsterBase>();
		for (int i = 0; i < 5; i++)
		{
			MonsterBase item = new MonsterBase();
			item.Id = i+1;
			item.Name = "怪物"+item.Id;
			item.OutLook = "Assets/GameData/Prefabs/Attack.prefab";
			item.Rare = 2;
			item.Hight = item.Id*1f/1.2f;
			AllMonster.Add(item);
		}
	}

	/// <summary>
	/// 数据初始化
	/// </summary>
	public override void Init()
	{
		base.Init();

		allMonstDict.Clear();

		foreach (MonsterBase item in AllMonster)
		{
			if(allMonstDict.ContainsKey(item.Id))
			{
				Debug.LogError(item.Name + " 有重复ID：" + item.Id);
			}
			else
			{
				allMonstDict.Add(item.Id, item);
			}
		}
	}

	/// <summary>
	/// 根据ID查找到数据
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public MonsterBase FindDataByID(int id)
	{
		return allMonstDict[id];
	}

	//缓存数据
	[XmlIgnore]
	public Dictionary<int, MonsterBase> allMonstDict = new Dictionary<int, MonsterBase>();


	//所有怪物数据
	[XmlElement("AllMonster")]
	public List<MonsterBase> AllMonster{ get; set; }
}

[System.Serializable]
public class MonsterBase
{
	//ID
	[XmlAttribute("Id")]
	public int Id{get;set;}

	//名字
	[XmlAttribute("Name")]
	public string Name{get;set;}

	//预知路径[全路径]
	[XmlAttribute("OutLook")]
	public string OutLook{get;set;}

	//怪物等级
	[XmlAttribute("Lv")]
	public int Lv{get;set;}

	//怪物稀有度
	[XmlAttribute("Rare")]
	public int Rare{get;set;}

	//怪物高度
	[XmlAttribute("Hight")]
	public float Hight{get;set;}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class BuffData : ExcelBase
{
    public override void Construction()
    {
        AllBuffList = new List<BuffBase>();
        for (int i = 0; i < 10; i++)
        {
            BuffBase buff = new BuffBase();
            buff.Id = i + 1;
            buff.Name = "全局BUFF" + i;
            buff.OutLook = "Assets/GameData/...";
            buff.Time = Random.Range(1000, 5000);
            buff.Buff = (BuffType)Random.Range(0, 4);
            buff.AllString = new List<string>()
            {
                "test"+i,
                "param"+i,
                "ppppp"+i,
            };
            buff.AllBuffList = new List<BuffTest>();
            int count = Random.Range(1,5);
            for (int j = 0; j < count; j++)
            {
                BuffTest test = new BuffTest();
                test.Id = j + Random.Range(0,5);
                test.Name = "name" + j;
                buff.AllBuffList.Add(test);
            }

            AllBuffList.Add(buff);
        }
        AllMonsterBuffList = new List<BuffBase>();
        for (int i = 0; i < 15; i++)
        {
            BuffBase buff = new BuffBase();
            buff.Id = i + 1;
            buff.Name = "怪物BUFF" + i;
            buff.OutLook = "Assets/GameData/...";
            buff.Time = Random.Range(10000, 50000);
            buff.Buff = (BuffType)Random.Range(0, 4);
            buff.AllString = new List<string>()
            {
                "MonsterTest"+i,
                "MonsterParam"+i,
            };

            buff.AllBuffList = new List<BuffTest>();
            int count = Random.Range(1, 8);
            for (int j = 0; j < count; j++)
            {
                BuffTest test = new BuffTest();
                test.Id = j + Random.Range(0, 10);
                test.Name = "name" + j;
                buff.AllBuffList.Add(test);
            }

            AllMonsterBuffList.Add(buff);
        }
    }

    public override void Init()
    {
        AllBuffDict.Clear();
        for (int i = 0; i < AllBuffList.Count; i++)
        {
            AllBuffDict.Add(AllBuffList[i].Id, AllBuffList[i]);
        }
    }

    public BuffBase FindBuffByID(int id)
    {
        return AllBuffDict[id];
    }

    [XmlIgnore]
    public Dictionary<int, BuffBase> AllBuffDict = new Dictionary<int, BuffBase>();

    [XmlElement("AllBuffList")]
    public List<BuffBase> AllBuffList { get; set; }

    [XmlElement("AllMonsterBuffList")]
    public List<BuffBase> AllMonsterBuffList { get; set; }
}

public enum BuffType
{
    None = 0,
    Ranshao,
    Bingdong,
    Du,
}


[System.Serializable]
public class BuffBase
{
    [XmlAttribute("Id")]
    public int Id { get; set; }

    [XmlAttribute("Name")]
    public string Name { get; set; }

    [XmlAttribute("OutLook")]
    public string OutLook { get; set; }

    [XmlAttribute("Time")]
    public long Time { get; set; }

    [XmlAttribute("Buff")]
    public BuffType Buff { get; set; }

    [XmlElement("AllString")]
    public List<string> AllString { get; set; }

    [XmlElement("AllBuffList")]
    public List<BuffTest> AllBuffList { get; set; }
}

[System.Serializable]
public class BuffTest
{
    [XmlAttribute("Id")]
    public int Id { get; set; }

    [XmlAttribute("Name")]
    public string Name { get; set; }
}

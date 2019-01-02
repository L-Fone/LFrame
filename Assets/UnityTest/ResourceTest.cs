using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ResourceTest : MonoBehaviour 
{

	void Start () 
	{
		//SerializeTest();
		//DeSerializeTest();
		//BinarySerializeTest();
		//BinaryDeSerializeTest();
		//ReadTestAssets();
		TestLoadAB();
	}

	void TestLoadAB()
	{
		AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/abdata");
		TextAsset textAsset = assetBundle.LoadAsset<TextAsset>("AssetBundleConfig");
		MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
		BinaryFormatter bf = new BinaryFormatter();
		AssetBundleConfig abConfig = (AssetBundleConfig)bf.Deserialize(memoryStream);
		memoryStream.Close();
		string path = "Assets/GameData/Prefabs/Attack.prefab";
		ABBase abBase = null;
		uint crc = Crc32.GetCrc32(path);
		for (int i = 0; i < abConfig.ABList.Count; i++)
		{
			if(abConfig.ABList[i].Crc == crc)
			{
				abBase = abConfig.ABList[i];
			}
		}

		//加载依赖项
		for (int i = 0; i < abBase.ABDependce.Count; i++)
		{
			AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABDependce[i]);
		}

		//加载AB包
		AssetBundle bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABName);
		GameObject go = GameObject.Instantiate(bundle.LoadAsset<GameObject>(abBase.AssetName));
	}

	//XML序列化测试
	void SerializeTest()
	{
		TestSerialize test = new TestSerialize();
		test.ID = 1;
		test.Name = "Xml";
		test.Lists = new List<int>();
		test.Lists.Add(2);
		test.Lists.Add(3);
		SerializeToXml(test);
	}
	//XML反序列化测试
	void DeSerializeTest()
	{
		TestSerialize testSerialize = XmlDeSerialize();
		Debug.Log(testSerialize.ID);
		Debug.Log(testSerialize.Name);
		foreach (var item in testSerialize.Lists)
		{
			Debug.Log(item);
		}
	}

	//xml序列化
	void SerializeToXml(TestSerialize testSerialize)
	{
		//创建流文件
		//参数1：创建到的位置
		//参数2：文件类型
		//参数3：文件权限
		//参数4：共享权限
		FileStream fileStream = new FileStream(Application.dataPath+"/test.xml",FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite);
		//创建写入流
		StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
		//xml序列化API
		XmlSerializer xml = new XmlSerializer(testSerialize.GetType());
		//参数1：写入流; 参数2：要序列化的类
		//xml.Serialize() 把类序列化到写入流里面
		xml.Serialize(writer, testSerialize);

		writer.Close();
		fileStream.Close();
		
		Debug.Log("done");
	}

	//xml反序列化
	TestSerialize XmlDeSerialize()
	{
		FileStream fileStream = new FileStream(Application.dataPath+"/test.xml",FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite);
		XmlSerializer xml = new XmlSerializer(typeof(TestSerialize));
		TestSerialize testSerialize = (TestSerialize)xml.Deserialize(fileStream);

		fileStream.Close();
		return testSerialize;
	}

	//二进制文件序列化测试
	void BinarySerializeTest()
	{
		TestSerialize test = new TestSerialize();
		test.ID = 10;
		test.Name = "BinaryTest";
		test.Lists = new List<int>();
		test.Lists.Add(200);
		test.Lists.Add(310);
		BinarySerialize(test);
	}

	//二进制文件反序列化测试
	// void BinaryDeSerializeTest()
	// {
	// 	TestSerialize testSerialize = BinaryDeSerialize();
	// 	Debug.Log(testSerialize.ID);
	// 	Debug.Log(testSerialize.Name);
	// 	foreach (var item in testSerialize.Lists)
	// 	{
	// 		Debug.Log(item);
	// 	}
	// }

	//二进制文件序列化
	void BinarySerialize(TestSerialize testSerialize)
	{
		FileStream fs = new FileStream(Application.dataPath+"/test.bytes",FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite);
		//二进制流序列化
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(fs,testSerialize);

		fs.Close();
	}

	//二进制文件反序列化	
	// TestSerialize BinaryDeSerialize()
	// {
	// 	TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/test.bytes");
	// 	MemoryStream memoryStream = new MemoryStream(textAsset.bytes);
	// 	BinaryFormatter bf = new BinaryFormatter();
	// 	TestSerialize testSerialize = (TestSerialize)bf.Deserialize(memoryStream);
	// 	return testSerialize;
	// }

	//读取序列化的Assets文件
	// void ReadTestAssets()
	// {
	// 	AssetSerializer assets = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetSerializer>("Assets/LFram/Scripts/TestAsset.asset");

	// 	Debug.Log(assets.ID);
	// 	Debug.Log(assets.Name);
	// 	foreach (var item in assets.TestList)
	// 	{
	// 		Debug.Log(item);
	// 	}
	// }

}

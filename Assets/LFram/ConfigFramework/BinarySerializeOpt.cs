using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// 配置表工具：二进制序列化
/// </summary>
public class BinarySerializeOpt 
{
	/// <summary>
	/// [类 To XML] 类序列化成XML
	/// </summary>
	/// <param name="path">存储路径</param>
	/// <param name="obj">保存类型</param>
	/// <returns>成功序列化返回 true</returns>
	public static bool XmlSerialize(string path, System.Object obj)
	{
		try
		{
			using(FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using(StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
				{
					//去掉XML的版本头
					XmlSerializerNamespaces name = new XmlSerializerNamespaces();
					name.Add(string.Empty, string.Empty);
					//序列化
					XmlSerializer xs = new XmlSerializer(obj.GetType());
					xs.Serialize(sw, obj);
				}
			}		
			return true;	
		}
		catch (System.Exception e)
		{
			Debug.LogError("此类无法转换为XML: " + obj.GetType() + "  ==>  error：" + e);
			return false;
		}
	}
	
}

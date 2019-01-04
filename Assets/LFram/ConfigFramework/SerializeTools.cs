using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// 配置表工具：序列化
/// </summary>
public class SerializeTools 
{

	/* -----------------------------------------------XML文件操作-------------------------------------------------------------- */

	/// <summary>
	/// [Object To XML] 类序列化成XML
	/// </summary>
	/// <param name="path">存储路径</param>
	/// <param name="obj">保存类型</param>
	/// <returns>成功序列化返回 true</returns>
	public static bool ObjectToXmlFile(string path, System.Object obj)
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
					xs.Serialize(sw, obj, name);
				}
			}		
			return true;	
		}
		catch (System.Exception e)
		{
			Debug.LogError("此类无法转换为XML: " + obj.GetType() + "  ==>  error：" + e);
		}
		return false;
	}

	/// <summary>
	/// 编辑器读取XML文件
	/// </summary>
	/// <param name="path"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static T ReadXmlFile<T>(string path) where T : class
	{
		T t = default(T);

		try
		{
			using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				t = (T)xs.Deserialize(fs);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("编辑器读取XML文件出现异常,路径：" + path + "___异常状态：" + e);
		}
		return t;
	}

	/// <summary>
	/// 编辑器读取XML文件
	/// </summary>
	/// <param name="path"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static System.Object ReadXmlFile(string path, System.Type type)
	{
		System.Object obj = null;
		try
		{
			using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				XmlSerializer xs = new XmlSerializer(type);
				obj = xs.Deserialize(fs);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("编辑器读取XML文件出现异常,路径：" + path + "___异常状态：" + e);
		}
		return obj;
	}


	/// <summary>
	/// 运行时读取xml
	/// </summary>
	/// <param name="path"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static T ReadXmlFileByRun<T>(string path) where T : class
	{
		T t = default(T);

		TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);

		if(textAsset == null)
		{
			Debug.LogError("读取XML文件失败, 文件路径：" + path);
			return null;
		}

		try
		{
			using(MemoryStream stream = new MemoryStream(textAsset.bytes))
			{
				XmlSerializer xs = new XmlSerializer(typeof(T));
				t = (T)xs.Deserialize(stream);
			}
			//对应回收
			ResourceManager.Instance.DisposeResource(path, true);
		}
		catch (System.Exception e)
		{
			Debug.LogError("XML转二进制文件出现异常,路径：" + path + "___异常状态：" + e);
		}


		return t;
	}

	/* -----------------------------------------------二进制文件操作------------------------------------------------------------ */

	/// <summary>
	/// 类转换成二进制文件
	/// </summary>
	/// <param name="path"></param>
	/// <param name="obj"></param>
	/// <returns></returns>
	public static bool ObjectToBinaryFile(string path, System.Object obj)
	{
		try
		{
			using(FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(fs, obj);
			}
			return true;
		}
		catch (System.Exception e)
		{
			Debug.LogError("此类无法转成二进制：" + obj.GetType() + "__错误状态：" + e);
			return false;
		}
	}
	
	/// <summary>
	/// 读取二进制文件
	/// </summary>
	/// <param name="path"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static T ReadBinaryFile<T>(string path) where T : class
	{
		T t = default(T);

		TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);

		if(textAsset == null)
		{
			Debug.LogError("反序列化 Binary文件失败, 文件路径：" + path);
			return null;
		}

		try
		{
			using(MemoryStream stream = new MemoryStream(textAsset.bytes))
			{
				BinaryFormatter bf = new BinaryFormatter();
				t = (T)bf.Deserialize(stream);
			}
			//对应回收
			ResourceManager.Instance.DisposeResource(path, true);
		}
		catch (System.Exception e)
		{
			Debug.LogError("反序列化Binary出现异常,路径：" + path + "___异常状态：" + e);
		}

		return t;
	}
	
}

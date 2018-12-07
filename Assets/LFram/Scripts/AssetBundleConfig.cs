/* ------------- 生成AB包的配置表 XML文件 -------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class AssetBundleConfig 
{
	[XmlAnyElement("ABList")]
	public List<ABBase> ABList{ get; set;}
	
}



///AB包item元素
[System.Serializable]
public class ABBase
{
	/* 路径 */
	[XmlAttribute("Path")]
	public string Path{ get; set; }

	/* CRC [路径转成Crc 用做效验 文件唯一标识] */
	[XmlAttribute("Crc")]
	public uint Crc{ get; set; }

	/* 所在AB包名 */
	[XmlAttribute("ABName")]
	public string ABName{ get; set; }

	/* 资源名 */
	[XmlAttribute("AssetName")]
	public string AssetName{ get; set; }

	/* 该AB包所有依赖列表 */
	[XmlElement("ABDependce")]
	public List<string> ABDependce{ get; set; }
}

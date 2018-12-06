using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class TestSerialize
{

	[XmlAttribute("id")]
	public int ID{get;set;}

	[XmlAttribute("name")]
	public string Name{get;set;}

	[XmlElement("list")]
	public List<int> Lists{get;set;}
	
}

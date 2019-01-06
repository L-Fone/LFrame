using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using OfficeOpenXml;
using System.Reflection;

/// <summary>
/// 编辑器下的配置表控制
/// </summary>
public class ConfigEditor
{
	//路径
	public static string XmlPath = PathConst.CONFIG_PATH + PathConst.XML_DIR + "/";
	public static string BinaryPath = PathConst.CONFIG_PATH + PathConst.BINARY_DIR + "/";
	public static string OutPath = PathConst.CONFIG_DATA_PATH;



	/* ---------------------------------------------------编辑器下类转XML文件-------------------------------------------------------------- */

	//右键控制单个选中类转XML
	[MenuItem("Assets/选中的类转XML")]
	public static void AssetsClassToXml()
	{
		UnityEngine.Object[] objs = Selection.objects;

		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayCancelableProgressBar("文件下的类转XML","正在扫描" + objs[i].name + "......" , 1.0f/objs.Length * i);
			DoClassToXml(objs[i].name);
		}

		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();	
	}

	/// <summary>
	/// 执行单个类转XML文件
	/// </summary>
	private static void DoClassToXml(string name)
	{
		
		try
		{
			//获取到的类
			Type type = null;

			//反射 通过名字找到类
			// AppDomain.CurrentDomain.GetAssemblies() 获取当前主程序集
			// System.Reflection.Assembly 每个子程序集
			foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				//根据类名获得这个类
				Type temp = asm.GetType(name);
				if(temp != null)
				{
					type = temp;
					break;
				}
			}

			if(type != null)
			{
				//New 出这个类
				var temp = Activator.CreateInstance(type);

				if(temp is ExcelBase)
				{
					//执行编辑器方法
					(temp as ExcelBase).Construction();
				}

				//类转成XML
				SerializeTools.ObjectToXmlFile(XmlPath + name + ".xml", temp);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("类转XML失败了，文件名：" + name + "___错误状态：" + e);
		}
	}


	/* ---------------------------------------------------编辑器下XML转二进制文件-------------------------------------------------------------- */


	[MenuItem("配置表工具/所有XML文件转二进制文件")]
	public static void AllXmlToBinary()
	{
		//完整的全路径
		string path =  Application.dataPath.Replace("Assets","") + XmlPath;
		//获取路径下所有文件
		string[] filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
		for (int i = 0; i < filesPath.Length; i++)
		{
			EditorUtility.DisplayCancelableProgressBar("查找文件下的所有XML并转成二进制文件","正在扫描" + filesPath[i] + "......" , 1.0f/filesPath.Length * i);

			//只转XML文件
			if(filesPath[i].EndsWith(".xml"))
			{
				//获取文件路径
				string fileName = filesPath[i].Substring(filesPath[i].LastIndexOf("/") + 1);
				fileName = fileName.Replace(".xml", "");
				DoXmlToBinary(fileName);
			}
		}

		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();	
	}


	[MenuItem("Assets/选中的XML转二进制文件")]
	public static void AssetsXmlToBinary()
	{
		UnityEngine.Object[] objs = Selection.objects;

		for (int i = 0; i < objs.Length; i++)
		{
			EditorUtility.DisplayCancelableProgressBar("文件下的XML转二进制文件","正在扫描" + objs[i].name + "......" , 1.0f/objs.Length * i);
			DoXmlToBinary(objs[i].name);
		}

		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();	
	}

	/// <summary>
	/// 执行单个XML转二进制文件
	/// </summary>
	/// <param name="name"></param>
	private static void DoXmlToBinary(string name)
	{
		if(string.IsNullOrEmpty(name))
		{
			return;
		}

		try
		{
			Type type = null;
			foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type temp = asm.GetType(name);
				if(temp != null)
				{
					type = temp;
					break;
				}
			}

			if(type != null)
			{
				//反序列化xml文件
				string path = XmlPath + name + ".xml";
				System.Object obj = SerializeTools.ReadXmlFile(path, type);

				//再序列化二进制文件
				string binaryPath = BinaryPath + name + ".bytes";
				SerializeTools.ObjectToBinaryFile(binaryPath, obj);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("XML转二进制文件失败了，文件名：" + name + "___错误状态：" + e);
		}
	}



    /* -------------------------------------Excel文件写入--------------------------------------------------------- */

    /// <summary>
    /// XML转Excel
    /// </summary>
    [MenuItem("Assets/选中的XML转Excel")]
    public static void AssetXmlToExcel()
    {
        UnityEngine.Object[] objs = Selection.objects;

        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayCancelableProgressBar("选中的XML转Excel", "正在扫描" + objs[i].name + "......", 1.0f / objs.Length * i);
            XmlToExcel(objs[i].name);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 通过名字单XML转Excel
    /// </summary>
    private static void XmlToExcel(string name)
    {
        string className = "";
        string xmlName = "";
        string excelName = "";
        Dictionary<string, SheetClass> allSheetClassDict = ReadReg(name, ref excelName, ref xmlName, ref className);

        //跟类做对应  读取类的值
        object data = GetObjectFromXml(className);
        
        List<SheetClass> outSheetList = new List<SheetClass>();
        foreach (SheetClass sheetClass in allSheetClassDict.Values)
        {
            if (sheetClass.Depth == 1)
            {
                //获取到最外层
                outSheetList.Add(sheetClass);
            }
        }

        Dictionary<string, SheetData> sheetDataDict = new Dictionary<string, SheetData>();

        for (int i = 0; i < outSheetList.Count; i++)
        {
            ReadData(data, outSheetList[i], allSheetClassDict, sheetDataDict, "");
        }

        //写入Excel
        string xlsxPath = PathConst.EXCEL_PATH + excelName;
        //检查文件是否被占用
        if (FileIsUsed(xlsxPath))
        {
            Debug.LogError("文件被占用，无法修改！！！！");
            return;
        }

        //写入Excel
        try
        {
            FileInfo xlsxFile = new FileInfo(xlsxPath);
            if (xlsxFile.Exists)
            {
                xlsxFile.Delete();
                xlsxFile = new FileInfo(xlsxPath);
            }
            using (ExcelPackage package = new ExcelPackage(xlsxFile))
            {
                foreach (string str in sheetDataDict.Keys)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(str);
                    worksheet.Cells.AutoFitColumns();//自适应宽度
                    SheetData sheetData = sheetDataDict[str];
                    for (int i = 0; i < sheetData.AllName.Count; i++)
                    {
                        ExcelRange range = worksheet.Cells[1, i + 1];
                        range.Value = sheetData.AllName[i];
                        range.AutoFitColumns();
                    }
                    for (int i = 0; i < sheetData.AllData.Count; i++)
                    {
                        RowData rowData = sheetData.AllData[i];
                        for (int j = 0; j < sheetData.AllData[i].RowDataDict.Count; j++)
                        {
                            ExcelRange range = worksheet.Cells[i + 2, j + 1];
                            string value = rowData.RowDataDict[sheetData.AllName[j]];
                            range.Value = value;
                            range.AutoFitColumns();
                            if (value.Contains("\n"))
                            {
                                range.Style.WrapText = true;
                            }
                        }
                    }
                }
                package.Save();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        Debug.Log("写Excel成功！！！！");
    }

    /// <summary>
    /// 递归读取XML配置文件
    /// </summary>
    /// <param name="xmlElement"></param>
    private static void ReadXmlNode(XmlElement xmlElement, Dictionary<string, SheetClass> allSheetClassDict, int depth)
    {
        depth++;
        foreach (XmlNode node in xmlElement.ChildNodes)
        {
            XmlElement xe = (XmlElement)node;
            if (xe.GetAttribute("type") == "list")
            {
                XmlElement listEle = (XmlElement)node.FirstChild;

                VarClass parentVar = new VarClass()
                {
                    Name = xe.GetAttribute("name"),
                    Type = xe.GetAttribute("type"),
                    Col = xe.GetAttribute("col"),
                    DeafultValue = xe.GetAttribute("defaultvalue"),
                    Foregin = xe.GetAttribute("foregin"),
                    SplitStr = xe.GetAttribute("split"),
                };

                if (parentVar.Type == "list")
                {
                    parentVar.ListName = ((XmlElement)xe.FirstChild).GetAttribute("name");
                    parentVar.ListSheetName = ((XmlElement)xe.FirstChild).GetAttribute("sheetname");
                }

                SheetClass sheetClass = new SheetClass()
                {
                    Name = listEle.GetAttribute("name"),
                    SheetName = listEle.GetAttribute("sheetname"),
                    SplitStr = listEle.GetAttribute("split"),
                    MainKey = listEle.GetAttribute("miankey"),
                    ParentVar = parentVar,
                    Depth = depth,
                };

                if (!string.IsNullOrEmpty(sheetClass.SheetName))
                {
                    if (!allSheetClassDict.ContainsKey(sheetClass.SheetName))
                    {
                        //获取该类下面所有变量
                        foreach (XmlNode insideNode in listEle.ChildNodes)
                        {
                            XmlElement insideXe = (XmlElement)insideNode;

                            VarClass varClass = new VarClass()
                            {
                                Name = insideXe.GetAttribute("name"),
                                Type = insideXe.GetAttribute("type"),
                                Col = insideXe.GetAttribute("col"),
                                DeafultValue = insideXe.GetAttribute("defaultvalue"),
                                Foregin = insideXe.GetAttribute("foregin"),
                                SplitStr = insideXe.GetAttribute("split"),
                            };

                            if (varClass.Type == "list")
                            {
                                varClass.ListName = ((XmlElement)insideXe.FirstChild).GetAttribute("name");
                                varClass.ListSheetName = ((XmlElement)insideXe.FirstChild).GetAttribute("sheetname");
                            }

                            sheetClass.VarList.Add(varClass);
                        }
                        allSheetClassDict.Add(sheetClass.SheetName, sheetClass);
                    }
                }

                //递归读取
                ReadXmlNode(listEle, allSheetClassDict, depth);
            }
        }
    }

    /// <summary>
    /// 递归存储类里面的数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sheetClass"></param>
    /// <param name="allSheetClassDict"></param>
    /// <param name="allSheetDataDict"></param>
    private static void ReadData(object data, SheetClass sheetClass, 
        Dictionary<string, SheetClass> allSheetClassDict, Dictionary<string, SheetData> sheetDataDict, string miankey)
    {
        List<VarClass> varList = sheetClass.VarList;
        VarClass varClass = sheetClass.ParentVar;

        object dataList = AssemblyTools.GetMemberValue(data, varClass.Name);

        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count",
            BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));


        SheetData sheetData = new SheetData();

        if (!string.IsNullOrEmpty(varClass.Foregin))
        {
            sheetData.AllName.Add(varClass.Foregin);
            sheetData.AllTpye.Add(varClass.Type);
        }

        for (int i = 0; i < varList.Count; i++)
        {
            //存到Excel里面
            if (!string.IsNullOrEmpty(varList[i].Col))
            {
                sheetData.AllName.Add(varList[i].Col);
                sheetData.AllTpye.Add(varList[i].Type);
            }
        }

        string tempKey = miankey;
        for (int i = 0; i < listCount; i++)
        {
            //获取下一层的类
            object item = dataList.GetType().InvokeMember("get_Item", 
                BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i });

            //每一行数据
            RowData rowData = new RowData();

            if (!string.IsNullOrEmpty(varClass.Foregin) && !string.IsNullOrEmpty(tempKey))
            {
                rowData.RowDataDict.Add(varClass.Foregin, tempKey);
            }

            if (!string.IsNullOrEmpty(sheetClass.MainKey))
            {
                miankey = AssemblyTools.GetMemberValue(item, sheetClass.MainKey).ToString();
            }
    
            //每一行数据添加
            for (int j = 0; j < varList.Count; j++)
            {
                if (varList[j].Type == "list")
                {
                    if (string.IsNullOrEmpty(varList[j].SplitStr))
                    {
                        //递归读取
                        SheetClass tempSheetClass = allSheetClassDict[varList[j].ListSheetName];
                        ReadData(item, tempSheetClass, allSheetClassDict, sheetDataDict, miankey);
                    }
                    else
                    {
                        SheetClass tempSheetClass = allSheetClassDict[varList[j].ListSheetName];
                        string value = GetSplitBaseList(item, varList[j], tempSheetClass);
                        rowData.RowDataDict.Add(varList[j].Col, value);
                    }
                }               
                else if (varList[j].Type == "listStr" || varList[j].Type == "listFloat" ||
                         varList[j].Type == "listInt" || varList[j].Type == "listBool")
                {
                     string value = GetSplitBaseList(item, varList[j]);
                     rowData.RowDataDict.Add(varList[j].Col, value);
                }
                //储存
                else
                {
                    object value = AssemblyTools.GetMemberValue(item, varList[j].Name);
                    if (varList != null)
                    {
                        rowData.RowDataDict.Add(varList[j].Col, value.ToString());
                    }
                    else
                    {
                        Debug.LogError(varList[j].Name + "反射出来为空！！！");
                    }
                }
            }

            string key = varClass.ListSheetName;

            if (sheetDataDict.ContainsKey(key))
            {
                sheetDataDict[key].AllData.Add(rowData);
            }
            else
            {
                sheetData.AllData.Add(rowData);
                sheetDataDict.Add(key, sheetData);
            }
        }
    }

    /// <summary>
    /// 获取基础List<object>里面所有值
    /// </summary>
    /// <returns></returns>
    private static string GetSplitBaseList(object data, VarClass varClass)
    {

        string str = "";

        if (string.IsNullOrEmpty(varClass.SplitStr))
        {
            Debug.LogError("基础List的分割符为空！！！");
            return str;
        }

        object dataList = AssemblyTools.GetMemberValue(data, varClass.Name);

        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", 
            BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));

        for (int i = 0; i < listCount; i++)
        {
            object item = dataList.GetType().InvokeMember("get_Item",
                BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i });
            str += item.ToString();
            if (i != listCount - 1)
            {
                //最后一个不加分隔符
                str += varClass.SplitStr.Replace("\\n", "\n").Replace("\\r", "\r");
            }
        }
        return str;
    }


    /// <summary>
    /// 获取基本List<Type>里面所有值
    /// </summary>
    /// <returns></returns>
    private static string GetSplitBaseList(object data, VarClass varClass, SheetClass sheetClass)
    {
        string str = "";
        string split = varClass.SplitStr;
        string classSplit = sheetClass.SplitStr;

        if (string.IsNullOrEmpty(split))
        {
            Debug.LogError("基础List的类的列分割符为空！！！");
            return str;
        }

        if (string.IsNullOrEmpty(classSplit))
        {
            Debug.LogError("基础List的类的变量分割符为空！！！");
            return str;
        }

        object dataList = AssemblyTools.GetMemberValue(data, varClass.Name);

        int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count",
           BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { }));

        for (int i = 0; i < listCount; i++)
        {
            object item = dataList.GetType().InvokeMember("get_Item",
                BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i });

            for (int j = 0; j < sheetClass.VarList.Count; j++)
            {
                object value = AssemblyTools.GetMemberValue(item, sheetClass.VarList[j].Name);
                str += value.ToString();
                if (j != sheetClass.VarList.Count - 1)
                {
                    str += classSplit.Replace("\\n","\n").Replace("\\r","\r");
                }
            }

            if (i != listCount - 1)
            {
                str += split.Replace("\\n", "\n").Replace("\\r", "\r");
            }
        }
        return str;
    }



    /// <summary>
    /// 检查文件是否被占用
    /// </summary>
    /// <param name="path"></param>
    private static bool FileIsUsed(string path)
    {
        bool result = false;
        if (!File.Exists(path))
        {
            return false;
        }

        FileStream fileStream = null;

        try
        {
            using (fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                result = false;
            }            
        }
        catch
        {
            result = true;
        }
        
        return result;
    }

    /// <summary>
    /// 从XML文件获取object
    /// 反序列化XML到类
    /// </summary>
    /// <param name="name">传入的XML文件名字</param>
    /// <returns></returns>
    private static object GetObjectFromXml(string name)
    {
        Type type = null;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type temp = asm.GetType(name);
            if (temp != null)
            {
                type = temp;
                break;
            }
        }
        if (type != null)
        {
            string xmlPath = XmlPath + name + ".xml";
            return SerializeTools.ReadXmlFile(xmlPath, type);
        }
        return null;
    }



    /* -------------------------------------Reg文件读取--------------------------------------------------------- */

    /// <summary>
    /// 读取Reg配置表
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="excelName">对应excel名字</param>
    /// <param name="xmlName">对应xml名字</param>
    /// <param name="className">对应类名</param>
    /// <returns></returns>
    private static Dictionary<string, SheetClass> ReadReg(string name, ref string excelName, ref string xmlName, ref string className)
    {
        string path = PathConst.REG_PATH + name + ".xml";

        if (!File.Exists(path))
        {
            Debug.LogError("此配置表数据文件不存在：" + name);
            return null;
        }

        XmlDocument xml = new XmlDocument();
        XmlReader reader = XmlReader.Create(path);
        //忽略XML里面的注释内容
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreComments = true;
        xml.Load(reader);

        //读取XML
        XmlNode xn = xml.SelectSingleNode("data");
        XmlElement xe = (XmlElement)xn;
        className = xe.GetAttribute("name");
        xmlName = xe.GetAttribute("to");
        excelName = xe.GetAttribute("from");

        //储存所有变量
        Dictionary<string, SheetClass> allSheetClassDict = new Dictionary<string, SheetClass>();
        ReadXmlNode(xe, allSheetClassDict, 0);

        reader.Close();

        return allSheetClassDict;
    }



    /* -------------------------------------测试--------------------------------------------------------- */

    [MenuItem("配置表工具/测试/测试读取XML")]
    public static void TestReadXml()
    {
        string path = Application.dataPath + "/../ConfigData/Reg/MonstData.xml";
        XmlReader reader = null;
        try
        {
            //读取XML文件
            XmlDocument xml = new XmlDocument();
            reader = XmlReader.Create(path);
            xml.Load(reader);

            //读取data首节点
            XmlNode xn = xml.SelectSingleNode("data");
            XmlElement xe = (XmlElement)xn;

            //获取属性值 name
            string className = xe.GetAttribute("name");
            string xmlName = xe.GetAttribute("to");
            string excelName = xe.GetAttribute("from");

            reader.Close();

            //遍历子节点
            foreach (XmlNode node in xe.ChildNodes)
            {
                XmlElement tempxe = (XmlElement)node;

                //当层节点元素
                string name = tempxe.GetAttribute("name");
                string type = tempxe.GetAttribute("type");

                //下层子节点
                XmlNode list = tempxe.FirstChild;
                XmlElement listNode = (XmlElement)list;
                string listName = listNode.GetAttribute("name");
                string sheetName = listNode.GetAttribute("sheetname");
                string mainkey = listNode.GetAttribute("mainkey");

                Debug.LogError(listName + "____" + sheetName + "___" + mainkey);
            }
        }
        catch (System.Exception e)
        {
            if (reader != null) reader.Close();
            Debug.LogError(e);
        }
    }

    [MenuItem("配置表工具/测试/测试写入Excel")]
	public static void TestWriteExcel()
	{
		string path = Application.dataPath + "/../ConfigData/Excel/G怪物.xlsx";

		FileInfo xlsxFile = new FileInfo(path);
		if(xlsxFile.Exists)
		{
			xlsxFile.Delete();
			xlsxFile = new FileInfo(path);
		}

		//用EPPlus进行写入Excel文件
		using(ExcelPackage package = new ExcelPackage(xlsxFile))
		{
			//写入表单名称
			ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("怪物配置");
			//单元格
			ExcelRange range = worksheet.Cells[1,1];
			range.Value = "测试测试测试测试测试测试测试测试测试测试测试\n测试测试测试测试测试测试测试测试测试测试测试测试测试测试测试测试测试";
			//特性：AutoFitColumns自动单元格长度; range.Style.WrapText 自动换行
			//如果先自动换行，则自动适应宽度会失效
			// 填充样式：range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
			// 填充颜色：range.Style.Fill.BackgroundColor.SetColor();		
			// 对齐方式：range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
			range.AutoFitColumns();
			range.Style.WrapText = true;
			package.Save();
		}
	}


	[MenuItem("配置表工具/测试/测试已有类反射")]
	public static void TestReflection()
	{
		TestInfo info = new TestInfo()
		{
			ID = 1,
			Name = "测试反射",
			Hight = 0.5f,
			AllStrList = new List<string>()
			{
				"测试1",
				"测试2",
				"测试3",
				"测试4",
			},
			AllTwoList = new List<TestTwo>()
			{
				new TestTwo(){ ID = 101 },
				new TestTwo(){ ID = 102 },
				new TestTwo(){ ID = 103 },
			}
		};
		
		object nameValue = AssemblyTools.GetMemberValue(info, "Hight");		
		Debug.LogError(nameValue);

		//反射获取List<object>
		// object list = GetMemberValue(info,"AllStrList");
		// int count = System.Convert.ToInt32
		// (
		// 	GetMemberValue(info, "AllStrList")
		// 	.GetType()
		// 	.InvokeMember("get_Count",BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[]{})
		// );

		// Debug.LogError("List count = " + count);

		// for (int i = 0; i < count; i++)
		// {
		// 	//获取List列表中每一个值
		// 	object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[]{ i });

		// 	Debug.LogError("List Item [" + i + "] = " + item);
		// }

		//反射获取List<Type>
		object list = AssemblyTools.GetMemberValue(info,"AllTwoList");
		int count = System.Convert.ToInt32
		(
			AssemblyTools.GetMemberValue(info, "AllTwoList")
			.GetType()
			.InvokeMember("get_Count",BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[]{})
		);
		for (int i = 0; i < count; i++)
		{
			//获取List列表中每一个值
			object item = list.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[]{ i });

			//获取Item里面变量的值
			object id = AssemblyTools.GetMemberValue(item, "ID");

			Debug.LogError("List Item [" + i + "] = " + id);
		}
	}


	[MenuItem("配置表工具/测试/测试已有数据进行反射")]
	public static void TestReflectionByData()
	{
		//通过名字实例化创建一个类
		object obj = AssemblyTools.CreateClass("TestInfo");
		//获取到一个属性
		PropertyInfo info = obj.GetType().GetProperty("ID");
		//info.SetValue(obj, System.Convert.ToInt32("100"));
		AssemblyTools.SetPropertyValue(info, obj, "22", "int");
		//获取到一个属性
		PropertyInfo nameInfo = obj.GetType().GetProperty("Name");
		//nameInfo.SetValue(obj, "反射赋值 666");
		AssemblyTools.SetPropertyValue(nameInfo, obj, "反射赋值 666", "string");
		//获取到一个属性
		PropertyInfo hightInfo = obj.GetType().GetProperty("Hight");
		//hightInfo.SetValue(obj, System.Convert.ToSingle("0.12542"));
		AssemblyTools.SetPropertyValue(hightInfo, obj, "0.12542", "float");

		//获取到一个枚举
		PropertyInfo enumInfo = obj.GetType().GetProperty("TestType");
		// object infovalue = TypeDescriptor.GetConverter(enumInfo.PropertyType).ConvertFromInvariantString("VAR1");//枚举赋值
		// enumInfo.SetValue(obj, infovalue);
		AssemblyTools.SetPropertyValue(enumInfo, obj, "VAR2", "enum");

		//给List<object>赋值
		// Type type = typeof(string);//要创建的类型
		// Type listType = typeof(List<>);//获取一个泛型List
		// Type specType = listType.MakeGenericType(new System.Type[]{ type });//构造泛型类型为string的List
		// //创建实例 new出来
		// object list = Activator.CreateInstance(specType, new object[]{});
		// Type type = typeof(string);
		// object list = AssemblyTools.CreateList(type);
		// //填数据
		// for (int i = 0; i < 3; i++)
		// {
		// 	object item = "反射添加的"+i;
		// 	//调用List的Add方法 添加数据
		// 	list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, list, new object[]{ item });
		// }
		// //把做好的list给添加到类中
		// obj.GetType().GetProperty("AllStrList").SetValue(obj, list);

		//给List<Type>赋值
		Type type = typeof(TestTwo);
		object twoList = AssemblyTools.CreateList(type);
		for (int i = 0; i < 3; i++)
		{
			object item = AssemblyTools.CreateClass(type.ToString());
			//获取属性
			PropertyInfo itemInfo = item.GetType().GetProperty("ID");
			AssemblyTools.SetPropertyValue(itemInfo, item, (i*150).ToString(), "int");
			twoList.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, twoList, new object[]{ item });
		}
		//把做好的list给添加到类中
		obj.GetType().GetProperty("AllTwoList").SetValue(obj, twoList);

		TestInfo testInfo = obj as TestInfo;
		Debug.LogError(testInfo.ID +"___"+testInfo.Name+"___"+testInfo.Hight+"___"+testInfo.TestType);

		foreach (var item in testInfo.AllTwoList)
		{
			Debug.LogError(item.ID);
		}
	}
	
}


//测试用
public class TestInfo
{
	public int ID{get;set;}
	public string Name{get;set;}
	public float Hight{get;set;}

	public TestEnum TestType{get;set;}//枚举

	public List<string> AllStrList{get;set;}

	public List<TestTwo> AllTwoList{get;set;}
}

public class TestTwo
{
	public int ID{get;set;}
}

public enum TestEnum
{
	None = 0,
	VAR1 = 1,	
	VAR2 = 2,
}

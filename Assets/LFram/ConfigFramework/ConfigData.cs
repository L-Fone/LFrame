using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 对应Excel文件的中间类
/// </summary>
public class VarClass
{
    //原类里面变量的名称
    public string Name { get; set; }

    //变量类型
    public string Type { get; set; }

    //变量对应的Excel里的列
    public string Col { get; set; }

    //变量的默认值
    public string DeafultValue { get; set; }

    //变量是List的话，外联部分列
    public string Foregin { get; set; }

    //分割符号
    public string SplitStr { get; set; }

    //如果自己是List，存对应的List类名
    public string ListName { get; set; }

    //如果自己是List，存对应的sheet名
    public string ListSheetName { get; set; }
}

/// <summary>
/// Excel文件类
/// </summary>
public class SheetClass
{

    //每一层深度值
    public int Depth { get; set; }

    //所属父级的Var变量
    public VarClass ParentVar { get; set; }

    //Excel文件名
    public string Name { get; set; }

    //Excel表单名
    public string SheetName { get; set; }

    //主key值
    public string MainKey { get; set; }

    //分割符
    public string SplitStr { get; set; }

    //所包含的变量
    public List<VarClass> VarList = new List<VarClass>();
}

/// <summary>
/// 表里面所有的值
/// </summary>
public class SheetData
{
    public List<string> AllName = new List<string>();//每一行的名字
    public List<string> AllTpye = new List<string>();//每一行对应的数据类型
    public List<RowData> AllData = new List<RowData>();//有多少行
}


/// <summary>
/// 每列数据
/// </summary>
public class RowData
{
    //key = 行
    public Dictionary<string, string> RowDataDict = new Dictionary<string, string>();
}
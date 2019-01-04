using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Excel配置表管理器
/// </summary>
/// <typeparam name="ConfigMgr"></typeparam>
public class ConfigMgr : Singleton<ConfigMgr> 
{

    //储存已经加载的配置表容器
    protected Dictionary<string, ExcelBase> allExcelData = new Dictionary<string, ExcelBase>();


    /// <summary>
    /// 加载数据配置表[初始化]
    /// </summary>
    /// <param name="path">二进制文件路径</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T LoadData<T>(string path) where T : ExcelBase
    {
        if(string.IsNullOrEmpty(path))
        {
            return null;
        }

        if(allExcelData.ContainsKey(path))
        {
            Debug.LogError("重复加载相同配置文件，路径：" + path);
            return allExcelData[path] as T;
        }


        //从二进制文件加载配置表
        T data = SerializeTools.ReadBinaryFile<T>(path);

#if UNITY_EDITOR        
        if(data == null)
        {
            Debug.Log(path + "不存在该二进制文件，从XML加载数据了！！");

            //修改读取路径
            string xmlPath = path.Replace(PathConst.BINARY_DIR, PathConst.XML_DIR).Replace(".bytes", ".xml");
            
            data = SerializeTools.ReadXmlFile<T>(xmlPath);
        }
#endif        

        //初始化
        if(data != null)
        {
            data.Init();
        }

        //缓存起来
        allExcelData.Add(path, data);

        return data;
    }
	


    /// <summary>
    /// 通过路径查找配置表数据
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T FindData<T>(string path) where T : ExcelBase
    {
        if(string.IsNullOrEmpty(path))
        {
            return null;
        }

        ExcelBase excelBase = null;

        if(allExcelData.TryGetValue(path, out excelBase))
        {
            return excelBase as T;
        }
        else
        {
            //未加载表
            excelBase = LoadData<T>(path);
        }

        return excelBase == null ? null : excelBase as T;
    }


}

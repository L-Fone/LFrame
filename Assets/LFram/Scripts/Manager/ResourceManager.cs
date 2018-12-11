using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源异步加载完成回调
/// </summary>
/// <param name="path">路径</param>
/// <param name="obj">资源</param>
/// <param name="param">传递的参数</param>
public delegate void OnAsyncObjFinish(string path, Object obj, Hashtable param = null);

/// <summary>
/// 异步加载的参数
/// </summary>
public class AsyncLoadResParam
{
    public uint Crc;
    public string Path;
    public LoadResPriority Priority = LoadResPriority.RES_SLOW;

    public List<OnAsyncObjFinish>

    public void Reset() 
    {
        Crc = 0;
        Path = string.Empty;
        Priority = LoadResPriority.RES_SLOW;
    }
}

/// <summary>
/// 资源加载管理
/// </summary>
public class ResourceManager : Singleton<ResourceManager> 
{

    //[缓存使用的资源列表]
    public Dictionary<uint, ResourceItem> AssetDict { get; set; } = new Dictionary<uint, ResourceItem>();

    //[缓存没人使用的资源列表]构建ResourceItem的双向列表 资源池 存的引用计数为0的缓存对象 达到缓存最大的时候，释放这个列表里面最早没用的资源
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();

    //mono脚本
    protected MonoBehaviour startMono;

    //正在异步加载的资源列表 队列
    protected List<AsyncLoadResParam>[] LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.RES_NUM];
    //保存正在异步加载的Dict
    protected Dictionary<uint, AsyncLoadResParam> LoadingAssetDict = new Dictionary<uint, AsyncLoadResParam>();

    /// <summary>
    /// 入口初始化
    /// </summary>
    public void Init(MonoBehaviour mono)
    {
        //优先级列表 初始化
        for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
        {
            LoadingAssetList[i] = new List<AsyncLoadResParam>();
        }

        startMono = mono;
        startMono.StartCoroutine(AsyncLoadCor());
    }

    /* ---------------------------------------资源同步加载-------------------------------------------------- */

    /// <summary>
    /// 资源同步加载，外部直接调用(仅加载不需要实例化资源，如：图片,音频等)
    /// </summary>
    /// <param name="path">路径</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if(string.IsNullOrEmpty(path)){ return null; }

        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if(item != null)
        {
            return item.obj as T;
        }

        //加载资源
        T obj = null;

#if UNITY_EDITOR
        if(!PathConst.LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResouceItem(crc);

            if(item.obj != null)
            {
                obj = item.obj as T;
            }
            else
            {
                obj = LoadAssetByEditor<T>(path);
            }            
        }
#endif

        if(obj == null)
        {
            item = AssetBundleManager.Instance.LoadReouseAssetBundle(crc);

            if(item != null && item.CurBundle != null)
            {
                if(item.obj != null)
                {
                    obj = item.obj as T;
                }
                else
                {
                    obj = item.CurBundle.LoadAsset<T>(item.AssetName);
                }                
            }
        }

        //缓存
        CacheResource(path, ref item, crc, obj);

        return obj;

    }

    /// <summary>
    /// 获取缓存的资源item
    /// </summary>
    /// <param name="crc">Crc</param>
    /// <param name="refCount">引用计数</param>
    /// <returns></returns>
    ResourceItem GetCacheResourceItem(uint crc, int refCount = 1)
    {
        ResourceItem item = null;  
        if(AssetDict.TryGetValue(crc, out item))
        {
            item.RefCount += refCount;
            item.LastUseTime = Time.realtimeSinceStartup;

            if(item.RefCount <= 1)
            {
                //从双向链表中移除
                m_NoRefrenceAssetMapList.Remove(item);
            }
        }
        return item;
    }

    /// <summary>
    /// 编辑器下加载资源
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
#if UNITY_EDITOR
    protected T LoadAssetByEditor<T>(string path) where T : UnityEngine.Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif

    /// <summary>
    /// 缓存加载的资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="item"></param>
    /// <param name="crc"></param>
    /// <param name="obj"></param>
    void CacheResource(string path, ref ResourceItem item, uint crc, Object obj, int refCount = 1)
    {
        //当缓存过多，清除最早没使用的缓存
        WashOut();

        if(item == null)
        {
            Debug.LogError("ResourceItem 为空，请检查： path = " + path);
        }

        if(obj == null)
        {
            Debug.LogError("资源缓存失败，请检查： path = " + path);
        }

        item.obj = obj;
        item.GUID = obj.GetInstanceID();
        item.LastUseTime = Time.realtimeSinceStartup;
        item.RefCount += refCount;

        //如果有老的资源 则替换
        ResourceItem oldItem = null;
        if(AssetDict.TryGetValue(item.Crc, out oldItem))
        {
            AssetDict[item.Crc] = item;
        }
        else
        {
            AssetDict.Add(item.Crc, item);
        }
    }

    /// <summary>
    /// 释放内存中的资源[不需要实例化的资源卸载]
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool DisposeResource(Object obj, bool destroyObj = false)
    {
        if(obj == null)
        {
            return false;
        }
        ResourceItem item = null;
        foreach (ResourceItem res in AssetDict.Values)
        {
            if(res.GUID == obj.GetInstanceID())
            {
                item = res;
                break;
            }
        }

        if(item == null)
        {
            Debug.LogError("AssetDict 未包含该资源：" + obj.name + "__原因：可能释放了多次，请检查！！！");
            return false;
        }
         item.RefCount --;

         DestroyResourceItem(item, destroyObj);
         return true;
    }


    /// <summary>
    /// 清除最早没用的缓存
    /// </summary>
    protected void WashOut()
    {
        //若当前内存使用大于80%时，进行清除最早没用的资源


    }

    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destroyCache">是否删除缓存</param>
    protected void DestroyResourceItem(ResourceItem item, bool destroyCache = false)
    {
        if(item == null || item.RefCount > 0)
        {
            return;
        }

        if(!AssetDict.Remove(item.Crc))
        {
            return;
        }

        if(!destroyCache)
        {
            m_NoRefrenceAssetMapList.InsertToHead(item);
            return;
        }

        //释放AssetBundle引用
        AssetBundleManager.Instance.DisposeAsset(item);

        if(item.obj != null)
        {
            item.obj = null;
        }
    }


    /* ---------------------------------------资源异步加载-------------------------------------------------- */

    /// <summary>
    /// 异步加载资源[仅是不需要实例化的资源，如音频,图片等]
    /// </summary>
    public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority, Hashtable param, uint crc = 0)
    {
        if(crc == 0)
        {
            crc = Crc32.GetCrc32(path);
        }

        //如果是已加载过并缓存中的资源
        ResourceItem item = GetCacheResourceItem(crc);
        if(item != null)
        {
            if(dealFinish != null)
            {
                dealFinish(path, item.obj, param);
            }
            return;
        }
        //判断是否在加载中
        AsyncLoadResParam temp = null;
        if(!LoadingAssetDict.TryGetValue(crc, out temp) || temp == null)
        {
            
        }
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncLoadCor()
    {
        while (true)
        {

            yield return null;
        }
    }

}








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

public delegate void OnAsyncFinish(string path, ResourceObject resobj, Hashtable param = null);

/// <summary>
/// 异步加载的参数
/// </summary>
public class AsyncLoadResParam
{
    public uint Crc;
    public string Path;
    //因为object不能转sprite所以要标记是否是图片，以便特殊处理
    public bool isSprite;
    public LoadResPriority Priority = LoadResPriority.RES_SLOW;
    //回调列表
    public List<AsyncCallBack> CallBackList = new List<AsyncCallBack>();

    public void Reset() 
    {
        Crc = 0;
        Path = string.Empty;
        Priority = LoadResPriority.RES_SLOW;
        CallBackList.Clear();
    }
}

/// <summary>
/// 异步加载资源回调类
/// </summary>
public class AsyncCallBack
{
    //加载完成的回调[针对ObjectManager]
    public OnAsyncFinish DealFinish = null;
    //ObjectManager 对应的中间类
    public ResourceObject Resobj = null;

    /* ---------------------------------------- */

    //加载完成的回调
    public OnAsyncObjFinish DealObjFinish = null;
    //回调参数
    public Hashtable Param = null;

    public void Reset() 
    {
        DealObjFinish = null;
        DealFinish = null;
        Resobj = null;
        Param = null;
    }
}

/// <summary>
/// 资源加载管理
/// </summary>
public class ResourceManager : Singleton<ResourceManager> 
{

    protected long M_GUID = 0;

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

    //异步加载 中间回调类对象池
    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);
    //异步回调类对象池
    protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);



    /* ---------------------------------------初始化-------------------------------------------------- */

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

    /// <summary>
    /// 创建唯一GUID 用于异步实例化资源标记
    /// </summary>
    /// <returns></returns>
    public long CreatGUID()
    {
        return M_GUID ++;
    }

    /* ---------------------------------------跳转场景,清空缓存-------------------------------------------------- */

    /// <summary>
    /// 跳场景时清空缓存
    /// </summary>
    public void ClearCache(bool close = false)
    {
        List<ResourceItem> tempList = new List<ResourceItem>();
        foreach (ResourceItem item in AssetDict.Values)
        {
            if(item.Clear || close)
            {
                tempList.Add(item);
            }
        }

        foreach (ResourceItem item in tempList)
        {
            DestroyResourceItem(item, true);
        }
        tempList.Clear();
    }

    /* ---------------------------------------资源预加载-------------------------------------------------- */

    /// <summary>
    /// 预加载入口
    /// </summary>
    public void ProLoadRes(string path)
    {
        if(string.IsNullOrEmpty(path))
        {
            return;
        }
         uint crc = Crc32.GetCrc32(path);

        ResourceItem item = GetCacheResourceItem(crc, 0);
        if(item != null)
        {
            return;
        }

        //加载资源
        Object obj = null;

#if UNITY_EDITOR
        if(!PathConst.LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResouceItem(crc);

            if(item.obj != null)
            {
                obj = item.obj as Object;
            }
            else
            {
                obj = LoadAssetByEditor<Object>(path);
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
                    obj = item.obj as Object;
                }
                else
                {
                    obj = item.CurBundle.LoadAsset<Object>(item.AssetName);
                }                
            }
        }

        //缓存
        CacheResource(path, ref item, crc, obj);
        //跳场景不清空缓存
        item.Clear = false;
        DisposeResource(path, false);
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
    /// 游戏资源同步加载[针对给 ObjectManager的接口]
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <returns></returns>
    public ResourceObject LoadResource(string path, ResourceObject resObj)
    {   
        if(resObj == null)
        {
            return null;
        }
        uint crc = resObj.Crc == 0 ? Crc32.GetCrc32(path) : resObj.Crc;

        //尝试从缓存当中去取
        ResourceItem item = GetCacheResourceItem(crc);
        if(item != null)
        {
            resObj.ResItem = item;
            return resObj;
        }

        //缓存取不到，则进行加载
        Object obj = null;
        
#if UNITY_EDITOR
        //编辑器下的加载
        if(!PathConst.LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResouceItem(crc);

            if(item != null && item.obj != null)
            {
                obj = item.obj;
            }
            else
            {
                if(item == null)
                {
                    item = new ResourceItem();
                    item.Crc = crc;
                }
                obj = LoadAssetByEditor<Object>(path);
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
                    obj = item.obj;
                }
                else
                {
                    obj = item.CurBundle.LoadAsset<Object>(item.AssetName);
                }                
            }
        }

        CacheResource(path, ref item, crc, obj);
        resObj.ResItem = item;
        item.Clear = resObj.Clear;

        return resObj;
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
    /// 释放内存中的资源[不需要实例化的资源卸载]
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool DisposeResource(ResourceObject resobj, bool destroyObj = false)
    {
        if(resobj == null)
        {
            return false;
        }        
        ResourceItem item = null;
        if(!AssetDict.TryGetValue(resobj.Crc, out item) || item == null)
        {
            Debug.LogError("AssetDict 未包含该资源：" + resobj.CloneObj.name + "__原因：可能释放了多次，请检查！！！");
        }

        item.RefCount --;
        
        //卸载资源
        GameObject.Destroy(resobj.CloneObj);

        DestroyResourceItem(item, destroyObj);
        return true;
    }

    /// <summary>
    /// 释放内存中的资源[不需要实例化的资源卸载]
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool DisposeResource(string path, bool destroyObj = false)
    {
        if(string.IsNullOrEmpty(path))
        {
            return false;
        }

        uint crc = Crc32.GetCrc32(path);

        ResourceItem item = null;
        if(!AssetDict.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError("AssetDict 未包含该资源：" + path + "__原因：可能释放了多次，请检查！！！");
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
        //当大于缓存个数时，进行一半释放
        while (m_NoRefrenceAssetMapList.Size() >= PathConst.MAX_CACHE_COUNT)
        {
            for (int i = 0; i < PathConst.MAX_CACHE_COUNT/2; i++)
            {
                ResourceItem item = m_NoRefrenceAssetMapList.GetTail();
                DestroyResourceItem(item, true);
            }
        }

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

        if(!destroyCache)
        {
            m_NoRefrenceAssetMapList.InsertToHead(item);
            return;
        }

        if(!AssetDict.Remove(item.Crc))
        {
            return;
        }

        m_NoRefrenceAssetMapList.Remove(item);

        //释放AssetBundle引用
        AssetBundleManager.Instance.DisposeAsset(item);

        //清空资源对应的对象池
        ObjectManager.Instance.ClearPoolObject(item.Crc);

        if(item.obj != null)
        {
            item.obj = null;
        }
#if UNITY_EDITOR
        //Resources.UnloadAsset(item.obj);
        Resources.UnloadUnusedAssets();
#endif
    }


    /* ---------------------------------------资源异步加载-------------------------------------------------- */

    /// <summary>
    /// 异步加载资源[仅是不需要实例化的资源，如音频,图片等]
    /// </summary>
    public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority, Hashtable param = null, uint crc = 0, bool isSprite = false)
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
            temp = m_AsyncLoadResParamPool.Spawn();
            temp.Crc = crc;
            temp.Path = path;
            temp.isSprite = isSprite;
            temp.Priority = priority;
            LoadingAssetDict.Add(crc, temp);
            //添加进异步队列
            LoadingAssetList[(int)priority].Add(temp);
        }

        //初始化回调类
        AsyncCallBack callback = m_AsyncCallBackPool.Spawn();
        callback.DealObjFinish = dealFinish;
        callback.Param = param;

        //添加到回调列表中
        temp.CallBackList.Add(callback);
    }

    /// <summary>
    /// 为GameObject提供的异步加载
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="resobj">资源Object</param>
    /// <param name="dealfinish">加载完成回调</param>
    /// <param name="priority">优先级</param>
    public void AsyncLoadResource(string path, ResourceObject resobj, OnAsyncFinish dealfinish, LoadResPriority priority, Hashtable param = null)
    {
        ResourceItem item = GetCacheResourceItem(resobj.Crc);
        if(item != null)
        {
            resobj.ResItem = item;
            if(dealfinish != null)
            {
                dealfinish(path, resobj, param);
            }
            return;
        }

        //判断是否在加载中
        AsyncLoadResParam temp = null;
        if(!LoadingAssetDict.TryGetValue(resobj.Crc, out temp) || temp == null)
        {
            temp = m_AsyncLoadResParamPool.Spawn();
            temp.Crc = resobj.Crc;
            temp.Path = path;
            temp.Priority = priority;
            LoadingAssetDict.Add(resobj.Crc, temp);
            //添加进异步队列
            LoadingAssetList[(int)priority].Add(temp);
        }

        //初始化回调类
        AsyncCallBack callback = m_AsyncCallBackPool.Spawn();
        callback.DealFinish = dealfinish;
        callback.Resobj = resobj;
        callback.Param = param;
        //添加到回调列表中
        temp.CallBackList.Add(callback);

    }




    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncLoadCor()
    {
        //回调列表
        List<AsyncCallBack> callbackList = null;

        //上一次Yield加载的时间
        long LastYieldTime = System.DateTime.Now.Ticks;

        while (true)
        {

            bool haveYield = false;

            //遍历优先级
            for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
            {
                //按低中高优先级加载
                //如果高级里面有东西
                if(LoadingAssetList[(int)LoadResPriority.RES_HIGHT].Count > 0)
                {
                    i = (int)LoadResPriority.RES_HIGHT;
                }
                //如果中级里面有东西
                else if(LoadingAssetList[(int)LoadResPriority.RES_MIDDLE].Count > 0)
                {
                    i = (int)LoadResPriority.RES_MIDDLE;
                }

                List<AsyncLoadResParam> loadingList = LoadingAssetList[i];
                if(loadingList.Count <=0 )
                {
                    continue;
                }

                AsyncLoadResParam loadingItem = loadingList[0];
                loadingList.RemoveAt(0);
                callbackList = loadingItem.CallBackList;

                Object obj = null;
                //加载资源
                ResourceItem item = null;
#if UNITY_EDITOR
                if (!PathConst.LoadFromAssetBundle)
                {
                    
                    //编辑器下加载
                    if(loadingItem.isSprite)
                    {
                        obj = LoadAssetByEditor<Sprite>(loadingItem.Path);
                    }
                    else
                    {
                        obj = LoadAssetByEditor<Object>(loadingItem.Path);
                    }

                    //模拟异步加载
                    yield return new WaitForSeconds(0.5f);

                    item = AssetBundleManager.Instance.FindResouceItem(loadingItem.Crc);

                    if(item == null)
                    {
                        item = new ResourceItem();
                        item.Crc = loadingItem.Crc;
                    }
                }
#endif

                if(obj == null)
                {
                    //AssetBundle加载
                    item = AssetBundleManager.Instance.LoadReouseAssetBundle(loadingItem.Crc);

                    if(item != null && item.CurBundle != null)
                    {
                        //异步加载
                        AssetBundleRequest abRequest = null;
                        if(loadingItem.isSprite)
                        {
                            abRequest = item.CurBundle.LoadAssetAsync<Sprite>(item.AssetName);
                        }
                        else
                        {
                            abRequest = item.CurBundle.LoadAssetAsync(item.AssetName);
                        }
                        
                        yield return abRequest;

                        //等待加载完成
                        if(abRequest.isDone)
                        {
                            obj = abRequest.asset;
                        }
                        LastYieldTime = System.DateTime.Now.Ticks;
                    }
                }

                //缓存资源
                CacheResource(loadingItem.Path, ref item, loadingItem.Crc, obj, callbackList.Count);

                //执行加载完成回调
                for (int j = 0; j < callbackList.Count; j++)
                {
                    AsyncCallBack callback = callbackList[j];

                    if(callback != null && callback.DealFinish != null && callback.Resobj != null)
                    {
                        ResourceObject tempResobj = callback.Resobj;
                        tempResobj.ResItem = item;
                        callback.DealFinish(loadingItem.Path, tempResobj, tempResobj.Param);
                        callback.DealFinish = null;
                        tempResobj = null;
                    }

                    if(callback != null && callback.DealObjFinish != null)
                    {
                        callback.DealObjFinish(loadingItem.Path, obj, callback.Param);
                        callback.DealObjFinish = null;
                    }

                    //还原并且回收到类对象池
                    callback.Reset();
                    m_AsyncCallBackPool.Recycle(callback);
                }                

                //还原并且置空
                obj = null;
                callbackList.Clear();
                LoadingAssetDict.Remove(loadingItem.Crc);

                loadingItem.Reset();
                m_AsyncLoadResParamPool.Recycle(loadingItem);


                //判断用时[单位是微秒]
                if(System.DateTime.Now.Ticks - LastYieldTime > PathConst.MAX_LOADRESOURCE_TIME)
                {
                    yield return null;
                    LastYieldTime = System.DateTime.Now.Ticks;
                    haveYield = true;
                }
            }

            //判断用时[单位是微秒]
            if(!haveYield || System.DateTime.Now.Ticks - LastYieldTime > PathConst.MAX_LOADRESOURCE_TIME)
            {
                LastYieldTime = System.DateTime.Now.Ticks;
                yield return null;
            }

        }//where end
    }

    /* ---------------------------------------取消异步资源加载-------------------------------------------------- */

    /// <summary>
    /// 取消异步加载资源
    /// </summary>
    /// <returns></returns>
    public bool CancelLoad(ResourceObject resobj)
    {
        AsyncLoadResParam para = null;
        if(LoadingAssetDict.TryGetValue(resobj.Crc, out para) && LoadingAssetList[(int)para.Priority].Contains(para))
        {
            for (int i = para.CallBackList.Count - 1; i >= 0; i--)
            {
                AsyncCallBack tempcall = para.CallBackList[i];
                if(tempcall != null && resobj == tempcall.Resobj)
                {
                    tempcall.Reset();
                    m_AsyncCallBackPool.Recycle(tempcall);
                    para.CallBackList.Remove(tempcall);
                }
            }

            //移除完成之后
            if(para.CallBackList.Count <= 0)
            {
                para.Reset();
                LoadingAssetList[(int)para.Priority].Remove(para);
                m_AsyncLoadResParamPool.Recycle(para);
                LoadingAssetDict.Remove(resobj.Crc);
                return true;
            }
        }

        return false;
    }



    /* ---------------------------------------引用计数-------------------------------------------------- */

    /// <summary>
    /// 根据ResoourceOebject增加引用计数
    /// </summary>
    /// <returns></returns>
    public int IncreaseResourceRef(ResourceObject resobj, int count = 1)
    {
        return resobj == null ? IncreaseResourceRef(resobj.Crc, count) : 0;
    }

    /// <summary>
    /// 根据path crc增加引用计数
    /// </summary>
    /// <returns></returns>
    public int IncreaseResourceRef(uint crc, int count = 1)
    {
        ResourceItem item = null;
        if(!AssetDict.TryGetValue(crc, out item) || item == null)
        {
            return 0;
        }
        item.RefCount += count;
        item.LastUseTime = Time.realtimeSinceStartup;

        return item.RefCount;
    }  


    /// <summary>
    /// 根据ResoourceOebject减少引用计数
    /// </summary>
    /// <returns></returns>
    public int DecreaseResourceRef(ResourceObject resobj, int count = 1)
    {
        return resobj == null ? DecreaseResourceRef(resobj.Crc, count) : 0;
    }


    /// <summary>
    /// 根据path crc减少引用计数
    /// </summary>
    /// <returns></returns>
    public int DecreaseResourceRef(uint crc, int count = 1)
    {
        ResourceItem item = null;
        if(!AssetDict.TryGetValue(crc, out item) || item == null)
        {
            return 0;
        }
        item.RefCount -= count;
        return item.RefCount;
    }


}








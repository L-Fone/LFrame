using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> MonoBehaviour单例基类 </summary>
public class MonoSingleton<T> : MonoBehaviourEx where T : MonoBehaviour   ///？？？？
{
    static T mInstance;

    public static T Instance {
        get {
            if (null == mInstance) {
                mInstance = FindObjectOfType(typeof(T)) as T;  //查询T类型的对象
                if (null == mInstance) {
                    //if (Application.isPlaying) {
                    //    GameObject go = new GameObject("_" + typeof(T).Name);
                    //    mInstance = go.AddComponent<T>();
                    //}
                    if (null == mInstance) {
                        if (Application.isPlaying && !Application.isEditor) {
                            Debug.LogError(typeof(T) + " 没有实例化！"); 
                        }
                    }
                }
            } else if (mInstance.gameObject == null) {
                Debug.LogError(typeof(T) + " 没有清理干净！"); 
            }

            return mInstance;
        }
    }

    void Awake() {
#if Debug_Init_TimeWatch
        HelpFun.timeWatch.Reset();
        HelpFun.timeWatch.Start();
        OnAwake();
        HelpFun.timeWatch.Stop();
        if (HelpFun.timeWatch.Elapsed.TotalSeconds > 0.05f) {
            DebugManager.Log(DebugerName.Public, string.Format("MonoBehaviour<{0}>OnAwake：{1}", this.ToString(), HelpFun.timeWatch.Elapsed));  
        }
#else
        OnAwake();
#endif
        //RegisterNotificationListener(true);
        DontDestroyOnLoad(this);
    }

    void Start() {
#if Debug_Init_TimeWatch
        HelpFun.timeWatch.Reset();
        HelpFun.timeWatch.Start();
        OnStart();
        HelpFun.timeWatch.Stop();
        if (HelpFun.timeWatch.Elapsed.TotalSeconds > 0.05f) {
            DebugManager.Log(DebugerName.Public, string.Format("MonoBehaviour<{0}>OnStart：{1}", this.ToString(), HelpFun.timeWatch.Elapsed)); 
        }
#else
        OnStart();
#endif
    }

    void OnDestroy() {
        RegisterNotificationListener(false);
    }


    protected virtual void OnAwake() {
    }

    protected virtual void OnStart() {
    }

    protected virtual void OnOnDestroy() {
    }

    protected virtual void RegisterNotificationListener(bool isAdd = true) {

    }
}

public abstract class MonoBehaviourEx : MonoBehaviour
{


}

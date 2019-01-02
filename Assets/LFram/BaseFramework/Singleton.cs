using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 普通单例基类(没有MonoBehaviour) </summary>
public class Singleton<T> where T : new()
{
    protected static T mInstance;  //通过静态变量获取类实例

    public static T Instance {
        get {
            if (mInstance == null) {
#if Debug_Init_TimeWatch
                HelpFun.timeWatch.Reset();
                HelpFun.timeWatch.Start();
                mInstance = new T();
                HelpFun.timeWatch.Stop();
                if (HelpFun.timeWatch.Elapsed.TotalSeconds > 0.1f) {
                    Debug.Log(string.Format("Singleton<{0}>Ctor：{1}", mInstance.ToString(), HelpFun.timeWatch.Elapsed));  
                }
#else
                mInstance = new T();
#endif
            }
            return mInstance;
        }
    }

    public static void New() {
        mInstance = new T();
    }
}

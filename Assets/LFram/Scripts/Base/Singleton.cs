using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例基类
/// </summary>
public class Singleton<T> where T : new()
{
    private static T m_instence;
    public static T Instance
    {
        get
        {
            if(m_instence == null)
            {
                m_instence = new T();
            }
            return m_instence;
        }
    }
	
}

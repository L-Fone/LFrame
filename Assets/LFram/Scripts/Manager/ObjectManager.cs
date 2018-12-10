using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 对象池管理
/// </summary>
public class ObjectManager : Singleton<ObjectManager> 
{
	// 根据类型存储对应类对象池	
	protected Dictionary<Type, object> m_ClassPoolDict = new Dictionary<Type, object>();

	/// <summary>
	/// 创建类对象池
	/// 创建完成后可以调用 Spawn 和 Recycle 进行创建和回收
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public ClassObjectPool<T> GetOrCreatClassPool<T> (int maxCount) where T : class,new()
	{
		Type type = typeof(T);
		object outObj = null;
		if(!m_ClassPoolDict.TryGetValue(type, out outObj) || outObj == null)
		{
			ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
			m_ClassPoolDict.Add(type, newPool);
			return newPool;
		}
		return outObj as ClassObjectPool<T>;
	}
	
	
}

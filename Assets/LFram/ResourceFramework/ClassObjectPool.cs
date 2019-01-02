using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 类对象池
/// </summary>
public class ClassObjectPool<T> where T : class, new()
{
	//池
	protected Stack<T> m_Pool = new Stack<T>();

	//最大对象个数 [小于等于0 为不限制个数]
	protected int m_MaxCount = 0;

	//没有回收的个数
	protected int m_NoRecycleCount = 0;


	public ClassObjectPool(int maxCount)
	{
		m_MaxCount = maxCount;
		for (int i = 0; i < maxCount; i++)
		{
			m_Pool.Push(new T());
		}
	}

	/// <summary>
	/// 从类对象池中取对象
	/// </summary>
	/// <param name="creatIfPoolEmpty">当可用个数为0，是否New一个出来</param>
	/// <returns>T</returns>
	public T Spawn(bool creatIfPoolEmpty = true)
	{
		if(m_Pool.Count > 0)
		{
			T rtn = m_Pool.Pop();
			if(rtn == null)
			{
				if(creatIfPoolEmpty)
				{
					rtn = new T();
				}
			}
			m_NoRecycleCount ++;
			return rtn;
		}
		else
		{
			if(creatIfPoolEmpty)
			{
				T rtn = new T();
				m_NoRecycleCount ++;
				return rtn;
			}
		}
		return null;
	}

	/// <summary>
	/// 回收到类对象池
	/// </summary>
	/// <param name="obj">要回收的对象</param>
	/// <returns>返回：是否成功回收</returns>
	public bool Recycle(T obj)
	{
		if(obj == null)
		{
			return false;
		}

		m_NoRecycleCount --;

		if(m_Pool.Count >= m_MaxCount && m_MaxCount > 0)
		{
			obj = null;
			return false;
		}

		m_Pool.Push(obj);
		return true;
	}
	
}

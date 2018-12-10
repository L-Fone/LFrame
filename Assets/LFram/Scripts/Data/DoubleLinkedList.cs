using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 双向链表节点集合
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkedList<T> where T : class, new()
{
	//表头
	public DoubleLinkedListNode<T> Head = null;
	//表尾
	public DoubleLinkedListNode<T> Tail = null;
	//双向链表结构类对象池
	protected ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkNodePool = ObjectManager.Instance.GetOrCreatClassPool<DoubleLinkedListNode<T>>(500);

	//链表保存个数
	protected int m_Count = 0;

	/// <summary>
	/// 双向链表保存个数
	/// </summary>
	public int Count
	{
		get{ return m_Count; }
	}

	/// <summary>
	/// 添加节点到头部
	/// </summary>
	/// <param name="t"></param>
	public DoubleLinkedListNode<T> AddToHead(T t)
	{
		DoubleLinkedListNode<T> pNode = m_DoubleLinkNodePool.Spawn();
		pNode.Next = null;
		pNode.Prev = null;
		pNode.CurNode = t;
		return AddToHead(pNode);
	}

	/// <summary>
	/// 添加节点到头部
	/// </summary>
	/// <param name="pNode"></param>
	public DoubleLinkedListNode<T> AddToHead(DoubleLinkedListNode<T> pNode)
	{
		if(pNode == null)
		{
			return null;
		}

		pNode.Prev = null;

		//如果头部是空的说明这是第一个数据，既是头也是尾
		if(Head == null)
		{
			Head = Tail = pNode;
		}
		else
		{
			pNode.Next = Head;
			Head.Prev = pNode;
			Head = pNode;
		}

		m_Count ++;
		return Head;
	}

	/// <summary>
	/// 添加节点到尾部
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public DoubleLinkedListNode<T> AddToTail(T t)
	{
		DoubleLinkedListNode<T> pNode = m_DoubleLinkNodePool.Spawn();
		pNode.Next = null;
		pNode.Prev = null;
		pNode.CurNode = t;
		return AddToTail(pNode);
	}

	/// <summary>
	/// 添加节点到尾部
	/// </summary>
	/// <param name="pList"></param>
	/// <returns></returns>
	public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
	{
		if(pNode == null)
		{
			return null;
		}

		pNode.Next = null;

		//如果尾部是空的说明这是第一个数据，既是头也是尾
		if(Tail == null)
		{
			Head = Tail = pNode;
		}
		else
		{
			pNode.Prev = Tail;
			Tail.Next = pNode;
			Tail = pNode;
		}

		m_Count ++;
		return Tail;
	}


	/// <summary>
	/// 移除链表的某个节点
	/// </summary>
	/// <param name="pNode"></param>
	public void RemoveNode(DoubleLinkedListNode<T> pNode)
	{
		if(pNode == null)
		{
			return;
		}

		//如果这个节点是头部
		if(pNode == Head)
		{
			Head = pNode.Next;
		}

		//如果这个节点是尾部
		if(pNode == Tail)
		{
			Tail = pNode.Prev;
		}

		//如果该节点前面有节点
		if(pNode.Prev != null)
		{
			pNode.Prev.Next = pNode.Next;
		}

		//如果该节点后一个有节点
		if(pNode.Next != null)
		{
			pNode.Next.Prev = pNode.Prev;
		}

		pNode.Next = pNode.Prev = null;
		pNode.CurNode = null;

		m_DoubleLinkNodePool.Recycle(pNode);
		m_Count--;
	}


	/// <summary>
	/// 把某个节点移动到头部
	/// </summary>
	/// <param name="pNode"></param>
	public void MoveToHead(DoubleLinkedListNode<T> pNode)
	{
		if(pNode == null || pNode == Head)
		{
			return;
		}

		if(pNode.Prev == null && pNode.Next == null)
		{
			return;
		}

		//如果这是一个尾部节点
		if(pNode == Tail)
		{
			Tail = pNode.Prev;
		}

		if(pNode.Prev != null)
		{
			pNode.Prev.Next = pNode.Next;
		}

		if(pNode.Next != null)
		{
			pNode.Next.Prev = pNode.Prev;
		}

		pNode.Prev = null;
		pNode.Next = Head;
		Head.Prev = pNode;
		Head = pNode;

		if(Tail == null)
		{
			Tail = Head;
		}
	}


}

/// <summary>
/// 双向链表节点数据
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkedListNode<T> where T : class, new()
{
	//前一个节点
	public DoubleLinkedListNode<T> Prev = null;
	//后一个节点
	public DoubleLinkedListNode<T> Next = null;
	//当前节点
	public T CurNode = null;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3D物体离线数据
/// </summary>
public class OfflineData : MonoBehaviour
{
	//刚体
	public Rigidbody mRigidbody;
	//碰撞体
	public Collider mCollider;
	//所有子节点
	public Transform[] mAllPoint;
	//所有子节点下面的个数
	public int[] mAllPointChildCount;
	//子节点显示状态
	public bool[] mAllPointActive;
	//每个节点位置信息
	public Vector3[] mPos;
	//每个节点大小信息
	public Vector3[] mScale;
	//每个节点旋转信息
	public Quaternion[] mRot;


	/// <summary>
	/// 还原属性
	/// </summary>
	public virtual void ResetProp() 
	{
		int allPointCount = mAllPoint.Length;
		for (int i = 0; i < allPointCount; i++)
		{
			Transform trans = mAllPoint[i];
			if(trans != null)
			{
				trans.localPosition = mPos[i];
				trans.localScale = mScale[i];
				trans.localRotation = mRot[i];

				if(mAllPointActive[i])
				{
					if(!trans.gameObject.activeSelf)
					{
						trans.gameObject.SetActive(true);
					}
				}
				else
				{
					if(trans.gameObject.activeSelf)
					{
						trans.gameObject.SetActive(false);
					}
				}

				if(trans.childCount > mAllPointChildCount[i])
				{
					int childCount = trans.childCount;
					for (int j = 0; j < childCount; j++)
					{
						GameObject obj = trans.GetChild(j).gameObject;
						//删除非对象池创建的
						if(!ObjectManager.Instance.IsObjectMgrCreat(obj))
						{
							GameObject.Destroy(obj);
						}
					}
				}
			}
		}
	}


	/// <summary>
	/// 编辑器下保存初始值
	/// </summary>
	public virtual void BindData()
	{
		mCollider = gameObject.GetComponentInChildren<Collider>(true);
		mRigidbody = gameObject.GetComponentInChildren<Rigidbody>(true);
		mAllPoint = gameObject.GetComponentsInChildren<Transform>(true);

		int allPointCount = mAllPoint.Length;
		mAllPointChildCount = new int[allPointCount];
		mAllPointActive = new bool[allPointCount];
		mPos = new Vector3[allPointCount];
		mScale = new Vector3[allPointCount];
		mRot = new Quaternion[allPointCount];

		for (int i = 0; i < allPointCount; i++)
		{
			Transform trans = mAllPoint[i] as Transform;
			mAllPointChildCount[i] = trans.childCount;
			mAllPointActive[i] = trans.gameObject.activeSelf;
			mPos[i] = trans.localPosition;
			mRot[i] = trans.localRotation;
			mScale[i] = trans.localScale;
		}

	}
	
}

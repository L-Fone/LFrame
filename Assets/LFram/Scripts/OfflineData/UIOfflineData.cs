using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI离线数据
/// </summary>
public class UIOfflineData : OfflineData
{
    //UGUI相关参数
    public Vector2[] mAnchorMax;
    public Vector2[] mAnchorMin;
    public Vector2[] mPivot;
    public Vector2[] mSizeDelta;
    public Vector3[] mAnchoredPos;

    //粒子
    public ParticleSystem[] mParticle;
	

    //还原
    public override void ResetProp()
    {
        int allPointCount = mAllPoint.Length;
        for (int i = 0; i < allPointCount; i++)
        {
            RectTransform trans = mAllPoint[i] as RectTransform;
            if(trans != null)
            {
                trans.localPosition = mPos[i];
                trans.localRotation = mRot[i];
                trans.localScale = mScale[i];
                
                trans.pivot = mPivot[i];
                trans.anchorMax = mAnchorMax[i];
                trans.anchorMin = mAnchorMin[i];
                trans.sizeDelta = mSizeDelta[i];
                trans.anchoredPosition3D = mAnchoredPos[i];
            }

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

        //还原粒子特效
        int particleCout = mParticle.Length;
        for (int p = 0; p < particleCout; p++)
        {
            mParticle[p].Clear(true);
            mParticle[p].Play();
        }
    }


    //绑定初始化
    public override void BindData()
    {
        Transform[] allTrans  = gameObject.GetComponentsInChildren<Transform>(true);
        int allTransCount = allTrans.Length;
        for (int i = 0; i < allTransCount; i++)
        {
            if(!(allTrans[i] is RectTransform))
            {
                allTrans[i].gameObject.AddComponent<RectTransform>();
            }
        }

        mAllPoint = gameObject.GetComponentsInChildren<RectTransform>(true);
        mParticle = gameObject.GetComponentsInChildren<ParticleSystem>(true);

        int allPointCount = mAllPoint.Length;
        mAllPointChildCount = new int[allPointCount];
		mAllPointActive = new bool[allPointCount];
		mPos = new Vector3[allPointCount];
		mScale = new Vector3[allPointCount];
		mRot = new Quaternion[allPointCount];
    
        mPivot = new Vector2[allPointCount];
        mAnchorMax = new Vector2[allPointCount];
        mAnchorMin = new Vector2[allPointCount];
        mSizeDelta = new Vector2[allPointCount];
        mAnchoredPos = new Vector3[allPointCount];

		for (int i = 0; i < allPointCount; i++)
		{
            RectTransform trans = mAllPoint[i] as RectTransform;
            mAllPointChildCount[i] = trans.childCount;
            mAllPointActive[i] = trans.gameObject.activeSelf;
            mPos[i] = trans.localPosition;
            mRot[i] = trans.localRotation;
            mScale[i] = trans.localScale;

            mPivot[i] = trans.pivot;
            mAnchorMax[i] = trans.anchorMax;
            mAnchorMin[i] = trans.anchorMin;
            mSizeDelta[i] = trans.sizeDelta;
            mAnchoredPos[i] = trans.anchoredPosition3D;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效离线数据
/// </summary>
public class EffectOfflineData : OfflineData
{

    //粒子
    public ParticleSystem[] mParticle;
    //拖尾
    public TrailRenderer[] mTrailRe;

	//还原
    public override void ResetProp()
    {
        base.ResetProp();

        int particleCount = mParticle.Length;
        for (int i = 0; i < particleCount; i++)
        {
            mParticle[i].Clear(true);
            mParticle[i].Play();
        }

        int trailCount = mTrailRe.Length;
        for (int i = 0; i < trailCount; i++)
        {
            mTrailRe[i].Clear();
        }
    }

        //绑定初始化
    public override void BindData()
    {
        base.BindData();

        mParticle = gameObject.GetComponentsInChildren<ParticleSystem>(true);
        mTrailRe = gameObject.GetComponentsInChildren<TrailRenderer>(true);
    }
}

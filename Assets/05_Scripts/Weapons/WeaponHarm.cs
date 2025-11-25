using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞伤害触发脚本（2D场景）
/// 功能：当当前物体（如子弹、技能特效）与标签为"Enemy"的敌人碰撞时，对敌人造成指定伤害
/// 挂载对象：需要触发伤害的2D物体（如子弹预制体、技能碰撞体）
/// </summary>
public class WeaponHarm : MonoBehaviour
{
    [Tooltip("单次碰撞造成的伤害值")]
    public float damageAmount;

    [Tooltip("销毁武器的时间")]
    public float lifeTime = 3f;

    [Tooltip("控制物体变大/变小的快慢，越小越慢")]
    public float growSpeed = 1f;
    /// 存储物体预设的最终缩放值（从Inspector面板设置的localScale读取）
    private Vector3 targetSize;

    [Header("是否开启击退效果（默认关闭）")]
    public bool shouldKnockBack = false;

    void Start()
    {
        //Destroy(gameObject, lifeTime);

        // 1. 记录物体预设的目标缩放值（即Inspector面板设置的localScale）
        targetSize = transform.localScale;
        // 2. 初始缩放设为0（物体生成时完全不可见，准备从小到大缩放）
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        // 核心逻辑：从当前值平滑移动到目标值
        // Vector3.MoveTowards：匀速线性，避免速度忽快忽慢
        transform.localScale = Vector3.MoveTowards(
            transform.localScale,  // 当前值
            targetSize,            // 目标值（生成时是预设大小，结束时是0）
            growSpeed * Time.deltaTime  // 每帧缩放增量
        );

        // 销毁武器的时间计时：每帧减少当前剩余时间
        lifeTime -= Time.deltaTime;

        // 当销毁武器的时间结束（剩余时间≤0），开始缩小销毁流程
        if (lifeTime <= 0)
        {
            // 把目标缩放值设为0（触发缩小动画）
            targetSize = Vector3.zero;

            // 当物体完全缩小到0（X轴缩放为0，因XYZ等比缩放，X=0则整体为0），销毁物体
            if (transform.localScale.x == 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 2D碰撞触发回调（当当前物体的Collider2D与其他物体的Collider2D首次碰撞时执行）
    /// 注意：需确保当前物体和目标物体都挂载了Collider2D组件，且至少有一方勾选"Is Trigger"
    /// </summary>
    /// <param name="collision">碰撞到的目标物体的Collider2D组件</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 判断碰撞到的物体标签是否为"Enemy"（仅对敌人造成伤害）
        if (collision.tag == "Enemy")
        {
            // 1. 通过碰撞体获取敌人身上的EnemyController脚本（敌人的控制脚本，需包含TakeDamage方法）
            // 2. 调用EnemyController中的TakeDamage方法，传入伤害值，实现敌人扣血
            collision.GetComponent<EnemyController>().TakeDamage(damageAmount,shouldKnockBack);
        }
    }
}

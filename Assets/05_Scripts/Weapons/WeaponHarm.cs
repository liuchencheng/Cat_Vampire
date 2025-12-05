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

    [Tooltip("武器持续的时间")]
    public float lifeTime = 3f;

    [Tooltip("控制物体变大/变小的快慢，越小越慢")]
    public float growSpeed = 1f;
    /// 存储物体预设的最终缩放值（从Inspector面板设置的localScale读取）
    private Vector3 targetSize;

    [Header("是否开启击退效果（默认关闭）")]
    public bool shouldKnockBack = false;

    /// <summary>
    /// 是否销毁父物体的开关标识
    /// 通常用于：当当前物体（如子弹、特效）生命周期结束时，是否连带销毁它的父物体（比如武器载体、特效容器）
    /// 暂时没用到
    /// </summary>
    [Header("是否销毁父物体的开关标识")]
    public bool destroyParent;

    // 是否开启持续伤害模式
    [Tooltip("是否开启持续伤害模式")]
    public bool damageOverTime;
    // 持续伤害的间隔时间（秒）
    [Tooltip("持续伤害的间隔时间（秒）")]
    public float timeBetweenDamage;
    // 持续伤害的计时器（私有，仅脚本内使用）
    private float damageCounter;

    // 存储范围内的敌人列表（用于持续伤害）
    // 核心修改：将原视频中的List改为HashSet，提升增删效率并自动去重
    private HashSet<EnemyController> enemiesInRange = new HashSet<EnemyController>();

    /// <summary>
    /// 碰撞后是否销毁当前抛射物的开关
    /// - true：抛射物碰撞到物体后立即销毁
    /// - false：抛射物碰撞后不自动销毁（需手动控制生命周期）
    /// </summary>
    [Tooltip("碰撞后是否销毁抛射物")]
    public bool destroyOnImpact;

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
            //缩放判断优化（避免浮点精度问题：transform.localScale.x == 0f 可能因精度不触发）
            if (Mathf.Approximately(transform.localScale.x, 0f))
            {
                Destroy(gameObject);
            }
        }

        // 持续伤害模式的逻辑
        if (damageOverTime) // 布尔判断简化写法
        {
            // 计时器递减（按帧更新）
            damageCounter -= Time.deltaTime;

            // 计时器归零时，对范围内所有敌人造成一次伤害
            if (damageCounter <= 0f)
            {
                // 重置计时器（增加边界保护，避免负数累计）
                damageCounter = Mathf.Max(timeBetweenDamage, 0.01f);

                // 先复制一份HashSet的快照（避免遍历原集合时被修改）
                List<EnemyController> enemySnapshot = new List<EnemyController>(enemiesInRange);
                // 先清理快照里的空敌人
                enemySnapshot.RemoveAll(enemy => enemy == null);

                // 遍历“快照”而不是原HashSet
                foreach (var enemy in enemySnapshot)
                {
                    enemy?.TakeDamage(damageAmount, shouldKnockBack);
                }

                // 清理HashSet中已销毁的敌人（适配HashSet特性的安全清理方法）
                List<EnemyController> enemiesToRemove = new List<EnemyController>(); // 新建专属列表
                foreach (var enemy in enemiesInRange)
                {
                    if (enemy == null)
                    {
                        enemiesToRemove.Add(enemy); // 只存空敌人
                    }
                }
                // 批量移除空敌人（仅删空，不影响有效敌人）
                foreach (var enemy in enemiesToRemove)
                {
                    enemiesInRange.Remove(enemy);
                }
            }
        }
    }

    /// <summary>
    /// 2D碰撞触发回调（当当前物体的Collider2D与其他物体的Collider2D首次碰撞时执行）
    /// 注意：需确保当前物体和目标物体都挂载了Collider2D组件，且至少有一方勾选"Is Trigger"
    /// </summary>
    /// <param name="collision">进入的碰撞体</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 非持续伤害模式：触发时直接造成一次伤害
        if (damageOverTime == false)
        {
            // 只对标签为"Enemy"的对象生效
            if (collision.tag == "Enemy")
            {
                // 1. 通过碰撞体获取敌人身上的EnemyController脚本（敌人的控制脚本，需包含TakeDamage方法）
                // 2. 调用EnemyController中的TakeDamage方法，传入伤害值，实现敌人扣血
                collision.GetComponent<EnemyController>().TakeDamage(damageAmount, shouldKnockBack);

                // 若开启“碰撞后销毁”开关，则碰撞时立即销毁当前抛射物
                if (destroyOnImpact)
                {
                    Destroy(gameObject);
                }
            }
        }
        // 持续伤害模式：将进入的敌人加入“范围内敌人HashSet”
        else
        {
            if (collision.tag == "Enemy")
            {
                var enemyCtrl = collision.GetComponent<EnemyController>();
                if (enemyCtrl != null) // 避免添加空引用到HashSet
                {
                    enemiesInRange.Add(enemyCtrl); // HashSet自动去重，无需手动检查是否已存在
                }
            }
        }
    }


    /// <summary>
    /// 触发器退出事件（2D）
    /// 当有碰撞体离开武器范围时触发
    /// </summary>
    /// <param name="collision">离开的碰撞体</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 仅在持续伤害模式下生效
        if (damageOverTime == true)
        {
            // 将离开的敌人从“范围内敌人HashSet”中移除
            if (collision.tag == "Enemy")
            {
                var enemyCtrl = collision.GetComponent<EnemyController>();
                if (enemyCtrl != null)
                {
                    // 延迟移除（避免在伤害遍历中修改HashSet）
                    StartCoroutine(DelayRemoveEnemy(enemyCtrl));
                }
            }
        }
    }

    // 下一帧再移除，避开当前遍历流程
    private IEnumerator DelayRemoveEnemy(EnemyController enemy)
    {
        yield return null; // 等待一帧
        enemiesInRange.Remove(enemy);
    }
}

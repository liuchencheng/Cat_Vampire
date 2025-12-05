using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人追踪控制器
/// 功能：让敌人自动朝向并追击玩家（2D场景）
/// 核心逻辑：通过计算敌人与玩家的方向向量，设置刚体速度实现追踪
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("组件")]
    public Rigidbody2D theRB; // 敌人的2D刚体组件（用于控制移动，需在Inspector拖入）
    [Header("移动速度")]
    public float moveSpeed = 0.3f; // 敌人移动速度（公开可在Inspector调整）
    [Header("造成的伤害值")]
    public float damage = 10; // 敌人造成的伤害值
    [Header("攻击冷却计时器")]
    private float hitCounter; // 攻击冷却计时器
    [Header("攻击间隔/秒")]
    public float hitWaitTime = 1f; // 攻击间隔

    [Header("内部引用（无需手动赋值）")]
    private Transform target; // 追踪目标的Transform组件（这里特指玩家）

    [Header("敌人生命值")]
    public float health = 5f;

    [Header("击退持续时间（单位：秒，默认0.5秒）")]
    public float knockBackTime = .5f;
    // 击退计时器（私有，仅脚本内使用）
    private float knockBackCounter;

    [Header("该敌人提供的经验值")]
    public int expToGive = 1;

    /// <summary>
    /// 游戏启动时执行一次
    /// 作用：获取玩家的Transform引用，作为敌人的追击目标
    /// </summary>
    void Start()
    {
        // 查找场景中挂载了PlayerController脚本的对象（即玩家），并获取其Transform组件
        // 前提：场景中仅存在一个PlayerController（确保玩家唯一）
        //target = FindObjectOfType<PlayerController>().transform;
        target = PlayerHealthController.Instance.transform;

        // 容错处理：若未找到玩家，打印错误日志（避免空引用导致游戏崩溃）
        if (target == null)
        {
            Debug.LogError("EnemyController：场景中未找到PlayerController组件！敌人无法追踪目标");
        }

        // 容错处理：若未赋值Rigidbody2D，打印错误日志
        if (theRB == null)
        {
            Debug.LogError("EnemyController：请给EnemyController脚本的theRB字段赋值Rigidbody2D组件！");
        }
    }

    /// <summary>
    /// 每帧更新时执行
    /// 作用：实时计算追击方向，更新敌人移动速度（控制追击行为）
    /// </summary>
    void Update()
    {
        //玩家活着才会执行
        if (!PlayerController.Instance.isDead)
        {
            // 处理击退逻辑（计时器>0时执行）
            HandleKnockback();

            // 仅当目标（玩家）和刚体组件都存在时，才执行追击逻辑
            if (target != null && theRB != null)
            {
                // 1. 计算敌人指向玩家的方向向量（目标位置 - 自身位置）
                // 2. 归一化方向向量：确保斜向移动和水平/垂直移动速度一致（避免斜向更快）
                // 3. 设置刚体速度：方向向量 × 移动速度 = 最终移动速度（实现朝向玩家移动）
                theRB.velocity = (target.position - transform.position).normalized * moveSpeed;
            }

            // 攻击冷却倒计时
            if (hitCounter > 0f)
            {
                hitCounter -= Time.deltaTime;
            }
        }
        else
        {
            //死了直接所有敌人静止
            theRB.velocity = Vector2.zero;
        }
    }

    // 当敌人与玩家碰撞时触发伤害
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测碰撞对象是否是玩家（标签为"Player"）
        if (collision.gameObject.tag == "Player" && hitCounter <= 0f)
        {
            PlayerHealthController.Instance.TakeDamage(damage);
            hitCounter = hitWaitTime; // 重置攻击冷却
        }
    }

    //Hp扣除方法
    public void TakeDamage(float damageToTake)
    {
        health -= damageToTake;
        if (health <= 0)
        {
            Destroy(gameObject);

            // 调用经验等级控制器的SpawnExp方法，在当前物体的位置生成经验拾取物
            // 参数1：经验值生成位置（当前敌人的世界坐标）
            // 参数2：要生成的经验值（使用上面配置的expToGive）
            ExperienceLevelController.Instance.SpawnExp(transform.position, expToGive);
        }

        // 调用伤害数字控制器的单例实例，生成伤害数字
        // DamageNumberController.Instance：获取DamageNumberController的单例对象（确保全局唯一的控制器）
        // SpawnDamage：调用生成伤害数字的方法
        // damageToTake：要显示的伤害数值（浮点型，会在SpawnDamage中转为整数）
        // transform.position：伤害数字的生成位置（当前物体的世界坐标位置，比如敌人受击点）
        DamageNumberController.Instance.SpawnDamage(damageToTake, transform.position);
    }

    /// <summary>
    /// 带击退的受击方法（扩展基础TakeDamage）
    /// </summary>
    /// <param name="damageToTake">本次受到的伤害值</param>
    /// <param name="shouldKnockback">是否触发击退</param>
    public void TakeDamage(float damageToTake, bool shouldKnockback)
    {
        // 调用基础扣血方法
        TakeDamage(damageToTake);

        // 若开启击退且参数为true，启动击退计时器
        if (shouldKnockback == true)
        {
            knockBackCounter = knockBackTime;
        }
    }

    /// <summary>
    /// 处理击退移动逻辑
    /// </summary>
    private void HandleKnockback()
    {
        if (knockBackCounter > 0)
        {
            // 计时器随时间递减
            knockBackCounter -= Time.deltaTime;

            // 若当前移动速度为正（敌人正向移动），反转并放大速度实现击退
            if (moveSpeed > 0)
            {
                // 击退速度 = 原速度 × (-2) → 反向且更快（实现击退效果）
                moveSpeed = -moveSpeed * 2f;
            }

            // 击退结束后，恢复原速度
            if (knockBackCounter <= 0)
            {
                // Mathf.Abs确保速度为正，*0.5f是恢复原速度，因为上面的两行代码乘了2
                moveSpeed = Mathf.Abs(moveSpeed * .5f);
            }
        }
    }

}

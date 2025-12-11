using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抛射物型武器控制脚本（如追踪子弹、定向炮弹）
/// 功能：根据武器等级属性，周期性生成朝向敌人的抛射物
/// </summary>
public class ProjectileWeapon : Weapon
{
    // 抛射物预制体
    [Tooltip("抛射物预制体")]
    public WeaponHarm damager;
    // 抛射物预制体
    [Tooltip("抛射物预制体")]
    public Projectile projectile;
    // 武器基础攻击范围（与等级属性的range相乘）
    [Tooltip("武器基础攻击范围（与等级属性的range相乘）")]
    public float weaponRange;
    // 敌人层级遮罩（仅检测该层级下的敌人）
    [Tooltip("敌人层级遮罩（仅检测该层级下的敌人）")]
    public LayerMask whatIsEnemy;
    // 射击计时器（控制两次攻击的间隔）
    private float shotCounter;

    void Start()
    {
        // 初始化武器属性（加载当前等级的配置）
        SetStats();
    }


    // 根据当前武器等级，更新伤害器、抛射物的属性
    void SetStats()
    {
        // 给伤害器赋值当前等级的伤害值
        damager.damageAmount = stats[weaponLevel].damage;
        // 给伤害器赋值当前等级的持续时间
        damager.lifeTime = stats[weaponLevel].duration;
        // 按当前等级的大小，缩放伤害器的尺寸
        damager.transform.localScale = Vector3.one * stats[weaponLevel].weaponSize;

        // 重置射击计时器（避免初始状态立即攻击）
        shotCounter = 0f;
        // 给抛射物赋值当前等级的移动速度
        projectile.moveSpeed = stats[weaponLevel].speed;
    }


    void Update()
    {
        // 若属性更新标记为true，升级武器属性
        if (statsUpdated == true)
        {
            statsUpdated = false; // 重置更新标记
            SetStats(); // 重新加载新等级的属性
        }

        // 射击计时器按帧递减（Time.deltaTime保证时间流速稳定）
        shotCounter -= Time.deltaTime;

        // 计时器归零时，触发攻击逻辑
        if (shotCounter <= 0)
        {
            // 重置计时器为当前等级的攻击间隔
            shotCounter = stats[weaponLevel].timeBetweenAttacks;

            // 检测范围内的所有敌人：
            // 参数1：检测中心（武器所在位置）
            // 参数2：检测半径（基础范围 × 当前等级的范围倍率）
            // 参数3：目标层级（仅检测whatIsEnemy指定的敌人）
            Collider2D[] enemies = Physics2D.OverlapCircleAll(
                transform.position,
                weaponRange * stats[weaponLevel].range,
                whatIsEnemy
            );

            // 若范围内有敌人，执行抛射物发射逻辑
            if (enemies.Length > 0)
            {
                // 按当前等级的“单次发射数量”，循环生成抛射物
                for (int i = 0; i < stats[weaponLevel].amount; i++)
                {
                    // 随机选择一个范围内的敌人作为目标
                    Vector3 targetPosition = enemies[Random.Range(0, enemies.Length)].transform.position;

                    // 计算武器到目标敌人的方向向量
                    Vector3 direction = targetPosition - transform.position;
                    // 将方向向量转为角度（弧度转角度）
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    // 角度偏移90度（适配2D物体的旋转方向）
                    angle -= 90;

                    // 设置抛射物的旋转角度（让抛射物朝向目标）
                    projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    // 实例化抛射物：
                    // 参数1：抛射物预制体
                    // 参数2：生成位置（预制体自身的位置）
                    // 参数3：生成旋转（已计算的朝向）
                    // SetActive(true)：确保生成后抛射物立即激活
                    Instantiate(projectile, projectile.transform.position, projectile.transform.rotation)
                        .gameObject.SetActive(true);
                }

                //抛射类武器的音效
                SFXManager.Instance.PlaySFXPitched(4);
            }
        }
    }
}

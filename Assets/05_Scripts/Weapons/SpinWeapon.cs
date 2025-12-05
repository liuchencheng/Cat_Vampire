using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器环绕旋转发射脚本
/// 功能：控制指定的载体（Holder）沿Z轴持续旋转，带动武器（预制体）一起旋转
/// 核心逻辑：通过计时器间隔生成子弹（预制体 Orbital_Bolt）
/// </summary>
public class SpinWeapon : Weapon
{
    [Tooltip("旋转速度（单位：度/秒）")]
    public float rotateSpeed = 150f;

    [Tooltip("要旋转的目标载体")]
    public Transform holder;

    [Tooltip("武器预制体")]
    public Transform weapon;
    [Tooltip("子弹生成的间隔时间（单位：秒）")]
    public float timeBetweenSpawn = 4f;
    /// <summary>
    /// 生成计时器（私有变量，仅脚本内部使用）
    /// 用于记录间隔时间，判断是否达到生成子弹的时机
    /// </summary>
    private float spawnCounter;

    // 武器组件引用
    public WeaponHarm damager;

    private Transform playerTransform; // 新增：玩家Transform

    void Start()
    {
        SetStats();

        /// <summary>
        /// 调用升级按钮的显示更新方法
        /// 将当前武器数据（this代表调用该代码的Weapon实例）传递给按钮，刷新其显示内容
        /// </summary>
        //UIController.Instance.levelUpButtons[0].UpdateButtonDisplay(this);

        // 获取玩家Transform（和WeaponPivotController保持一致）
        playerTransform = PlayerHealthController.Instance.transform;
    }

    void Update()
    {
        // 1. 获取holder当前的Z轴旋转角度（基于世界坐标系）
        // 2. 计算当前帧应增加的旋转角度（rotateSpeed：度/秒 × Time.deltaTime：当前帧耗时）
        //    确保旋转速度稳定，每秒旋转角度固定为rotateSpeed
        // 3. 通过Quaternion.Euler将X/Y/Z轴旋转角度转为四元数（避免万向锁问题）
        //    其中X/Y轴保持0度不旋转，仅修改Z轴角度
        // 4. 将计算后的旋转角度赋值给holder，实现实际旋转
        /*holder.rotation = Quaternion.Euler(
            0f,                          // X轴旋转角度：固定0度，不旋转
            0f,                          // Y轴旋转角度：固定0度，不旋转
            holder.rotation.eulerAngles.z + (rotateSpeed * Time.deltaTime)  // Z轴累加旋转角度
        );*/

        // 计算新的Z轴旋转角度：
        // 基于当前旋转角度 + （旋转速度 * 帧间隔时间 * 当前等级的速度属性）
        // 作用：武器等级提升后，旋转速度会随stats[weaponLevel].speed增加而加快
        holder.rotation = Quaternion.Euler(0f, 0f,
            holder.rotation.eulerAngles.z + (rotateSpeed * Time.deltaTime * stats[weaponLevel].speed));

        // 子弹生成的计时逻辑
        spawnCounter -= Time.deltaTime;  // 计时器时间递减
        if (spawnCounter <= 0)  // 当计时器≤0时，触发子弹生成
        {
            spawnCounter = timeBetweenSpawn;  // 重置计时器，准备下一次生成

            // 生成子弹逻辑：
            // - 预制体：使用指定的weapon预制体
            // - 生成位置：与weapon预制体的位置保持一致
            // - 生成旋转：与weapon预制体的旋转保持一致
            // - 父物体：将生成的子弹挂载到holder下，跟随holder一起旋转
            // - 激活物体：生成后立即激活（SetActive(true)）
            //Instantiate(weapon, weapon.position, weapon.rotation, holder).gameObject.SetActive(true);

            //加月光水晶数量的时候调用，融入了武器攻击范围
            SpawnOrbitalFireballs();
        }

        //升级的时候增强武器
        if (statsUpdated == true)
        {
            statsUpdated = false; // 重置升级标记
            SetStats(); // 调用属性设置方法，刷新武器当前等级的属性
        }
    }

    /// <summary>
    /// 根据当前武器等级更新所有相关属性
    /// 通常在武器升级后或初始化时调用
    /// </summary>
    public void SetStats()
    {
        // 1. 设置伤害器的伤害值 = 武器单次攻击伤害
        damager.damageAmount = stats[weaponLevel].damage;

        // 2. 设置武器缩放（范围可视化）= 当前武器的大小（以1倍基础缩放为基准）
        transform.localScale = Vector3.one * stats[weaponLevel].weaponSize;

        // 3. 设置攻击间隔时间 = 当前武器的攻击间隔属性
        timeBetweenSpawn = stats[weaponLevel].timeBetweenAttacks;

        // 4. 设置武器的持续时间= 当前武器的持续时间属性
        damager.lifeTime = stats[weaponLevel].duration;

        // 5. 重置生成计数器（如攻击冷却计时器）
        spawnCounter = 0f;
    }

    /// <summary>
    /// 生成以玩家为中心的环形火球（融入range属性）
    /// </summary>
    private void SpawnOrbitalFireballs()
    {
        // 前置校验：避免空引用或无效配置
        if (playerTransform == null || weapon == null || stats == null || stats.Count <= weaponLevel)
        {
            Debug.LogWarning("火球生成参数异常，跳过生成");
            return;
        }

        // 缓存当前等级配置（减少重复索引访问）
        WeaponStats currentStat = stats[weaponLevel];
        int fireballCount = Mathf.RoundToInt(currentStat.amount); // 确保数量为整数
        if (fireballCount <= 0) return; // 无火球需生成时直接退出

        // 核心：用当前等级的range作为环形分布半径,需要乘0.33，不然范围太大（替代固定distance）
        float orbitalRadius = currentStat.range * 0.33f;
        float angleStep = 360f / fireballCount; // 每个火球的角度间隔
        Vector3 playerPos = playerTransform.position; // 缓存玩家位置

        // 循环生成火球
        for (int i = 0; i < fireballCount; i++)
        {
            // 计算当前火球的弧度（三角函数需弧度制）
            float currentRadian = angleStep * i * Mathf.Deg2Rad;

            // 计算以玩家为中心、range为半径的环形世界坐标
            Vector3 spawnPos = new Vector3(
                playerPos.x + Mathf.Cos(currentRadian) * orbitalRadius,
                playerPos.y + Mathf.Sin(currentRadian) * orbitalRadius,
                playerPos.z
            );

            // 计算火球旋转角度（与环形角度一致）
            Quaternion spawnRot = Quaternion.Euler(0f, 0f, angleStep * i);

            // 生成火球并激活
            Instantiate(weapon, spawnPos, spawnRot, holder).gameObject.SetActive(true);
        }
    }
}
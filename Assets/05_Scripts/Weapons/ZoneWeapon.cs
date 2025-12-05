using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneWeapon : Weapon
{
    // 武器组件引用
    public WeaponHarm damager;
    [Tooltip("生成的间隔时间（单位：秒）")]
    public float spawnTime = 4f;
    /// <summary>
    /// 生成计时器（私有变量，仅脚本内部使用）
    /// 用于记录间隔时间，判断是否达到生成子弹的时机
    /// </summary>
    private float spawnCounter;

    // Start is called before the first frame update
    void Start()
    {
        SetStats();
    }

    // Update is called once per frame
    void Update()
    {
        //升级的时候增强武器
        if (statsUpdated == true)
        {
            statsUpdated = false; // 重置升级标记
            SetStats(); // 调用属性设置方法，刷新武器当前等级的属性
        }

        // 生成计时器递减（按帧更新，Time.deltaTime保证时间流速稳定）
        // spawnCounter：记录距离下次生成的剩余时间
        spawnCounter -= Time.deltaTime;

        // 当计时器≤0时，触发预制体生成逻辑
        if (spawnCounter <= 0f)
        {
            // 重置生成计时器为配置的间隔时间（spawnTime：两次生成的间隔秒数）
            spawnCounter = spawnTime;

            // 生成伤害预制体（如子弹、技能特效）
            // 参数说明：
            // 1. damager：要生成的伤害预制体（需提前在Inspector赋值）
            // 2. damager.transform.position：生成位置（与预制体原始位置一致）
            // 3. Quaternion.identity：生成旋转（无旋转，保持默认角度）
            // 4. transform：生成的预制体挂载到当前物体下（管理层级）
            // 5. SetActive(true)：确保生成后预制体处于激活状态
            Instantiate(damager, damager.transform.position, Quaternion.identity, transform).gameObject.SetActive(true);
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
        damager.transform.localScale = Vector3.one * stats[weaponLevel].range;

        // 3. 设置攻击间隔时间 = 当前武器的攻击间隔属性
        spawnTime = stats[weaponLevel].timeBetweenAttacks;

        // 4. 设置武器的持续时间= 当前武器的持续时间属性
        damager.lifeTime = stats[weaponLevel].duration;

        // 5. 重置生成计数器（如攻击冷却计时器）
        spawnCounter = 0f;

        // 6.持续伤害的间隔时间（秒）
        damager.timeBetweenDamage = stats[weaponLevel].speed;
    }
}

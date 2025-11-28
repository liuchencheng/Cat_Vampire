using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 武器父类
/// 管理武器的等级和对应等级的属性
/// </summary>
public class Weapon : MonoBehaviour
{
    // 存储武器升级路线的列表
    public List<WeaponStats> stats;

    // 随着等级提升，而升级武器
    [Tooltip("随着等级提升，而升级武器")]
    public int weaponLevel;

    // 隐藏在Inspector面板中，不允许手动编辑
    [HideInInspector]
    public bool statsUpdated;//是否升级的标记

    /// <summary>
    /// 武器跟随玩家升级而变强
    /// </summary>
    public void LevelUp()
    {
        // 检查：当前武器等级是否小于最大可升级等级（避免超出stats列表范围）
        // stats.Count - 1 表示stats列表的最大索引（即武器可达到的最高等级）
        if (weaponLevel < stats.Count - 1)
        {
            weaponLevel++; // 等级提升1级

            statsUpdated = true;//升级过之后改为true
        }
    }
}

/// <summary>
/// 武器属性数据类（可序列化，支持在Inspector面板显示和编辑）
/// 存储单个等级的所有武器属性
/// </summary>
[System.Serializable] // 标记为可序列化，使该类实例能在Unity编辑器中显示和保存
public class WeaponStats
{
    // 武器攻击速度
    [Tooltip("武器攻击速度")]
    public float speed;

    // 武器单次攻击伤害
    [Tooltip("武器单次攻击伤害")]
    public float damage;

    // 武器攻击范围（有效攻击距离）
    [Tooltip("武器攻击范围")]
    public float range;

    // 攻击间隔时间（两次攻击之间的冷却）
    [Tooltip("攻击间隔时间（两次攻击之间的冷却）")]
    public float timeBetweenAttacks;

    // 攻击数量
    [Tooltip("攻击数量")]
    public float amount;

    // 武器效果持续时间（如技能持续时间、灼烧/冰冻效果时长）
    [Tooltip("武器效果持续时间")]
    public float duration;
}
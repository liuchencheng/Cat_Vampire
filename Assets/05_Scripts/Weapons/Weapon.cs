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

    [Tooltip("武器名")]
    public string weaponName;

    // 隐藏在Inspector面板中，不允许手动编辑
    [HideInInspector]
    public bool statsUpdated;//是否升级的标记

    /// <summary>
    /// 升级选项对应的图标Sprite
    /// 用于在UI中显示该升级项的图标（如武器图标、技能图标等）
    /// </summary>
    [Tooltip("升级选项对应的图标")]
    public Sprite icon;

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

        // 判断当前武器等级是否达到满级（stats是该武器的等级属性列表，stats.Count-1是最高等级的索引）
        if (weaponLevel >= stats.Count - 1)
        {
            // 将当前武器（this代表当前Weapon实例）添加到“已满级武器列表”
            PlayerController.Instance.fullyLevelledWeapons.Add(this);
            // 从“玩家已持有武器列表”中移除当前武器（满级后不再参与升级选择）
            PlayerController.Instance.assignedWeapons.Remove(this);
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

    // 武器大小
    [Tooltip("武器大小")]
    public float weaponSize;

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

    /// <summary>
    /// 升级选项的描述文本
    /// 用于在UI中显示该升级项的名称或说明（如“武器等级+1”“增加伤害”等）
    /// </summary>
    [Tooltip("升级选项的描述文本")]
    public string upgradeText;
}
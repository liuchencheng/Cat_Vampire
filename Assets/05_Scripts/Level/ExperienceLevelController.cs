using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 经验等级控制器（单例）
/// 功能：全局管理玩家的经验值，提供增加经验的方法
/// 挂载对象：场景中的全局管理对象（如GameManager）
/// </summary>
public class ExperienceLevelController : MonoBehaviour
{
    // 单例实例（全局唯一，供外部脚本调用）
    public static ExperienceLevelController Instance;

    /// <summary>
    /// 初始化单例（在Start之前执行）
    /// </summary>
    private void Awake()
    {
        // 确保场景中只有一个经验控制器实例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 重复实例直接销毁
        }
    }


    [Tooltip("玩家当前的经验值")]
    public int currentExperience; // 存储玩家当前的经验值

    /// <summary>
    /// 公开的经验拾取物脚本引用
    /// 用途：用于关联/访问经验拾取物（ExpPickup）的脚本组件，后续可通过该变量调用拾取物的功能（如设置经验值、控制移动等）
    /// </summary>
    [Tooltip("关联拾取物")]
    public ExpPickup pickup;

    [Tooltip("存储每级升级所需的经验值列表）")]
    public List<int> expLevels;
    [Tooltip("当前玩家等级（默认从1级开始）")]
    public int currentLevel = 1;
    [Tooltip("总等级上限，默认50")]
    public int levelCount = 50;

    /// <summary>
    /// 游戏启动时初始化升级所需经验列表
    /// </summary>
    void Start()
    {
        // 循环填充经验列表，直到列表长度达到等级上限（levelCount）
        // 例如：如果levelCount=50，最终列表会有50个元素（对应1→2级到100→101级的所需经验）
        while (expLevels.Count < levelCount)
        {
            // 计算下一级所需经验：以上一级经验值为基础，乘以1.1倍（10%的增长），并向上取整
            // 注意：首次添加时（列表为空），需要确保列表已有至少1个初始值（否则会报索引错误）
            expLevels.Add(Mathf.CeilToInt(expLevels[expLevels.Count - 1] * 1.1f));
        }
    }

    /// <summary>
    /// 增加玩家经验的方法（供外部调用）
    /// </summary>
    /// <param name="amountToGet">要增加的经验数值</param>
    public void GetExp(int amountToGet)
    {
        currentExperience += amountToGet; // 累加经验值

        // 检查：当前经验是否达到"当前等级升级所需的经验值"
        if (currentExperience >= expLevels[currentLevel])
        {
            // 满足升级条件，执行升级逻辑
            LevelUp();
        }

        //更新玩家的经验条
        //当前拥有的经验值、当前等级升级所需的总经验值、当前玩家等级
        UIController.Instance.UpdateExperience(currentExperience, expLevels[currentLevel], currentLevel);
    }


    /// <summary>
    /// 敌人死后，生成经验拾取物并设置其经验值的方法
    /// </summary>
    /// <param name="position">拾取物的生成位置（世界空间坐标）</param>
    /// <param name="expValue">该拾取物包含的经验数值</param>
    public void SpawnExp(Vector3 position, int expValue)
    {
        // 1. 实例化经验拾取物预制体（pickup是提前赋值的经验拾取物预制体）
        // 2. 直接访问实例化后的拾取物的expValue字段，将其设置为传入的经验值
        // 3. Quaternion.identity表示生成时无旋转（使用预制体默认姿态）
        Instantiate(pickup, position, Quaternion.identity).expValue = expValue;
    }

    /// <summary>
    /// 玩家升级的核心逻辑
    /// </summary>
    void LevelUp()
    {
        // 扣除当前等级升级所需的经验（多余的经验会保留，用于下次升级）
        currentExperience -= expLevels[currentLevel];

        // 等级提升1级
        currentLevel++;

        // 边界检查：如果当前等级超过经验列表的最大索引（达到满级）
        // 则强制将等级设为经验列表的最大索引（避免访问expLevels时越界）
        if (currentLevel >= expLevels.Count)
        {
            currentLevel = expLevels.Count - 1; // 锁定在满级
        }

        //调用武器升级方法
        PlayerController.Instance.activeWeapon.LevelUp();
    }
}

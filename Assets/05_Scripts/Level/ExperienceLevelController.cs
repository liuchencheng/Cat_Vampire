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

    /// <summary>
    /// 增加玩家经验的方法（供外部调用）
    /// </summary>
    /// <param name="amountToGet">要增加的经验数值</param>
    public void GetExp(int amountToGet)
    {
        currentExperience += amountToGet; // 累加经验值
        // （可扩展：此处可添加“经验满后升级”的逻辑）
    }


    /// <summary>
    /// 生成经验拾取物并设置其经验值的方法
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器环绕旋转发射脚本
/// 功能：控制指定的载体（Holder）沿Z轴持续旋转，带动武器（预制体）一起旋转
/// 核心逻辑：通过计时器间隔生成子弹（预制体 Orbital_Bolt）
/// </summary>
public class SpinWeapon : MonoBehaviour
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

    void Start()
    {

    }

    void Update()
    {
        // 1. 获取holder当前的Z轴旋转角度（基于世界坐标系）
        // 2. 计算当前帧应增加的旋转角度（rotateSpeed：度/秒 × Time.deltaTime：当前帧耗时）
        //    确保旋转速度稳定，每秒旋转角度固定为rotateSpeed
        // 3. 通过Quaternion.Euler将X/Y/Z轴旋转角度转为四元数（避免万向锁问题）
        //    其中X/Y轴保持0度不旋转，仅修改Z轴角度
        // 4. 将计算后的旋转角度赋值给holder，实现实际旋转
        holder.rotation = Quaternion.Euler(
            0f,                          // X轴旋转角度：固定0度，不旋转
            0f,                          // Y轴旋转角度：固定0度，不旋转
            holder.rotation.eulerAngles.z + (rotateSpeed * Time.deltaTime)  // Z轴累加旋转角度
        );

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
            Instantiate(weapon, weapon.position, weapon.rotation, holder).gameObject.SetActive(true);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抛射物（如子弹、炮弹）控制脚本
/// 功能：管理抛射物的移动、碰撞后销毁等核心逻辑
/// 挂载对象：抛射物预制体（如子弹、技能飞弹）
/// </summary>
public class Projectile : MonoBehaviour
{
    // 抛射物的移动速度
    [Tooltip("抛射物的移动速度")]
    public float moveSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // 核心移动逻辑：让抛射物沿自身的“up方向”（预制体的“前方向”）匀速移动
        // - transform.up：抛射物局部坐标系的向上方向（由预制体的朝向决定）
        // - moveSpeed * Time.deltaTime：保证移动速度不受帧率影响，每秒移动距离固定为moveSpeed
        transform.position += -transform.right * moveSpeed * Time.deltaTime;
    }
}

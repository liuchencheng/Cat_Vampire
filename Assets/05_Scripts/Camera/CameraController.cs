using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 相机跟随控制器
/// 功能：让相机始终跟随玩家的2D位置（保持Z轴不变，避免画面缩放/偏移）
/// 适用场景：2D游戏（如吸血鬼幸存者类、平台跳跃类）
/// </summary>
public class CameraController : MonoBehaviour
{
    // 跟随的目标对象（这里特指玩家的Transform组件）
    private Transform target;

    /// <summary>
    /// 游戏启动时执行一次
    /// 作用：获取玩家的Transform引用，作为相机跟随的目标
    /// </summary>
    void Start()
    {
        // 查找场景中挂载了PlayerController脚本的对象，获取其Transform组件
        // 前提：场景中只有一个PlayerController（玩家唯一）
        //target = FindObjectOfType<PlayerController>().transform;
        target = PlayerHealthController.Instance.transform;

        // 容错处理：如果没找到玩家，打印错误日志（避免空引用崩溃）
        if (target == null)
        {
            Debug.LogError("CameraController：未在场景中找到PlayerController组件！相机无法跟随");
        }
    }

    /// <summary>
    /// 每帧更新后执行（LateUpdate确保在玩家移动后再更新相机，避免画面抖动）
    /// 作用：更新相机位置，使其与玩家位置同步（Z轴保持初始值，不改变景深）
    /// </summary>
    void LateUpdate()
    {
        // 只有找到目标（玩家）时才执行跟随逻辑
        if (target != null)
        {
            // 相机新位置 = 玩家的X、Y坐标 + 相机原有的Z坐标（保持相机在2D层的深度）
            transform.position = new Vector3(
                target.position.x,    // 同步玩家X轴位置
                target.position.y,    // 同步玩家Y轴位置
                transform.position.z  // 保留相机初始Z轴（关键：避免相机和玩家在同一Z层导致画面异常）
            );
        }
    }
}

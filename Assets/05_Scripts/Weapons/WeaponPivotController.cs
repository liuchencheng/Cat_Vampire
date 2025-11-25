using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 控制武器枢轴跟随玩家移动
/// 挂载对象：Player_Weapons (新的空父物体)
/// </summary>
public class WeaponPivotController : MonoBehaviour
{
    private Transform playerTransform; // 玩家的 Transform

    void Start()
    {
        // 查找玩家对象，假设玩家对象是名为 "Player" 的对象
        // 或者使用 PlayerHealthController.Instance 来获取玩家 Transform
        playerTransform = PlayerHealthController.Instance.transform;
    }

    void LateUpdate()
    {
        if (playerTransform != null)
        {
            // 核心逻辑：WeaponPivot 只跟随 Player 的位置，但不会继承 Player 的缩放。
            transform.position = playerTransform.position;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家运动脚本
public class PlayerController : MonoBehaviour
{
    public Animator player;
    [Header("玩家移动速度")]
    public float moveSpeed = 0.5f; // 公开的速度变量，方便在 Inspector 中调整
    [Header("死亡状态标记，勾是死亡")]
    public bool isDead = false; // 死亡状态标记（和 PlayerHealthController 同步）

    [Header("经验水晶靠近多少距离会被吸走")]
    public float pickupRange = 0.5f;

    void Update()
    {
        // 核心：如果已经死亡，不再执行任何移动和动画逻辑
        if (isDead) return;

        Run();
    }


    private void Run()
    {
        // 获取水平输入值：-1 (左) 到 1 (右)
        float moveValue = Input.GetAxis("Horizontal");
        float moveValue2 = Input.GetAxisRaw("Vertical");

        // --- 角色翻转（可选但推荐）---
        if (moveValue > 0)
        {
            // 面朝右
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (moveValue < 0)
        {
            // 面朝左
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // 动画切换：如果 moveValue 不为 0 就播放 Run，否则播放 Idle
        // 优化：使用逻辑或 (||) 判断，只要任一输入不为零，就播放 Run
        bool isMoving = (moveValue != 0 || moveValue2 != 0);

        // 只调用一次 Play()
        player.Play(isMoving ? "Run" : "Idle");

        // 1. 计算新的水平速度。垂直速度 (Y) 保持不变，以保留重力或跳跃效果。
        Vector3 movement = new Vector3(0f,0f,0f);

            // 2. 将计算出的速度应用到 Rigidbody2D
        movement.x = moveValue;
        movement.y = moveValue2;

        movement.Normalize();

        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    // 提供给 PlayerHealthController 调用的“死亡触发方法”
    public void SetDead()
    {
        isDead = true;
    }
}

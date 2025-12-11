using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 拾取硬币脚本
public class CoinPickup : MonoBehaviour
{
    [Tooltip("硬币价值")]
    public int coinAmount; // 硬币价值

    [Tooltip("是否处于向玩家移动的状态（无需手动修改，脚本自动控制）")]
    private bool movingToPlayer; // 标记是否开始向玩家移动

    [Tooltip("向玩家移动的基础速度")]
    public float moveSpeed; // 移动基础速度

    [Tooltip("距离检测的间隔时间（单位：秒，值越小检测越频繁）")]
    public float timeBetweenChecks = .2f; // 检测玩家距离的间隔
    private float checkCounter; // 距离检测的倒计时器

    [Tooltip("玩家控制器的引用（无需手动赋值，脚本自动获取）")]
    private PlayerController player; // 缓存玩家控制器的引用


    void Start()
    {
        // 从PlayerHealthController的单例中，获取玩家的PlayerController组件
        // 前提：PlayerHealthController和PlayerController挂载在同一个玩家对象上
        player = PlayerController.Instance;
        // 初始化检测倒计时器
        checkCounter = timeBetweenChecks;
    }


    void Update()
    {
        // 若处于“向玩家移动”状态 → 持续向玩家位置移动
        if (movingToPlayer == true)
        {
            // Vector3.MoveTowards：匀速向目标位置移动（避免穿模）
            transform.position = Vector3.MoveTowards(
                transform.position,    // 当前拾取物位置
                player.transform.position, // 玩家位置
                moveSpeed * Time.deltaTime // 每帧移动的距离（与帧率无关）
            );
        }
        // 未处于移动状态 → 定时检测玩家距离
        else
        {
            // 检测倒计时递减
            checkCounter -= Time.deltaTime;
            // 倒计时结束 → 执行距离检测
            if (checkCounter <= 0)
            {
                checkCounter = timeBetweenChecks; // 重置倒计时

                // 若玩家进入“拾取范围” → 开始向玩家移动
                if (Vector3.Distance(transform.position, player.transform.position) < player.pickupRange)
                {
                    movingToPlayer = true; // 标记为移动状态
                    moveSpeed += player.moveSpeed; // 移动速度叠加玩家的移动速度（让拾取物更容易追上玩家）
                }
            }
        }
    }


    /// <summary>
    /// 触发碰撞检测（当拾取物接触玩家时触发）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 若碰撞对象是玩家（标签为“Player”）
        if (collision.tag == "Player")
        {
            // 调用硬币控制器的“增加硬币”方法
            CoinController.Instance.AddCoins(coinAmount);
            Destroy(gameObject); // 销毁当前拾取物
        }
    }
}

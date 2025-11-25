using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人呼吸缩放动画脚本
/// 功能：让敌人的Sprite在最小/最大尺寸之间平滑切换，实现"呼吸"效果
/// 适用场景：2D敌人（如吸血鬼幸存者类游戏中的怪物，增加视觉动态感）
/// </summary>
public class EnemyAnimation : MonoBehaviour
{
    [Header("组件")]
    public Transform sprite; // 敌人的Sprite根节点（需在Inspector拖入，控制缩放的目标）
    [Header("缩放动画速度,值越大越快")]
    public float speed = 0.5f; // 缩放动画速度（值越大，呼吸越快，可在Inspector调整）
    [Header("最小缩放比例")]
    public float minSize = 0.5f; // 最小缩放比例（如0.8=原始尺寸的80%）
    [Header("最大缩放比例")]
    public float maxSize = 0.6f; // 最大缩放比例（如1.2=原始尺寸的120%）

    [Header("内部状态（无需手动赋值）")]
    private float activeSize; // 当前目标缩放尺寸（切换于minSize和maxSize之间）

    /// <summary>
    /// 游戏启动时执行一次
    /// 作用：初始化动画参数，设置初始目标缩放尺寸和随机速度
    /// </summary>
    void Start()
    {
        // 初始目标尺寸设为最大尺寸，动画从"放大到最大"开始
        activeSize = maxSize;

        // 随机调整缩放速度（范围：原速度的75% ~ 125%），让多个敌人的呼吸节奏不同，增加多样性
        speed = speed * Random.Range(0.75f, 1.25f);

        // 容错处理：若未赋值sprite节点，打印错误日志
        if (sprite == null)
        {
            Debug.LogError("EnemyAnimation：请给sprite字段赋值敌人的Sprite根节点！");
        }
    }

    /// <summary>
    /// 每帧更新时执行
    /// 作用：驱动缩放动画，平滑切换目标尺寸，实现呼吸效果
    /// </summary>
    void Update()
    {
        // 若未赋值sprite，直接返回（避免空引用崩溃）
        if (sprite == null) return;

        // 平滑移动到目标缩放尺寸：Vector3.MoveTowards(当前值, 目标值, 每帧最大移动量)
        // speed * Time.deltaTime：确保不同帧率下动画速度一致（时间增量适配）
        sprite.localScale = Vector3.MoveTowards(
            sprite.localScale,          // 当前缩放尺寸
            Vector3.one * activeSize,   // 目标缩放尺寸（x/y/z轴统一缩放，保持物体比例）
            speed * Time.deltaTime      // 每帧最大缩放变化量（控制平滑度）
        );

        // 当当前缩放尺寸达到目标尺寸时，切换下一个目标尺寸（最大→最小，最小→最大）
        // 注意：用Mathf.Approximately判断浮点值相等（避免因浮点精度问题导致判断失效）
        if (Mathf.Approximately(sprite.localScale.x, activeSize))
        {
            // 切换目标尺寸：当前是最大→切最小，当前是最小→切最大
            activeSize = (activeSize == maxSize) ? minSize : maxSize;
        }
    }
}
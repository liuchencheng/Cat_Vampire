using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    // 1. 公开静态字段
    public static PlayerHealthController Instance;

    // 2. 使用 Awake() 初始化，确保在所有 Start() 之前完成
    private void Awake()
    {
        // 容错机制：确保单例的唯一性
        if (Instance == null)
        {
            // 如果没有，设置当前对象为实例
            Instance = this;
        }
        else
        {
            // 如果已经存在其他实例，销毁当前对象
            Destroy(gameObject);
        }
    }

    [Header("组件")]
    public Animator player; // 玩家的Animator组件（需在Inspector拖入）
    [Header("当前血量")]
    public float currentHealth;//当前血量
    [Header("最大血量")]
    public float maxHealth = 100f; // 默认最大血量，可直接在Inspector调整
    private bool isDead = false; // 死亡标记：避免重复触发死亡逻辑

    // 关联PlayerController脚本（用于同步死亡状态、禁用移动）
    private PlayerController playerController;

    //血条滑块
    [Header("血条滑块")]
    public Slider healthSlider;

    [Header("血条的 Canvas 父物体，解决翻转")]
    // 既然您用的是 Canvas，最好用 RectTransform
    public RectTransform healthCanvas; // 请在 Inspector 中将 Health_Canvas 拖入此字段！
    // 存储 Canvas 正常的、未被拉伸的本地缩放值，防止血条被拉伸的特别长
    private Vector3 initialCanvasScale;

    // 玩家死亡特效的预制体
    [Header("玩家死亡特效的预制体")]
    public GameObject deathEffect;

    void Start()
    {
        // 初始化玩家最大生命值为“生命值属性的0级数值”
        maxHealth = PlayerStatController.Instance.health[0].value;

        currentHealth = maxHealth; // 开局血量拉满
        // 获取当前对象上的PlayerController组件（前提：两个脚本挂在同一个玩家对象上）
        playerController = GetComponent<PlayerController>();

        // 容错：如果没找到PlayerController，打印日志（避免后续空引用）
        if (playerController == null)
        {
            Debug.LogError("PlayerHealthController：未找到PlayerController脚本！请确保两个脚本挂在同一个玩家对象上");
        }

        // 初始化血条的最大值和当前值
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        // 【新增】在 Start() 中获取并存储血条 Canvas 的初始缩放
        if (healthCanvas != null)
        {
            // 在游戏启动时，假设此时 Canvas 的 localScale 是我们想要的“正常”大小。
            // 无论您设置的是 (1, 1, 1) 还是 (0.01, 0.01, 0.01)，我们都记录下来。
            initialCanvasScale = healthCanvas.localScale;
        }
    }

    void LateUpdate()
    {
        if (isDead) return;

        if (healthCanvas != null)
        {
            // 玩家的水平缩放值 (1f 或 -1f)
            float playerScaleX = transform.localScale.x;

            // 【核心修改】我们从 Canvas 正常的缩放值开始
            Vector3 canvasScale = initialCanvasScale;

            // 如果玩家是面向左 (playerScaleX < 0)，
            // 我们需要将 Canvas 的 X 缩放也设置为负值，以抵消父级的翻转。
            if (playerScaleX < 0)
            {
                // 如果 initialCanvasScale.x 是正数（例如 0.01），
                // 那么这里就变成负数（例如 -0.01）。
                canvasScale.x = -initialCanvasScale.x;
            }
            // 否则，保持为 initialCanvasScale.x（正数）。

            // 应用这个新的、保持大小但可能符号相反的缩放值
            healthCanvas.localScale = canvasScale;
        }
    }

    void Update()
    {
        
    }


    public void TakeDamage(float damageToTake)
    {
        // 已经死亡则不再扣血
        if (isDead) return;

        // 扣血（确保血量不低于0）
        currentHealth = Mathf.Max(0, currentHealth - damageToTake);

        //更新血条
        healthSlider.value = currentHealth;

        // 血量归0触发死亡
        if (currentHealth <= 0)
        {
            Die();

            //倒计时结束
            LevelManager.Instance.EndLevel();

            // 在指定位置（当前物体的位置）生成死亡特效
            // 参数说明：
            // - deathEffect：要生成的特效预制体
            // - transform.position：特效生成的位置（与当前物体位置一致）
            // - transform.rotation：特效生成的旋转角度（与当前物体旋转一致）
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
    }

    // 死亡核心逻辑（单独封装，方便扩展）
    private void Die()
    {
        isDead = true; // 标记为死亡

        // 1. 播放Die动画（容错：确保Animator已赋值）
        if (player != null)
        {
            player.Play("Die");
        }
        else
        {
            Debug.LogError("PlayerHealthController：请给player字段拖入玩家的Animator组件！");
        }

        // 2. 通知PlayerController停止移动和动画控制（关键：避免覆盖Die动画）
        if (playerController != null)
        {
            playerController.SetDead(); // 调用PlayerController的死亡方法
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家运动脚本
public class PlayerController : MonoBehaviour
{
    // 单例实例（全局唯一，供外部脚本调用）
    public static PlayerController Instance;

    /// 初始化单例（在Start之前执行）
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

    public Animator player;
    [Header("玩家移动速度")]
    public float moveSpeed = 0.5f; // 公开的速度变量，方便在 Inspector 中调整
    [Header("死亡状态标记，勾是死亡")]
    public bool isDead = false; // 死亡状态标记（和 PlayerHealthController 同步）

    [Header("经验水晶靠近多少距离会被吸走")]
    public float pickupRange = 0.5f;

    /// <summary>
    /// 激活的武器
    /// 用于指向玩家当前正在使用的武器实例，方便访问其属性和调用方法
    /// </summary>
    //[Header("激活的武器")]
    //public Weapon activeWeapon;

    // 未分配的武器列表
    [Header("未分配的武器列表")]
    public List<Weapon> unassignedWeapons;
    // 已分配的武器列表
    [Header("已分配的武器列表")]
    public List<Weapon> assignedWeapons;

    // 玩家可持有的最大武器数量
    [Header("玩家可持有的最大武器数量")]
    public int maxWeapons = 3;

    // [HideInInspector]：让该字段不在Unity Inspector面板中显示（避免手动修改）
    // 存储所有已经升到满级的武器
    // 初始化一个空的Weapon列表，避免后续为null
    [HideInInspector]
    public List<Weapon> fullyLevelledWeapons = new List<Weapon>();

    void Start()
    {
        // 随机从unassignedWeapons中选一个武器添加
        AddWeapon(Random.Range(0, unassignedWeapons.Count));
    }

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

    //添加武器
    public void AddWeapon(int weaponNumber)
    {
        // 先判断索引是否在unassignedWeapons的有效范围内
        if (weaponNumber < unassignedWeapons.Count)
        {
            // 将该武器添加到已分配列表
            assignedWeapons.Add(unassignedWeapons[weaponNumber]);
            // 激活该武器对应的游戏对象
            unassignedWeapons[weaponNumber].gameObject.SetActive(true);
            // 从未分配列表中移除该武器
            unassignedWeapons.RemoveAt(weaponNumber);
        }
    }

    // 添加武器的方法，参数为要添加的武器对象
    public void AddWeapon(Weapon weaponToAdd)
    {
        // 激活该武器对应的游戏对象
        weaponToAdd.gameObject.SetActive(true);
        // 将武器添加到已分配列表
        assignedWeapons.Add(weaponToAdd);
        // 从未分配列表中移除该武器
        unassignedWeapons.Remove(weaponToAdd);
    }
}

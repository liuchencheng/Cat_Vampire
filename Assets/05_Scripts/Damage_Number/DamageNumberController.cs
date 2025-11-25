using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 伤害数字生成器脚本
/// 功能：控制伤害数字预制体的生成、位置指定，配合DamageNumber脚本显示伤害
/// 挂载对象：伤害数字管理物体（如Damage Number Controller）
/// </summary>
public class DamageNumberController : MonoBehaviour
{
    // 1. 公开静态字段
    public static DamageNumberController Instance;

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

    [Tooltip("要生成的伤害数字预制体")]
    public DamageNumber numberToSpawn;
    [Tooltip("伤害数字的父画布")]
    public Transform numberCanvas;

    [Tooltip("对象池初始缓存数量（高频调用设10-20，根据游戏需求调整）")]
    public int poolSize = 15;
    [Tooltip("画布是否为Screen Space - Overlay模式（勾选后自动转屏幕坐标，避免偏移）")]
    public bool isOverlayCanvas = false;

    // 核心：对象池（Queue实现O(1)入队/出队，适配高频调用）
    private Queue<DamageNumber> damageObjectPool = new Queue<DamageNumber>();
    // 错误日志标记（避免频繁调用时打印大量重复错误）
    private bool hasLoggedError = false;

    // 3. 游戏启动时初始化对象池（只执行一次）
    void Start()
    {
        InitializeObjectPool(); // 初始化对象池
        WarmupFontAtlas();     // 预热字体图集（保留原有预热逻辑）
    }

    /// <summary>
    /// 初始化对象池：提前创建实例缓存，避免首次调用时创建
    /// </summary>
    private void InitializeObjectPool()
    {
        // 容错：预制体或画布未赋值，直接返回
        if (numberToSpawn == null || numberCanvas == null)
        {
            LogInitError();
            return;
        }

        // 批量创建实例，加入对象池（隐藏备用）
        for (int i = 0; i < poolSize; i++)
        {
            DamageNumber tempInstance = Instantiate(numberToSpawn, numberCanvas);
            tempInstance.gameObject.SetActive(false); // 隐藏备用
            ResetInstanceState(tempInstance);        // 重置状态，避免残留
            damageObjectPool.Enqueue(tempInstance);  // 加入对象池
        }
    }

    /// <summary>
    /// 预热字体图集（触发TMP首次渲染初始化，避免首次显示卡顿）
    /// </summary>
    private void WarmupFontAtlas()
    {
        if (damageObjectPool.Count > 0)
        {
            DamageNumber warmupInstance = damageObjectPool.Peek();
            warmupInstance.Setup(0); // 触发字体渲染
        }
    }

    /// <summary>
    /// 生成伤害数字（对象池复用版，调用方式和原来完全一致）
    /// </summary>
    /// <param name="damageAmount">原始伤害值（浮点型，自动转整数）</param>
    /// <param name="location">世界空间中的生成位置</param>
    public void SpawnDamage(float damageAmount, Vector3 location)
    {
        // 快速容错：未初始化成功直接返回
        if (numberToSpawn == null || numberCanvas == null)
        {
            LogInitError();
            return;
        }

        // 1. 预计算伤害值（提前四舍五入，减少实例化后操作）
        int roundedDamage = Mathf.RoundToInt(damageAmount);

        // 2. 从对象池取实例（无则创建兜底实例）
        DamageNumber targetInstance = GetInstanceFromPool();

        // 3. 重置实例状态（关键！避免复用旧数据）
        ResetInstanceState(targetInstance);

        // 4. 适配画布模式：Overlay模式下转换世界坐标→屏幕坐标
        targetInstance.transform.position = isOverlayCanvas && Camera.main != null
            ? Camera.main.WorldToScreenPoint(location)
            : location;

        // 5. 设置伤害数值并激活
        targetInstance.Setup(roundedDamage);
        targetInstance.gameObject.SetActive(true);
    }

    /// <summary>
    /// 从对象池获取实例（无则创建新实例兜底）
    /// </summary>
    private DamageNumber GetInstanceFromPool()
    {
        if (damageObjectPool.Count > 0)
        {
            return damageObjectPool.Dequeue(); // 池中有实例，直接取出
        }
        else
        {
            return Instantiate(numberToSpawn, numberCanvas); // 池为空，创建新实例
        }
    }

    /// <summary>
    /// 重置实例状态（复用前必调，确保无残留旧数据）
    /// </summary>
    private void ResetInstanceState(DamageNumber instance)
    {
        if (instance == null) return;

        // 重置Transform：父物体、旋转、缩放
        instance.transform.SetParent(numberCanvas);
        instance.transform.localRotation = Quaternion.identity; // 无旋转
        instance.transform.localScale = Vector3.one;             // 正常缩放

        // 重置TMP文本：清空旧文本、恢复透明度
        if (instance.damageText != null)
        {
            instance.damageText.text = "";
            instance.damageText.alpha = 1f; // 避免半透明残留
        }

        // 重置生命周期计时器（确保每次显示时长一致）
        instance.lifeCounter = instance.lifetime;
    }

    /// <summary>
    /// 回收实例到对象池（由DamageNumber脚本调用，替代Destroy）
    /// </summary>
    public void RecycleInstance(DamageNumber instance)
    {
        if (instance == null) return;
        instance.gameObject.SetActive(false); // 隐藏实例
        damageObjectPool.Enqueue(instance);   // 加入对象池，供下次复用
    }

    /// <summary>
    /// 打印初始化错误（仅打印一次）
    /// </summary>
    private void LogInitError()
    {
        if (!hasLoggedError)
        {
            Debug.LogError("DamageNumberController：请给【numberToSpawn（预制体）】和【numberCanvas（画布）】赋值！");
            hasLoggedError = true;
        }
    }
}

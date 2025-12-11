using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//设置玩家各属性的最大升级
public class PlayerStatController : MonoBehaviour
{
    // 单例实例（全局唯一，供外部脚本调用）
    public static PlayerStatController Instance;

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

    // 玩家的各类属性列表（每个属性是可升级的PlayerStatValue类型）：
    // moveSpeed：移动速度属性
    // health：生命值属性
    // pickupRange：拾取范围属性
    // maxWeapons：最大武器数量属性
    [Tooltip("移动速度属性")]
    public List<PlayerStatValue> moveSpeed;
    [Tooltip("生命值属性")]
    public List<PlayerStatValue> health;
    [Tooltip("拾取范围属性")]
    public List<PlayerStatValue> pickupRange;
    [Tooltip("最大武器数量属性")]
    public List<PlayerStatValue> maxWeapons;

    // 各类属性的“最大等级数”（配置要生成多少级的属性）
    [Header("移动速度属性的“最大等级数”")]
    public int moveSpeedLevelCount;
    [Header("生命值属性的“最大等级数”")]
    public int healthLevelCount;
    [Header("拾取范围属性的“最大等级数”")]
    public int pickupRangeLevelCount;

    // 各类属性的“当前等级”（记录玩家当前升级到哪一级）
    [Header("各类属性的“当前等级”（记录玩家当前升级到哪一级）")]
    public int moveSpeedLevel, healthLevel, pickupRangeLevel, maxWeaponsLevel;

    void Start()
    {
        // 使用通用方法初始化所有属性，避免重复代码
        GenerateStatLevels(moveSpeed, moveSpeedLevelCount, "移动速度");
        GenerateStatLevels(health, healthLevelCount, "生命值");
        GenerateStatLevels(pickupRange, pickupRangeLevelCount, "拾取范围");
    }

    /// <summary>
    /// 通用属性生成方法
    /// </summary>
    /// <param name="statsList">要操作的属性列表</param>
    /// <param name="targetCount">目标生成的总等级数</param>
    /// <param name="statName">属性名称（用于报错提示）</param>
    private void GenerateStatLevels(List<PlayerStatValue> statsList, int targetCount, string statName)
    {
        // 安全检查：防止列表为空或元素不足导致索引越界
        if (statsList == null || statsList.Count < 2)
        {
            Debug.LogWarning($"[{statName}] 列表初始数据不足（至少需要配置前2级）用于自动生成。");
            return;
        }

        // 预先计算增量（避免在循环中重复计算，提高效率）
        // 逻辑遵循原代码：
        // 价格增量 = Index[1]的价格
        // 数值增量 = Index[1]的数值 - Index[0]的数值
        int costIncrement = statsList[1].cost;
        float valueIncrement = statsList[1].value - statsList[0].value;

        // 从当前列表的最后一个元素开始生成，直到达到目标数量
        // 使用 while 循环比 for 循环在这里更直观，因为我们在动态增加 Count
        while (statsList.Count < targetCount)
        {
            // 获取当前最后一级的属性
            PlayerStatValue currentLast = statsList[statsList.Count - 1];

            // 计算新的一级
            int newCost = currentLast.cost + costIncrement;
            float newValue = currentLast.value + valueIncrement;

            // 添加到列表
            statsList.Add(new PlayerStatValue(newCost, newValue));
        }
    }

    void Update()
    {
        // 面板显示时，才更新所有属性升级UI（避免无效更新）
        if (UIController.Instance.levelUpPanel.activeSelf)
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// 同步更新所有属性升级面板的显示内容（消耗、当前值、下一级值）
    /// </summary>
    public void UpdateDisplay()
    {
        // 控制“移动速度”升级面板的显示逻辑
        if (moveSpeedLevel < moveSpeed.Count - 1)
        {
            // 未达最大等级：调用UpdateDisplay更新“消耗+属性值变化”的UI
            UIController.Instance.moveSpeedUpgradeDisplay.UpdateDisplay(
                moveSpeed[moveSpeedLevel + 1].cost,   // 下一级消耗
                moveSpeed[moveSpeedLevel].value,      // 当前等级值
                moveSpeed[moveSpeedLevel + 1].value   // 下一级等级值
            );
        }
        else
        {
            // 已达最大等级：调用ShowMaxLevel显示“满级”UI
            UIController.Instance.moveSpeedUpgradeDisplay.ShowMaxLevel();
        }


        // 控制“生命值”升级面板的显示逻辑
        if (healthLevel < health.Count - 1)
        {
            UIController.Instance.healthUpgradeDisplay.UpdateDisplay(
                health[healthLevel + 1].cost,
                health[healthLevel].value,
                health[healthLevel + 1].value
            );
        }
        else
        {
            UIController.Instance.healthUpgradeDisplay.ShowMaxLevel();
        }


        // 控制“拾取范围”升级面板的显示逻辑
        if (pickupRangeLevel < pickupRange.Count - 1)
        {
            UIController.Instance.pickupRangeUpgradeDisplay.UpdateDisplay(
                pickupRange[pickupRangeLevel + 1].cost,
                pickupRange[pickupRangeLevel].value,
                pickupRange[pickupRangeLevel + 1].value
            );
        }
        else
        {
            UIController.Instance.pickupRangeUpgradeDisplay.ShowMaxLevel();
        }


        // 控制“最大武器数”升级面板的显示逻辑
        if (maxWeaponsLevel < maxWeapons.Count - 1)
        {
            UIController.Instance.maxWeaponsUpgradeDisplay.UpdateDisplay(
                maxWeapons[maxWeaponsLevel + 1].cost,
                maxWeapons[maxWeaponsLevel].value,
                maxWeapons[maxWeaponsLevel + 1].value
            );
        }
        else
        {
            UIController.Instance.maxWeaponsUpgradeDisplay.ShowMaxLevel();
        }
    }

    /// <summary>
    /// 购买（升级）移动速度的方法
    /// </summary>
    public void PurchaseMoveSpeed()
    {
        moveSpeedLevel++; // 移动速度等级+1
                          // 消耗对应等级的金币（扣除当前等级的升级成本）
        CoinController.Instance.SpendCoins(moveSpeed[moveSpeedLevel].cost);
        UpdateDisplay(); // 更新升级面板的UI显示

        // 将玩家控制器的移动速度同步为当前等级的数值
        PlayerController.Instance.moveSpeed = moveSpeed[moveSpeedLevel].value;
    }


    /// <summary>
    /// 购买（升级）生命值的方法
    /// </summary>
    public void PurchaseHealth()
    {
        healthLevel++; // 生命值等级+1
        CoinController.Instance.SpendCoins(health[healthLevel].cost); // 消耗对应等级金币
        UpdateDisplay(); // 更新UI

        // 同步玩家最大生命值为当前等级数值
        PlayerHealthController.Instance.maxHealth = health[healthLevel].value;
        // 当前生命值增加“升级前后的差值”（实现升级后生命值“补满差值”的效果）
        PlayerHealthController.Instance.currentHealth += health[healthLevel].value - health[healthLevel - 1].value;
    }


    /// <summary>
    /// 购买（升级）拾取范围的方法
    /// </summary>
    public void PurchasePickupRange()
    {
        pickupRangeLevel++; // 拾取范围等级+1
        CoinController.Instance.SpendCoins(pickupRange[pickupRangeLevel].cost); // 消耗对应等级金币
        UpdateDisplay(); // 更新UI

        // 将玩家控制器的拾取范围同步为当前等级的数值
        PlayerController.Instance.pickupRange = pickupRange[pickupRangeLevel].value;
    }


    /// <summary>
    /// 购买（升级）最大武器数的方法
    /// </summary>
    public void PurchaseMaxWeapons()
    {
        maxWeaponsLevel++; // 最大武器数等级+1
        CoinController.Instance.SpendCoins(maxWeapons[maxWeaponsLevel].cost); // 消耗对应等级金币
        UpdateDisplay(); // 更新UI

        // 将玩家控制器的最大武器数同步为当前等级数值（取整）
        PlayerController.Instance.maxWeapons = Mathf.RoundToInt(maxWeapons[maxWeaponsLevel].value);
    }
}

// 可序列化的玩家属性值类（用于在Inspector面板中配置属性的“升级消耗”和“属性值”）
[System.Serializable]
public class PlayerStatValue
{
    // 升级该属性所需硬币价值（比如金币数量）
    [Tooltip("升级该属性所需硬币价值")]
    public int cost;

    // 该属性对应的具体升级后数值（比如移动速度的数值、生命值的数值）
    [Tooltip("该属性对应的具体升级后数值")]
    public float value;

    // 构造函数：创建属性实例时直接赋值消耗和数值
    public PlayerStatValue(int newCost, float newValue)
    {
        cost = newCost;
        value = newValue;
    }
}
using UnityEngine;

// 投掷类武器控制脚本
public class WeaponThrower : Weapon
{
    // 武器组件
    [Tooltip("武器组件")]
    public WeaponHarm damager;

    // 投掷计时器：控制两次投掷攻击的间隔时间
    private float throwCounter;


    void Start()
    {
        SetStats();
    }


    // 根据当前武器等级，更新伤害器属性与投掷计时器
    void SetStats()
    {
        // 给伤害器赋值当前等级对应的伤害值
        damager.damageAmount = stats[weaponLevel].damage;
        // 给伤害器赋值当前等级对应的存在时间（生成后多久销毁）
        damager.lifeTime = stats[weaponLevel].duration;
        // 按当前等级的大小属性，缩放武器的尺寸
        damager.transform.localScale = Vector3.one * stats[weaponLevel].weaponSize;

        // 重置投掷计时器（避免游戏启动时立即触发投掷）
        throwCounter = 0f;
    }


    void Update()
    {
        // 若武器属性更新标记为true，重新加载当前等级的属性
        if (statsUpdated == true)
        {
            statsUpdated = false; // 重置更新标记
            SetStats(); // 重新应用新等级的属性
        }

        // 投掷计时器按帧递减（Time.deltaTime保证时间流速与帧率无关）
        throwCounter -= Time.deltaTime;

        // 当计时器归零时，触发投掷攻击逻辑
        if (throwCounter <= 0)
        {
            // 重置计时器为当前等级对应的攻击间隔
            throwCounter = stats[weaponLevel].timeBetweenAttacks;

            // 按当前等级的“单次投掷数量”，循环生成多个伤害器实例
            for (int i = 0; i < stats[weaponLevel].amount; i++)
            {
                // 实例化投掷物：
                // 参数1：要生成的伤害器预制体
                // 参数2：生成位置（使用预制体自身的位置）
                // 参数3：生成旋转（使用预制体自身的旋转）
                // SetActive(true)：确保生成后投掷物立即激活生效
                Instantiate(damager, damager.transform.position, damager.transform.rotation)
                    .gameObject.SetActive(true);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 作用：近战攻击武器的控制类
public class CloseAttackWeapon : Weapon
{
    // 武器组件
    [Tooltip("武器组件")]
    public WeaponHarm damager;

    // 私有变量：
    // attackCounter：攻击计时器（控制近战攻击的间隔）
    // direction：攻击方向（用于控制近战攻击的朝向）
    private float attackCounter, direction;

    void Start()
    {
        SetStats();
    }


    // 根据当前武器等级，更新伤害器属性与计时器
    void SetStats()
    {
        // 给伤害器赋值当前等级的伤害值
        damager.damageAmount = stats[weaponLevel].damage;
        // 给伤害器赋值当前等级的持续时间（存在时间）
        damager.lifeTime = stats[weaponLevel].duration;
        // 按当前等级的武器大小，缩放武器的尺寸
        damager.transform.localScale = Vector3.one * stats[weaponLevel].weaponSize;

        // 重置攻击计时器（避免初始状态立即攻击）
        attackCounter = 0f;
    }


    // 每帧更新逻辑：处理攻击间隔、输入方向、攻击生成
    void Update()
    {
        // 若属性更新标记为true，重新加载武器属性
        if (statsUpdated == true)
        {
            statsUpdated = false;
            SetStats();
        }

        // 攻击计时器按帧递减（Time.deltaTime保证时间流速稳定）
        attackCounter -= Time.deltaTime;

        // 计时器归零时，触发近战攻击逻辑
        if (attackCounter <= 0)
        {
            // 重置计时器为当前等级的攻击间隔
            attackCounter = stats[weaponLevel].timeBetweenAttacks;

            // 获取水平输入方向（-1=左，1=右，0=无输入）
            direction = Input.GetAxisRaw("Horizontal");

            // 若存在水平输入，调整伤害器的朝向
            if (direction != 0)
            {
                // 输入为右（direction>0）：伤害器设为默认旋转（0度）
                if (direction > 0)
                {
                    transform.rotation = Quaternion.identity;
                }
                // 输入为左（direction<0）：伤害器旋转180度（反向）
                else
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                }
            }

            // 生成第一个伤害器实例：
            // 参数说明：预制体、生成位置（伤害器自身位置）、生成旋转（伤害器当前旋转）、父物体（当前武器对象）
            // SetActive(true)：确保生成后立即激活
            Instantiate(damager, damager.transform.position, damager.transform.rotation, transform)
                .gameObject.SetActive(true);

            // 按当前等级的“单次攻击数量”，循环生成额外的伤害器（从1开始，避免重复生成第一个）
            for (int i = 1; i < stats[weaponLevel].amount; i++)
            {
                // 计算每个额外伤害器的旋转角度：360度均分（实现“环形攻击”效果）
                float rot = (360f / stats[weaponLevel].amount) * i;

                // 生成额外伤害器：旋转角度在原基础上叠加计算出的rot
                Instantiate(damager, damager.transform.position,
                    Quaternion.Euler(0f, 0f, damager.transform.rotation.eulerAngles.z + rot),
                    transform).gameObject.SetActive(true);
            }
        }
    }
}

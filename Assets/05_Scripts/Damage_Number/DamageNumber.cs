using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 伤害数字显示脚本
/// 功能：控制伤害数字的生命周期（显示后自动销毁），并设置要显示的伤害数值
/// 挂载对象：伤害数字预制体（需包含TMP_Text组件）
/// </summary>
public class DamageNumber : MonoBehaviour
{

    [Tooltip("用于显示伤害的TextMeshPro文本组件")]
    public TMP_Text damageText;
    [Tooltip("数字显示多久后消失（单位：秒）")]
    public float lifetime = 0.5f;
    /// <summary>
    /// 生命周期计时器（私有，仅脚本内使用）
    /// 用于累计时间，判断是否到达销毁时机
    /// </summary>
    internal float lifeCounter;

    [Tooltip("伤害数字的上浮消失速度（单位：单位长度/秒）")]
    public float floatSpeed = 0.5f;


    void Update()
    {
        // 仅当生命周期未结束时，执行上浮和倒计时
        if (lifeCounter > 0)
        {
            // 生命周期倒计时
            lifeCounter -= Time.deltaTime;

            // 伤害数字上浮（与帧率无关）
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // 生命周期结束：回收至对象池（替代Destroy，无GC开销）
            if (lifeCounter <= 0)
            {
                DamageNumberController.Instance.RecycleInstance(this);
            }
        }
    }

    /// <summary>
    /// 设置并显示伤害数字
    /// </summary>
    /// <param name="damageDisplay">要显示的伤害数值</param>
    public void Setup(int damageDisplay)
    {
        // 重置生命周期计时器（确保每次显示都能完整持续lifetime时长）
        lifeCounter = lifetime;
        // 将伤害数值转换为字符串，赋值给Text组件显示
        if (damageText != null)
        {
            damageText.text = damageDisplay.ToString();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// UI面板上玩家的升级属性
public class PlayerStatUpgradeDisplay : MonoBehaviour
{
    [Tooltip("玩家属性文字")]
    public TMP_Text valueText;

    [Tooltip("购买金币文字")]
    public TMP_Text costText;

    [Tooltip("升级按钮")]
    public GameObject upgradeButton;

    /// <summary>
    /// 更新升级面板的显示内容
    /// </summary>
    /// <param name="cost">升级所需的金币消耗</param>
    /// <param name="oldValue">属性当前等级的数值</param>
    /// <param name="newValue">属性升级后（下一级）的数值</param>
    public void UpdateDisplay(int cost, float oldValue, float newValue)
    {
        // 更新“属性值变化”文本：格式为: 旧值 → 新值”
        //  F2：保留2位小数）
        valueText.text = $"数值: {oldValue:F2}→{newValue:F2}";
        // 更新“升级消耗”文本：格式为“Cost: 消耗数值”
        costText.text = $"金币: {cost}";


        // 判断当前金币是否足够支付升级消耗
        if (cost <= CoinController.Instance.currentCoins)
        {
            // 金币足够：显示升级按钮
            upgradeButton.SetActive(true);
        }
        else
        {
            // 金币不足：隐藏升级按钮
            upgradeButton.SetActive(false);
        }
    }

    /// <summary>
    /// 当属性达到最大等级时，更新UI为“满级”状态
    /// </summary>
    public void ShowMaxLevel()
    {
        // 将属性值文本改为 已满级
        valueText.text = "已满级";
        // 将消耗文本也改为 已满级
        costText.text = "已满级";
        // 隐藏升级按钮（满级后无法再升级）
        upgradeButton.SetActive(false);
    }
}

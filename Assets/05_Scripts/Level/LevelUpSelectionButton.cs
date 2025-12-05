using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 升级选择按钮的控制类
/// 用于管理角色升级时选择界面中单个按钮的显示逻辑
/// </summary>
public class LevelUpSelectionButton : MonoBehaviour
{
    // 升级选项的标题
    [Tooltip("升级选项的标题")]
    public TMP_Text nameLevelText;

    // 升级选项的描述
    [Tooltip("升级选项的描述")]
    public TMP_Text upgradeDescText;

    // 升级选项的图片
    [Tooltip("升级选项的图片")]
    public Image weaponIcon;

    // 用于存储当前按钮绑定的武器实例
    private Weapon assignedWeapon;

    /// <summary>
    /// 更新按钮的显示内容
    /// 根据传入的武器数据刷新图标、名称、等级和升级描述
    /// </summary>
    /// <param name="theWeapon">需要显示的武器数据对象</param>
    public void UpdateButtonDisplay(Weapon theWeapon)
    {
        // 判断武器对应的游戏对象是否处于激活状态
        if (theWeapon.gameObject.activeSelf == true)
        {
            // 设置升级选项的标题（格式：武器名称 + " - Lvl " + 武器等级）
            nameLevelText.text = $"{theWeapon.weaponName} - Lvl {theWeapon.weaponLevel}";

            // 设置升级描述文本（从武器当前等级的状态数据中获取）
            upgradeDescText.text = theWeapon.stats[theWeapon.weaponLevel].upgradeText;

            // 设置武器图标
            weaponIcon.sprite = theWeapon.icon;
        }
        else
        {
            // 设置升级选项的标题（格式：武器名称 + " - Lvl " + 武器等级）
            nameLevelText.text = theWeapon.weaponName;

            // 设置升级描述文本（从武器当前等级的状态数据中获取）
            upgradeDescText.text = $"解锁  {theWeapon.weaponName}";

            // 设置武器图标
            weaponIcon.sprite = theWeapon.icon;
        }
        // 将当前武器绑定到按钮（后续选择升级时使用）
        assignedWeapon = theWeapon;
    }

    /// <summary>
    /// 选择该按钮对应的升级选项
    /// </summary>
    public void SelectUpgrade()
    {
        // 先判断assignedWeapon是否不为空
        if (assignedWeapon != null)
        {
            // 判断武器是否已激活
            if (assignedWeapon.gameObject.activeSelf == true)
            {
                // 已激活则执行武器升级
                assignedWeapon.LevelUp();
            }
            else
            {
                // 未激活则调用PlayerController的AddWeapon方法添加该武器
                PlayerController.Instance.AddWeapon(assignedWeapon);
            }
        }
        // 隐藏升级面板
        UIController.Instance.levelUpPanel.SetActive(false);
        // 恢复游戏时间流速（通常升级面板显示时会暂停时间）
        Time.timeScale = 1f;
    }
}

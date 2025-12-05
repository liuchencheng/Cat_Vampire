using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI控制器（单例）
/// 功能：管理游戏中的UI元素更新，如经验条、等级文本等
/// </summary>
public class UIController : MonoBehaviour
{
    // 单例实例（全局唯一，供外部脚本调用）
    public static UIController Instance;

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

    [Tooltip("经验条滑块（显示当前等级的经验进度）")]
    public Slider explvlSlider; // 经验条UI组件

    [Tooltip("等级显示文本（如：Level: 3）")]
    public TMP_Text explvlText; // 等级文本UI组件（TextMeshPro）

    /// 存储所有升级选择按钮的数组
    /// 用于在UI控制器中批量管理多个升级选项按钮（如一次显示3个可选升级）
    [Tooltip("存储所有升级选择按钮的数组")]
    public LevelUpSelectionButton[] levelUpButtons;

    /// 升级选择面板的游戏对象
    /// 用于控制升级界面的显示/隐藏（如玩家升级时显示面板，选择后隐藏）
    [Tooltip("升级选择面板的游戏对象")]
    public GameObject levelUpPanel;

    void Start()
    {
        // 初始化UI显示（可选：避免初始状态为空）
        if (explvlText != null)
        {
            explvlText.text = "Level: 1"; // 默认显示1级
        }
    }

    void Update()
    {
        
    }

    /// <summary>
    /// 更新经验条和等级文本的显示
    /// </summary>
    /// <param name="currentExp">当前拥有的经验值</param>
    /// <param name="levelExp">当前等级升级所需的总经验值</param>
    /// <param name="currentLvl">当前玩家等级</param>
    public void UpdateExperience(int currentExp, int levelExp, int currentLvl)
    {
        // 安全校验：避免UI组件未赋值导致空引用错误
        if (explvlSlider == null || explvlText == null)
        {
            Debug.LogWarning("UI组件未赋值，请在Inspector中关联经验条和等级文本！");
            return;
        }

        // 设置经验条的最大数值（当前等级升级所需总经验）
        explvlSlider.maxValue = levelExp;
        // 设置经验条的当前数值（当前拥有的经验）
        explvlSlider.value = currentExp;
        // 更新等级文本显示（格式：Level: X）
        explvlText.text = $"Level: {currentLvl}" ;
    }

    /// <summary>
    /// 跳过升级界面的方法
    /// 用于玩家不想选择升级时，直接关闭升级面板并恢复游戏运行
    /// </summary>
    public void SkipLevelUp()
    {
        // 隐藏升级选择面板（将面板的激活状态设为false）
        levelUpPanel.SetActive(false);
        // 恢复游戏时间流速（升级时通常会暂停游戏，这里设为1表示正常速度）
        Time.timeScale = 1f;
    }
}

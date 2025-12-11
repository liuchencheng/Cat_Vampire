using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    // 拖入UGUI的CoinText文本组件（用于显示金币数量）
    [Tooltip("拖入UGUI的CoinText文本组件")]
    public TMP_Text coinText;

    // 引用玩家属性对应的“升级UI显示组件”
    [Tooltip("移动速度升级UI")]
    public PlayerStatUpgradeDisplay moveSpeedUpgradeDisplay;   // 移动速度升级UI
    [Tooltip("生命值升级UI")]
    public PlayerStatUpgradeDisplay healthUpgradeDisplay;      // 生命值升级UI
    [Tooltip("拾取范围升级UI")]
    public PlayerStatUpgradeDisplay pickupRangeUpgradeDisplay; // 拾取范围升级UI
    [Tooltip("最大武器数升级UI")]
    public PlayerStatUpgradeDisplay maxWeaponsUpgradeDisplay;  // 最大武器数升级UI

    // 显示时间的Time Text文本组件
    public TMP_Text timeText;

    // 拖入关卡结束UI界面
    [Tooltip("拖入关卡结束UI界面")]
    public GameObject levelEndScreen;
    // 显示被什么打死的文本组件
    [Tooltip("显示被什么打死的文本组件")]
    public TMP_Text endText;
    // 显示"你死了"的文本组件
    [Tooltip("显示\"你死了\"的文本组件")]
    public TMP_Text titleText;
    // 显示"你赢了"的文本组件
    [Tooltip("显示\"你赢了\"的文本组件")]
    public TMP_Text survivedText;

    [Tooltip("主菜单名字")]
    public string mainMenuName;

    // 暂停界面的UI界面
    [Tooltip("暂停界面的UI界面")]
    public GameObject pauseScreen;
    

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
        // 监听“ESC键”按下事件，触发暂停/取消暂停逻辑
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
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

    /// <summary>
    /// 更新UI上的金币显示文本
    /// </summary>
    public void UpdateCoins()
    {
        // 将文本内容设置为 "Coins: 当前金币数"
        // 从CoinController单例中获取当前金币数量（currentCoins）并显示
        coinText.text = $"金币：{CoinController.Instance.currentCoins}";
    }

    /// <summary>
    /// 购买（升级）移动速度的按钮点击回调
    /// </summary>
    public void PurchaseMoveSpeed()
    {
        // 调用玩家属性控制器中对应的移动速度升级方法（实际升级逻辑在PlayerStatController中）
        PlayerStatController.Instance.PurchaseMoveSpeed();
        // 升级后关闭升级面板
        SkipLevelUp();
    }


    /// <summary>
    /// 购买（升级）生命值的按钮点击回调
    /// </summary>
    public void PurchaseHealth()
    {
        // 调用玩家属性控制器中对应的生命值升级方法
        PlayerStatController.Instance.PurchaseHealth();
        // 升级后关闭升级面板
        SkipLevelUp();
    }


    /// <summary>
    /// 购买（升级）拾取范围的按钮点击回调
    /// </summary>
    public void PurchasePickupRange()
    {
        // 调用玩家属性控制器中对应的拾取范围升级方法
        PlayerStatController.Instance.PurchasePickupRange();
        // 升级后关闭升级面板
        SkipLevelUp();
    }


    /// <summary>
    /// 购买（升级）最大武器数的按钮点击回调
    /// </summary>
    public void PurchaseMaxWeapons()
    {
        // 调用玩家属性控制器中对应的最大武器数升级方法
        PlayerStatController.Instance.PurchaseMaxWeapons();
        // 升级后关闭升级面板
        SkipLevelUp();
    }

    /// <summary>
    /// 精准倒计时UI显示（接收剩余时间，格式：Time: 分:秒，秒数补0）
    /// </summary>
    /// <param name="remainingTime">剩余倒计时时间（单位：秒）</param>
    public void UpdateTimer(float remainingTime)
    {
        // 取整处理：避免小数导致的秒数“快速跳变”（核心优化）
        int totalSeconds = Mathf.FloorToInt(Mathf.Max(remainingTime, 0f)); //转为整数秒

        // 更新UI：秒数不足2位补0（如3秒→03），避免显示“1:5”而是“1:05”
        timeText.text = $"未知倒计时: {totalSeconds / 60}:{((int)(totalSeconds % 60)).ToString("00")}";
    }

    /// <summary>
    /// 跳转到主菜单场景的方法（点击“主菜单”按钮时调用）
    /// </summary>
    public void GoToMainMenu()
    {
        // 加载主菜单场景
        SceneManager.LoadScene(mainMenuName);
        Time.timeScale = 1f;
    }


    /// <summary>
    /// 重新开始当前关卡的方法
    /// </summary>
    public void Restart()
    {
        // 加载“当前活跃的场景”（即重新加载当前关卡）
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 退出游戏的方法（通常在暂停界面的“退出”按钮调用）
    /// </summary>
    public void QuitGame()
    {
        // 关闭应用程序（仅在打包后生效，Editor模式下无反应）
        Application.Quit();
    }


    /// <summary>
    /// 暂停/取消暂停的切换逻辑
    /// </summary>
    public void PauseUnpause()
    {
        // 若暂停界面当前是隐藏状态 → 开启暂停
        if (pauseScreen.activeSelf == false)
        {
            pauseScreen.SetActive(true); // 显示暂停界面
            Time.timeScale = 0f; // 时间缩放设为0，暂停游戏逻辑（动画、移动等）
        }
        // 若暂停界面当前是显示状态 → 取消暂停
        else
        {
            pauseScreen.SetActive(false); // 隐藏暂停界面

            // 额外判断：若升级面板也处于隐藏状态，才恢复时间缩放
            if (levelUpPanel.activeSelf == false)
            {
                Time.timeScale = 1f; // 时间缩放设为1，恢复正常游戏速度
            }
        }
    }
}

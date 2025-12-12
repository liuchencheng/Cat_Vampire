using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//游戏倒计时
public class LevelManager : MonoBehaviour
{
    // 单例实例（全局唯一，供外部脚本调用）
    public static LevelManager Instance;

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

    // 标记游戏是否处于活跃状态
    private bool gameActive;
    // 记录计时的时间（单位：秒）
    [Tooltip("记录计时的时间，秒")]
    public float timer;

    // 显示关卡结束界面的延迟时间
    [Tooltip("显示关卡结束界面的延迟时间")]
    public float waitToShowEndScreen = 1f;

    // 新增：用于确保 Boss Panel 渐变只触发一次的标记
    private bool bossPanelFadedIn = false;

    // LevelManager脚本中新增：
    [Tooltip("Boss预制体")]
    public GameObject bossPrefab;
    //boss血量
    public float bossPanelState;

    // 拖入 BGM 对象上的 AudioSource 组件
    [Tooltip("拖入 BGM")]
    public AudioSource bgmSource;
    // 拖入关卡结束音乐 AudioClip
    [Tooltip("拖入关卡结束音乐")]
    public AudioSource levelEndBGM;

    // 关卡结束标记（关键！防止EndLevel2重复执行）
    private bool isLevel2Ended = false;

    void Start()
    {
        // 初始化游戏为活跃状态（开始计时）
        gameActive = true;
    }


    void Update()
    {
        // 仅当游戏活跃时，才累加计时
        if (gameActive == true)
        {
            // 累加当前帧与上一帧的时间差，更新总时长
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                // 调用UI控制器的方法，同步更新计时器显示
                UIController.Instance.UpdateTimer(timer);
            }
            else
            {
                timer = 0;

                // 【核心逻辑】倒计时归零且未触发 Boss 出现时
                if (!bossPanelFadedIn)
                {
                    // 通过 UIController 单例调用 Boss Panel 的渐变方法（从明到暗）
                    if (UIController.Instance != null)
                    {
                        // 传入“是否冻结主游戏”的标记（这里设为true）
                        UIController.Instance.StartBossPanelFadeIn(true);
                    }

                    // 标记为已触发，防止重复调用
                    bossPanelFadedIn = true;
                    // 冻结主游戏逻辑（玩家、怪物、物理等停止）
                    gameActive = false;
                    // 关键：设置时间缩放为0，冻结基于Time.deltaTime的逻辑（如移动、动画）
                    Time.timeScale = 0f;
                }

                // 倒计时结束后，激活Boss
                SpawnBoss(true);
            }
        }

        // 玩家生命值大于0，并且当前倒计时小于等于0，才会触发boss血量检查
        if (timer <= 0 && PlayerHealthController.Instance.currentHealth > 0 && !isLevel2Ended)
        {
            // 读取Boss的当前血量，赋值给LevelManager的bossPanelState
            bossPanelState = bossPrefab.GetComponent<EnemyController>().health;

            //由于BOSS血量清零就会被删除，所以干脆在原有血量上+100
            //这里的判定也改为小于100就不激活BOSS
            if (bossPanelState <= 100)
            {
                //不激活boss
                SpawnBoss(false);

                EndLevel2();
            }
        }
    }

    /// <summary>
    /// 结束关卡的入口方法
    /// </summary>
    public void EndLevel()
    {
        gameActive = false; // 标记游戏停止活跃（暂停计时、停止游戏逻辑）
        
        // 启动协程，实现“延迟后显示关卡结束界面”的效果
        StartCoroutine(EndLevelCo());
    }


    /// <summary>
    /// 关卡结束的协程（处理延迟逻辑）
    /// </summary>
    IEnumerator EndLevelCo()
    {
        // 等待指定时长（waitToShowEndScreen）后，再执行后续逻辑
        yield return new WaitForSeconds(waitToShowEndScreen);

        //把“你赢了”去除
        UIController.Instance.survivedText.text = null;

        //如果倒计时不等于0
        if (timer != 0) UIController.Instance.endText.text = "死于怪物群中……";

        else UIController.Instance.endText.text = "死于神秘帝国士兵之手……";

        // 激活UI控制器中的“关卡结束界面”（显示结算面板）
        UIController.Instance.levelEndScreen.SetActive(true);
    }

    /// <summary>
    /// 结束关卡的入口方法
    /// </summary>
    public void EndLevel2()
    {
        // 标记关卡已结束，防止重复调用
        isLevel2Ended = true;

        gameActive = false; // 标记游戏停止活跃（暂停计时、停止游戏逻辑）

        // 1. 停止当前的 BGM
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }

        // 2. 用音频设备绝对时间+0.2秒延迟播放（给音频系统缓冲时间）
        if (levelEndBGM != null && levelEndBGM.clip != null)
        {
            // 播放音频
            levelEndBGM.Play();
        }

        //把“你死了”去除
        UIController.Instance.titleText.text = null;

        // 激活UI控制器中的“关卡结束界面”（显示结算面板）
        UIController.Instance.levelEndScreen.SetActive(true);

        // 暂停游戏时间
        Time.timeScale = 0f;
    }



    // 激活Boss的方法
    private void SpawnBoss(bool activate)
    {
        if (activate) bossPrefab.SetActive(true);// 激活场景中已存在的Boss
        else bossPrefab.SetActive(false); //否则则关闭已激活的boss
    }
}

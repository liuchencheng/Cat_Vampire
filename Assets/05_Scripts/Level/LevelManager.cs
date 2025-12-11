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
    public float waitToShowEndScreen = 1f;

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
            if(timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                timer = 0;
            }

            // 调用UI控制器的方法，同步更新计时器显示
            UIController.Instance.UpdateTimer(timer);
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
        if (timer != 0) UIController.Instance.endText.text = "死于怪物群殴……";

        else UIController.Instance.endText.text = "死于神秘修仙者之手……";

        // 激活UI控制器中的“关卡结束界面”（显示结算面板）
        UIController.Instance.levelEndScreen.SetActive(true);
    }

}

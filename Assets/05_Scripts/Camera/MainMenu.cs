using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 主菜单逻辑脚本（管理游戏启动、退出等操作）
public class MainMenu : MonoBehaviour
{
    // 要跳转的场景名称
    [Tooltip("要跳转的场景名称")]
    public string firstLevelName;


    /// <summary>
    /// 开始游戏的方法（点击“开始游戏”按钮时调用）
    /// </summary>
    public void StartGame()
    {
        // 加载指定名称的场景（即进入第一个关卡）
        SceneManager.LoadScene(firstLevelName);

        //打死BOSS的时候会暂停时间，这里是为了防止时间暂停
        Time.timeScale = 1f;
    }


    /// <summary>
    /// 退出游戏的方法（点击“退出游戏”按钮时调用）
    /// </summary>
    public void QuitGame()
    {
        // 关闭当前应用程序（仅在打包后的游戏中生效，Editor模式下无反应）
        Application.Quit();
    }
}

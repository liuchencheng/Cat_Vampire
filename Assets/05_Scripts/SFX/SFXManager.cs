using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 音效管理类（继承自Unity的MonoBehaviour，需挂载到游戏物体上）
public class SFXManager : MonoBehaviour
{
    // 单例实例（全局唯一，供外部脚本调用）
    public static SFXManager Instance;

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

    // 音效数组：需要在Inspector面板中拖入对应的AudioSource（每个AudioSource对应一个音效）
    public AudioSource[] soundEffects;

    // 播放指定下标的音效
    // 参数sfxToPlay：要播放的音效在soundEffects数组中的下标
    public void PlaySFX(int sfxToPlay)
    {
        // 先停止该音效（防止重复播放叠加）
        soundEffects[sfxToPlay].Stop();
        // 播放该音效
        soundEffects[sfxToPlay].Play();
    }

    // 播放指定下标的音效（带随机音高变化，让音效更丰富）
    // 参数sfxToPlay：要播放的音效在soundEffects数组中的下标
    public void PlaySFXPitched(int sfxToPlay)
    {
        // 随机设置音高（范围0.8到1.2，1是原音高）
        soundEffects[sfxToPlay].pitch = Random.Range(.8f, 1.2f);

        PlaySFX(sfxToPlay);
    }
}

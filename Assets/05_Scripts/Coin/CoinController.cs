using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//硬币的控制器
public class CoinController : MonoBehaviour
{
    // 1. 公开静态字段
    public static CoinController Instance;

    // 2. 使用 Awake() 初始化，确保在所有 Start() 之前完成
    private void Awake()
    {
        // 容错机制：确保单例的唯一性
        if (Instance == null)
        {
            // 如果没有，设置当前对象为实例
            Instance = this;
        }
        else
        {
            // 如果已经存在其他实例，销毁当前对象
            Destroy(gameObject);
        }
    }

    // 当前拥有的金币数量
    [Tooltip("当前拥有的金币数量")]
    public int currentCoins;

    // 引用金币拾取物预制体（挂载了CoinPickup脚本）
    [Tooltip("引用金币拾取物预制体")]
    public CoinPickup coin;

    /// <summary>
    /// 增加金币的方法
    /// </summary>
    /// <param name="coinsToAdd">要增加的金币数量</param>
    public void AddCoins(int coinsToAdd)
    {
        // 将传入的金币数量累加到当前金币中
        currentCoins += coinsToAdd;

        // 同步玩家右上角UI上的金币数量
        UIController.Instance.UpdateCoins();

        //拾取硬币的音效
        SFXManager.Instance.PlaySFXPitched(2);
    }

    // 生成金币的方法
    /// <param name="position">金币生成的基准位置</param>
    /// <param name="value">该枚金币对应的货币价值</param>
    public void DropCoin(Vector3 position, int value)
    {
        // 实例化金币预制体：
        // 参数1：要生成的金币预制体
        // 参数2：生成位置（在基准位置上偏移，避免和物体重叠）
        // 参数3：生成旋转（无旋转，保持默认）
        CoinPickup newCoin = Instantiate(coin, position + new Vector3(0.1f, 0.1f, 0f), Quaternion.identity);

        // 给新生成的金币赋值对应的货币价值
        newCoin.coinAmount = value;
        // 确保金币生成后立即激活（避免预制体默认禁用的情况）
        newCoin.gameObject.SetActive(true);
    }

    /// <summary>
    /// 消耗指定数量的金币（扣除金币并同步更新UI）
    /// </summary>
    /// <param name="coinsToSpend">要消耗的金币数量</param>
    public void SpendCoins(int coinsToSpend)
    {
        // 从当前金币中扣除指定数量
        currentCoins -= coinsToSpend;
        // 调用UI控制器的方法，同步更新金币显示文本
        UIController.Instance.UpdateCoins();
    }
}

using UnityEngine;

/// <summary>
/// 投掷类武器的控制脚本
/// 功能：控制投掷武器的初速度、飞行过程中的旋转逻辑
/// 挂载对象：投掷武器的预制体（如飞镖、投掷剑等）
/// </summary>
public class ThrownWeapon : MonoBehaviour
{
    // 投掷的基础力度（影响飞行速度）
    [Tooltip("投掷的基础力度")]
    public float throwPower;

    // 武器挂载的2D刚体组件（用于控制物理运动）
    [Tooltip("武器挂载的2D刚体组件")]
    public Rigidbody2D theRB;

    /// 武器飞行过程中的旋转速度（单位：倍/秒，1=每秒转1圈）
    [Tooltip("武器飞行过程中的旋转速度")]
    public float rotateSpeed;

    void Start()
    {
        // 设置刚体的初速度：
        // - 水平方向：在[-throwPower, throwPower]之间随机（实现左右随机的投掷方向）
        // - 垂直方向：固定为throwPower（实现向上投掷的基础轨迹）
        theRB.velocity = new Vector2(Random.Range(-throwPower, throwPower), throwPower);
    }

    void Update()
    {
        // 更新武器的旋转角度：
        // 1. Quaternion.Euler：将欧拉角（Z轴旋转）转换为四元数（Unity中物体旋转的标准存储方式）
        // 2. transform.rotation.eulerAngles.z：获取武器当前的Z轴旋转角度（作为旋转基准）
        // 3. rotateSpeed * 360f * Time.deltaTime：计算每帧的旋转增量（360f=1圈，乘以Time.deltaTime实现“每秒旋转rotateSpeed圈”的效果）
        // 4. Mathf.Sign(theRB.velocity.x)：根据刚体水平速度的方向，决定旋转方向（速度向左则负向旋转，向右则正向旋转）
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            transform.rotation.eulerAngles.z + (rotateSpeed * 360f * Time.deltaTime * Mathf.Sign(theRB.velocity.x))
        );
    }
}
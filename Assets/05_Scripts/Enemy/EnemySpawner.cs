using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人生成器脚本（挂载在生成点对象上）
/// 功能：在指定位置按固定时间间隔自动生成敌人
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("生成配置")]
    [Tooltip("需要生成的敌人预制体（拖拽场景中的敌人预制体到此处）")]
    public GameObject enemyToSpawn; // 待生成的敌人预制体

    [Tooltip("生成间隔时间（单位：秒），值越小生成越频繁")]
    public float timeToSpawn = 2f; // 敌人生成间隔

    private float spawnCounter; // 生成倒计时计数器（无需手动修改，脚本自动维护)

    [Tooltip("大小生成点，用于生成敌人")]
    public Transform minSpawn, maxSpawn;

    private Transform target; //存储玩家当前坐标，用于大小生成点跟着玩家动

    //"敌人超出范围自动删除"
    private float despawnDistance; //敌人超出该距离后会被销毁
    private List<GameObject> spawnedEnemies = new List<GameObject>();//存储已生成的所有敌人的列表
    [Tooltip("每秒检测的敌人数量（用于性能优化，避免一次性遍历所有敌人）")]
    public int checkPerFrame;
    private int enemyToCheck;//当前要检测的敌人索引（遍历列表的游标）
    // 【新增】协程相关的配置
    [Header("清除配置 (协程)")]
    [Tooltip("执行敌人清除逻辑的时间间隔（单位：秒）。协程将每隔这么长时间执行一次清除检测。")]
    public float despawnCheckInterval = 1f;

    [Header("存储多个敌人生成的波次信息列表")]
    public List<WaveInfo> waves;
    private int currentWave = -1;                // 当前处于第几波,初始设为-1，方便第一次调用GoToNextWave直接进入第0波
    private float waveCounter;              // 当前波次的剩余持续时间倒计时

    /// <summary>
    /// 游戏开始时初始化（只执行一次）
    /// </summary>
    void Start()
    {
        // 初始化倒计时计数器为配置的生成间隔，确保首次生成符合设定时间
        //spawnCounter = timeToSpawn;

        target = PlayerHealthController.Instance.transform;

        //超出的极限范围，超过这范围，就会删除敌人
        despawnDistance = Vector3.Distance(transform.position, maxSpawn.position) + 4f;

        //启动协程，处理异步的敌人超出范围清除逻辑
        StartCoroutine(DespawnEnemiesCo());

        GoToNextWave();     // 启动第一波
    }

    // ========== 切换到下一波的方法 ==========
    /// <summary>
    /// 切换到下一波敌人（处理波次越界，重置波次/生成倒计时）
    /// </summary>
    public void GoToNextWave()
    {
        currentWave++;  // 当前波次+1（从-1→0，即第一波）

        // 防越界：如果当前波次超过总波数，从第一波开始重新计算
        if (currentWave >= waves.Count)
        {
            currentWave = 0; //必须为0,不能是-1，不然会报错
        }

        // 重置当前波次的持续时间倒计时（取当前波的waveLength）
        waveCounter = waves[currentWave].waveLength;
        // 重置敌人的生成间隔倒计时（取当前波的timeBetweenSpawns）
        spawnCounter = waves[currentWave].timeBetweenSpawns;
    }


    /// <summary>
    /// 每帧更新（执行频率与帧率一致）
    /// 核心逻辑：倒计时 -> 倒计时结束生成敌人 -> 重置计数器
    /// </summary>
    void Update()
    {
        // 倒计时递减（Time.deltaTime 是上一帧到当前帧的时间差，确保时间计算与帧率无关）
        /*spawnCounter -= Time.deltaTime;

        // 当倒计时小于等于0时，执行敌人生成逻辑
        if (spawnCounter <= 0)
        {
            // 重置倒计时计数器，准备下一次生成
            spawnCounter = timeToSpawn;

            // 生成敌人：
            // 参数1：要生成的预制体（enemyToSpawn）
            // 参数2：生成位置（当前挂载脚本的对象的位置，即生成点位置）
            // 参数3：生成旋转角度（当前挂载脚本的对象的旋转角度，即生成点朝向）
            //Instantiate(enemyToSpawn, transform.position, transform.rotation);
            //Instantiate(enemyToSpawn, SelectSpawnPoint(), transform.rotation);
            GameObject newEnemy = Instantiate(enemyToSpawn, SelectSpawnPoint(), transform.rotation);

            // 3. 将新生成的敌人添加到已生成列表中
            spawnedEnemies.Add(newEnemy);
        
        }*/

        //玩家坐标同步到生成点父类，让大小生成点跟着动
        transform.position = target.position;

        // 仅当玩家存活时，才执行波次逻辑
        if (PlayerHealthController.Instance.currentHealth > 0)
        {
            // 仅当当前波次未超过总波数时，执行波次倒计时
            if (currentWave < waves.Count)
            {
                // 1. 当前波次的持续时间倒计时
                waveCounter -= Time.deltaTime;
                // 当波次持续时间耗尽 → 切换到下一波
                if (waveCounter <= 0)
                {
                    GoToNextWave();
                }

                // 2. 敌人生成间隔倒计时
                spawnCounter -= Time.deltaTime;
                // 当生成间隔耗尽 → 生成新敌人
                if (spawnCounter <= 0)
                {
                    // 重置生成间隔（取当前波的timeBetweenSpawns）
                    spawnCounter = waves[currentWave].timeBetweenSpawns;

                    // 生成敌人：实例化当前波的敌人预制体，位置由SelectSpawnPoint()选择，无旋转
                    GameObject newEnemy = Instantiate(
                        waves[currentWave].enemyToSpawn,
                        SelectSpawnPoint(),  // 自定义方法：选择敌人的生成点（比如随机出生点）
                        Quaternion.identity
                    );

                    // 将新生成的敌人加入列表，方便后续管理（比如波次结束清理敌人）
                    spawnedEnemies.Add(newEnemy);
                }
            }
        }
    }

    /// <summary>
    /// 使用协程来定时、异步地清除超出范围的敌人
    /// </summary>
    IEnumerator DespawnEnemiesCo()
    {
        // 使用无限循环，让协程持续运行
        while (true)
        {
            // 每次循环开始，等待指定的间隔时间
            yield return new WaitForSeconds(despawnCheckInterval);

            // --- 敌人距离检测与销毁逻辑 ---
            // 注意：因为我们是定时执行，所以可以一次性遍历所有敌人，
            // 但为了保持原有的性能优化思路，我们继续使用分批检测。

            // 1. 确保检测的敌人数量不超过列表总长度
            int enemiesToTest = Mathf.Min(checkPerFrame, spawnedEnemies.Count);

            // 2. 临时列表用于存储待删除的索引，避免在遍历时修改列表
            List<int> indicesToRemove = new List<int>();

            // 3. 循环遍历当前批次要检测的敌人
            for (int i = 0; i < enemiesToTest; i++)
            {
                // 获取当前要检测的实际索引，使用取模运算实现循环检测
                int actualIndex = enemyToCheck % spawnedEnemies.Count;

                // 安全判断：当前检测的索引对应的敌人对象不为空
                if (spawnedEnemies[actualIndex] != null)
                {
                    // 核心逻辑：计算距离并判断是否超出
                    if (Vector3.Distance(transform.position, spawnedEnemies[actualIndex].transform.position) > despawnDistance)
                    {
                        Destroy(spawnedEnemies[actualIndex]); // 销毁敌人对象
                        indicesToRemove.Add(actualIndex); // 标记索引，稍后删除
                    }
                    // 如果未超出距离，不需要特殊操作，因为我们会在循环结束后移动游标。
                }
                else
                {
                    // 敌人对象为空（可能已在外部被销毁）→ 标记索引，稍后移除
                    indicesToRemove.Add(actualIndex);
                }

                // 游标向后移动，准备检测列表中的下一个敌人
                enemyToCheck++;
            }

            // 4. 清理列表：从后向前移除被标记的敌人（防止索引错位）
            // 这一步是关键，确保列表操作的安全性
            indicesToRemove.Sort();
            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            {
                // 确保索引没有超出新列表的范围 (尽管从后向前移除通常是安全的)
                int index = indicesToRemove[i];
                if (index < spawnedEnemies.Count)
                {
                    spawnedEnemies.RemoveAt(index);
                }
            }

            // 5. 重置游标：如果游标超出了列表范围，将其重置
            // 确保游标始终指向列表中的有效索引
            if (spawnedEnemies.Count > 0)
            {
                enemyToCheck %= spawnedEnemies.Count;
            }
            else
            {
                enemyToCheck = 0;
            }
        }
    }

    /// <summary>
    /// 随机选择生成点位置
    /// 功能：在以minSpawn、maxSpawn为对角的矩形区域边缘，随机选择一个生成点
    /// </summary>
    /// <returns>随机生成的世界坐标点</returns>
    public Vector3 SelectSpawnPoint()
    {
        // 初始化生成点为原点（后续会覆盖该值）
        Vector3 spawnPoint = Vector3.zero;

        // 随机决定：是否在“垂直边缘”（左右边缘）生成（50%概率）
        // Random.Range(0f,1f)返回0~1的随机数，>0.5则为true
        bool spawnVerticalEdge = Random.Range(0f, 1f) > 0.5f;


        // --- 分支1：在垂直边缘（左/右边缘）生成 ---
        if (spawnVerticalEdge)
        {
            // 1. Y轴坐标：在minSpawn和maxSpawn的Y范围之间随机取值
            spawnPoint.y = Random.Range(minSpawn.position.y, maxSpawn.position.y);

            // 2. X轴坐标：50%概率取maxSpawn的X（右边缘），50%取minSpawn的X（左边缘）
            if (Random.Range(0f, 1f) > .5f)
            {
                spawnPoint.x = maxSpawn.position.x; // 右边缘
            }
            else
            {
                spawnPoint.x = minSpawn.position.x; // 左边缘
            }
        }

        // --- 分支2：在水平边缘（上/下边缘）生成 ---
        else
        {
            // 1. X轴坐标：在minSpawn和maxSpawn的X范围之间随机取值
            spawnPoint.x = Random.Range(minSpawn.position.x, maxSpawn.position.x);

            // 2. Y轴坐标：50%概率取maxSpawn的Y（上边缘），50%取minSpawn的Y（下边缘）
            if (Random.Range(0f, 1f) > .5f)
            {
                spawnPoint.y = maxSpawn.position.y; // 上边缘
            }
            else
            {
                spawnPoint.y = minSpawn.position.y; // 下边缘
            }
        }

        // 返回最终随机选择的生成点坐标
        return spawnPoint;
    }
}


// 序列化特性：让该类可以在Unity Inspector面板中显示和编辑
[System.Serializable]
public class WaveInfo
{
    [Tooltip("该波次要生成的敌人预制体")]
    public GameObject enemyToSpawn;
    [Tooltip("该波次的持续时长（默认10秒）")]
    // 该波次的持续时长（默认10秒）
    public float waveLength = 10f;
    [Tooltip("该波次中敌人的生成间隔（默认1秒）")]
    public float timeBetweenSpawns = 1f;
}

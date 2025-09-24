using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    [Header("生成设置")]
    public float spawnInterval = 3f;
    public int spawnCountPerWave = 1;
    public float spawnDistanceFromEdge = 1f;

    [Header("瓦片地图引用（可选）")]
    public Tilemap originalTilemap;
    public Tilemap convertedTilemap;
    public TileBase originalTile;
    public TileBase convertedTile;

    // 公共属性，允许其他代码访问和修改
    public float SpawnInterval
    {
        get => spawnInterval;
        set => spawnInterval = Mathf.Max(0.1f, value); // 最小间隔0.1秒
    }

    public int SpawnCountPerWave
    {
        get => spawnCountPerWave;
        set => spawnCountPerWave = Mathf.Max(1, value); // 最少生成1个
    }

    private Camera mainCamera;
    private float minX, maxX, minY, maxY;
    private Coroutine spawnCoroutine;

    void Start()
    {
        mainCamera = Camera.main;
        CalculateSpawnBounds();
        StartSpawning();
    }

    void CalculateSpawnBounds()
    {
        if (mainCamera == null) return;

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        minX = bottomLeft.x - spawnDistanceFromEdge;
        maxX = topRight.x + spawnDistanceFromEdge;
        minY = bottomLeft.y - spawnDistanceFromEdge;
        maxY = topRight.y + spawnDistanceFromEdge;

        Debug.Log($"敌人生成范围: X({minX:F1}, {maxX:F1}), Y({minY:F1}, {maxY:F1})");
    }

    // 开始生成敌人
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnEnemiesRoutine());
    }

    // 停止生成敌人
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnWave();
        }
    }

    void SpawnWave()
    {
        for (int i = 0; i < spawnCountPerWave; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // 设置敌人的瓦片地图引用
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.originalTilemap = originalTilemap;
            enemy.convertedTilemap = convertedTilemap;
            enemy.originalTile = originalTile;
            enemy.convertedTile = convertedTile;
        }
    }

    Vector3 GetSpawnPosition()
    {
        int side = Random.Range(0, 4);
        Vector3 spawnPosition = Vector3.zero;

        switch (side)
        {
            case 0: // 上边
                spawnPosition = new Vector3(Random.Range(minX, maxX), maxY, 0);
                break;
            case 1: // 下边
                spawnPosition = new Vector3(Random.Range(minX, maxX), minY, 0);
                break;
            case 2: // 左边
                spawnPosition = new Vector3(minX, Random.Range(minY, maxY), 0);
                break;
            case 3: // 右边
                spawnPosition = new Vector3(maxX, Random.Range(minY, maxY), 0);
                break;
        }

        return spawnPosition;
    }

    // 其他代码调用示例方法
    public void IncreaseSpawnRate(float intervalReduction)
    {
        SpawnInterval -= intervalReduction;
        Debug.Log($"生成间隔减少到: {SpawnInterval}秒");
    }

    public void IncreaseSpawnCount(int additionalCount)
    {
        SpawnCountPerWave += additionalCount;
        Debug.Log($"每波生成数量增加到: {SpawnCountPerWave}个");
    }

    public void SetSpawnParameters(float newInterval, int newCount)
    {
        SpawnInterval = newInterval;
        SpawnCountPerWave = newCount;
        Debug.Log($"设置生成参数: 间隔{SpawnInterval}秒, 数量{SpawnCountPerWave}个");
    }

    void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        CalculateSpawnBounds();

        Gizmos.color = Color.red;

        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        Gizmos.color = Color.green;
        Vector3 screenBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 screenBottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane));
        Vector3 screenTopLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane));
        Vector3 screenTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        Gizmos.DrawLine(screenBottomLeft, screenBottomRight);
        Gizmos.DrawLine(screenBottomRight, screenTopRight);
        Gizmos.DrawLine(screenTopRight, screenTopLeft);
        Gizmos.DrawLine(screenTopLeft, screenBottomLeft);
    }
}
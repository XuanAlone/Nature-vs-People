using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public float spawnDistanceFromEdge = 1f; // 距离屏幕边缘的距离

    private Camera mainCamera;
    private float minX, maxX, minY, maxY;

    void Start()
    {
        mainCamera = Camera.main;
        CalculateSpawnBounds();
        StartCoroutine(SpawnEnemiesRoutine());
    }

    void CalculateSpawnBounds()
    {
        if (mainCamera == null) return;

        // 获取屏幕边界的世界坐标
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        // 计算生成范围（屏幕外一定距离）
        minX = bottomLeft.x - spawnDistanceFromEdge;
        maxX = topRight.x + spawnDistanceFromEdge;
        minY = bottomLeft.y - spawnDistanceFromEdge;
        maxY = topRight.y + spawnDistanceFromEdge;

        Debug.Log($"生成范围: X({minX}, {maxX}), Y({minY}, {maxY})");
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    Vector3 GetSpawnPosition()
    {
        // 随机选择生成边（0:上, 1:下, 2:左, 3:右）
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

    // 在Scene视图中显示生成范围
    void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        CalculateSpawnBounds();

        Gizmos.color = Color.red;

        // 绘制生成边界
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        // 绘制屏幕边界（绿色）
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
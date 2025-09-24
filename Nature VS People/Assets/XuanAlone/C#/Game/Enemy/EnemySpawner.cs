using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3f;
    public float spawnDistanceFromEdge = 1f;

    [Header("��Ƭ��ͼ���ã���ѡ��")]
    public Tilemap originalTilemap;
    public Tilemap convertedTilemap;
    public TileBase originalTile;
    public TileBase convertedTile;

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

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        minX = bottomLeft.x - spawnDistanceFromEdge;
        maxX = topRight.x + spawnDistanceFromEdge;
        minY = bottomLeft.y - spawnDistanceFromEdge;
        maxY = topRight.y + spawnDistanceFromEdge;

        Debug.Log($"�������ɷ�Χ: X({minX:F1}, {maxX:F1}), Y({minY:F1}, {maxY:F1})");
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
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // ���õ��˵���Ƭ��ͼ����
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
            case 0: // �ϱ�
                spawnPosition = new Vector3(Random.Range(minX, maxX), maxY, 0);
                break;
            case 1: // �±�
                spawnPosition = new Vector3(Random.Range(minX, maxX), minY, 0);
                break;
            case 2: // ���
                spawnPosition = new Vector3(minX, Random.Range(minY, maxY), 0);
                break;
            case 3: // �ұ�
                spawnPosition = new Vector3(maxX, Random.Range(minY, maxY), 0);
                break;
        }

        return spawnPosition;
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
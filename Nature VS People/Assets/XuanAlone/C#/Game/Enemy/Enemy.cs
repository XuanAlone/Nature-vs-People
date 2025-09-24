using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("敌人属性")]
    public float health = 100f;
    public float moveSpeed = 2f;
    public float attackPower = 10f;
    public float attackRate = 1f;
    public float stayDuration = 3f;

    [Header("瓦片设置")]
    public Tilemap originalTilemap;      // 原来的瓦片地图（森林等）
    public Tilemap convertedTilemap;     // 转换后的瓦片地图（沙漠等）
    public TileBase originalTile;        // 需要转换的原始瓦片
    public TileBase convertedTile;       // 转换后的瓦片
    public float detectionRadius = 0.3f; // 检测半径（敌人脚下区域）

    [Header("生成设置")]
    public float spawnPadding = 1.5f;

    private Vector3 centerPoint;
    private Vector3 moveDirection;
    private bool isStopped = false;
    private bool hasPassedCenter = false;
    private Camera mainCamera;
    private Vector3Int lastConvertedCell = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

    void Start()
    {
        mainCamera = Camera.main;
        centerPoint = Vector3.zero;

        // 设置移动方向
        moveDirection = (centerPoint - transform.position).normalized;

        StartCoroutine(CheckGroundRoutine());
    }

    void Update()
    {
        if (!isStopped)
        {
            MoveEnemy();
        }

        CheckOutOfBounds();
    }

    void MoveEnemy()
    {
        if (hasPassedCenter)
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        else
        {
            Vector3 directionToCenter = (centerPoint - transform.position).normalized;
            transform.position += directionToCenter * moveSpeed * Time.deltaTime;

            if (Vector3.Dot(moveDirection, centerPoint - transform.position) < 0)
            {
                hasPassedCenter = true;
            }
        }
    }

    IEnumerator CheckGroundRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.3f);

            if (!isStopped && originalTilemap != null && convertedTilemap != null)
            {
                CheckAndConvertTilesUnderEnemy();
            }
        }
    }

    void CheckAndConvertTilesUnderEnemy()
    {
        // 计算敌人脚下的矩形区域（基于检测半径）
        Vector3 enemyPos = transform.position;

        // 计算矩形区域的四个角
        Vector3 corner1 = enemyPos + new Vector3(-detectionRadius, -detectionRadius, 0);
        Vector3 corner2 = enemyPos + new Vector3(detectionRadius, -detectionRadius, 0);
        Vector3 corner3 = enemyPos + new Vector3(detectionRadius, detectionRadius, 0);
        Vector3 corner4 = enemyPos + new Vector3(-detectionRadius, detectionRadius, 0);

        // 将世界坐标转换为网格坐标
        Vector3Int minCell = originalTilemap.WorldToCell(corner1);
        Vector3Int maxCell = originalTilemap.WorldToCell(corner3);

        bool foundConvertibleTile = false;

        // 遍历矩形区域内的所有网格
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                // 检查这个网格是否在敌人脚下的圆形/矩形区域内
                if (IsCellInEnemyArea(cellPos, enemyPos, detectionRadius))
                {
                    // 检查是否需要转换瓦片
                    if (ShouldConvertTileAtPosition(cellPos))
                    {
                        foundConvertibleTile = true;
                        break;
                    }
                }
            }
            if (foundConvertibleTile) break;
        }

        // 如果找到可转换的瓦片，停止并转换
        if (foundConvertibleTile && !isStopped)
        {
            StartCoroutine(StopAndConvertTiles(enemyPos));
        }
    }

    bool IsCellInEnemyArea(Vector3Int cellPos, Vector3 enemyPos, float radius)
    {
        // 获取网格的中心世界坐标
        Vector3 worldPos = originalTilemap.GetCellCenterWorld(cellPos);

        // 检查是否在检测半径内（圆形检测）
        return Vector3.Distance(worldPos, enemyPos) <= radius;
    }

    bool ShouldConvertTileAtPosition(Vector3Int cellPosition)
    {
        // 检查原来的瓦片地图是否有指定类型的瓦片
        if (originalTilemap.HasTile(cellPosition))
        {
            TileBase currentTile = originalTilemap.GetTile(cellPosition);

            // 如果指定了特定瓦片类型，只转换该类型
            if (originalTile != null)
            {
                return currentTile == originalTile;
            }
            // 如果没有指定特定类型，转换任何瓦片
            else
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator StopAndConvertTiles(Vector3 enemyPos)
    {
        isStopped = true;

        // 等待停留时间
        yield return new WaitForSeconds(stayDuration);

        // 转换敌人脚下的瓦片
        ConvertTilesUnderEnemy(enemyPos);

        isStopped = false;
    }

    void ConvertTilesUnderEnemy(Vector3 enemyPos)
    {
        if (originalTilemap == null || convertedTilemap == null || convertedTile == null)
            return;

        // 计算矩形区域的四个角
        Vector3 corner1 = enemyPos + new Vector3(-detectionRadius, -detectionRadius, 0);
        Vector3 corner2 = enemyPos + new Vector3(detectionRadius, -detectionRadius, 0);
        Vector3 corner3 = enemyPos + new Vector3(detectionRadius, detectionRadius, 0);
        Vector3 corner4 = enemyPos + new Vector3(-detectionRadius, detectionRadius, 0);

        // 将世界坐标转换为网格坐标
        Vector3Int minCell = originalTilemap.WorldToCell(corner1);
        Vector3Int maxCell = originalTilemap.WorldToCell(corner3);

        // 遍历矩形区域内的所有网格
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                // 检查这个网格是否在敌人脚下的区域内
                if (IsCellInEnemyArea(cellPos, enemyPos, detectionRadius))
                {
                    ConvertTileAtPosition(cellPos);
                }
            }
        }
    }

    // 转换指定位置的瓦片 - 仿照ImprovedLineSegmentController的逻辑
    void ConvertTileAtPosition(Vector3Int cellPosition)
    {
        // 避免重复转换同一个瓦片
        if (cellPosition == lastConvertedCell)
            return;

        // 检查原来的瓦片地图是否有指定类型的瓦片
        if (originalTilemap.HasTile(cellPosition) && ShouldConvertTileAtPosition(cellPosition))
        {
            // 移除原来的瓦片
            originalTilemap.SetTile(cellPosition, null);
            originalTilemap.RefreshTile(cellPosition);

            // 在转换后的瓦片地图上设置新瓦片
            convertedTilemap.SetTile(cellPosition, convertedTile);
            convertedTilemap.RefreshTile(cellPosition);

            lastConvertedCell = cellPosition;
            Debug.Log($"敌人转换瓦片位置: {cellPosition}");
        }
    }

    void CheckOutOfBounds()
    {
        if (mainCamera == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPos.x < -0.2f || viewportPos.x > 1.2f ||
            viewportPos.y < -0.2f || viewportPos.y > 1.2f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // 可视化调试
    void OnDrawGizmosSelected()
    {
        // 显示检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 显示移动方向
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, moveDirection * 1f);
        }
    }
}
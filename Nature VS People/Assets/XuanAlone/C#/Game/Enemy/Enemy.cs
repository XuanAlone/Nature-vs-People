using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("��������")]
    public float health = 100f;
    public float moveSpeed = 2f;
    public float attackPower = 10f;
    public float attackRate = 1f;
    public float stayDuration = 3f;

    [Header("��Ƭ����")]
    public Tilemap originalTilemap;      // ԭ������Ƭ��ͼ��ɭ�ֵȣ�
    public Tilemap convertedTilemap;     // ת�������Ƭ��ͼ��ɳĮ�ȣ�
    public TileBase originalTile;        // ��Ҫת����ԭʼ��Ƭ
    public TileBase convertedTile;       // ת�������Ƭ
    public float detectionRadius = 0.3f; // ���뾶�����˽�������

    [Header("��������")]
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

        // �����ƶ�����
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
        // ������˽��µľ������򣨻��ڼ��뾶��
        Vector3 enemyPos = transform.position;

        // �������������ĸ���
        Vector3 corner1 = enemyPos + new Vector3(-detectionRadius, -detectionRadius, 0);
        Vector3 corner2 = enemyPos + new Vector3(detectionRadius, -detectionRadius, 0);
        Vector3 corner3 = enemyPos + new Vector3(detectionRadius, detectionRadius, 0);
        Vector3 corner4 = enemyPos + new Vector3(-detectionRadius, detectionRadius, 0);

        // ����������ת��Ϊ��������
        Vector3Int minCell = originalTilemap.WorldToCell(corner1);
        Vector3Int maxCell = originalTilemap.WorldToCell(corner3);

        bool foundConvertibleTile = false;

        // �������������ڵ���������
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                // �����������Ƿ��ڵ��˽��µ�Բ��/����������
                if (IsCellInEnemyArea(cellPos, enemyPos, detectionRadius))
                {
                    // ����Ƿ���Ҫת����Ƭ
                    if (ShouldConvertTileAtPosition(cellPos))
                    {
                        foundConvertibleTile = true;
                        break;
                    }
                }
            }
            if (foundConvertibleTile) break;
        }

        // ����ҵ���ת������Ƭ��ֹͣ��ת��
        if (foundConvertibleTile && !isStopped)
        {
            StartCoroutine(StopAndConvertTiles(enemyPos));
        }
    }

    bool IsCellInEnemyArea(Vector3Int cellPos, Vector3 enemyPos, float radius)
    {
        // ��ȡ�����������������
        Vector3 worldPos = originalTilemap.GetCellCenterWorld(cellPos);

        // ����Ƿ��ڼ��뾶�ڣ�Բ�μ�⣩
        return Vector3.Distance(worldPos, enemyPos) <= radius;
    }

    bool ShouldConvertTileAtPosition(Vector3Int cellPosition)
    {
        // ���ԭ������Ƭ��ͼ�Ƿ���ָ�����͵���Ƭ
        if (originalTilemap.HasTile(cellPosition))
        {
            TileBase currentTile = originalTilemap.GetTile(cellPosition);

            // ���ָ�����ض���Ƭ���ͣ�ֻת��������
            if (originalTile != null)
            {
                return currentTile == originalTile;
            }
            // ���û��ָ���ض����ͣ�ת���κ���Ƭ
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

        // �ȴ�ͣ��ʱ��
        yield return new WaitForSeconds(stayDuration);

        // ת�����˽��µ���Ƭ
        ConvertTilesUnderEnemy(enemyPos);

        isStopped = false;
    }

    void ConvertTilesUnderEnemy(Vector3 enemyPos)
    {
        if (originalTilemap == null || convertedTilemap == null || convertedTile == null)
            return;

        // �������������ĸ���
        Vector3 corner1 = enemyPos + new Vector3(-detectionRadius, -detectionRadius, 0);
        Vector3 corner2 = enemyPos + new Vector3(detectionRadius, -detectionRadius, 0);
        Vector3 corner3 = enemyPos + new Vector3(detectionRadius, detectionRadius, 0);
        Vector3 corner4 = enemyPos + new Vector3(-detectionRadius, detectionRadius, 0);

        // ����������ת��Ϊ��������
        Vector3Int minCell = originalTilemap.WorldToCell(corner1);
        Vector3Int maxCell = originalTilemap.WorldToCell(corner3);

        // �������������ڵ���������
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                // �����������Ƿ��ڵ��˽��µ�������
                if (IsCellInEnemyArea(cellPos, enemyPos, detectionRadius))
                {
                    ConvertTileAtPosition(cellPos);
                }
            }
        }
    }

    // ת��ָ��λ�õ���Ƭ - ����ImprovedLineSegmentController���߼�
    void ConvertTileAtPosition(Vector3Int cellPosition)
    {
        // �����ظ�ת��ͬһ����Ƭ
        if (cellPosition == lastConvertedCell)
            return;

        // ���ԭ������Ƭ��ͼ�Ƿ���ָ�����͵���Ƭ
        if (originalTilemap.HasTile(cellPosition) && ShouldConvertTileAtPosition(cellPosition))
        {
            // �Ƴ�ԭ������Ƭ
            originalTilemap.SetTile(cellPosition, null);
            originalTilemap.RefreshTile(cellPosition);

            // ��ת�������Ƭ��ͼ����������Ƭ
            convertedTilemap.SetTile(cellPosition, convertedTile);
            convertedTilemap.RefreshTile(cellPosition);

            lastConvertedCell = cellPosition;
            Debug.Log($"����ת����Ƭλ��: {cellPosition}");
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

    // ���ӻ�����
    void OnDrawGizmosSelected()
    {
        // ��ʾ��ⷶΧ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // ��ʾ�ƶ�����
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, moveDirection * 1f);
        }
    }
}
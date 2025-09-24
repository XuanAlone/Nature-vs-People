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
    public List<TileBase> targetTiles = new List<TileBase>(); // 需要转换的瓦片类型
    public TileBase convertedTile; // 转换后的瓦片类型
    public string[] tilemapLayers = { "Ground", "Terrain" }; // 瓦片图层名称

    [Header("生成设置")]
    public float spawnPadding = 1.5f; // 生成时距离屏幕边缘的额外距离

    private Vector3 centerPoint;
    private Vector3 moveDirection;
    private List<Tilemap> groundTilemaps;
    private bool isStopped = false;
    private float attackTimer = 0f;
    private bool hasPassedCenter = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        centerPoint = Vector3.zero;

        // 获取所有指定的瓦片地图层
        groundTilemaps = new List<Tilemap>();
        foreach (string layerName in tilemapLayers)
        {
            GameObject layerObject = GameObject.Find(layerName);
            if (layerObject != null)
            {
                Tilemap tilemap = layerObject.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    groundTilemaps.Add(tilemap);
                }
            }
        }

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
            yield return new WaitForSeconds(0.3f); // 更频繁地检测

            if (!isStopped)
            {
                CheckCurrentTile();
            }
        }
    }

    void CheckCurrentTile()
    {
        if (groundTilemaps == null || groundTilemaps.Count == 0) return;

        foreach (Tilemap tilemap in groundTilemaps)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
            TileBase currentTile = tilemap.GetTile(cellPosition);

            // 检查当前瓦片是否在目标转换列表中
            if (targetTiles.Contains(currentTile))
            {
                StartCoroutine(StopAndConvertTile(tilemap, cellPosition));
                break; // 只转换一个瓦片地图上的瓦片
            }
        }
    }

    IEnumerator StopAndConvertTile(Tilemap tilemap, Vector3Int tilePosition)
    {
        isStopped = true;

        // 等待停留时间
        yield return new WaitForSeconds(stayDuration);

        // 转换瓦片
        if (tilemap != null && convertedTile != null)
        {
            tilemap.SetTile(tilePosition, convertedTile);
            Debug.Log($"转换瓦片在位置: {tilePosition}");
        }

        isStopped = false;
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
}
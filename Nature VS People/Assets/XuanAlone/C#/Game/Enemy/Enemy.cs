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
    public List<TileBase> targetTiles = new List<TileBase>(); // ��Ҫת������Ƭ����
    public TileBase convertedTile; // ת�������Ƭ����
    public string[] tilemapLayers = { "Ground", "Terrain" }; // ��Ƭͼ������

    [Header("��������")]
    public float spawnPadding = 1.5f; // ����ʱ������Ļ��Ե�Ķ������

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

        // ��ȡ����ָ������Ƭ��ͼ��
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
            yield return new WaitForSeconds(0.3f); // ��Ƶ���ؼ��

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

            // ��鵱ǰ��Ƭ�Ƿ���Ŀ��ת���б���
            if (targetTiles.Contains(currentTile))
            {
                StartCoroutine(StopAndConvertTile(tilemap, cellPosition));
                break; // ֻת��һ����Ƭ��ͼ�ϵ���Ƭ
            }
        }
    }

    IEnumerator StopAndConvertTile(Tilemap tilemap, Vector3Int tilePosition)
    {
        isStopped = true;

        // �ȴ�ͣ��ʱ��
        yield return new WaitForSeconds(stayDuration);

        // ת����Ƭ
        if (tilemap != null && convertedTile != null)
        {
            tilemap.SetTile(tilePosition, convertedTile);
            Debug.Log($"ת����Ƭ��λ��: {tilePosition}");
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
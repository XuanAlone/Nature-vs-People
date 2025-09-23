using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapChange : MonoBehaviour
{
    [Header("小球设置")]
    public GameObject ball;
    public float moveSpeed = 3f;
    public float directionChangeInterval = 2f;

    [Header("TileMap设置")]
    public Tilemap tilemap;
    public TileBase yellowTile;
    public TileBase blueTile;

    private Vector2 currentDirection;
    private Rigidbody2D ballRb; 



    void Start()
    {
        ballRb=ball.GetComponent <Rigidbody2D>();


        StartCoroutine(RandomMovement());

        StartCoroutine(DetectTileUnderBall());
    }

    
    void Update()
    {
        
    }



    IEnumerator RandomMovement()
    {
        while (true)
        {
            // 随机改变方向
            ChangeRandomDirection();

            // 等待指定间隔后再次改变方向
            yield return new WaitForSeconds(directionChangeInterval);
        }
    }

    void ChangeRandomDirection()
    {
        // 生成随机方向向量
        currentDirection = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        // 应用速度
        ballRb.velocity = currentDirection * moveSpeed;
    }



    IEnumerator DetectTileUnderBall()
    {
        while (true)
        {            
            Vector3Int cellPosition = tilemap.WorldToCell(ball.transform.position);
            
            TileBase currentTile = tilemap.GetTile(cellPosition);

            if (currentTile != null)
            {                
                TileBase newTile;
                if (currentTile == yellowTile)
                {
                    newTile = blueTile;  
                }
                else
                {
                    newTile = yellowTile; 
                }
                
                tilemap.SetTile(cellPosition, newTile);
                
                tilemap.RefreshTile(cellPosition);
                
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class ImprovedLineSegmentController : MonoBehaviour
{
    [Header("线段设置")]
    public float segmentLength = 3f;     // 线段长度
    public float segmentWidth = 0.1f;    // 线段宽度（覆盖范围）
    public float moveSpeed = 2f;         // 移动速度
    public float moveDistance = 10f;     // 移动距离（翻倍）
    public Color lineColor = Color.red;  // 线段颜色

    [Header("地图设置")]
    public Tilemap forestTilemap;
    public Tilemap desertTilemap;
    public TileBase forestTile;
    public TileBase desertTile;

    private enum SegmentState { Waiting, Placed, Moving, Finished }
    private SegmentState currentState = SegmentState.Waiting;

    private Vector3 segmentPosition;     // 线段中心位置
    private Vector3 moveDirection;       // 移动方向
    private Vector3 segmentDirection;    // 线段方向（与移动方向垂直）
    private LineRenderer lineRenderer;   // 线段渲染器
    private Vector3 originalPosition;    // 起始位置

    void Start()
    {
        InitializeLineRenderer();
    }

    void Update()
    {
        HandleMouseInput();
        UpdateSegment();
    }

    // 初始化线段渲染器 - 修复显示问题
    void InitializeLineRenderer()
    {
        // 确保有LineRenderer组件
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // 设置线段属性
        lineRenderer.startWidth = segmentWidth;
        lineRenderer.endWidth = segmentWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.enabled = false;

        // 确保线段在Tilemap上方显示
        lineRenderer.sortingOrder = 10;
    }

    // 处理鼠标输入
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (currentState)
            {
                case SegmentState.Waiting:
                    // 第一次点击：在屏幕中心创建线段
                    CreateSegmentAtCenter();
                    currentState = SegmentState.Placed;
                    break;

                case SegmentState.Placed:
                    // 第二次点击：确定移动方向
                    SetMovementDirection();
                    currentState = SegmentState.Moving;
                    originalPosition = segmentPosition; // 记录起始位置
                    break;

                case SegmentState.Finished:
                    // 完成后点击：立即重置并重新开始，无冷却时间
                    ResetAndRestartImmediately();
                    break;
            }
        }
    }

    // 新增方法：立即重置并重新开始
    void ResetAndRestartImmediately()
    {
        // 重置到等待状态
        currentState = SegmentState.Waiting;
        segmentPosition = Vector3.zero;

        // 立即创建新线段（跳过等待状态）
        CreateSegmentAtCenter();
        currentState = SegmentState.Placed;

        Debug.Log("线段已重置，可以立即设置新方向");
    }

    // 在屏幕中心创建线段
    void CreateSegmentAtCenter()
    {
        segmentPosition = Vector3.zero; // 中心点(0,0)
        UpdateLineVisual();
        lineRenderer.enabled = true;
    }

    // 设置移动方向（垂直于鼠标方向）
    void SetMovementDirection()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 获取从中心指向鼠标的方向
        Vector3 directionToMouse = (mouseWorldPos - segmentPosition).normalized;

        // 线段方向与鼠标方向垂直（法线方向）
        segmentDirection = new Vector3(-directionToMouse.y, directionToMouse.x, 0);

        // 移动方向与线段方向垂直（即鼠标方向）
        moveDirection = directionToMouse;

        // 更新线段视觉方向
        UpdateLineVisual();
    }

    // 更新线段视觉
    void UpdateLineVisual()
    {
        // 计算线段两个端点
        Vector3 startPoint = segmentPosition - segmentDirection * segmentLength * 0.5f;
        Vector3 endPoint = segmentPosition + segmentDirection * segmentLength * 0.5f;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    // 更新线段状态
    void UpdateSegment()
    {
        if (currentState == SegmentState.Moving)
        {
            // 移动线段
            segmentPosition += moveDirection * moveSpeed * Time.deltaTime;
            UpdateLineVisual();

            // 转换覆盖区域的瓦片
            ConvertTilesUnderSegment();

            // 检查是否移动足够远（使用翻倍的距离）
            if (Vector3.Distance(segmentPosition, originalPosition) > moveDistance)
            {
                currentState = SegmentState.Finished;
                Debug.Log("线段移动完成");
            }
        }
    }

    // 转换线段覆盖区域下的瓦片 - 修复覆盖不全问题
    void ConvertTilesUnderSegment()
    {
        // 获取线段的方向（垂直于移动方向）
        Vector3 startPoint = segmentPosition - segmentDirection * segmentLength * 0.5f;
        Vector3 endPoint = segmentPosition + segmentDirection * segmentLength * 0.5f;

        // 计算线段覆盖的矩形区域
        Vector3 perpendicular = moveDirection.normalized;
        Vector3 corner1 = startPoint - perpendicular * segmentWidth * 0.5f;
        Vector3 corner2 = startPoint + perpendicular * segmentWidth * 0.5f;
        Vector3 corner3 = endPoint + perpendicular * segmentWidth * 0.5f;
        Vector3 corner4 = endPoint - perpendicular * segmentWidth * 0.5f;

        // 计算矩形的边界
        float minX = Mathf.Min(corner1.x, corner2.x, corner3.x, corner4.x);
        float maxX = Mathf.Max(corner1.x, corner2.x, corner3.x, corner4.x);
        float minY = Mathf.Min(corner1.y, corner2.y, corner3.y, corner4.y);
        float maxY = Mathf.Max(corner1.y, corner2.y, corner3.y, corner4.y);

        // 将世界坐标转换为网格坐标
        Vector3Int minCell = forestTilemap.WorldToCell(new Vector3(minX, minY, 0));
        Vector3Int maxCell = forestTilemap.WorldToCell(new Vector3(maxX, maxY, 0));

        // 遍历矩形区域内的所有网格
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                // 检查这个网格是否在线段覆盖的矩形内
                if (IsCellInSegmentArea(cellPos, corner1, corner2, corner3, corner4))
                {
                    ConvertTileAtPosition(cellPos);
                }
            }
        }
    }

    // 检查网格是否在线段覆盖的矩形区域内
    bool IsCellInSegmentArea(Vector3Int cellPos, Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4)
    {
        // 获取网格的中心世界坐标
        Vector3 worldPos = forestTilemap.GetCellCenterWorld(cellPos);

        // 使用点积法判断点是否在凸四边形内
        return IsPointInQuadrilateral(worldPos, c1, c2, c3, c4);
    }

    // 判断点是否在凸四边形内
    bool IsPointInQuadrilateral(Vector3 point, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        // 将四边形分为两个三角形，检查点是否在任一三角形内
        return IsPointInTriangle(point, a, b, c) || IsPointInTriangle(point, a, c, d);
    }

    // 判断点是否在三角形内（使用重心坐标法）
    bool IsPointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = c - a;
        Vector3 v1 = b - a;
        Vector3 v2 = point - a;

        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);

        float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    // 转换指定位置的瓦片
    // 转换指定位置的瓦片 - 修改为无论原地图是什么都变成新地图
    void ConvertTileAtPosition(Vector3Int cellPosition)
    {
        // 检查所有可能的原地图瓦片，无论是什么都转换为新地图
        bool hasAnyTile = false;

        // 检查森林地图是否有瓦片
        if (forestTilemap.HasTile(cellPosition))
        {
            hasAnyTile = true;
            forestTilemap.SetTile(cellPosition, null);
            forestTilemap.RefreshTile(cellPosition);
        }

        // 检查沙漠地图是否有瓦片（如果需要也可以转换）
        if (desertTilemap.HasTile(cellPosition))
        {
            hasAnyTile = true;
            // 如果希望保留沙漠瓦片，可以注释掉下面两行
            // desertTilemap.SetTile(cellPosition, null);
            // desertTilemap.RefreshTile(cellPosition);
        }

        // 无论原地图是什么，都设置为新地图（沙漠）
        desertTilemap.SetTile(cellPosition, desertTile);
        desertTilemap.RefreshTile(cellPosition);
    }

    // 重置线段状态
    void ResetSegment()
    {
        currentState = SegmentState.Waiting;
        segmentPosition = Vector3.zero;
        lineRenderer.enabled = false;
        Debug.Log("线段已重置，点击屏幕中心创建新线段");
    }

    // 可视化调试
    void OnDrawGizmos()
    {
        if (currentState == SegmentState.Moving)
        {
            // 显示线段
            Gizmos.color = Color.green;
            Vector3 start = segmentPosition - segmentDirection * segmentLength * 0.5f;
            Vector3 end = segmentPosition + segmentDirection * segmentLength * 0.5f;
            Gizmos.DrawLine(start, end);

            // 显示移动方向
            Gizmos.color = Color.red;
            Gizmos.DrawRay(segmentPosition, moveDirection * 2f);

            // 显示覆盖区域
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Vector3 perpendicular = moveDirection.normalized;
            Vector3 c1 = start - perpendicular * segmentWidth * 0.5f;
            Vector3 c2 = start + perpendicular * segmentWidth * 0.5f;
            Vector3 c3 = end + perpendicular * segmentWidth * 0.5f;
            Vector3 c4 = end - perpendicular * segmentWidth * 0.5f;

            Gizmos.DrawLine(c1, c2);
            Gizmos.DrawLine(c2, c3);
            Gizmos.DrawLine(c3, c4);
            Gizmos.DrawLine(c4, c1);
        }
    }

    // GUI显示状态信息
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        string stateText = "";
        switch (currentState)
        {
            case SegmentState.Waiting:
                stateText = "状态: 等待中 - 点击屏幕中心创建线段";
                break;
            case SegmentState.Placed:
                stateText = "状态: 已放置 - 点击鼠标确定移动方向";
                break;
            case SegmentState.Moving:
                stateText = "状态: 移动中 - 线段正在转换瓦片";
                break;
            case SegmentState.Finished:
                stateText = "状态: 已完成 - 点击屏幕重新开始";
                break;
        }

        GUI.Label(new Rect(10, 10, 500, 30), stateText, style);

        if (currentState == SegmentState.Moving)
        {
            float progress = Vector3.Distance(segmentPosition, originalPosition) / moveDistance;
            GUI.Label(new Rect(10, 40, 300, 30), $"进度: {progress:P0}", style);
        }
    }
}
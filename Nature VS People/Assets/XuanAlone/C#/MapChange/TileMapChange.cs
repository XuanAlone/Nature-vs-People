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
    [SerializeField] private Tilemap originalTilemap;      // 改为SerializeField
    [SerializeField] private Tilemap convertedTilemap;     // 改为SerializeField
    public TileBase originalTile;        // 原来的瓦片（森林瓦片）
    public TileBase convertedTile;       // 变成的瓦片（沙漠瓦片）


    [Header("Tilemap查找设置")]
    public string originalTilemapName = "ForestTilemap";   // 原始瓦片地图名称
    public string convertedTilemapName = "DesertTilemap";  // 转换后瓦片地图名称
    public bool autoFindTilemaps = true;                   // 是否自动查找Tilemap


    [Header("UI设置")]
    public UnityEngine.UI.Button createSegmentButton; // 创建线段的按钮

    private enum SegmentState { Waiting, Placed, Moving, Finished }
    private SegmentState currentState = SegmentState.Waiting;

    private Vector3 segmentPosition;     // 线段中心位置
    private Vector3 moveDirection;       // 移动方向
    private Vector3 segmentDirection;    // 线段方向（与移动方向垂直）
    private LineRenderer lineRenderer;   // 线段渲染器
    private Vector3 originalPosition;    // 起始位置


    

    void Start()
    {
        
        InitializeTilemaps(); // 初始化Tilemap引用
        InitializeLineRenderer();

        // 绑定按钮点击事件
        if (createSegmentButton != null)
        {
            createSegmentButton.onClick.AddListener(OnCreateSegmentButtonClicked);
        }
    }

    void InitializeTilemaps()
    {
        // 如果已经在Inspector中手动设置了Tilemap，则不需要自动查找
        if (originalTilemap != null && convertedTilemap != null)
        {
            //Debug.Log("Tilemap引用已通过Inspector设置");
            return;
        }

        if (!autoFindTilemaps)
        {
            Debug.LogWarning("自动查找Tilemap已禁用，请确保在Inspector中手动设置Tilemap引用");
            return;
        }

        // 自动查找Tilemap
        FindAndAssignTilemaps();
    }


    // 新增：查找并分配Tilemap
    void FindAndAssignTilemaps()
    {
        // 方法1：通过名称查找
        GameObject originalTilemapObj = GameObject.Find(originalTilemapName);
        GameObject convertedTilemapObj = GameObject.Find(convertedTilemapName);

        if (originalTilemapObj != null)
        {
            originalTilemap = originalTilemapObj.GetComponent<Tilemap>();
            //Debug.Log($"找到原始Tilemap: {originalTilemapName}");
        }
        else
        {
            Debug.LogError($"未找到原始Tilemap: {originalTilemapName}");
        }

        if (convertedTilemapObj != null)
        {
            convertedTilemap = convertedTilemapObj.GetComponent<Tilemap>();
            //Debug.Log($"找到转换Tilemap: {convertedTilemapName}");
        }
        else
        {
            Debug.LogError($"未找到转换Tilemap: {convertedTilemapName}");
        }

        // 方法2：如果方法1失败，尝试通过标签查找
        if (originalTilemap == null)
        {
            GameObject[] tilemaps = GameObject.FindGameObjectsWithTag("Tilemap");
            foreach (GameObject obj in tilemaps)
            {
                Tilemap tm = obj.GetComponent<Tilemap>();
                if (tm != null && originalTilemap == null) // 简单的分配逻辑，可根据需要改进
                {
                    originalTilemap = tm;
                    break;
                }
            }
        }

        // 方法3：查找所有Tilemap并尝试智能分配（更复杂的逻辑）
        if (originalTilemap == null || convertedTilemap == null)
        {
            FindTilemapsAutomatically();
        }
    }

    // 新增：自动查找Tilemap的备选方案
    void FindTilemapsAutomatically()
    {
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();

        if (allTilemaps.Length >= 2)
        {
            // 简单的逻辑：假设第一个是original，第二个是converted
            // 你可以根据实际需求改进这个逻辑
            if (originalTilemap == null) originalTilemap = allTilemaps[0];
            if (convertedTilemap == null) convertedTilemap = allTilemaps[1];
            //Debug.Log("通过自动查找分配了Tilemap");
        }
        else if (allTilemaps.Length == 1)
        {
            originalTilemap = allTilemaps[0];
            Debug.LogWarning("只找到一个Tilemap，将使用同一个Tilemap进行转换");
            convertedTilemap = originalTilemap;
        }
        else
        {
            Debug.LogError("场景中未找到任何Tilemap！");
        }
    }

    // 新增：公共方法用于手动设置Tilemap（可以在其他脚本中调用）
    public void SetTilemaps(Tilemap original, Tilemap converted)
    {
        originalTilemap = original;
        convertedTilemap = converted;
        //Debug.Log("Tilemap已通过代码设置");
    }

    // 新增：验证Tilemap引用是否有效
    bool AreTilemapsValid()
    {
        if (originalTilemap == null)
        {
            Debug.LogError("原始Tilemap引用为空！");
            return false;
        }

        if (convertedTilemap == null)
        {
            Debug.LogError("转换Tilemap引用为空！");
            return false;
        }

        return true;
    }




    void Update()
    {
        HandleMouseInput();
        UpdateSegment();

        // 在已放置状态下，让线段跟随鼠标旋转
        if (currentState == SegmentState.Placed)
        {
            UpdateSegmentDirectionToMouse();
        }

        // 根据状态更新按钮的交互性
        UpdateButtonInteractivity();
    }

    // 初始化线段渲染器 - 确保线段可见
    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.startWidth = segmentWidth;
        lineRenderer.endWidth = segmentWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.enabled = false;
        lineRenderer.sortingOrder = 10;
    }

    // 处理鼠标输入
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (currentState)
            {
                case SegmentState.Placed:
                    // 在已放置状态下点击：确定移动方向
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

    // 按钮点击事件处理
    public void OnCreateSegmentButtonClicked()
    {
        if (currentState == SegmentState.Waiting || currentState == SegmentState.Finished)
        {
            // 在屏幕中心创建线段
            CreateSegmentAtCenter();
            currentState = SegmentState.Placed;
            //Debug.Log("线段已创建，请移动鼠标旋转线段，然后点击屏幕设置移动方向");
        }
    }

    // 更新按钮的交互性
   public void UpdateButtonInteractivity()
    {
        if (createSegmentButton != null)
        {
            // 在等待状态和完成状态下按钮可交互
            createSegmentButton.interactable = (currentState == SegmentState.Waiting || currentState == SegmentState.Finished);
        }
    }

    // 立即重置并重新开始
    void ResetAndRestartImmediately()
    {
        // 重置到等待状态
        currentState = SegmentState.Waiting;
        segmentPosition = Vector3.zero;
        lineRenderer.enabled = false;

        //Debug.Log("线段已重置，请点击按钮创建新线段");
    }

    // 在屏幕中心创建线段 - 确保线段可见
    void CreateSegmentAtCenter()
    {
        segmentPosition = Vector3.zero; // 中心点(0,0)

        // 设置初始方向（例如向右）
        segmentDirection = Vector3.right;
        moveDirection = Vector3.up; // 默认向上移动

        UpdateLineVisual();
        lineRenderer.enabled = true;
    }

    // 新增方法：更新线段方向以跟随鼠标
    void UpdateSegmentDirectionToMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 获取从中心指向鼠标的方向
        Vector3 directionToMouse = (mouseWorldPos - segmentPosition).normalized;

        // 线段方向与鼠标方向垂直（法线方向）
        segmentDirection = new Vector3(-directionToMouse.y, directionToMouse.x, 0);

        // 更新线段视觉方向
        UpdateLineVisual();
    }

    // 设置移动方向（垂直于线段方向）
    void SetMovementDirection()
    {
        // 移动方向与线段方向垂直
        // 这里我们选择与线段方向垂直的两个方向中的一个
        // 可以根据需要选择正向或反向
        moveDirection = new Vector3(segmentDirection.y, -segmentDirection.x, 0);

        // 确保移动方向是单位向量
        moveDirection.Normalize();

        //Debug.Log("移动方向已设置，线段开始移动");
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
                //Debug.Log("线段移动完成，按钮已恢复可用");
            }
        }
    }

    // 转换线段覆盖区域下的瓦片
    void ConvertTilesUnderSegment()
    {
        if (!AreTilemapsValid()) return; // 如果Tilemap无效，直接返回

        // 原有的转换逻辑...
        Vector3 startPoint = segmentPosition - segmentDirection * segmentLength * 0.5f;
        Vector3 endPoint = segmentPosition + segmentDirection * segmentLength * 0.5f;

        Vector3 perpendicular = moveDirection.normalized;
        Vector3 corner1 = startPoint - perpendicular * segmentWidth * 0.5f;
        Vector3 corner2 = startPoint + perpendicular * segmentWidth * 0.5f;
        Vector3 corner3 = endPoint + perpendicular * segmentWidth * 0.5f;
        Vector3 corner4 = endPoint - perpendicular * segmentWidth * 0.5f;

        float minX = Mathf.Min(corner1.x, corner2.x, corner3.x, corner4.x);
        float maxX = Mathf.Max(corner1.x, corner2.x, corner3.x, corner4.x);
        float minY = Mathf.Min(corner1.y, corner2.y, corner3.y, corner4.y);
        float maxY = Mathf.Max(corner1.y, corner2.y, corner3.y, corner4.y);

        Vector3Int minCell = originalTilemap.WorldToCell(new Vector3(minX, minY, 0));
        Vector3Int maxCell = originalTilemap.WorldToCell(new Vector3(maxX, maxY, 0));

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
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
        Vector3 worldPos = originalTilemap.GetCellCenterWorld(cellPos);

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

    // 转换指定位置的瓦片 - 只在原来的瓦片存在的位置上变化
    // 修改ConvertTileAtPosition方法，添加验证
    void ConvertTileAtPosition(Vector3Int cellPosition)
    {
        if (!AreTilemapsValid()) return;

        if (originalTilemap.HasTile(cellPosition))
        {
            originalTilemap.SetTile(cellPosition, null);
            originalTilemap.RefreshTile(cellPosition);

            convertedTilemap.SetTile(cellPosition, convertedTile);
            convertedTilemap.RefreshTile(cellPosition);
        }
    }

    // 可视化调试
    void OnDrawGizmos()
    {
        if (currentState == SegmentState.Moving || currentState == SegmentState.Placed)
        {
            // 显示线段
            Gizmos.color = Color.green;
            Vector3 start = segmentPosition - segmentDirection * segmentLength * 0.5f;
            Vector3 end = segmentPosition + segmentDirection * segmentLength * 0.5f;
            Gizmos.DrawLine(start, end);

            // 显示移动方向（仅在移动状态下）
            if (currentState == SegmentState.Moving)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(segmentPosition, moveDirection * 2f);
            }

            // 显示覆盖区域
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Vector3 perpendicular = (currentState == SegmentState.Moving) ? moveDirection.normalized : Vector3.up;
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
                //stateText = "状态: 等待中 - 点击按钮创建线段";
                break;
            case SegmentState.Placed:
                //stateText = "状态: 已放置 - 移动鼠标旋转线段，点击屏幕设置移动方向";
                break;
            case SegmentState.Moving:
                //stateText = "状态: 移动中 - 线段正在转换瓦片";
                break;
            case SegmentState.Finished:
                //stateText = "状态: 已完成 - 按钮已可用，点击按钮创建新线段";
                break;
        }

        GUI.Label(new Rect(10, 10, 600, 30), stateText, style);

        if (currentState == SegmentState.Moving)
        {
            float progress = Vector3.Distance(segmentPosition, originalPosition) / moveDistance;
            GUI.Label(new Rect(10, 40, 300, 30), $"进度: {progress:P0}", style);
        }
    }
}
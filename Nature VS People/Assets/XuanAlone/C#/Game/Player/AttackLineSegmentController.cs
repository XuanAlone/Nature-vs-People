using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackLineSegmentController : MonoBehaviour
{
    [Header("线段设置")]
    public float segmentLength = 3f;     // 线段长度
    public float segmentWidth = 0.1f;    // 线段宽度（覆盖范围）
    public float moveSpeed = 2f;         // 移动速度
    public float moveDistance = 10f;     // 移动距离（翻倍）
    public Color lineColor = Color.red;  // 线段颜色
    public float attackDamage = 50f;     // 攻击伤害值

    [Header("UI设置")]
    public UnityEngine.UI.Button createSegmentButton; // 创建线段的按钮

    private enum SegmentState { Waiting, Placed, Moving, Finished }
    private SegmentState currentState = SegmentState.Waiting;

    private Vector3 segmentPosition;     // 线段中心位置
    private Vector3 moveDirection;       // 移动方向
    private Vector3 segmentDirection;    // 线段方向（与移动方向垂直）
    private LineRenderer lineRenderer;   // 线段渲染器
    private Vector3 originalPosition;    // 起始位置

    private HashSet<Enemy> damagedEnemies; // 已经受到伤害的敌人列表，避免重复伤害

    void Start()
    {
        InitializeLineRenderer();
        damagedEnemies = new HashSet<Enemy>();

        // 绑定按钮点击事件
        if (createSegmentButton != null)
        {
            createSegmentButton.onClick.AddListener(OnCreateSegmentButtonClicked);
        }
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
                case SegmentState.Placed:
                    // 在已放置状态下点击：确定移动方向
                    SetMovementDirection();
                    currentState = SegmentState.Moving;
                    originalPosition = segmentPosition; // 记录起始位置
                    damagedEnemies.Clear(); // 清空已伤害敌人列表
                    break;

                case SegmentState.Finished:
                    // 完成后点击：立即重置并重新开始，无冷却时间
                    ResetAndRestartImmediately();
                    break;
            }
        }
    }

    // 按钮点击事件处理
    void OnCreateSegmentButtonClicked()
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
    void UpdateButtonInteractivity()
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
        damagedEnemies.Clear();

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

            // 对线段覆盖区域的敌人造成伤害
            DamageEnemiesUnderSegment();

            // 检查是否移动足够远（使用翻倍的距离）
            if (Vector3.Distance(segmentPosition, originalPosition) > moveDistance)
            {
                currentState = SegmentState.Finished;
                //Debug.Log("线段移动完成，按钮已恢复可用");
            }
        }
    }

    // 对线段覆盖区域下的敌人造成伤害
    void DamageEnemiesUnderSegment()
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

        // 获取所有敌人
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in allEnemies)
        {
            // 如果敌人已经受到过伤害，跳过
            if (damagedEnemies.Contains(enemy))
                continue;

            // 检查敌人是否在线段覆盖的矩形区域内
            if (IsEnemyInSegmentArea(enemy.transform.position, corner1, corner2, corner3, corner4))
            {
                // 对敌人造成伤害
                enemy.TakeDamage(attackDamage);

                // 添加到已伤害敌人列表，避免重复伤害
                damagedEnemies.Add(enemy);

                // 可选：添加视觉反馈，如闪烁效果
                StartCoroutine(FlashEnemy(enemy));
            }
        }
    }

    // 检查敌人是否在线段覆盖的矩形区域内
    bool IsEnemyInSegmentArea(Vector3 enemyPos, Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4)
    {
        // 使用点积法判断点是否在凸四边形内
        return IsPointInQuadrilateral(enemyPos, c1, c2, c3, c4);
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

    // 敌人受到伤害时的闪烁效果
    IEnumerator FlashEnemy(Enemy enemy)
    {
        if (enemy == null) yield break;

        SpriteRenderer enemyRenderer = enemy.GetComponent<SpriteRenderer>();
        if (enemyRenderer == null) yield break;

        Color originalColor = enemyRenderer.color;
        enemyRenderer.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (enemy != null && enemyRenderer != null)
        {
            enemyRenderer.color = originalColor;
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
                //stateText = "状态: 移动中 - 线段正在攻击敌人";
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
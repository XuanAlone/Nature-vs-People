using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class ImprovedLineSegmentController : MonoBehaviour
{
    [Header("�߶�����")]
    public float segmentLength = 3f;     // �߶γ���
    public float segmentWidth = 0.1f;    // �߶ο�ȣ����Ƿ�Χ��
    public float moveSpeed = 2f;         // �ƶ��ٶ�
    public float moveDistance = 10f;     // �ƶ����루������
    public Color lineColor = Color.red;  // �߶���ɫ

    [Header("��ͼ����")]
    public Tilemap forestTilemap;
    public Tilemap desertTilemap;
    public TileBase forestTile;
    public TileBase desertTile;

    private enum SegmentState { Waiting, Placed, Moving, Finished }
    private SegmentState currentState = SegmentState.Waiting;

    private Vector3 segmentPosition;     // �߶�����λ��
    private Vector3 moveDirection;       // �ƶ�����
    private Vector3 segmentDirection;    // �߶η������ƶ�����ֱ��
    private LineRenderer lineRenderer;   // �߶���Ⱦ��
    private Vector3 originalPosition;    // ��ʼλ��

    void Start()
    {
        InitializeLineRenderer();
    }

    void Update()
    {
        HandleMouseInput();
        UpdateSegment();
    }

    // ��ʼ���߶���Ⱦ�� - �޸���ʾ����
    void InitializeLineRenderer()
    {
        // ȷ����LineRenderer���
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // �����߶�����
        lineRenderer.startWidth = segmentWidth;
        lineRenderer.endWidth = segmentWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.enabled = false;

        // ȷ���߶���Tilemap�Ϸ���ʾ
        lineRenderer.sortingOrder = 10;
    }

    // �����������
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (currentState)
            {
                case SegmentState.Waiting:
                    // ��һ�ε��������Ļ���Ĵ����߶�
                    CreateSegmentAtCenter();
                    currentState = SegmentState.Placed;
                    break;

                case SegmentState.Placed:
                    // �ڶ��ε����ȷ���ƶ�����
                    SetMovementDirection();
                    currentState = SegmentState.Moving;
                    originalPosition = segmentPosition; // ��¼��ʼλ��
                    break;

                case SegmentState.Finished:
                    // ��ɺ������������ò����¿�ʼ������ȴʱ��
                    ResetAndRestartImmediately();
                    break;
            }
        }
    }

    // �����������������ò����¿�ʼ
    void ResetAndRestartImmediately()
    {
        // ���õ��ȴ�״̬
        currentState = SegmentState.Waiting;
        segmentPosition = Vector3.zero;

        // �����������߶Σ������ȴ�״̬��
        CreateSegmentAtCenter();
        currentState = SegmentState.Placed;

        Debug.Log("�߶������ã��������������·���");
    }

    // ����Ļ���Ĵ����߶�
    void CreateSegmentAtCenter()
    {
        segmentPosition = Vector3.zero; // ���ĵ�(0,0)
        UpdateLineVisual();
        lineRenderer.enabled = true;
    }

    // �����ƶ����򣨴�ֱ����귽��
    void SetMovementDirection()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // ��ȡ������ָ�����ķ���
        Vector3 directionToMouse = (mouseWorldPos - segmentPosition).normalized;

        // �߶η�������귽��ֱ�����߷���
        segmentDirection = new Vector3(-directionToMouse.y, directionToMouse.x, 0);

        // �ƶ��������߶η���ֱ������귽��
        moveDirection = directionToMouse;

        // �����߶��Ӿ�����
        UpdateLineVisual();
    }

    // �����߶��Ӿ�
    void UpdateLineVisual()
    {
        // �����߶������˵�
        Vector3 startPoint = segmentPosition - segmentDirection * segmentLength * 0.5f;
        Vector3 endPoint = segmentPosition + segmentDirection * segmentLength * 0.5f;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    // �����߶�״̬
    void UpdateSegment()
    {
        if (currentState == SegmentState.Moving)
        {
            // �ƶ��߶�
            segmentPosition += moveDirection * moveSpeed * Time.deltaTime;
            UpdateLineVisual();

            // ת�������������Ƭ
            ConvertTilesUnderSegment();

            // ����Ƿ��ƶ��㹻Զ��ʹ�÷����ľ��룩
            if (Vector3.Distance(segmentPosition, originalPosition) > moveDistance)
            {
                currentState = SegmentState.Finished;
                Debug.Log("�߶��ƶ����");
            }
        }
    }

    // ת���߶θ��������µ���Ƭ - �޸����ǲ�ȫ����
    void ConvertTilesUnderSegment()
    {
        // ��ȡ�߶εķ��򣨴�ֱ���ƶ�����
        Vector3 startPoint = segmentPosition - segmentDirection * segmentLength * 0.5f;
        Vector3 endPoint = segmentPosition + segmentDirection * segmentLength * 0.5f;

        // �����߶θ��ǵľ�������
        Vector3 perpendicular = moveDirection.normalized;
        Vector3 corner1 = startPoint - perpendicular * segmentWidth * 0.5f;
        Vector3 corner2 = startPoint + perpendicular * segmentWidth * 0.5f;
        Vector3 corner3 = endPoint + perpendicular * segmentWidth * 0.5f;
        Vector3 corner4 = endPoint - perpendicular * segmentWidth * 0.5f;

        // ������εı߽�
        float minX = Mathf.Min(corner1.x, corner2.x, corner3.x, corner4.x);
        float maxX = Mathf.Max(corner1.x, corner2.x, corner3.x, corner4.x);
        float minY = Mathf.Min(corner1.y, corner2.y, corner3.y, corner4.y);
        float maxY = Mathf.Max(corner1.y, corner2.y, corner3.y, corner4.y);

        // ����������ת��Ϊ��������
        Vector3Int minCell = forestTilemap.WorldToCell(new Vector3(minX, minY, 0));
        Vector3Int maxCell = forestTilemap.WorldToCell(new Vector3(maxX, maxY, 0));

        // �������������ڵ���������
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                // �����������Ƿ����߶θ��ǵľ�����
                if (IsCellInSegmentArea(cellPos, corner1, corner2, corner3, corner4))
                {
                    ConvertTileAtPosition(cellPos);
                }
            }
        }
    }

    // ��������Ƿ����߶θ��ǵľ���������
    bool IsCellInSegmentArea(Vector3Int cellPos, Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4)
    {
        // ��ȡ�����������������
        Vector3 worldPos = forestTilemap.GetCellCenterWorld(cellPos);

        // ʹ�õ�����жϵ��Ƿ���͹�ı�����
        return IsPointInQuadrilateral(worldPos, c1, c2, c3, c4);
    }

    // �жϵ��Ƿ���͹�ı�����
    bool IsPointInQuadrilateral(Vector3 point, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        // ���ı��η�Ϊ���������Σ������Ƿ�����һ��������
        return IsPointInTriangle(point, a, b, c) || IsPointInTriangle(point, a, c, d);
    }

    // �жϵ��Ƿ����������ڣ�ʹ���������귨��
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

    // ת��ָ��λ�õ���Ƭ
    // ת��ָ��λ�õ���Ƭ - �޸�Ϊ����ԭ��ͼ��ʲô������µ�ͼ
    void ConvertTileAtPosition(Vector3Int cellPosition)
    {
        // ������п��ܵ�ԭ��ͼ��Ƭ��������ʲô��ת��Ϊ�µ�ͼ
        bool hasAnyTile = false;

        // ���ɭ�ֵ�ͼ�Ƿ�����Ƭ
        if (forestTilemap.HasTile(cellPosition))
        {
            hasAnyTile = true;
            forestTilemap.SetTile(cellPosition, null);
            forestTilemap.RefreshTile(cellPosition);
        }

        // ���ɳĮ��ͼ�Ƿ�����Ƭ�������ҪҲ����ת����
        if (desertTilemap.HasTile(cellPosition))
        {
            hasAnyTile = true;
            // ���ϣ������ɳĮ��Ƭ������ע�͵���������
            // desertTilemap.SetTile(cellPosition, null);
            // desertTilemap.RefreshTile(cellPosition);
        }

        // ����ԭ��ͼ��ʲô��������Ϊ�µ�ͼ��ɳĮ��
        desertTilemap.SetTile(cellPosition, desertTile);
        desertTilemap.RefreshTile(cellPosition);
    }

    // �����߶�״̬
    void ResetSegment()
    {
        currentState = SegmentState.Waiting;
        segmentPosition = Vector3.zero;
        lineRenderer.enabled = false;
        Debug.Log("�߶������ã������Ļ���Ĵ������߶�");
    }

    // ���ӻ�����
    void OnDrawGizmos()
    {
        if (currentState == SegmentState.Moving)
        {
            // ��ʾ�߶�
            Gizmos.color = Color.green;
            Vector3 start = segmentPosition - segmentDirection * segmentLength * 0.5f;
            Vector3 end = segmentPosition + segmentDirection * segmentLength * 0.5f;
            Gizmos.DrawLine(start, end);

            // ��ʾ�ƶ�����
            Gizmos.color = Color.red;
            Gizmos.DrawRay(segmentPosition, moveDirection * 2f);

            // ��ʾ��������
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

    // GUI��ʾ״̬��Ϣ
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        string stateText = "";
        switch (currentState)
        {
            case SegmentState.Waiting:
                stateText = "״̬: �ȴ��� - �����Ļ���Ĵ����߶�";
                break;
            case SegmentState.Placed:
                stateText = "״̬: �ѷ��� - ������ȷ���ƶ�����";
                break;
            case SegmentState.Moving:
                stateText = "״̬: �ƶ��� - �߶�����ת����Ƭ";
                break;
            case SegmentState.Finished:
                stateText = "״̬: ����� - �����Ļ���¿�ʼ";
                break;
        }

        GUI.Label(new Rect(10, 10, 500, 30), stateText, style);

        if (currentState == SegmentState.Moving)
        {
            float progress = Vector3.Distance(segmentPosition, originalPosition) / moveDistance;
            GUI.Label(new Rect(10, 40, 300, 30), $"����: {progress:P0}", style);
        }
    }
}
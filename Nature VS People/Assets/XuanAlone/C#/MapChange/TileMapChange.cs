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
    [SerializeField] private Tilemap originalTilemap;      // ��ΪSerializeField
    [SerializeField] private Tilemap convertedTilemap;     // ��ΪSerializeField
    public TileBase originalTile;        // ԭ������Ƭ��ɭ����Ƭ��
    public TileBase convertedTile;       // ��ɵ���Ƭ��ɳĮ��Ƭ��


    [Header("Tilemap��������")]
    public string originalTilemapName = "ForestTilemap";   // ԭʼ��Ƭ��ͼ����
    public string convertedTilemapName = "DesertTilemap";  // ת������Ƭ��ͼ����
    public bool autoFindTilemaps = true;                   // �Ƿ��Զ�����Tilemap


    [Header("UI����")]
    public UnityEngine.UI.Button createSegmentButton; // �����߶εİ�ť

    private enum SegmentState { Waiting, Placed, Moving, Finished }
    private SegmentState currentState = SegmentState.Waiting;

    private Vector3 segmentPosition;     // �߶�����λ��
    private Vector3 moveDirection;       // �ƶ�����
    private Vector3 segmentDirection;    // �߶η������ƶ�����ֱ��
    private LineRenderer lineRenderer;   // �߶���Ⱦ��
    private Vector3 originalPosition;    // ��ʼλ��


    

    void Start()
    {
        
        InitializeTilemaps(); // ��ʼ��Tilemap����
        InitializeLineRenderer();

        // �󶨰�ť����¼�
        if (createSegmentButton != null)
        {
            createSegmentButton.onClick.AddListener(OnCreateSegmentButtonClicked);
        }
    }

    void InitializeTilemaps()
    {
        // ����Ѿ���Inspector���ֶ�������Tilemap������Ҫ�Զ�����
        if (originalTilemap != null && convertedTilemap != null)
        {
            //Debug.Log("Tilemap������ͨ��Inspector����");
            return;
        }

        if (!autoFindTilemaps)
        {
            Debug.LogWarning("�Զ�����Tilemap�ѽ��ã���ȷ����Inspector���ֶ�����Tilemap����");
            return;
        }

        // �Զ�����Tilemap
        FindAndAssignTilemaps();
    }


    // ���������Ҳ�����Tilemap
    void FindAndAssignTilemaps()
    {
        // ����1��ͨ�����Ʋ���
        GameObject originalTilemapObj = GameObject.Find(originalTilemapName);
        GameObject convertedTilemapObj = GameObject.Find(convertedTilemapName);

        if (originalTilemapObj != null)
        {
            originalTilemap = originalTilemapObj.GetComponent<Tilemap>();
            //Debug.Log($"�ҵ�ԭʼTilemap: {originalTilemapName}");
        }
        else
        {
            Debug.LogError($"δ�ҵ�ԭʼTilemap: {originalTilemapName}");
        }

        if (convertedTilemapObj != null)
        {
            convertedTilemap = convertedTilemapObj.GetComponent<Tilemap>();
            //Debug.Log($"�ҵ�ת��Tilemap: {convertedTilemapName}");
        }
        else
        {
            Debug.LogError($"δ�ҵ�ת��Tilemap: {convertedTilemapName}");
        }

        // ����2���������1ʧ�ܣ�����ͨ����ǩ����
        if (originalTilemap == null)
        {
            GameObject[] tilemaps = GameObject.FindGameObjectsWithTag("Tilemap");
            foreach (GameObject obj in tilemaps)
            {
                Tilemap tm = obj.GetComponent<Tilemap>();
                if (tm != null && originalTilemap == null) // �򵥵ķ����߼����ɸ�����Ҫ�Ľ�
                {
                    originalTilemap = tm;
                    break;
                }
            }
        }

        // ����3����������Tilemap���������ܷ��䣨�����ӵ��߼���
        if (originalTilemap == null || convertedTilemap == null)
        {
            FindTilemapsAutomatically();
        }
    }

    // �������Զ�����Tilemap�ı�ѡ����
    void FindTilemapsAutomatically()
    {
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();

        if (allTilemaps.Length >= 2)
        {
            // �򵥵��߼��������һ����original���ڶ�����converted
            // ����Ը���ʵ������Ľ�����߼�
            if (originalTilemap == null) originalTilemap = allTilemaps[0];
            if (convertedTilemap == null) convertedTilemap = allTilemaps[1];
            //Debug.Log("ͨ���Զ����ҷ�����Tilemap");
        }
        else if (allTilemaps.Length == 1)
        {
            originalTilemap = allTilemaps[0];
            Debug.LogWarning("ֻ�ҵ�һ��Tilemap����ʹ��ͬһ��Tilemap����ת��");
            convertedTilemap = originalTilemap;
        }
        else
        {
            Debug.LogError("������δ�ҵ��κ�Tilemap��");
        }
    }

    // �������������������ֶ�����Tilemap�������������ű��е��ã�
    public void SetTilemaps(Tilemap original, Tilemap converted)
    {
        originalTilemap = original;
        convertedTilemap = converted;
        //Debug.Log("Tilemap��ͨ����������");
    }

    // ��������֤Tilemap�����Ƿ���Ч
    bool AreTilemapsValid()
    {
        if (originalTilemap == null)
        {
            Debug.LogError("ԭʼTilemap����Ϊ�գ�");
            return false;
        }

        if (convertedTilemap == null)
        {
            Debug.LogError("ת��Tilemap����Ϊ�գ�");
            return false;
        }

        return true;
    }




    void Update()
    {
        HandleMouseInput();
        UpdateSegment();

        // ���ѷ���״̬�£����߶θ��������ת
        if (currentState == SegmentState.Placed)
        {
            UpdateSegmentDirectionToMouse();
        }

        // ����״̬���°�ť�Ľ�����
        UpdateButtonInteractivity();
    }

    // ��ʼ���߶���Ⱦ�� - ȷ���߶οɼ�
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

    // �����������
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (currentState)
            {
                case SegmentState.Placed:
                    // ���ѷ���״̬�µ����ȷ���ƶ�����
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

    // ��ť����¼�����
    public void OnCreateSegmentButtonClicked()
    {
        if (currentState == SegmentState.Waiting || currentState == SegmentState.Finished)
        {
            // ����Ļ���Ĵ����߶�
            CreateSegmentAtCenter();
            currentState = SegmentState.Placed;
            //Debug.Log("�߶��Ѵ��������ƶ������ת�߶Σ�Ȼ������Ļ�����ƶ�����");
        }
    }

    // ���°�ť�Ľ�����
   public void UpdateButtonInteractivity()
    {
        if (createSegmentButton != null)
        {
            // �ڵȴ�״̬�����״̬�°�ť�ɽ���
            createSegmentButton.interactable = (currentState == SegmentState.Waiting || currentState == SegmentState.Finished);
        }
    }

    // �������ò����¿�ʼ
    void ResetAndRestartImmediately()
    {
        // ���õ��ȴ�״̬
        currentState = SegmentState.Waiting;
        segmentPosition = Vector3.zero;
        lineRenderer.enabled = false;

        //Debug.Log("�߶������ã�������ť�������߶�");
    }

    // ����Ļ���Ĵ����߶� - ȷ���߶οɼ�
    void CreateSegmentAtCenter()
    {
        segmentPosition = Vector3.zero; // ���ĵ�(0,0)

        // ���ó�ʼ�����������ң�
        segmentDirection = Vector3.right;
        moveDirection = Vector3.up; // Ĭ�������ƶ�

        UpdateLineVisual();
        lineRenderer.enabled = true;
    }

    // ���������������߶η����Ը������
    void UpdateSegmentDirectionToMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // ��ȡ������ָ�����ķ���
        Vector3 directionToMouse = (mouseWorldPos - segmentPosition).normalized;

        // �߶η�������귽��ֱ�����߷���
        segmentDirection = new Vector3(-directionToMouse.y, directionToMouse.x, 0);

        // �����߶��Ӿ�����
        UpdateLineVisual();
    }

    // �����ƶ����򣨴�ֱ���߶η���
    void SetMovementDirection()
    {
        // �ƶ��������߶η���ֱ
        // ��������ѡ�����߶η���ֱ�����������е�һ��
        // ���Ը�����Ҫѡ���������
        moveDirection = new Vector3(segmentDirection.y, -segmentDirection.x, 0);

        // ȷ���ƶ������ǵ�λ����
        moveDirection.Normalize();

        //Debug.Log("�ƶ����������ã��߶ο�ʼ�ƶ�");
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
                //Debug.Log("�߶��ƶ���ɣ���ť�ѻָ�����");
            }
        }
    }

    // ת���߶θ��������µ���Ƭ
    void ConvertTilesUnderSegment()
    {
        if (!AreTilemapsValid()) return; // ���Tilemap��Ч��ֱ�ӷ���

        // ԭ�е�ת���߼�...
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


    // ��������Ƿ����߶θ��ǵľ���������
    bool IsCellInSegmentArea(Vector3Int cellPos, Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4)
    {
        // ��ȡ�����������������
        Vector3 worldPos = originalTilemap.GetCellCenterWorld(cellPos);

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

    // ת��ָ��λ�õ���Ƭ - ֻ��ԭ������Ƭ���ڵ�λ���ϱ仯
    // �޸�ConvertTileAtPosition�����������֤
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

    // ���ӻ�����
    void OnDrawGizmos()
    {
        if (currentState == SegmentState.Moving || currentState == SegmentState.Placed)
        {
            // ��ʾ�߶�
            Gizmos.color = Color.green;
            Vector3 start = segmentPosition - segmentDirection * segmentLength * 0.5f;
            Vector3 end = segmentPosition + segmentDirection * segmentLength * 0.5f;
            Gizmos.DrawLine(start, end);

            // ��ʾ�ƶ����򣨽����ƶ�״̬�£�
            if (currentState == SegmentState.Moving)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(segmentPosition, moveDirection * 2f);
            }

            // ��ʾ��������
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
                //stateText = "״̬: �ȴ��� - �����ť�����߶�";
                break;
            case SegmentState.Placed:
                //stateText = "״̬: �ѷ��� - �ƶ������ת�߶Σ������Ļ�����ƶ�����";
                break;
            case SegmentState.Moving:
                //stateText = "״̬: �ƶ��� - �߶�����ת����Ƭ";
                break;
            case SegmentState.Finished:
                //stateText = "״̬: ����� - ��ť�ѿ��ã������ť�������߶�";
                break;
        }

        GUI.Label(new Rect(10, 10, 600, 30), stateText, style);

        if (currentState == SegmentState.Moving)
        {
            float progress = Vector3.Distance(segmentPosition, originalPosition) / moveDistance;
            GUI.Label(new Rect(10, 40, 300, 30), $"����: {progress:P0}", style);
        }
    }
}
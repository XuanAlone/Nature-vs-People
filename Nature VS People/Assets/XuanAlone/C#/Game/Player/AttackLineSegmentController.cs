using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackLineSegmentController : MonoBehaviour
{
    [Header("�߶�����")]
    public float segmentLength = 3f;     // �߶γ���
    public float segmentWidth = 0.1f;    // �߶ο�ȣ����Ƿ�Χ��
    public float moveSpeed = 2f;         // �ƶ��ٶ�
    public float moveDistance = 10f;     // �ƶ����루������
    public Color lineColor = Color.red;  // �߶���ɫ
    public float attackDamage = 50f;     // �����˺�ֵ

    [Header("UI����")]
    public UnityEngine.UI.Button createSegmentButton; // �����߶εİ�ť

    private enum SegmentState { Waiting, Placed, Moving, Finished }
    private SegmentState currentState = SegmentState.Waiting;

    private Vector3 segmentPosition;     // �߶�����λ��
    private Vector3 moveDirection;       // �ƶ�����
    private Vector3 segmentDirection;    // �߶η������ƶ�����ֱ��
    private LineRenderer lineRenderer;   // �߶���Ⱦ��
    private Vector3 originalPosition;    // ��ʼλ��

    private HashSet<Enemy> damagedEnemies; // �Ѿ��ܵ��˺��ĵ����б������ظ��˺�

    void Start()
    {
        InitializeLineRenderer();
        damagedEnemies = new HashSet<Enemy>();

        // �󶨰�ť����¼�
        if (createSegmentButton != null)
        {
            createSegmentButton.onClick.AddListener(OnCreateSegmentButtonClicked);
        }
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
                case SegmentState.Placed:
                    // ���ѷ���״̬�µ����ȷ���ƶ�����
                    SetMovementDirection();
                    currentState = SegmentState.Moving;
                    originalPosition = segmentPosition; // ��¼��ʼλ��
                    damagedEnemies.Clear(); // ������˺������б�
                    break;

                case SegmentState.Finished:
                    // ��ɺ������������ò����¿�ʼ������ȴʱ��
                    ResetAndRestartImmediately();
                    break;
            }
        }
    }

    // ��ť����¼�����
    void OnCreateSegmentButtonClicked()
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
    void UpdateButtonInteractivity()
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
        damagedEnemies.Clear();

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

            // ���߶θ�������ĵ�������˺�
            DamageEnemiesUnderSegment();

            // ����Ƿ��ƶ��㹻Զ��ʹ�÷����ľ��룩
            if (Vector3.Distance(segmentPosition, originalPosition) > moveDistance)
            {
                currentState = SegmentState.Finished;
                //Debug.Log("�߶��ƶ���ɣ���ť�ѻָ�����");
            }
        }
    }

    // ���߶θ��������µĵ�������˺�
    void DamageEnemiesUnderSegment()
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

        // ��ȡ���е���
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in allEnemies)
        {
            // ��������Ѿ��ܵ����˺�������
            if (damagedEnemies.Contains(enemy))
                continue;

            // �������Ƿ����߶θ��ǵľ���������
            if (IsEnemyInSegmentArea(enemy.transform.position, corner1, corner2, corner3, corner4))
            {
                // �Ե�������˺�
                enemy.TakeDamage(attackDamage);

                // ��ӵ����˺������б������ظ��˺�
                damagedEnemies.Add(enemy);

                // ��ѡ������Ӿ�����������˸Ч��
                StartCoroutine(FlashEnemy(enemy));
            }
        }
    }

    // �������Ƿ����߶θ��ǵľ���������
    bool IsEnemyInSegmentArea(Vector3 enemyPos, Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4)
    {
        // ʹ�õ�����жϵ��Ƿ���͹�ı�����
        return IsPointInQuadrilateral(enemyPos, c1, c2, c3, c4);
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

    // �����ܵ��˺�ʱ����˸Ч��
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
                //stateText = "״̬: �ƶ��� - �߶����ڹ�������";
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
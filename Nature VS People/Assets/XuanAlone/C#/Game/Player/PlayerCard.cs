using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PlayerCard : MonoBehaviour
{
    [Header("��������")]
    public float health = 50f;
    public float moveSpeed = 5f;
    public float attackPower = 15f;
    public float attackRate = 2f;
    public float detectionRange = 2f;

    [Header("״̬")]
    public CardState currentState = CardState.ReadyToSpawn;

    private Vector3 targetDirection;
    private Enemy currentTarget;
    private float attackTimer = 0f;
    private Button deployButton;
    private Vector3 originalPosition;

    public enum CardState
    {
        ReadyToSpawn,   // ׼������
        Spawned,        // �����������ģ��ȴ�������÷���
        Deployed,       // �Ѳ����ƶ��У�
        Attacking,      // ������
        Dead            // ����
    }

    void Start()
    {
        currentState = CardState.ReadyToSpawn;

        // ���Ҳ���ť
        deployButton = GameObject.Find("DeployButton")?.GetComponent<Button>();
        if (deployButton != null)
        {
            deployButton.onClick.AddListener(OnDeployButtonClicked);
        }
    }

    void Update()
    {
        HandleMouseInput();

        switch (currentState)
        {
            case CardState.Spawned:
                UpdateCardDirectionToMouse();
                break;

            case CardState.Deployed:
                MoveForward();
                CheckForEnemies();
                CheckOutOfBounds();
                break;

            case CardState.Attacking:
                AttackEnemy();
                CheckOutOfBounds();
                break;
        }
    }

    // ����������� - �����߶ο��������߼�
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            switch (currentState)
            {
                case CardState.Spawned:
                    // ��������״̬�µ����ȷ���ƶ����򲢿�ʼ�ƶ�
                    SetMovementDirection();
                    currentState = CardState.Deployed;
                    originalPosition = transform.position; // ��¼��ʼλ��
                    break;

                case CardState.ReadyToSpawn:
                    // ������ƻ�û���ɣ�����ҵ������Ļ��Ҳ��������
                    if (deployButton != null && deployButton.interactable)
                    {
                        OnDeployButtonClicked();
                    }
                    break;
            }
        }
    }

    // �������Ƿ���UI��
    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // �ӿ��ƹ����������������
    public void SpawnAtCenter()
    {
        if (currentState == CardState.ReadyToSpawn)
        {
            transform.position = Vector3.zero;
            currentState = CardState.Spawned;

            // ��ʾ����
            GetComponent<SpriteRenderer>().enabled = true;

            Debug.Log("���������ɣ�������Ļ�����ƶ�����");
        }
    }

    // ���¿��Ʒ����Ը������ - �����߶ο��������߼�
    void UpdateCardDirectionToMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // ��ȡ�ӿ���ָ�����ķ���
        targetDirection = (mouseWorldPos - transform.position).normalized;

        // ���ÿ��Ƴ���
        if (targetDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // �����ƶ����� - �����߶ο��������߼�
    void SetMovementDirection()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // �ƶ������Ǵӿ���ָ�����λ��
        targetDirection = (mouseWorldPos - transform.position).normalized;

        // ȷ���ƶ������ǵ�λ����
        targetDirection.Normalize();

        // �������ճ���
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Debug.Log("�ƶ����������ã����ƿ�ʼ�ƶ�");
    }

    void OnDeployButtonClicked()
    {
        if (currentState == CardState.ReadyToSpawn)
        {
            SpawnAtCenter();
        }
        // �Ƴ�����Spawned״̬�µ����ť������߼�������ͨ�����������
    }

    public void DeployCard()
    {
        // ���������������������������ǰ�ť
        currentState = CardState.Deployed;
    }

    void MoveForward()
    {
        transform.position += targetDirection * moveSpeed * Time.deltaTime;
    }

    void CheckForEnemies()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && enemy != currentTarget)
            {
                currentTarget = enemy;
                currentState = CardState.Attacking;
                break;
            }
        }
    }

    void AttackEnemy()
    {
        if (currentTarget == null)
        {
            currentState = CardState.Deployed;
            return;
        }

        // �������Ƿ��ڹ�����Χ��
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > detectionRange * 1.5f)
        {
            currentState = CardState.Deployed;
            currentTarget = null;
            return;
        }

        // ������ʱ
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / attackRate)
        {
            attackTimer = 0f;
            currentTarget.TakeDamage(attackPower);

            // �������Ƿ�����
            if (currentTarget.health <= 0)
            {
                currentState = CardState.Deployed;
                currentTarget = null;
            }
        }
    }

    void CheckOutOfBounds()
    {
        Camera mainCamera = Camera.main;
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
        currentState = CardState.Dead;
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // ��ʾ�ƶ�����
        if (Application.isPlaying && currentState == CardState.Deployed)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, targetDirection * 2f);
        }
    }
}
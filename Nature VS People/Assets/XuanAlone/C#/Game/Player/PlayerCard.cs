using UnityEngine;
using UnityEngine.UI;
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

    public enum CardState
    {
        ReadyToSpawn,   // ׼�����ɣ���ʾ�ڰ�ť�ϣ�
        Spawned,        // �����������ģ��ȴ�����
        Deployed,       // �Ѳ����ƶ��У�
        Attacking,      // ������
        Dead            // ����
    }

    void Start()
    {
        // ��ʼ״̬Ϊ׼������
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
        switch (currentState)
        {
            case CardState.Spawned:
                HandleAiming();
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

    // �ӿ��ƹ����������������
    public void SpawnAtCenter()
    {
        if (currentState == CardState.ReadyToSpawn)
        {
            transform.position = Vector3.zero;
            currentState = CardState.Spawned;

            // ��ʾ���ƣ����֮ǰ�����صģ�
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    void HandleAiming()
    {
        // �ÿ��Ƴ������
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        targetDirection = (mousePos - transform.position).normalized;

        if (targetDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnDeployButtonClicked()
    {
        if (currentState == CardState.Spawned)
        {
            DeployCard();
        }
        else if (currentState == CardState.ReadyToSpawn)
        {
            SpawnAtCenter();
        }
    }

    public void DeployCard()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        targetDirection = (mousePos - transform.position).normalized;

        currentState = CardState.Deployed;

        // �������ճ���
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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
        if (distanceToTarget > detectionRange * 1.5f) // ��΢����Χ������Ƶ���л�״̬
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
    }
}
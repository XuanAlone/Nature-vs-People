using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerCard : MonoBehaviour
{
    [Header("卡牌属性")]
    public float health = 50f;
    public float moveSpeed = 5f;
    public float attackPower = 15f;
    public float attackRate = 2f;
    public float detectionRange = 2f;

    [Header("状态")]
    public CardState currentState = CardState.ReadyToSpawn;

    private Vector3 targetDirection;
    private Enemy currentTarget;
    private float attackTimer = 0f;
    private Button deployButton;

    public enum CardState
    {
        ReadyToSpawn,   // 准备生成（显示在按钮上）
        Spawned,        // 已生成在中心（等待部署）
        Deployed,       // 已部署（移动中）
        Attacking,      // 攻击中
        Dead            // 死亡
    }

    void Start()
    {
        // 初始状态为准备生成
        currentState = CardState.ReadyToSpawn;

        // 查找部署按钮
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

    // 从卡牌管理器调用这个方法
    public void SpawnAtCenter()
    {
        if (currentState == CardState.ReadyToSpawn)
        {
            transform.position = Vector3.zero;
            currentState = CardState.Spawned;

            // 显示卡牌（如果之前是隐藏的）
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    void HandleAiming()
    {
        // 让卡牌朝向鼠标
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

        // 设置最终朝向
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

        // 检查敌人是否在攻击范围内
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > detectionRange * 1.5f) // 稍微扩大范围，避免频繁切换状态
        {
            currentState = CardState.Deployed;
            currentTarget = null;
            return;
        }

        // 攻击计时
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / attackRate)
        {
            attackTimer = 0f;
            currentTarget.TakeDamage(attackPower);

            // 检查敌人是否死亡
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
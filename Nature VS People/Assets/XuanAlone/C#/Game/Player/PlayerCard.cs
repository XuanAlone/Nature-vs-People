using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    private Vector3 originalPosition;

    public enum CardState
    {
        ReadyToSpawn,   // 准备生成
        Spawned,        // 已生成在中心（等待点击设置方向）
        Deployed,       // 已部署（移动中）
        Attacking,      // 攻击中
        Dead            // 死亡
    }

    void Start()
    {
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

    // 处理鼠标输入 - 仿照线段控制器的逻辑
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            switch (currentState)
            {
                case CardState.Spawned:
                    // 在已生成状态下点击：确定移动方向并开始移动
                    SetMovementDirection();
                    currentState = CardState.Deployed;
                    originalPosition = transform.position; // 记录起始位置
                    break;

                case CardState.ReadyToSpawn:
                    // 如果卡牌还没生成，但玩家点击了屏幕，也尝试生成
                    if (deployButton != null && deployButton.interactable)
                    {
                        OnDeployButtonClicked();
                    }
                    break;
            }
        }
    }

    // 检查鼠标是否在UI上
    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // 从卡牌管理器调用这个方法
    public void SpawnAtCenter()
    {
        if (currentState == CardState.ReadyToSpawn)
        {
            transform.position = Vector3.zero;
            currentState = CardState.Spawned;

            // 显示卡牌
            GetComponent<SpriteRenderer>().enabled = true;

            Debug.Log("卡牌已生成，请点击屏幕设置移动方向");
        }
    }

    // 更新卡牌方向以跟随鼠标 - 仿照线段控制器的逻辑
    void UpdateCardDirectionToMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 获取从卡牌指向鼠标的方向
        targetDirection = (mouseWorldPos - transform.position).normalized;

        // 设置卡牌朝向
        if (targetDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // 设置移动方向 - 仿照线段控制器的逻辑
    void SetMovementDirection()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 移动方向是从卡牌指向鼠标位置
        targetDirection = (mouseWorldPos - transform.position).normalized;

        // 确保移动方向是单位向量
        targetDirection.Normalize();

        // 设置最终朝向
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Debug.Log("移动方向已设置，卡牌开始移动");
    }

    void OnDeployButtonClicked()
    {
        if (currentState == CardState.ReadyToSpawn)
        {
            SpawnAtCenter();
        }
        // 移除了在Spawned状态下点击按钮部署的逻辑，现在通过鼠标点击部署
    }

    public void DeployCard()
    {
        // 这个方法现在由鼠标点击触发，不是按钮
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

        // 检查敌人是否在攻击范围内
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > detectionRange * 1.5f)
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

        // 显示移动方向
        if (Application.isPlaying && currentState == CardState.Deployed)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, targetDirection * 2f);
        }
    }
}
using UnityEngine;
using UnityEngine.AI;

public class Zoombie : MonoBehaviour
{
    [SerializeField] private int HP = 100;                     // Máu của zombie
    [SerializeField] private int attackDamage = 10;            // Sát thương mỗi lần tấn công
    [SerializeField] private float attackRange = 2f;           // Khoảng cách tấn công
    [SerializeField] private float attackCooldown = 1.5f;      // Thời gian chờ giữa 2 đòn tấn công

    private Animator animator;                                 // Dùng để điều khiển animation
    private NavMeshAgent navMeshAgent;                         // Dùng để di chuyển theo player
    private Transform player;                                  // Tham chiếu đến người chơi
    private float lastAttackTime;                              // Lưu thời gian của lần tấn công gần nhất
    private bool isDead = false;                               // Kiểm tra zombie đã chết chưa

    private void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Tìm Player trong scene (gán tag cho nhân vật Player)
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (isDead) return; // Nếu đã chết thì không làm gì nữa

        if (player == null) return;

        // Tính khoảng cách tới player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            // Nếu còn xa → di chuyển tới player
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(player.position);
            animator.SetBool("isWalking", true);
        }
        else
        {
            // Nếu gần đủ để tấn công
            navMeshAgent.isStopped = true;
            animator.SetBool("isWalking", false);

            // Quay mặt về phía player
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 0.2f);

            // Tấn công nếu đủ thời gian hồi
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                animator.SetTrigger("Attack"); // Gọi animation đánh
                lastAttackTime = Time.time;

                // Gây sát thương cho player
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        HP -= damageAmount;

        if (HP <= 0)
        {
            isDead = true;
            animator.SetTrigger("Die");
            navMeshAgent.isStopped = true;
            Destroy(gameObject, 1f); // Xóa xác sau 5 giây
        }
        else
        {
            animator.SetTrigger("DAMAGE");
        }
    }
}

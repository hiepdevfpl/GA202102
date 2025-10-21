using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public event Action<GameObject> onDeath; // event khi enemy chết

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Trigger animation chết
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Gọi event để thông báo spawner
        onDeath?.Invoke(gameObject);

        // Destroy sau 2s để animation chạy xong
        Destroy(gameObject, 2f);
    }
}

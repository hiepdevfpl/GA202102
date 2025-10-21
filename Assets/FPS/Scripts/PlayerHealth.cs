using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player bị tấn công! Máu còn lại: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player đã chết!");
            // Bạn có thể thêm animation hoặc scene chết ở đây
        }
    }
}

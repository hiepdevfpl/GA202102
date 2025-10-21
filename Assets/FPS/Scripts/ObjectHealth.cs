using UnityEngine;

public enum MaterialType
{
    Wood,
    Metal,
    Barrel,
    Skin,
    Stone,
    Wall
}

public class ObjectHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    [Header("Material Type")]
    public MaterialType materialType;

    [Header("Effects & Sounds")]
    public GameObject bulletImpactEffect; // Hiệu ứng đạn trúng
    public GameObject smallExplosionEffect; // Hiệu ứng phá hủy nhỏ

    public AudioSource woodHitSound;
    public AudioSource metalHitSound;
    public AudioSource characterHitSound;
    public AudioSource destructionSound;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Hàm này sẽ được gọi khi vật thể bị bắn
    /// </summary>
    /// <param name="damage">Lượng sát thương</param>
    /// <param name="hitPoint">Vị trí trúng đạn</param>
    /// <param name="hitNormal">Hướng đạn trúng</param>
    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 1️⃣ Tạo hiệu ứng bullet impact
        if (bulletImpactEffect != null)
        {
            GameObject effect = Instantiate(bulletImpactEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(effect, 1f); // tự hủy sau 1 giây
        }

        // 2️⃣ Giảm HP
        currentHealth -= damage;

        // 3️⃣ Phát âm thanh theo vật liệu
        switch (materialType)
        {
            case MaterialType.Wood:
                if (woodHitSound != null) woodHitSound.Play();
                break;
            case MaterialType.Metal:
                if (metalHitSound != null) metalHitSound.Play();
                break;
            case MaterialType.Skin:
                if (characterHitSound != null) characterHitSound.Play();
                break;
        }

        // 4️⃣ Kiểm tra HP để phá hủy
        if (currentHealth <= 0)
        {
            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        // Phát âm thanh phá hủy
        if (destructionSound != null) destructionSound.Play();

        // Hiệu ứng phá hủy nhỏ
        if (smallExplosionEffect != null)
        {
            Instantiate(smallExplosionEffect, transform.position, transform.rotation);
        }

        // Xóa vật thể
        Destroy(gameObject);
    }
}

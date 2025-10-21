using System.Collections.Generic;
using UnityEngine;

public class WeaponDamageTable : MonoBehaviour
{
    // Bảng lưu trữ lượng sát thương của từng loại vũ khí đối với từng loại vật liệu
    public Dictionary<WeaponType, Dictionary<MaterialType, int>> damageTable = new Dictionary<WeaponType, Dictionary<MaterialType, int>>();

    private void Start()
    {
        // Các giá trị ví dụ về sát thương cho từng sự kết hợp giữa loại vũ khí và loại vật liệu
        // Khóa của từ điển đầu tiên là loại vũ khí, còn từ điển thứ hai chứa giá trị sát thương cho từng loại vật liệu
        damageTable[WeaponType.Semi] = new Dictionary<MaterialType, int>
        {
            { MaterialType.Wood, 5 },   // Sát thương khi bắn vào gỗ
            { MaterialType.Metal, 5 },  // Sát thương khi bắn vào kim loại
            { MaterialType.Barrel, 8 }, // Sát thương khi bắn vào thùng
            { MaterialType.Skin, 10 },  // Sát thương khi bắn vào da (kẻ địch)
            { MaterialType.Stone, 10 }  // Sát thương khi bắn vào đá
        };

        damageTable[WeaponType.Auto] = new Dictionary<MaterialType, int>
        {
            { MaterialType.Wood, 3 },   // Sát thương khi bắn vào gỗ
            { MaterialType.Metal, 3 },  // Sát thương khi bắn vào kim loại
            { MaterialType.Barrel, 4 }, // Sát thương khi bắn vào thùng
            { MaterialType.Skin, 6 },   // Sát thương khi bắn vào da
            { MaterialType.Stone, 4 }   // Sát thương khi bắn vào đá
        };

        damageTable[WeaponType.Laser] = new Dictionary<MaterialType, int>
        {
            { MaterialType.Wood, 99 },   // Sát thương laser vào gỗ
            { MaterialType.Metal, 99 },  // Sát thương laser vào kim loại
            { MaterialType.Barrel, 33 }, // Sát thương laser vào thùng
            { MaterialType.Skin, 99 },   // Sát thương laser vào da
            { MaterialType.Stone, 33 }   // Sát thương laser vào đá
        };
    }

    // Hàm lấy giá trị sát thương dựa trên loại vũ khí và loại vật liệu
    public int GetDamage(WeaponType weaponType, MaterialType materialType)
    {
        // Kiểm tra xem bảng có chứa loại vũ khí và vật liệu này không
        if (damageTable.ContainsKey(weaponType) && damageTable[weaponType].ContainsKey(materialType))
        {
            return damageTable[weaponType][materialType]; // Trả về giá trị sát thương tương ứng
        }

        return 0; // Nếu không tìm thấy, trả về 0 (không gây sát thương)
    }
}

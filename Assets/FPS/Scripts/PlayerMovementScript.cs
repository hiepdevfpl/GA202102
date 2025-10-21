using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementScript : MonoBehaviour
{
    Rigidbody rb;

    [Tooltip("Tốc độ hiện tại của người chơi")]
    public float currentSpeed;
    [Tooltip("Gán camera của người chơi tại đây")]
    [HideInInspector] public Transform cameraMain;
    [Tooltip("Lực nhảy của người chơi")]
    public float jumpForce = 500;
    [Tooltip("Vị trí của camera bên trong người chơi")]
    [HideInInspector] public Vector3 cameraPosition;

    /*
	 * Lấy thành phần Rigidbody của người chơi.
	 * Và tìm camera chính (Main Camera) từ con của Player.
	 */
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cameraMain = transform.Find("Main Camera").transform;
        bulletSpawn = cameraMain.Find("BulletSpawn").transform;
        ignoreLayer = 1 << LayerMask.NameToLayer("Player");
    }

    private Vector3 slowdownV;
    private Vector2 horizontalMovement;

    /*
	 * Thực hiện raycast cho đòn cận chiến (melee)
	 * và xử lý di chuyển của người chơi trong FixedUpdate.
	 */
    void FixedUpdate()
    {
        RaycastForMeleeAttacks();
        PlayerMovementLogic();
    }

    /*
	 * Thêm lực di chuyển theo input.
	 * Nếu tốc độ lớn hơn maxSpeed thì giới hạn lại.
	 * Khi không nhấn phím thì sẽ giảm tốc dần.
	 */
    void PlayerMovementLogic()
    {
        currentSpeed = rb.linearVelocity.magnitude;
        horizontalMovement = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);

        if (horizontalMovement.magnitude > maxSpeed)
        {
            horizontalMovement = horizontalMovement.normalized;
            horizontalMovement *= maxSpeed;
        }

        rb.linearVelocity = new Vector3(
            horizontalMovement.x,
            rb.linearVelocity.y,
            horizontalMovement.y
        );

        if (grounded)
        {
            rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity,
                new Vector3(0, rb.linearVelocity.y, 0),
                ref slowdownV,
                deaccelerationSpeed);
        }

        if (grounded)
        {
            // Di chuyển bình thường khi đang đứng trên mặt đất
            rb.AddRelativeForce(Input.GetAxis("Horizontal") * accelerationSpeed * Time.deltaTime, 0, Input.GetAxis("Vertical") * accelerationSpeed * Time.deltaTime);
        }
        else
        {
            // Giảm lực di chuyển khi đang trên không
            rb.AddRelativeForce(Input.GetAxis("Horizontal") * accelerationSpeed / 2 * Time.deltaTime, 0, Input.GetAxis("Vertical") * accelerationSpeed / 2 * Time.deltaTime);
        }

        /*
		 * Giảm độ trơn trượt khi đứng yên
		 */
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            deaccelerationSpeed = 0.5f;
        }
        else
        {
            deaccelerationSpeed = 0.1f;
        }
    }

    /*
	 * Xử lý nhảy: nếu nhấn phím cách và đang trên mặt đất thì thêm lực nhảy.
	 * Phát âm thanh nhảy, tắt âm thanh đi bộ/chạy.
	 */
    void Jumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddRelativeForce(Vector3.up * jumpForce);
            if (_jumpSound)
                _jumpSound.Play();
            else
                print("Thiếu âm thanh nhảy (jump sound).");
            _walkSound.Stop();
            _runSound.Stop();
        }
    }

    /*
	 * Vòng lặp Update – gọi các hành vi chính mỗi khung hình.
	 */
    void Update()
    {
        Jumping();
        Crouching();
        WalkingSound();
    }

    /*
	 * Kiểm tra nếu người chơi đang trên mặt đất và phát âm thanh đi bộ/chạy
	 * dựa vào tốc độ hiện tại.
	 */
    void WalkingSound()
    {
        if (_walkSound && _runSound)
        {
            if (RayCastGrounded())
            { // Dùng raycast vì mặt đất có thể không phẳng
                if (currentSpeed > 1)
                {
                    if (maxSpeed == 3)
                    {
                        if (!_walkSound.isPlaying)
                        {
                            _walkSound.Play();
                            _runSound.Stop();
                        }
                    }
                    else if (maxSpeed == 5)
                    {
                        if (!_runSound.isPlaying)
                        {
                            _walkSound.Stop();
                            _runSound.Play();
                        }
                    }
                }
                else
                {
                    _walkSound.Stop();
                    _runSound.Stop();
                }
            }
            else
            {
                _walkSound.Stop();
                _runSound.Stop();
            }
        }
        else
        {
            print("Thiếu âm thanh đi bộ hoặc chạy.");
        }
    }

    /*
	 * Raycast hướng xuống để kiểm tra người chơi có đang đứng trên mặt đất hay không.
	 * Dùng để chắc chắn rằng nhân vật không bị “ON/OFF” liên tục khi đứng trên bề mặt gồ ghề.
	 */
    private bool RayCastGrounded()
    {
        RaycastHit groundedInfo;
        if (Physics.Raycast(transform.position, transform.up * -1f, out groundedInfo, 1, ~ignoreLayer))
        {
            Debug.DrawRay(transform.position, transform.up * -1f, Color.red, 0.0f);
            if (groundedInfo.transform != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    /*
	 * Cúi người (Crouch) khi nhấn phím C – thu nhỏ chiều cao người chơi.
	 */
    void Crouching()
    {
        if (Input.GetKey(KeyCode.C))
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 0.6f, 1), Time.deltaTime * 15);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 1, 1), Time.deltaTime * 15);
        }
    }

    [Tooltip("Tốc độ tối đa của người chơi")]
    public int maxSpeed = 5;
    [Tooltip("Giá trị càng lớn thì dừng lại càng nhanh")]
    public float deaccelerationSpeed = 15.0f;

    [Tooltip("Lực di chuyển khi đi tới hoặc lùi lại")]
    public float accelerationSpeed = 50000.0f;

    [Tooltip("Kiểm tra xem người chơi có đang đứng trên mặt đất hay không")]
    public bool grounded;

    /*
	 * Kiểm tra khi người chơi va chạm với bề mặt có góc nghiêng nhỏ hơn 60 độ → đang đứng trên mặt đất.
	 */
    void OnCollisionStay(Collision other)
    {
        foreach (ContactPoint contact in other.contacts)
        {
            if (Vector2.Angle(contact.normal, Vector3.up) < 60)
            {
                grounded = true;
            }
        }
    }

    /*
	 * Khi rời khỏi va chạm thì đặt grounded = false.
	 */
    void OnCollisionExit()
    {
        grounded = false;
    }

    // --- PHẦN CẬN CHIẾN (MELEE ATTACK) ---

    RaycastHit hitInfo;
    private float meleeAttack_cooldown;
    private string currentWeapo;
    [Tooltip("Thêm Layer 'Player' tại đây")]
    [Header("Thuộc tính bắn (Shooting Properties)")]
    private LayerMask ignoreLayer;
    Ray ray1, ray2, ray3, ray4, ray5, ray6, ray7, ray8, ray9;
    private float rayDetectorMeeleSpace = 0.15f;
    private float offsetStart = 0.05f;
    [Tooltip("Gán đối tượng BulletSpawn tại đây – vị trí tạo đạn.")]
    [HideInInspector]
    public Transform bulletSpawn;

    /*
	 * Phát ra 9 tia ray theo các hướng khác nhau (có thể xem trong Scene).
	 * Dùng để tăng độ chính xác khi tấn công cận chiến.
	 * Kiểm tra thời gian hồi chiêu (cooldown) cho đòn đánh cận chiến.
	 */
    public bool been_to_meele_anim = false;
    private void RaycastForMeleeAttacks()
    {
        if (meleeAttack_cooldown > -5)
        {
            meleeAttack_cooldown -= 1 * Time.deltaTime;
        }

        if (GetComponent<GunInventory>().currentGun)
        {
            if (GetComponent<GunInventory>().currentGun.GetComponent<GunScript>())
                currentWeapo = "gun";
        }

        // Tạo 9 tia ray theo các hướng khác nhau để kiểm tra va chạm
        ray1 = new Ray(bulletSpawn.position + (bulletSpawn.right * offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace));
        ray2 = new Ray(bulletSpawn.position - (bulletSpawn.right * offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace));
        ray3 = new Ray(bulletSpawn.position, bulletSpawn.forward);
        ray4 = new Ray(bulletSpawn.position + (bulletSpawn.right * offsetStart) + (bulletSpawn.up * offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace) + (bulletSpawn.up * rayDetectorMeeleSpace));
        ray5 = new Ray(bulletSpawn.position - (bulletSpawn.right * offsetStart) + (bulletSpawn.up * offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace) + (bulletSpawn.up * rayDetectorMeeleSpace));
        ray6 = new Ray(bulletSpawn.position + (bulletSpawn.up * offsetStart), bulletSpawn.forward + (bulletSpawn.up * rayDetectorMeeleSpace));
        ray7 = new Ray(bulletSpawn.position + (bulletSpawn.right * offsetStart) - (bulletSpawn.up * offsetStart), bulletSpawn.forward + (bulletSpawn.right * rayDetectorMeeleSpace) - (bulletSpawn.up * rayDetectorMeeleSpace));
        ray8 = new Ray(bulletSpawn.position - (bulletSpawn.right * offsetStart) - (bulletSpawn.up * offsetStart), bulletSpawn.forward - (bulletSpawn.right * rayDetectorMeeleSpace) - (bulletSpawn.up * rayDetectorMeeleSpace));
        ray9 = new Ray(bulletSpawn.position - (bulletSpawn.up * offsetStart), bulletSpawn.forward - (bulletSpawn.up * rayDetectorMeeleSpace));

        Debug.DrawRay(ray1.origin, ray1.direction, Color.cyan);
        Debug.DrawRay(ray2.origin, ray2.direction, Color.cyan);
        Debug.DrawRay(ray3.origin, ray3.direction, Color.cyan);
        Debug.DrawRay(ray4.origin, ray4.direction, Color.red);
        Debug.DrawRay(ray5.origin, ray5.direction, Color.red);
        Debug.DrawRay(ray6.origin, ray6.direction, Color.red);
        Debug.DrawRay(ray7.origin, ray7.direction, Color.yellow);
        Debug.DrawRay(ray8.origin, ray8.direction, Color.yellow);
        Debug.DrawRay(ray9.origin, ray9.direction, Color.yellow);

        if (GetComponent<GunInventory>().currentGun)
        {
            if (GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().meeleAttack == false)
            {
                been_to_meele_anim = false;
            }
            if (GetComponent<GunInventory>().currentGun.GetComponent<GunScript>().meeleAttack == true && been_to_meele_anim == false)
            {
                been_to_meele_anim = true;
                StartCoroutine("MeeleAttackWeaponHit");
            }
        }
    }

    /*
	 * Khi đòn tấn công cận chiến được kích hoạt,
	 * sẽ kiểm tra va chạm và gây sát thương nếu trúng mục tiêu.
	 */
    IEnumerator MeeleAttackWeaponHit()
    {
        if (Physics.Raycast(ray1, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray2, out hitInfo, 2f, ~ignoreLayer) ||
            Physics.Raycast(ray3, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray4, out hitInfo, 2f, ~ignoreLayer) ||
            Physics.Raycast(ray5, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray6, out hitInfo, 2f, ~ignoreLayer) ||
            Physics.Raycast(ray7, out hitInfo, 2f, ~ignoreLayer) || Physics.Raycast(ray8, out hitInfo, 2f, ~ignoreLayer) ||
            Physics.Raycast(ray9, out hitInfo, 2f, ~ignoreLayer))
        {

            if (hitInfo.transform.tag == "Dummie")
            {
                Transform _other = hitInfo.transform.root.transform;
                if (_other.transform.tag == "Dummie")
                {
                    print("Đã đánh trúng mục tiêu Dummie");
                }
                InstantiateBlood(hitInfo, false);
            }
        }
        yield return new WaitForEndOfFrame();
    }

    [Header("Hiệu ứng máu khi tấn công cận chiến")]
    RaycastHit hit;
    [Tooltip("Gán prefab hiệu ứng máu tại đây")]
    public GameObject bloodEffect;

    /*
	 * Khi trúng kẻ địch, tạo hiệu ứng máu tại vị trí va chạm.
	 */
    void InstantiateBlood(RaycastHit _hitPos, bool swordHitWithGunOrNot)
    {
        if (currentWeapo == "gun")
        {
            GunScript.HitMarkerSound();
            if (_hitSound)
                _hitSound.Play();
            else
                print("Thiếu âm thanh trúng đạn.");

            if (!swordHitWithGunOrNot)
            {
                if (bloodEffect)
                    Instantiate(bloodEffect, _hitPos.point, Quaternion.identity);
                else
                    print("Thiếu prefab hiệu ứng máu trong Inspector.");
            }
        }
    }

    private GameObject myBloodEffect;

    [Header("ÂM THANH CỦA NGƯỜI CHƠI")]
    [Tooltip("Âm thanh khi nhảy")]
    public AudioSource _jumpSound;
    [Tooltip("Âm thanh khi nạp đạn thành công")]
    public AudioSource _freakingZombiesSound;
    [Tooltip("Âm thanh khi đạn trúng mục tiêu")]
    public AudioSource _hitSound;
    [Tooltip("Âm thanh đi bộ")]
    public AudioSource _walkSound;
    [Tooltip("Âm thanh chạy")]
    public AudioSource _runSound;
}

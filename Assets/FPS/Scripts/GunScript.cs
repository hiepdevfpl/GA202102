using UnityEngine;
using System.Collections;
//using UnityStandardAssets.ImageEffects;

public enum GunStyles
{
    nonautomatic, automatic
}

public enum WeaponType
{
    Semi,
    Auto,
    Laser
}

public class GunScript : MonoBehaviour
{
    public WeaponDamageTable damageTable;
    public WeaponType currentWeaponType;

    private int damage; // Lượng sát thương do súng gây ra

    [Tooltip("Chọn kiểu bắn: bắn nhanh liên tục hoặc bắn từng viên.")]
    public GunStyles currentStyle;

    [HideInInspector]
    public MouseLookScript mls;

    [Header("Thuộc tính di chuyển của người chơi")]
    [Tooltip("Tốc độ đi bộ, mỗi súng có trọng lượng khác nhau nên tốc độ có thể khác nhau.")]
    public int walkingSpeed = 3;
    [Tooltip("Tốc độ chạy, mỗi súng có trọng lượng khác nhau nên tốc độ có thể khác nhau.")]
    public int runningSpeed = 5;

    [Header("Thuộc tính đạn")]
    [Tooltip("Số đạn sẵn có khi bắt đầu.")]
    public float bulletsIHave = 20;
    [Tooltip("Số đạn hiện tại trong súng.")]
    public float bulletsInTheGun = 5;
    [Tooltip("Số đạn tối đa trong mỗi băng đạn.")]
    public float amountOfBulletsPerLoad = 5;

    private Transform player;
    private Camera cameraComponent;
    private Transform gunPlaceHolder;

    private PlayerMovementScript pmS;

    /*
     * Thu thập các biến cần thiết khi Awake.
     */
    void Awake()
    {
        mls = GameObject.FindGameObjectWithTag("Player").GetComponent<MouseLookScript>();
        player = mls.transform;
        mainCamera = mls.myCamera;
        secondCamera = GameObject.FindGameObjectWithTag("SecondCamera").GetComponent<Camera>();
        cameraComponent = mainCamera.GetComponent<Camera>();
        pmS = player.GetComponent<PlayerMovementScript>();

        bulletSpawnPlace = GameObject.FindGameObjectWithTag("BulletSpawn");
        hitMarker = transform.Find("hitMarkerSound").GetComponent<AudioSource>();

        startLook = mouseSensitvity_notAiming;
        startAim = mouseSensitvity_aiming;
        startRun = mouseSensitvity_running;

        rotationLastY = mls.currentYRotation;
        rotationLastX = mls.currentCameraXRotation;
    }

    [HideInInspector]
    public Vector3 currentGunPosition;

    [Header("Vị trí súng")]
    [Tooltip("Vị trí súng khi không ngắm.")]
    public Vector3 restPlacePosition;
    [Tooltip("Vị trí súng khi ngắm.")]
    public Vector3 aimPlacePosition;
    [Tooltip("Thời gian để súng chuyển sang tư thế ngắm.")]
    public float gunAimTime = 0.1f;

    [HideInInspector]
    public bool reloading;

    private Vector3 gunPosVelocity;
    private float cameraZoomVelocity;
    private float secondCameraZoomVelocity;

    private Vector2 gunFollowTimeVelocity;

    /*
     * Vòng lặp Update gọi các phương thức xử lý chuyển động, bắn súng, cận chiến, sprint,...
     */
    void Update()
    {
        Animations();
        GiveCameraScriptMySensitvity();
        PositionGun();
        Shooting();
        MeeleAttack();
        LockCameraWhileMelee();
        Sprint(); // Nếu có súng thì gọi từ đây, nếu không có súng thì gọi từ movement script
        CrossHairExpansionWhenWalking();
    }

    /*
     * Vòng lặp FixedUpdate tính toán vị trí và quay súng, xử lý recoil và zoom khi ngắm.
     */
    void FixedUpdate()
    {
        RotationGun();
        MeeleAnimationsStates();

        // Nếu đang ngắm
        if (Input.GetAxis("Fire2") != 0 && !reloading && !meeleAttack)
        {
            gunPrecision = gunPrecision_aiming;
            recoilAmount_x = recoilAmount_x_;
            recoilAmount_y = recoilAmount_y_;
            recoilAmount_z = recoilAmount_z_;
            currentGunPosition = Vector3.SmoothDamp(currentGunPosition, aimPlacePosition, ref gunPosVelocity, gunAimTime);
            cameraComponent.fieldOfView = Mathf.SmoothDamp(cameraComponent.fieldOfView, cameraZoomRatio_aiming, ref cameraZoomVelocity, gunAimTime);
            secondCamera.fieldOfView = Mathf.SmoothDamp(secondCamera.fieldOfView, secondCameraZoomRatio_aiming, ref secondCameraZoomVelocity, gunAimTime);
        }
        // Nếu không ngắm
        else
        {
            gunPrecision = gunPrecision_notAiming;
            recoilAmount_x = recoilAmount_x_non;
            recoilAmount_y = recoilAmount_y_non;
            recoilAmount_z = recoilAmount_z_non;
            currentGunPosition = Vector3.SmoothDamp(currentGunPosition, restPlacePosition, ref gunPosVelocity, gunAimTime);
            cameraComponent.fieldOfView = Mathf.SmoothDamp(cameraComponent.fieldOfView, cameraZoomRatio_notAiming, ref cameraZoomVelocity, gunAimTime);
            secondCamera.fieldOfView = Mathf.SmoothDamp(secondCamera.fieldOfView, secondCameraZoomRatio_notAiming, ref secondCameraZoomVelocity, gunAimTime);
        }
    }

    [Header("Độ nhạy chuột")]
    [Tooltip("Độ nhạy khi không ngắm.")]
    public float mouseSensitvity_notAiming = 10;
    [Tooltip("Độ nhạy khi ngắm.")]
    public float mouseSensitvity_aiming = 5;
    [Tooltip("Độ nhạy khi chạy.")]
    public float mouseSensitvity_running = 4;

    /*
     * Gán độ nhạy cho Camera chính theo từng súng.
     */
    void GiveCameraScriptMySensitvity()
    {
        mls.mouseSensitvity_notAiming = mouseSensitvity_notAiming;
        mls.mouseSensitvity_aiming = mouseSensitvity_aiming;
    }

    /*
     * Mở rộng crosshair khi di chuyển hoặc ẩn khi chạy.
     */
    void CrossHairExpansionWhenWalking()
    {
        if (player.GetComponent<Rigidbody>().linearVelocity.magnitude > 1 && Input.GetAxis("Fire1") == 0)
        { // Nếu không bắn
            expandValues_crosshair += new Vector2(20, 40) * Time.deltaTime;
            if (player.GetComponent<PlayerMovementScript>().maxSpeed < runningSpeed)
            { // Nếu đi bộ
                expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 10), Mathf.Clamp(expandValues_crosshair.y, 0, 20));
                fadeout_value = Mathf.Lerp(fadeout_value, 1, Time.deltaTime * 2);
            }
            else
            { // Nếu chạy
                fadeout_value = Mathf.Lerp(fadeout_value, 0, Time.deltaTime * 10);
                expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 20), Mathf.Clamp(expandValues_crosshair.y, 0, 40));
            }
        }
        else
        { // Nếu đang bắn
            expandValues_crosshair = Vector2.Lerp(expandValues_crosshair, Vector2.zero, Time.deltaTime * 5);
            expandValues_crosshair = new Vector2(Mathf.Clamp(expandValues_crosshair.x, 0, 10), Mathf.Clamp(expandValues_crosshair.y, 0, 20));
            fadeout_value = Mathf.Lerp(fadeout_value, 1, Time.deltaTime * 2);
        }
    }

    /* 
     * Thay đổi tốc độ tối đa của người chơi và trigger animation chạy.
     */
    void Sprint()
    {
        if (Input.GetAxis("Vertical") > 0 && Input.GetAxisRaw("Fire2") == 0 && meeleAttack == false && Input.GetAxisRaw("Fire1") == 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (pmS.maxSpeed == walkingSpeed)
                {
                    pmS.maxSpeed = runningSpeed; // Tăng tốc độ di chuyển
                }
                else
                {
                    pmS.maxSpeed = walkingSpeed;
                }
            }
        }
        else
        {
            pmS.maxSpeed = walkingSpeed;
        }
    }

    [HideInInspector]
    public bool meeleAttack;
    [HideInInspector]
    public bool aiming;

    /*
     * Kiểm tra trạng thái cận chiến.
     */
    void MeeleAnimationsStates()
    {
        if (handsAnimator)
        {
            meeleAttack = handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(meeleAnimationName);
            aiming = handsAnimator.GetCurrentAnimatorStateInfo(0).IsName(aimingAnimationName);
        }
    }

    /*
     * Nhấn Q để thực hiện tấn công cận chiến.
     */
    void MeeleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !meeleAttack)
        {
            StartCoroutine("AnimationMeeleAttack");
        }
    }

    /*
     * Coroutine thực hiện animation cận chiến.
     */
    IEnumerator AnimationMeeleAttack()
    {
        handsAnimator.SetBool("meeleAttack", true);
        yield return new WaitForSeconds(0.1f);
        handsAnimator.SetBool("meeleAttack", false);
    }

    private float startLook, startAim, startRun;

    /*
     * Khóa chuột khi đang tấn công cận chiến, giảm độ nhạy chuột.
     */
    void LockCameraWhileMelee()
    {
        if (meeleAttack)
        {
            mouseSensitvity_notAiming = 2;
            mouseSensitvity_aiming = 1.6f;
            mouseSensitvity_running = 1;
        }
        else
        {
            mouseSensitvity_notAiming = startLook;
            mouseSensitvity_aiming = startAim;
            mouseSensitvity_running = startRun;
        }
    }

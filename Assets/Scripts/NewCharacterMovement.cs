using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(Rigidbody))]
public class NewCharacterMovement : MonoBehaviour
{

    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float verticalMinAngle = -90f, verticalMaxAngle = 90f;
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private GameObject cameraLookAt;

    [SerializeField]
    private ParticleSystem particleSystemShoot;

    private List<Enemy> enemyList = new List<Enemy>();

    public List<Enemy> enemyListRef { get { return enemyList; } }

    private float shootTimer;

    [SerializeField]
    public float spawnRadius;

    [SerializeField]
    private Enemy manequinPrefab = null;

    [SerializeField]
    public Cinemachine.CinemachineVirtualCamera executeCamera;

    [SerializeField]
    private Animator characterAnimator;

    public bool isExecuting = false;

    public float MaxMovingSpeed = 0;
    public float MovingSpeed = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        MovingSpeed = characterAnimator.GetFloat("MoveY");
    }

    private void Update()
    {

        MovingSpeed = characterAnimator.GetFloat("MoveY");

        if (isExecuting)
            return;

        shootTimer += Time.deltaTime * 7f;
        if (Input.GetMouseButton(0))
        {
            if (shootTimer >= 1f)
            {
                Shoot();
                shootTimer = 0f;
            }
        }

        //transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * Time.deltaTime * 500f, 0f));
        Vector3 rot = transform.localRotation.eulerAngles;
        rot.y += Input.GetAxis("Mouse X") * 500f * Time.fixedDeltaTime;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(rot), 1f);

        rot = cameraLookAt.transform.localRotation.eulerAngles;
        rot.x += Input.GetAxis("Mouse Y") * 500f * Time.fixedDeltaTime;

        if (rot.x > 180f)
        {
            rot.x -= 360f;
        }

        rot.x = Mathf.Clamp(rot.x, verticalMinAngle, verticalMaxAngle);
        cameraLookAt.transform.localRotation = Quaternion.Slerp(cameraLookAt.transform.localRotation, Quaternion.Euler(rot), 1f);
        rb.velocity = transform.rotation * Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), 1f) * moveSpeed;

        MaxMovingSpeed = 0.5f;
        //if (characterAnimator.GetFloat("MoveY") < 0.5)
        //{
        //    characterAnimator.SetFloat("MoveY", Input.GetAxis("Vertical"));
        //}
        characterAnimator.SetFloat("MoveY", Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.LeftShift))
        {
            characterAnimator.SetFloat("MoveY", 2f);
        }
        
            
               
        


        
    }
    // Update is called once per frame
    private void LateUpdate()
    {
        if (isExecuting)
        {
            return;
        }

        Enemy enemy = GetClosestStunnedEnemy();
        if (enemy != null)
        {
            enemy.ActiveWorldCanvas();
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(ExecuteCoroutine(enemy));
            }

        }
    }

    private void Shoot()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Player"))))
        {
            ParticleSystem ps = Instantiate(particleSystemShoot, hit.point, Quaternion.LookRotation(hit.normal));
        }


    }

    private Enemy GetClosestStunnedEnemy()
    {
        //Methode 1
        //IEnumerable<Enemy> enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        // enemies = enemies.Where(o => Vector3.Distance(o.transform.position, transform.forward) <= 2f).ToArray();

        //Method 2
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
        IEnumerable<Enemy> /* OR Enemy[]*/ enemies = colliders.Where(o => o.GetComponent<Enemy>()).Select(o => o.GetComponent<Enemy>())/* OR .ToArray()*/;
        //if (enemies.Count() /* OR Length*/ == 0) return null;

        return enemies.Where(o => o.isStunned == true).OrderBy(o => Vector3.Distance(o.transform.position, transform.position)).FirstOrDefault();
    }

    private IEnumerator ExecuteCoroutine(Enemy enemy)
    {
        isExecuting = true;
        executeCamera.Follow = enemy.transform;
        executeCamera.LookAt = enemy.transform;
        executeCamera.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        executeCamera.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        Destroy(enemy.gameObject);
        isExecuting = false;

    }
}

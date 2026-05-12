using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

public class CenterRaycastShooter : MonoBehaviour
{
    [Header("Raycast")]
    //카메라 (화면)의 중점에서 Ray 쏘고자 함-> 카메라를 알아야함
    [SerializeField]
    private Camera m_cam;
    //Ray 에 필터링될 영역 (LayerMask)
    [SerializeField]
    private LayerMask m_hittableMask;
    //Ray-> 최대거리
    [SerializeField]
    private float m_maxDistance = 100.0f;

    private PlayerInput _pi;
    private InputAction _fire;


    private void Awake()
    {
        _pi = GetComponent<PlayerInput>();
        _fire = _pi.actions.FindAction("Fire", true);

        if (m_cam == null) m_cam = Camera.main;
    }

    private void OnEnable()
    {
        _fire.performed += OnRayFire;
    }

    private void OnDisable()
    {
        _fire.performed -= OnRayFire;
    }


    //마우스 왼쪽 '클릭' _fire -> 등록 (bind)
    private void OnRayFire(InputAction.CallbackContext _)
    {
        // 화면중앙(카메라) 
        Vector2 _screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);

        // Ray 클래스 -? 
        Ray _ray = m_cam.ScreenPointToRay(_screenCenter);


        //QueryTriggerInteraction : 트리거 Collider는 무시
        if (Physics.Raycast(_ray, out RaycastHit hit, m_maxDistance, m_hittableMask))
        {
            // Ray 가 어떤 오브젝트에 맞았을 때 로그 출력 
            Debug.Log($"[CenterRaycastShooter] Hit {hit.collider.name} at {hit.point}");


            // Renderer rend = hit.collider.GetComponent<Renderer>();
            // if (rend) rend.material.color = Color.red;

            Debug.DrawLine(_ray.origin, hit.point, Color.green, 1.0f);
        }
        else
        {
            Debug.DrawLine(_ray.origin, _ray.direction * m_maxDistance, Color.yellow, 0.5f);
        }

        SphereCastExample();
    }


    void SphereCastExample()
    {
        float radius = 2.0f;// 구체 반지름 
        float maxDistance = 10.0f;

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, maxDistance, m_hittableMask))
        {
            Debug.Log($"Sphere Hit {hit.collider.name}");
        }
    }


    //즉발 장판기 
    //결과값을, Collider를 복수개 배열로 반환 
    //private Collider[];

    void OverlapExample(Vector3 centerPostion)
    {
        Vector3 center = centerPostion;
        float radius = 5.0f;

        Collider[] hitColliders = Physics.OverlapSphere(center, radius);

        foreach (var hitCollider in hitColliders)
        {
            Debug.Log($"Detected : {hitCollider.name}");
        }
    }

    //성능 최적화 - Overlap (방어코드 개념)
    private Collider[] results = new Collider[10];

    void OptimizedOverlap()
    {
        //Allocation (할당--> 메모리 할당)
        //Alloc --> 줄임단어 -> 표준어   

        int count = Physics.OverlapSphereNonAlloc(transform.position, 5.0f, results);

        for (int i = 0; i < count; i++)
        {
            Debug.Log($"NonAlloc Hit : {results[i].name}");
        }
    }

}

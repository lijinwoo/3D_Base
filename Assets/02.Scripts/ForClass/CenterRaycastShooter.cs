using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CenterRaycastShooter : MonoBehaviour
{
    private Camera m_cam;
    [SerializeField]
    private LayerMask m_hittableMask;
    private float m_maxDistance = 100f;

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

    private void OnRayFire(InputAction.CallbackContext _)
    {
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray ray = m_cam.ScreenPointToRay(screenCenter);


        if (Physics.Raycast(ray, out var hit, m_maxDistance, m_hittableMask, QueryTriggerInteraction.Ignore))
        {
            var rend = hit.collider.GetComponent<Renderer>();
            if (rend) rend.material.color = Color.red;

            Debug.DrawLine(ray.origin, hit.point, Color.green, 1f);
        }
        else
        {
            Debug.DrawLine(ray.origin,ray.direction * m_maxDistance, Color.yellow, 0.5f);
        }

    }

}

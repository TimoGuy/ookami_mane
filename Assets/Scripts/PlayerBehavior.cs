using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
    [Header("Lock cursor (F1 to toggle)")]
    public bool m_lock_cursor = true;
    private bool m_prev_lock_cursor;

    [Header("Controls")]
    [SerializeField]
    private InputActionReference m_action_move;
    [SerializeField]
    private InputActionReference m_action_look;
    [SerializeField]
    private InputActionReference m_action_attack;
    [SerializeField]
    private InputActionReference m_action_guard;

    [Header("Camera props")]
    public float m_cam_sensitivity_x = 1.0f;
    public float m_cam_sensitivity_y = 1.0f;
    public float m_cam_distance = 5.0f;
    public float m_cam_focus_offset_y = 1.0f;
    private float m_cam_orbit_x = 0.0f;
    private float m_cam_orbit_y = 0.0f;
    public float m_cam_raycast_extra_distance = 0.5f;
    public LayerMask m_cam_raycast_layermask;

    [Header("Movement props")]
    public float m_move_speed = 5.0f;
    public float m_guarding_move_speed = 0.75f;
    public float m_knockback_speed = 4.0f;

    public CharacterController m_char_con;
    public Rigidbody m_char_model;
    public AnimatorStateHandler m_char_animator;

    private Transform m_main_camera;
    private Vector3 m_prev_cc_pos;


    void Awake()
    {
        m_main_camera = Camera.main.transform;
    }

    void Start()
    {   // Force first tick to process the cursor locking.
        m_prev_lock_cursor = !m_lock_cursor;
    }

    void Update()
    {   // Move camera.
        var look_axes = m_action_look.action.ReadValue<Vector2>();
        update_camera(new Vector2(look_axes.x * m_cam_sensitivity_x,
                                  -look_axes.y * m_cam_sensitivity_y));

        bool is_currently_attacking;
        bool is_currently_guarding;
        {   // Check for current anim state.
            var current_anim_state = m_char_animator.GetState();
            is_currently_attacking =
                (current_anim_state >= AnimatorStateHandler.AnimatorState.FirstAttack &&
                 current_anim_state <= AnimatorStateHandler.AnimatorState.LastAttack);
            is_currently_guarding =
                (current_anim_state == AnimatorStateHandler.AnimatorState.Guard);
        }

        Vector3 mvt_velocity = Vector3.zero;
        bool lock_rotation = false;
        if (m_char_animator.GetCanMove())
        {   // Move towards movement.
            var move_input = calc_move_input();

            float actual_move_speed = (is_currently_guarding
                                       ? m_guarding_move_speed
                                       : m_move_speed);
            mvt_velocity.x += (move_input.x * actual_move_speed);
            mvt_velocity.z += (move_input.y * actual_move_speed);

            if (move_input.sqrMagnitude > 0.001f)
            {   // Turn towards movement.
                m_char_model.MoveRotation(
                    Quaternion.Euler(0.0f,
                                     Mathf.Atan2(move_input.x, move_input.y)
                                     * Mathf.Rad2Deg,
                                     0.0f));
            }
        }

        if (m_char_animator.GetKnockback())
        {
            var knockback_velocity = new Vector3(0.0f, 0.0f, -m_knockback_speed);
            knockback_velocity = (m_char_model.transform.rotation * knockback_velocity);

            // Apply knockback.
            lock_rotation = true;
            mvt_velocity += knockback_velocity;
        }

        if (mvt_velocity.sqrMagnitude > 0.000001f)
        {   // Apply movement.
            m_char_con.Move(mvt_velocity * Time.deltaTime);
            m_char_model.MovePosition(m_char_con.transform.position);  // Pos updates immediately after `.Move()`.
        }

        // Update animator running speed.
            var running_speed = (m_char_con.transform.position - m_prev_cc_pos).magnitude
                            / Time.deltaTime
                            / m_move_speed;
        m_char_animator.SetIdleRunningBTLerpVal(running_speed);
        m_prev_cc_pos = m_char_con.transform.position;

        // Attack.
        if (!is_currently_attacking && m_action_attack.action.ReadValue<float>() == 1)
        {
            m_char_animator.PlayState(AnimatorStateHandler.AnimatorState.Attack_Q);
        }

        // Guard.
        if (!is_currently_attacking && m_action_guard.action.ReadValue<float>() == 1)
        {
            m_char_animator.DisableAllHurtboxes();
            m_char_animator.PlayState(AnimatorStateHandler.AnimatorState.Guard);
        }
        else if (is_currently_guarding && m_action_guard.action.ReadValue<float>() != 1)
        {
            m_char_animator.PlayState(AnimatorStateHandler.AnimatorState.IdleRunningBT);
        }

        // Change lock cursor.
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            m_lock_cursor = !m_lock_cursor;
        }
        if (m_lock_cursor != m_prev_lock_cursor)
        {
            Cursor.lockState = (m_lock_cursor
                                ? CursorLockMode.Locked
                                : CursorLockMode.None);
            m_prev_lock_cursor = m_lock_cursor;
        }
    }

    private void update_camera(Vector2 cam_delta)
    {
        const float k_pi2 = Mathf.PI * 2.0f;
        const float k_max_orbit_y = 89.0f * Mathf.Deg2Rad;

        // Update orbit angles.
        m_cam_orbit_x += cam_delta.x;
        while (m_cam_orbit_x >= k_pi2)
            m_cam_orbit_x -= k_pi2;
        while (m_cam_orbit_x < 0.0f)
            m_cam_orbit_x += k_pi2;

        m_cam_orbit_y += cam_delta.y;
        m_cam_orbit_y = Mathf.Clamp(m_cam_orbit_y, -k_max_orbit_y, k_max_orbit_y);

        // Check cam distance.
        var cam_focus_pos = m_char_model.transform.position
                            + new Vector3(0.0f, m_cam_focus_offset_y, 0.0f);
        var cam_facing_dir = new Vector3(0.0f, 0.0f, 1.0f);
        cam_facing_dir = Quaternion.Euler(m_cam_orbit_y * Mathf.Rad2Deg,
                                          m_cam_orbit_x * Mathf.Rad2Deg, 0.0f)
                         * cam_facing_dir;

        float actual_cam_distance = m_cam_distance;
        RaycastHit hit;
        if (Physics.Raycast(cam_focus_pos,
                            -cam_facing_dir,
                            out hit,
                            m_cam_distance + m_cam_raycast_extra_distance,
                            m_cam_raycast_layermask))
        {
            actual_cam_distance =
                Mathf.Clamp(actual_cam_distance, 0.0f, hit.distance - m_cam_raycast_extra_distance);
        }

        // Position camera transform.
        m_main_camera.position = cam_focus_pos
                                 + (cam_facing_dir * -actual_cam_distance);
        m_main_camera.rotation = Quaternion.LookRotation(cam_facing_dir);
    }

    private Vector2 calc_move_input()
    {
        Vector3 cam_forward = m_main_camera.forward;
        Vector2 cam_forward_flat = new Vector2(cam_forward.x, cam_forward.z).normalized;
        Vector3 cam_right = m_main_camera.right;
        Vector2 cam_right_flat = new Vector2(cam_right.x, cam_right.z).normalized;
        Vector2 move_input = m_action_move.action.ReadValue<Vector2>();
        Vector2 move_input_projected =
            Vector2.ClampMagnitude(cam_forward_flat * move_input.y
                                   + cam_right_flat * move_input.x,
                                   1.0f);
        return move_input_projected;
    }
}

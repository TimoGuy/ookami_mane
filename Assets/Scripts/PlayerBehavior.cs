using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
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

    [Header("Movement props")]
    public float m_move_speed = 5.0f;
    public float m_guarding_move_speed = 0.75f;

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
    {

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

        if (m_char_animator.GetCanMove())
        {   // Move towards movement.
            var move_input = calc_move_input();

            float actual_move_speed = (is_currently_guarding
                                       ? m_guarding_move_speed
                                       : m_move_speed);
            m_char_con.Move(new Vector3(move_input.x * actual_move_speed,
                                        0.0f,
                                        move_input.y * actual_move_speed)
                            * Time.deltaTime);
            m_char_model.MovePosition(m_char_con.transform.position);  // Pos updates immediately after `.Move()`.

            if (move_input.sqrMagnitude > 0.001f)
            {   // Turn towards movement.
                m_char_model.MoveRotation(
                    Quaternion.Euler(0.0f,
                                     Mathf.Atan2(move_input.x, move_input.y)
                                     * Mathf.Rad2Deg,
                                     0.0f));
            }
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
            m_char_animator.PlayState(AnimatorStateHandler.AnimatorState.Guard);
        }
        else if (is_currently_guarding && m_action_guard.action.ReadValue<float>() != 1)
        {
            m_char_animator.PlayState(AnimatorStateHandler.AnimatorState.IdleRunningBT);
        }
    }

    private void update_camera(Vector2 cam_delta)
    {
        const float k_pi2 = Mathf.PI * 2.0f;
        const float k_max_orbit_y = 89.0f * Mathf.Deg2Rad;

        m_cam_orbit_x += cam_delta.x;
        while (m_cam_orbit_x >= k_pi2)
            m_cam_orbit_x -= k_pi2;
        while (m_cam_orbit_x < 0.0f)
            m_cam_orbit_x += k_pi2;

        m_cam_orbit_y += cam_delta.y;
        m_cam_orbit_y = Mathf.Clamp(m_cam_orbit_y, -k_max_orbit_y, k_max_orbit_y);

        // Position camera transform.
        var offset_from_cc = new Vector3(0.0f, 0.0f, -m_cam_distance);
        offset_from_cc = Quaternion.Euler(m_cam_orbit_y * Mathf.Rad2Deg,
                                          m_cam_orbit_x * Mathf.Rad2Deg, 0.0f)
                         * offset_from_cc;
        m_main_camera.position = m_char_model.transform.position
                                 + new Vector3(0.0f, m_cam_focus_offset_y, 0.0f)
                                 + offset_from_cc;
        m_main_camera.rotation = Quaternion.LookRotation(-offset_from_cc);
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

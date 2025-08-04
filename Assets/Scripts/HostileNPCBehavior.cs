using UnityEngine;

public class HostileNPCBehavior : MonoBehaviour
{
    public Transform m_tracking_transform;
    public float m_ideal_distance_away = 2.0f;
    public float m_attack_range = 3.0f;
    public float m_move_speed = 5.0f;
    public bool m_can_move = true;

    public CharacterController m_char_con;
    public Rigidbody m_char_model;
    public AnimatorStateHandler m_char_animator;

    private Vector3 m_prev_cc_pos;
    private double m_next_thought_tick_time;
    public double m_thought_tick_interval = 0.75f;


    void Awake()
    {
        m_char_model.maxLinearVelocity = Mathf.Infinity;  // I hate Unity.
        m_char_model.maxAngularVelocity = Mathf.Infinity;  // I hate Unity.
        m_prev_cc_pos = m_char_con.transform.position;
        m_next_thought_tick_time =
            Time.realtimeSinceStartupAsDouble + m_thought_tick_interval;
    }

    void Update()
    {
        if (m_can_move)
        {   // Move towards tracking transform.
            var char_con_pos = m_char_con.transform.position;
            var flat_delta_pos = m_tracking_transform.position - char_con_pos;
            flat_delta_pos.y = 0.0f;

            var target_point = char_con_pos
                               + flat_delta_pos
                               - (flat_delta_pos.normalized * m_ideal_distance_away);
            var next_point = Vector3.MoveTowards(char_con_pos,
                                                 target_point,
                                                 m_move_speed * Time.deltaTime);
            m_char_con.Move(next_point - char_con_pos);
            m_char_model.MovePosition(m_char_con.transform.position);  // Pos updates immediately after `.Move()`.

            if (flat_delta_pos.sqrMagnitude > 0.001f)
            {   // Turn towards tracking transform.
                m_char_model.MoveRotation(
                    Quaternion.Euler(0.0f,
                                     Mathf.Atan2(flat_delta_pos.x, flat_delta_pos.z)
                                     * Mathf.Rad2Deg,
                                     0.0f));
            }
        }

        // Update animator running speed.
        var running_speed = (m_char_con.transform.position - m_prev_cc_pos).magnitude
                            / Time.deltaTime
                            / m_move_speed;
        Debug.Log(running_speed);
        m_char_animator.SetIdleRunningBTLerpVal(running_speed);
        m_prev_cc_pos = m_char_con.transform.position;

        // Check for thought tick.
        bool thought_tick = false;
        if (Time.realtimeSinceStartupAsDouble >= m_next_thought_tick_time)
        {   // Thought tick triggered!
            thought_tick = true;
            m_next_thought_tick_time =
                Time.realtimeSinceStartupAsDouble + m_thought_tick_interval;
        }

        if (thought_tick)
        {   // Check if should attack.
            var current_anim_state = m_char_animator.GetState();
            bool is_currently_attacking =
                (current_anim_state >= AnimatorStateHandler.AnimatorState.FirstAttack &&
                 current_anim_state <= AnimatorStateHandler.AnimatorState.LastAttack);
            bool is_in_range =
                ((m_tracking_transform.position - m_char_con.transform.position).sqrMagnitude
                 <= m_attack_range * m_attack_range);
            bool i_should_attack_yupyup = (Random.Range(0.0f, 1.0f) > 0.75f);

            if (!is_currently_attacking && is_in_range && i_should_attack_yupyup)
            {   // Do attack.
                var next_attack = (AnimatorStateHandler.AnimatorState)
                                  Random.Range((int)AnimatorStateHandler.AnimatorState.FirstAttack,
                                               (int)AnimatorStateHandler.AnimatorState.LastAttack + 1);
                m_char_animator.PlayState(next_attack);
            }
        }
    }
}

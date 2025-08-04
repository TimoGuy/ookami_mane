using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public float m_move_speed = 5.0f;

    public CharacterController m_char_con;
    public Rigidbody m_char_model;
    public AnimatorStateHandler m_char_animator;

    private Vector3 m_prev_cc_pos;


    void Start()
    {
        
    }

    void Update()
    {
        if (m_char_animator.GetCanMove())
        {   // Move towards tracking transform.
            // Turn.
            // @TODO: START HERE!!!
        }

        // Update animator running speed.
        var running_speed = (m_char_con.transform.position - m_prev_cc_pos).magnitude
                            / Time.deltaTime
                            / m_move_speed;
        Debug.Log(running_speed);
        m_char_animator.SetIdleRunningBTLerpVal(running_speed);
        m_prev_cc_pos = m_char_con.transform.position;

        // Attack.
        var current_anim_state = m_char_animator.GetState();
        bool is_currently_attacking =
            (current_anim_state >= AnimatorStateHandler.AnimatorState.FirstAttack &&
             current_anim_state <= AnimatorStateHandler.AnimatorState.LastAttack);
        if (!is_currently_attacking && )
        {
            // @TODO: START HERE!!!!
        }
    }
}

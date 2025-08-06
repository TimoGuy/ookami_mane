using UnityEngine;

public class HitLifeHandler : MonoBehaviour
{
    [Header("Life Data")]
    public int m_life = 25;

    [Header("Hurt Settings")]
    public float m_hurt_debounce = 0.5f;
    private float m_hurt_debounce_timer = 0.0f;
    public AnimatorStateHandler m_animator;

    private Rigidbody m_my_rigidbody;


    void Awake()
    {
        m_my_rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        m_hurt_debounce_timer -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != m_my_rigidbody &&
            other.gameObject.layer == LayerMask.NameToLayer("Hitbox") &&
            m_hurt_debounce_timer <= 0.0f)
        {
            m_hurt_debounce_timer = m_hurt_debounce;
            m_life--;

            if (m_animator.GetState() != AnimatorStateHandler.AnimatorState.Guard)
            {   // Let hurt know.
                m_animator.DisableAllHurtboxes();
                if (m_life > 0)
                {
                    m_animator.PlayState(AnimatorStateHandler.AnimatorState.Hurt);
                }
                else
                {
                    m_animator.PlayState(AnimatorStateHandler.AnimatorState.Hurt_posturebreak);
                }
            }
        }
    }
}

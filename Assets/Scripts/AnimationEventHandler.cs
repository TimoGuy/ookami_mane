using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public Collider[] m_hurtboxes;
    private AnimatorStateHandler m_anim_state_handler;

    void Awake()
    {
        m_anim_state_handler = GetComponent<AnimatorStateHandler>();
    }

    void EnableHurtbox(int idx)
    {
        m_hurtboxes[idx].enabled = true;
    }

    void DisableHurtbox(int idx)
    {
        m_hurtboxes[idx].enabled = false;
    }

    void EndAttack()
    {
        m_anim_state_handler.NotifyEndAttack();
    }

    void EnableMove()
    {
        m_anim_state_handler.SetCanMove(true);
    }

    void DisableMove()
    {
        m_anim_state_handler.SetCanMove(false);
    }
}

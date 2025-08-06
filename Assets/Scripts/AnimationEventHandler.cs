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

    public void DisableAllHurtboxes()
    {
        foreach (var hurtbox in m_hurtboxes)
        {
            hurtbox.enabled = false;
        }
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

    void StartKnockback()
    {
        m_anim_state_handler.SetKnockback(true);
        m_anim_state_handler.SetCanMove(false);
    }

    void EndKnockback()
    {
        m_anim_state_handler.SetKnockback(false);
        m_anim_state_handler.SetCanMove(true);
    }

    void EndHurt()
    {
        // @NOTE: @HACK: Using the same thing.
        m_anim_state_handler.NotifyEndAttack();
    }
}

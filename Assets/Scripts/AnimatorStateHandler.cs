using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class AnimatorStateHandler : MonoBehaviour
{
    public enum AnimatorState
    {
        IdleRunningBT,
        Attack_qQQqQQq,
        Attack_Q,
        Attack_Q2,
        Attack_todome,
        Guard,
        DeflectAttack,
        Hurt,
        Hurt_posturebreak,
        Hurt_todome,

        FirstAttack = Attack_qQQqQQq,
        LastAttack = Attack_Q2,
    }

    private static Dictionary<AnimatorState, string> m_animator_state_strs =
        new Dictionary<AnimatorState, string>
        {
            { AnimatorState.IdleRunningBT,     "Idle-Running BT"            },
            { AnimatorState.Attack_qQQqQQq,    "Armature|Attack_q_QQqQQq"   },
            { AnimatorState.Attack_Q,          "Armature|Attack_p_Q"        },
            { AnimatorState.Attack_Q2,         "Armature|Attack_p_Q_2"      },
            { AnimatorState.Attack_todome,     "Armature|Attack_todome"     },
            { AnimatorState.Guard,             "Armature|Guard"             },
            { AnimatorState.DeflectAttack,     "Armature|Deflect_attack"    },
            { AnimatorState.Hurt,              "Armature|Hurt"              },
            { AnimatorState.Hurt_posturebreak, "Armature|Hurt_posturebreak" },
            { AnimatorState.Hurt_todome,       "Armature|Hurt_todome"       },
        };

    private Animator m_animator;
    private bool m_can_move = true;

    void Awake()
    {
        m_animator = GetComponent<Animator>();

        if (false)
        {
            // Verify that all state strs are valid.
            string first_state_str = "";
            foreach (var pair in m_animator_state_strs)
            {
                if (first_state_str == "")
                {
                    first_state_str = pair.Value;
                }
                Debug.Log(pair.Value);
                m_animator.Play(pair.Value);
            }
            Debug.Log(first_state_str);
            m_animator.Play(first_state_str);
        }
    }

    public void SetIdleRunningBTLerpVal(float t)
    {
        m_animator.SetFloat("Running", t);
    }

    public void PlayState(AnimatorState new_state)
    {
        m_animator.Play(m_animator_state_strs[new_state]);
    }

    public AnimatorState GetState()
    {
        foreach (var state in m_animator_state_strs)
            if (m_animator.GetCurrentAnimatorStateInfo(0).IsName(state.Value))
            {
                return state.Key;
            }

        // Was not able to find the state.
        throw new Exception();
    }

    public void NotifyEndAttack()
    {   // Just go to idle anim state.
        PlayState(AnimatorState.IdleRunningBT);
    }

    public void SetCanMove(bool flag)
    {
        m_can_move = flag;
    }

    public bool GetCanMove()
    {
        return m_can_move;
    }
}

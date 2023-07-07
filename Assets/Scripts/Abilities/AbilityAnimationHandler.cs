using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AbilityAnimationHandler : NetworkBehaviour
{
    Animator animator;
    NetworkAnimator networkAnimator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }

    public void TriggerAttackAnim(string characterLayer, string attackingStateName, string triggerName, int numAttackAnims)
    {
        if (!hasAuthority)
            return;

        int layerIndex = animator.GetLayerIndex(characterLayer);
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(attackingStateName))
        {
            animator.Play(attackingStateName, layerIndex, 0);
        }

        if (numAttackAnims > 1)
        {
            int rndm = Random.Range(0, numAttackAnims);
            animator.SetInteger("AttackAnimID", rndm);
        }
        else
            animator.SetInteger("AttackAnimID", 0);

        networkAnimator.SetTrigger(triggerName);

        Debug.Log("attack animation triggered");
    }

    public void SetAnimatorBoolean(string boolName, bool value)
    {
        if (hasAuthority)
            animator.SetBool(boolName, value);
    }

    public void PauseAnimation(string speedParameterName)
    {
        if (hasAuthority)
            animator.SetFloat(speedParameterName, 0);
        Debug.Log("animation paused");

    }

    public void ResumeAnimation(string speedParameterName)
    {
        if (hasAuthority)
            animator.SetFloat(speedParameterName, 1);
        Debug.Log("animation resumed");
    }

}

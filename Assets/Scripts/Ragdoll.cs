using System.Collections;
using System.Collections.Generic;
using UnityEngine;
   
public class Ragdoll : MonoBehaviour
{

    Animator anim;
    Rigidbody[] rb;
    Collider[] colliders;

    void Start()
    {
        rb = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        anim = GetComponent<Animator>();
        DeactivateRagdoll();
    }


    public void DeactivateRagdoll()
    {
        foreach (var rigidbody in rb)
        {
            rigidbody.isKinematic = true;
        }
        foreach (var col in colliders)
        {
            if (col != GetComponent<CharacterController>())
                col.isTrigger = true;
        }
        anim.enabled = true;
    }
    public void ActivateRagdoll()
    {
        foreach (var rigidbody in rb)
        {
            rigidbody.isKinematic = false;
        }
        foreach (var col in colliders)
        {
            if (col != GetComponent<CharacterController>()) 
                col.isTrigger = false;
        }
        anim.enabled = false;
    }
}

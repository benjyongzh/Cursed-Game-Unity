using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[System.Serializable]
public class HumanBone
{
    public HumanBodyBones bone;
    public float weight = 1.0f;
}


[RequireComponent(typeof(PlayerPrimaryController))]
public class PlayerTorsoAnimation : NetworkBehaviour
{
    
    Animator animator;
    CharacterController characterController;
    UnitStats unitStats;

    public Transform cameraTarget;
    public Transform aimTransform;
    public int torsoTargetIterations = 10;

    [Range(0,1)]
    public float weight = 1.0f;

    public HumanBone[] humanBones;
    Transform[] boneTransforms;
    //Quaternion[] boneRotations;
    //Vector3 cameraTargetForOthers;
    //bool isReceivingData = false;

    PlayerPrimaryController playerController;


    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerPrimaryController>();
        unitStats = GetComponent<UnitStats>();
        animator = GetComponent<Animator>();

        boneTransforms = new Transform[humanBones.Length];
        //boneRotations = new Quaternion[humanBones.Length];
        for (int i = 0; i < boneTransforms.Length; i++)
        {
            boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);
            //boneRotations[i] = boneTransforms[i].rotation;
        }
        //cameraTargetForOthers = cameraTarget.transform.position;
    }

    /*
    private void OnAnimatorIK()
    {
        animator.SetLookAtWeight(weight);
        animator.SetLookAtPosition(cameraTarget.transform.position);
    }
    */

    private void LateUpdate()
    {
        if (!unitStats.IsAlive)
            return;

        TorsoToTarget();

    }

    private void TorsoToTarget()
    {
        for (int i = 0; i < torsoTargetIterations; i++)
        {
            for (int b = 0; b < boneTransforms.Length; b++)
            {
                Transform bone = boneTransforms[b];
                float boneWeight = humanBones[b].weight;
                /*
                if (!playerController.IsStationary())
                {
                    Vector3 forwardTarget = transform.position + transform.forward * 40f;
                    //lacking vertical aiming
                    TorsoToTarget(bone, forwardTarget, boneWeight);
                }
                else
                {
                */
                //if (hasAuthority)
                BoneToTarget(bone, cameraTarget.transform.position, boneWeight);//user is rotating bones on his own

            }
        }
        //for (int c = 0; c < boneTransforms.Length; c++)
        //    CmdTorsoToTargetForOthers(c, boneTransforms[c].rotation);

    }

    /*
    [Command]
    private void CmdTorsoToTargetForOthers(int bone_int, Quaternion _rotation)
    {
        //RpcTorsoToTargetForOthers(bone_int, _rotation);
        boneTransforms[bone_int].rotation = _rotation;
    }

    [ClientRpc]
    private void RpcTorsoToTargetForOthers(int bone_int, Quaternion _rotation)
    {
        if (hasAuthority)
            return;

        boneTransforms[bone_int].rotation = _rotation;
    }
    */

    private void BoneToTarget(Transform bone, Vector3 targetPosition, float weight)
    {
        Vector3 aimDirection = aimTransform.forward;
        Vector3 targetDirection = targetPosition - aimTransform.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }

    /*
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            isReceivingData = false;
            stream.SendNext(cameraTarget.transform.position);
        }
        else if (stream.IsReading)
        {
            isReceivingData = true;
            cameraTargetForOthers = (Vector3)stream.ReceiveNext();
        }
    }
    */

}

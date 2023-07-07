using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(UnitStats))]
public class Footsteps : NetworkBehaviour
{
    [Header("General Parameters")]
    public float moveSpeedTolerance = 0.05f;

    [Header("Generic Footsteps")]
    [SerializeField]
    private AudioClip[] runningClips;
    [SerializeField]
    private AudioClip[] walkingClips;
    [SerializeField]
    private AudioClip[] crouchingClips;
    [SerializeField]
    private AudioClip jumpclip;
    [SerializeField]
    private AudioClip landingclip;

    private AudioSource audioSource;
    private CharacterController CC;
    private NavMeshAgent navMeshAgent;
    private UnitStats unitStats;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        unitStats = GetComponent<UnitStats>();

        if (gameObject.layer == 6)
        {
            CC = GetComponent<CharacterController>();

        }
        //else
        //{
        //    navMeshAgent = GetComponent<NavMeshAgent>();
        //}
    }

    private void Footstep(string direction)
    {
        if (CC != null)//is a player
        {
            PlayerPrimaryController playerController = GetComponent<PlayerPrimaryController>();
            float actualspeed = CC.velocity.magnitude;
            if (direction != null)
            {
                if (playerController.GetInputDirection().x != 0 && playerController.GetInputDirection().y != 0)
                {
                    //diagonal movement
                    if (actualspeed > unitStats.WalkSpeed + moveSpeedTolerance)
                    {
                        //running
                        if (direction == "move diagonal")
                        {
                            AudioClip clip = GetRandomClip(runningClips);
                            audioSource.PlayOneShot(clip);
                        }
                    }
                    else if (actualspeed > unitStats.CrouchSpeed + moveSpeedTolerance)
                    {
                        //walking
                        if (direction == "walk diagonal")
                        {
                            AudioClip clip = GetRandomClip(walkingClips);
                            audioSource.PlayOneShot(clip);
                        }
                    }
                }
                else
                {
                    //straight movement
                    if (actualspeed > unitStats.WalkSpeed + moveSpeedTolerance)
                    {
                        //running
                        if (direction == "move straight")
                        {
                            AudioClip clip = GetRandomClip(runningClips);
                            audioSource.PlayOneShot(clip);
                        }
                    }
                    else if (actualspeed > unitStats.CrouchSpeed + moveSpeedTolerance)
                    {
                        //walking
                        if (direction == "walk straight")
                        {
                            AudioClip clip = GetRandomClip(walkingClips);
                            audioSource.PlayOneShot(clip);
                        }
                    }
                }
            }
        }
        /*
        else if (navMeshAgent != null)//is an AI creep
        {
            actualspeed = navMeshAgent.velocity.magnitude;
        }

        check for correct speed
        if (GetMovementState(actualspeed) == GetMovementState(speed))
        {
            //check for correct direction
            Debug.Log("X velocity is " + CC.velocity.x + ", Z velocity is " + CC.velocity.z);

            AudioClip clip = GetRandomClip();
            audioSource.PlayOneShot(clip);
        }
        */
    }

    //private void FootstepCrawl(float speed, string direction)
    private void FootstepCrawl(string direction)
    {
        if (CC != null)
        {
            PlayerPrimaryController playerController = GetComponent<PlayerPrimaryController>();
            if (direction != null)
            {
                if (playerController.GetInputDirection().x != 0 && playerController.GetInputDirection().y != 0)
                {
                    if (direction == "diagonal")
                    {
                        AudioClip clip = GetRandomClip(crouchingClips);
                        audioSource.PlayOneShot(clip);
                    }
                }
                else if (direction == "straight")
                {
                    AudioClip clip = GetRandomClip(crouchingClips);
                    audioSource.PlayOneShot(clip);
                }
            }
        }
    }

    private void FootstepJump()
    {
        audioSource.PlayOneShot(jumpclip);
    }

    private void FootstepLand()
    {
        audioSource.PlayOneShot(landingclip);
    }

    private AudioClip GetRandomClip(AudioClip[] audioClipArray)
    {

        return audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)];
    }

    /*
    private float GetMovementState(float speed)
    {
        if (speed > 1.6f)
        {
            return 6f;
        }
        else if (speed > 1.2f)
        {
            return 1.6f;
        }
        else if (speed > 0.05f)
        {
            return 1.2f;
        }
        return 0f;
    }
    */
}

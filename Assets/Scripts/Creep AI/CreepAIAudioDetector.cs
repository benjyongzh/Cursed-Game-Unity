using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;

public class CreepAIAudioDetector : MonoBehaviour
{
    public float updateInterval = 0.25f;
    [HideInInspector]
    public Vector3 audioAttentionLocation;

    private float elapsedTime = 0f;

    private Transform[] AudioLocation = new Transform[50];

    //PhotonView PV;

    private void Awake()
    {
        //PV = GetComponent<PhotonView>();
    }

    public void AddAudioLocation(Transform inputTransform)
    {
        if (AudioLocation[AudioLocation.Length-1] != null)
        {
            //array reached max length. have to replace older transforms
            AudioLocation[0] = inputTransform;
            //Debug.Log("audiolocation fully used");
        }
        else
        {
            //add transform into audioLocation array
            for (int i = 0; i < AudioLocation.Length; i++)
            {
                if (AudioLocation[i] == null)
                {
                    AudioLocation[i] = inputTransform;
                    break;
                }
            }
        }
    }
    
    private void Update()
    {
        //if (!PV.IsMine)
        //    return;

        if (elapsedTime >= updateInterval)
        {
            elapsedTime = 0f;
            //set audioattentionpoint to closest point within audioLocation[]
            float distanceFromCreep = 500f; //max distance for receiving audio
            for (int i = 0; i < AudioLocation.Length; i++)
            {
                if (AudioLocation[i] != null)//does not run if audiolocation array only contains invalid transforms. ensures attentionpoint is not overridden by nothing
                {
                    float distance = Vector3.Distance(AudioLocation[i].position, transform.position);
                    if (distance < distanceFromCreep)
                    {
                        //set lowscore for distanceFromCreep
                        distanceFromCreep = distance;
                        //set attention point as closest place
                        audioAttentionLocation = AudioLocation[i].position;
                    }
                }
            }
            //clear audioLocation[]
            AudioLocation = new Transform[50];
            //Debug.Log("audio attention at " + audioAttentionLocation);
        }
        else
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public Vector3 GetAudioAttentionPoint()
    {
        return audioAttentionLocation;
    }
}

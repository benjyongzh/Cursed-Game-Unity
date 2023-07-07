using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AudioDetection : NetworkBehaviour
{
    [SyncVar]
    float loudness = 0f;

    [SerializeField] float loudnessDecrementFactor = 0.5f;
    [SerializeField] float linearSizeFactor = 1f;
    //[Range(0,1f)] public float fractionPowerFactor = 0.3f;
    [SerializeField] float minSize = 0f;
    [SerializeField] float maxSize = 500f;

    [SerializeField] LayerMask creepLayerMask;
    
    private int sampleDataLength = 1024;
    private float[] sampleData;
    private AudioSource audioSource;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loudness);
    }

    void Start()
    {
        sampleData = new float[sampleDataLength];
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        //if (!hasAuthority)
        //    return;

        audioSource.GetOutputData(sampleData, audioSource.timeSamples);
        //audioSource.GetSpectrumData(sampleData, audioSource.timeSamples, FFTWindow.BlackmanHarris);
        float soundAddition = 0f;
        foreach( var sample in sampleData)
        {
            soundAddition += Mathf.Abs(sample);

        }
        soundAddition /= sampleDataLength;
        soundAddition *= linearSizeFactor;
        if (soundAddition > loudness)
        {
            //new sound was loud enough to be registered
            loudness += soundAddition;

            loudness = Mathf.Clamp(loudness, minSize, maxSize);

            //alert creeps nearby enough to hear sound
            Collider[] overlaps = new Collider[30];
            int count = Physics.OverlapSphereNonAlloc(transform.position, loudness, overlaps, creepLayerMask);
            //check if there are any creep hitboxes at all
            if (count > 0)
            {
                //create empty list to store all creeps in AOE
                List<GameObject> creeps = new List<GameObject>();

                //cycle thru all colliders in AOE. will contain ragdoll hitboxes
                for (int i = 0; i < count + 1; i++)
                {
                    //make sure it isnt null. it shouldnt be
                    if (overlaps[i] != null)
                    {
                        Transform creep = overlaps[i].transform;
                        //cycle upwards to find ultimate grandparent of hitbox
                        while (creep.transform.parent)
                        {
                            creep = creep.transform.parent;
                        }
                        //if the grandparent has correct tag, and isnt already in the creeps list, add it
                        if (creep.tag == "Creep" && !creeps.Contains(creep.gameObject))
                        {
                            creeps.Add(creep.gameObject);
                        }
                    }
                }

                //cycle thru creeps list and call audiolocation()
                for (int j = 0; j < creeps.Count; j++)
                {
                    CreepAIAudioDetector creepAudioDetector = creeps[j].GetComponent<CreepAIAudioDetector>();
                    creepAudioDetector.AddAudioLocation(transform);
                }
            }
        }

        if (loudness > minSize)
            loudness -= (loudnessDecrementFactor*loudness) * Time.deltaTime;
        else
            loudness = minSize;
    }
}

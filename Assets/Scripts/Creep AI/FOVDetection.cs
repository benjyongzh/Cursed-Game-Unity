using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVDetection : MonoBehaviour
{
    public GameObject target;
    public LayerMask layerMask;

    public float maxAngle;
    public float maxRadius;
    public float FOVHeight = 1.8f;
    public float targetHeightVerticalOffset = 0.15f;

    Vector3 targetPosition;
    Vector3 FOVPosition;

    private bool targetIsInFOV = false;
    private bool anyTargetInFOV = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(FOVPosition, maxRadius);

        Vector3 fovLine1 = Quaternion.AngleAxis(maxAngle, transform.up) * transform.forward * maxRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-maxAngle, transform.up) * transform.forward * maxRadius;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(FOVPosition, fovLine1);
        Gizmos.DrawRay(FOVPosition, fovLine2);

        if (!targetIsInFOV || !anyTargetInFOV)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawRay(FOVPosition, (targetPosition - FOVPosition).normalized * maxRadius);
        
        Gizmos.color = Color.black;
        Gizmos.DrawRay(FOVPosition, transform.forward * maxRadius);
    }

    //used to check if target player is still in FOV. ignores other players
    public bool targetInFOV(Transform checkingObject, GameObject target, float FOVHeight, float maxAngle, float maxRadius, LayerMask layermask)
    {
        Collider[] overlaps = new Collider[10];
        Vector3 actualFOVpoint = new Vector3(checkingObject.position.x, checkingObject.position.y + FOVHeight, checkingObject.position.z);
        int count = Physics.OverlapSphereNonAlloc(actualFOVpoint, maxRadius, overlaps, layermask);

        for (int i = 0; i < count + 1; i++)
        {
            if (overlaps[i] != null)
            {
                if (overlaps[i].transform == target.transform)
                {
                    targetPosition = new Vector3(target.transform.position.x, target.transform.position.y + (target.GetComponent<CharacterController>().height-targetHeightVerticalOffset), target.transform.position.z);
                    Vector3 directionBetween = (targetPosition - actualFOVpoint).normalized;
                    //directionBetween.y *= 0;
                    float angle = Vector3.Angle(checkingObject.forward, directionBetween);
                    if (angle <= maxAngle)
                    {
                        Ray ray = new Ray(actualFOVpoint, targetPosition - actualFOVpoint);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, maxRadius))
                        {
                            if (hit.transform == target.transform)
                                return true;
                        }
                    }
                    
                }
            }
        }
        return false;
    }

    //used to check if theres ANY player in FOV. choose nearest player if more than 1
    public GameObject anyPlayerInFOV(Transform checkingObject, float FOVHeight, float maxAngle, float maxRadius, LayerMask layermask)
    {
        Vector3 FOVPosition = new Vector3(checkingObject.position.x, checkingObject.position.y + FOVHeight, checkingObject.position.z);
        Collider[] overlaps = new Collider[10];
        int count = Physics.OverlapSphereNonAlloc(FOVPosition, maxRadius, overlaps, layermask);
        if (count == 0)
        {
            return null;
        }
        else
        {
            //check for players only in FOV angle first
            GameObject[] targets = new GameObject[count];
            GameObject attentionTarget;

            for (int i = 0; i < count + 1; i++)
            {
                if (overlaps[i] != null)
                {
                    attentionTarget = overlaps[i].transform.gameObject;
                    //Vector3 targetPos;
                    targetPosition = new Vector3(attentionTarget.transform.position.x, attentionTarget.transform.position.y + (attentionTarget.GetComponent<CharacterController>().height-targetHeightVerticalOffset), attentionTarget.transform.position.z);
                    Vector3 directionBetween = (targetPosition - FOVPosition).normalized;
                    float angle = Vector3.Angle(checkingObject.forward, directionBetween);
                    if (angle <= maxAngle)
                    {
                        Ray ray = new Ray(FOVPosition, targetPosition - FOVPosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, maxRadius))
                        {
                            if (hit.transform == attentionTarget.transform)
                                targets[i] = attentionTarget;
                        }
                    }
                }
            }


            //selecting player gameobjects
            float distanceFromPlayer = maxRadius;
            if (targets.Length == 1)//theres only 1 player in FOV. return it
            {
                attentionTarget = targets[0];
                return attentionTarget;

            }
            else//more than 1 player in FOV. find closest player
            {
                for (int i = 0; i < targets.Length + 1; i++)
                {
                    float distance = Vector3.Distance(targets[i].transform.position, FOVPosition);
                    if (distance < distanceFromPlayer)
                    {
                        distanceFromPlayer = distance;
                        attentionTarget = targets[i];
                        return attentionTarget;
                    }
                }
            }
        }
        return null;
    }


    private void Update()
    {
        FOVPosition = new Vector3(transform.position.x, transform.position.y + FOVHeight, transform.position.z);
        //targetIsInFOV = targetInFOV(transform, target, maxAngle, maxRadius, layerMask);

        /*
        if (anyPlayerInFOV(transform, maxAngle, maxRadius, layerMask) != null)
        {
            Debug.Log("player detected in FOV. Position: " + anyPlayerInFOV(transform, maxAngle, maxRadius, layerMask).transform.position);
            anyTargetInFOV = true;
        }
        else
        {
            //Debug.Log("no players detected in FOV");
            anyTargetInFOV = false;
        }
        */
        
    }

    public float GetFOVHeight()
    {
        return FOVHeight;
    }

    public float GetMaxAngle()
    {
        return maxAngle;
    }

    public float GetMaxRadius()
    {
        return maxRadius;
    }

    public LayerMask GetLayerMask()
    {
        return layerMask;
    }
}

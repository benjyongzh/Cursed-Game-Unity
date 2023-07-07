using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer playerMesh;
    [SerializeField] private CinemachineVirtualCamera FPSCamera = null;
    [SerializeField] private CinemachineVirtualCamera TPSCamera = null;

    [Header("TPS Camera Position Parameters")]
    private bool isThirdPersonCamera = false;
    Camera cam;
    CinemachineBrain cambrain;

    PlayerInput playerInput;

    public override void OnStartAuthority()
    {
        FPSCamera.gameObject.SetActive(true);
        playerInput = GetComponent<PlayerInput>();
        playerMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        playerMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        cam = FindObjectOfType<Camera>();
        cambrain = cam.gameObject.GetComponent<CinemachineBrain>();
    }

    void Update()
    {
        if (!hasAuthority)
        {
            return;
        }


        if (playerInput.cameraSwitch)
        {
            isThirdPersonCamera = !isThirdPersonCamera;
            FPSCamera.gameObject.SetActive(!isThirdPersonCamera);
            TPSCamera.gameObject.SetActive(isThirdPersonCamera);

            if (isThirdPersonCamera)
                playerMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        else if (!isThirdPersonCamera && !cambrain.IsBlending)
        {
            if (playerMesh.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On)
                playerMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }

        
    }

}

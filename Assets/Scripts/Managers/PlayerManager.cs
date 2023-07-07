using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] GameObject playerController = null;
    [SerializeField, Range(0,30f)] float respawnTime = 5f;

    private Vector3 spawnPoint;
    private Quaternion spawnFacing;

    void SpawnPlayer(NetworkConnection conn)
    {
        //instantiate player controller
        //GameObject playerControllerInstance = Instantiate(PlayerSpawnSystem.PlayerController, SpawnPoint, SpawnFacing);
        //NetworkServer.Spawn(playerControllerInstance, conn);
        Debug.Log("SpawnPlayer()");
    }

    [Server]
    public void Die()
    {
        //remove playercontroller from game
        PlayerPrimaryController controller = playerController.GetComponent<PlayerPrimaryController>();
        controller.canMove = false;

        RpcDeathUI();

        //respawn
        //NetworkServer.Destroy(playerController);
        //SpawnPlayer(connectionToClient);
        StartCoroutine(Respawn());
    }

    [ClientRpc]
    void RpcDeathUI()
    {
        if (!hasAuthority)
            return;

        PlayerPrimaryController controller = playerController.GetComponent<PlayerPrimaryController>();
        controller.PlayerUI.SetActive(false);
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        NetworkServer.Destroy(playerController);
        SpawnPlayer(connectionToClient);
    }


    public GameObject PlayerController//used in playerspawnsystem
    {
        get { return playerController; }
        set { playerController = value; }
    }
    public Vector3 SpawnPoint//used in playerspawnsystem
    {
        get { return spawnPoint; }
        set { spawnPoint = value; }
    }
    public Quaternion SpawnFacing//used in playerspawnsystem
    {
        get { return spawnFacing; }
        set { spawnFacing = value; }
    }
}

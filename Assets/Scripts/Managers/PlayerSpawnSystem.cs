using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject playermanager = null;
    [SerializeField] private GameObject playercontroller = null;

    private static List<Transform> spawnPoints = new List<Transform>();
    private static List<int> indexList = new List<int>();

    private int nextIndex = 0;

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    public GameObject PlayerController
    {
        get { return playercontroller; }
    }

    public override void OnStartServer()
    {
        //populate indexlist
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            indexList.Add(i);
        }
        //jumble up indexlist
        for (int j = 0; j < indexList.Count; j++)
        {
            int CurrentIndex = indexList[j];
            int randomIndex = Random.Range(j, indexList.Count);
            indexList[j] = indexList[randomIndex];
            indexList[randomIndex] = CurrentIndex;
        }

        GameNetworkManager.OnServerReadied += SpawnPlayer;
        //for (int j = 0; j < indexList.Count; j++)
        //    Debug.Log(indexList[j]);
        
    }

    public override void OnStartClient()
    {
        //InputManager.Add(ActionMapNames.Player);
        //InputManager.Controls.Player.Look.Enable();
    }

    [ServerCallback]
    private void OnDestroy() => GameNetworkManager.OnServerReadied -= SpawnPlayer;

    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(indexList[nextIndex]);

        if (spawnPoint == null)
        {
            Debug.LogError($"Missing spawn point for player {nextIndex}");
            return;
        }

        GameObject playerControllerInstance = Instantiate(playercontroller, spawnPoints[indexList[nextIndex]].position, spawnPoints[indexList[nextIndex]].rotation);
        //NetworkServer.Spawn(playerControllerInstance, conn);
        NetworkServer.AddPlayerForConnection(conn, playerControllerInstance);



        /*
        GameObject playerManagerInstance = Instantiate(playermanager, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(playerManagerInstance, conn);

        playerManagerInstance.GetComponent<PlayerManager>().PlayerController = playerControllerInstance;
        playerManagerInstance.GetComponent<PlayerManager>().SpawnPoint = spawnPoints[indexList[nextIndex]].position;
        playerManagerInstance.GetComponent<PlayerManager>().SpawnFacing = spawnPoints[indexList[nextIndex]].rotation;

        playerControllerInstance.GetComponent<UnitStats>().PlayerManager = playerManagerInstance;
        */

        nextIndex++;
    }

    
}

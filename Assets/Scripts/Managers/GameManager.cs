using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    private void Awake()
    {
        if (Instance)//checks if another GameManager exists
        {
            Destroy(gameObject);//there can only be one GameManager
            return;
        }
        DontDestroyOnLoad(gameObject);//keep this GameManager
        Instance = this;
    }

    /*
    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.buildIndex == 1)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }
    */

    
}

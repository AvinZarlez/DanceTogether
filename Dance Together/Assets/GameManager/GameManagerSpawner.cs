using UnityEngine;
using UnityEngine.Networking;

public class GameManagerSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject gameManagerPrefab;

    public static GameObject gms;

    public override void OnStartServer()
    {
        gms = Instantiate(gameManagerPrefab);
        NetworkServer.Spawn(gms);
        base.OnStartServer();
    }

    public void StartGame()
    {
        gms.GetComponent<GameManagerScript>().StartGame();
    }
}
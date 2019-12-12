using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{
    public GameObject[] prefabs;
    public PlayerInputManager inputSystem;
    public int nbSteps;
    public float stepValue;
    public float freezeFrameDuration;
    public Vector3 groundOffset;
    private int nbPlayerReady;
    [HideInInspector] public List<Vector3> spots;
    public static MenuManager instance { get; private set; }
    public Cinemachine.CinemachineVirtualCamera camera;
    public InterpItem cameraCenterPoint;

    public delegate void Strike(int score);
    public Strike onStrike;

    private void Awake()
    {
        GameInfos.playerInfos = new List<PlayerInfo>();
        nbPlayerReady = 0;
        Time.timeScale = 1f;
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        //GameInfos.playerInfos = GameInfos.playerInfos;
        spots = new List<Vector3>();

        float startPoint = -((nbSteps * stepValue) - stepValue / 2);

        for (int i = 0; i < nbSteps * 2; ++i)
        {
            spots.Add(new Vector3(startPoint + i * stepValue, 0, 0) - groundOffset);
        }
        camera = Camera.main.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
    }
    
    public void PlayerReady() {
        ++nbPlayerReady;
        if (nbPlayerReady == 2) {
            GameInfos.playerInfos[0].controller.Unsubscribe();
            Destroy(GameInfos.playerInfos[0].controller.gameObject);
            GameInfos.playerInfos[1].controller.Unsubscribe();
            Destroy(GameInfos.playerInfos[1].controller.gameObject);
            Invoke("LoadScene", 0.5f);
        }
    }

    private void LoadScene() {
        SceneManager.LoadSceneAsync(1);
    }

    public int NewPlayer(PlayerController newPlayer)
    {
        int playerIndex = GameInfos.playerInfos.Count;

        if (playerIndex == 2)
        {
            Destroy(newPlayer);
            return 0;
        }

        switch (playerIndex)
        {
            case 0:
                newPlayer.transform.position = spots[nbSteps - 1];
                newPlayer.facingLeft = false;
                inputSystem.playerPrefab = prefabs[1];
                break;
            case 1:
                newPlayer.transform.position = spots[nbSteps];
                newPlayer.facingLeft = true; //ici
                /*
                newPlayer.opponent = playerInfos[0].controller;
                playerInfos[0].controller.opponent = newPlayer;
                cameraCenterPoint.startObject = playerInfos[0].controller.transform;
                cameraCenterPoint.endObject = newPlayer.transform;*/
                break;
        }
        if (playerIndex <= 2) {
            if (newPlayer.GetComponent<PlayerInput>().devices.Count > 1) {
                GameInfos.playerInfos.Add(new PlayerInfo(newPlayer, playerIndex, newPlayer.GetComponent<PlayerInput>().devices[0], newPlayer.GetComponent<PlayerInput>().devices[1]));
            } else {
                GameInfos.playerInfos.Add(new PlayerInfo(newPlayer, playerIndex, newPlayer.GetComponent<PlayerInput>().devices[0]));
            }
        }

        return playerIndex;
    }

    public Vector3 GetDashPos(int playerIndex)
    {
        PlayerController pc = GameInfos.playerInfos[playerIndex].controller;
        if (pc.facingLeft)
        {
            return (spots[nbSteps + pc.currentSpotIndex]);
        }
        else
        {
            return (spots[nbSteps - pc.currentSpotIndex - 1]);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;


public class MenuManager : MonoBehaviour
{
    public class PlayerInfo
    {
        [HideInInspector] public PlayerController controller;
        [HideInInspector] public int playerIndex;
        [HideInInspector] public int score;

        public PlayerInfo(PlayerController tController, int tPlayerIndex)
        {
            controller = tController;
            playerIndex = tPlayerIndex;
        }
    }

    public GameObject[] prefabs;
    public PlayerInputManager inputSystem;
    public int nbSteps;
    public float stepValue;
    public float freezeFrameDuration;
    public Vector3 groundOffset;
    [HideInInspector] public List<Vector3> spots;
    public List<PlayerInfo> playerInfos;
    public static MenuManager instance { get; private set; }
    public Cinemachine.CinemachineVirtualCamera camera;
    public InterpItem cameraCenterPoint;

    public delegate void Strike(int score);
    public Strike onStrike;

    private void Awake()
    {
        Time.timeScale = 1f;
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        playerInfos = new List<PlayerInfo>();
        spots = new List<Vector3>();

        float startPoint = -((nbSteps * stepValue) - stepValue / 2);

        for (int i = 0; i < nbSteps * 2; ++i)
        {
            spots.Add(new Vector3(startPoint + i * stepValue, 0, 0) - groundOffset);
        }
        camera = Camera.main.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public int NewPlayer(PlayerController newPlayer)
    {
        int playerIndex = playerInfos.Count;

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
        playerInfos.Add(new PlayerInfo(newPlayer, playerIndex));

        return playerIndex;
    }


    public Vector3 GetDashPos(int playerIndex)
    {
        PlayerController pc = playerInfos[playerIndex].controller;
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

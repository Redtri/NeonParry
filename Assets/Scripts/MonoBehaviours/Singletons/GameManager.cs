using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class GameManager : MonoBehaviour
{
    public class PlayerInfo {
        public PlayerController controller;
        public int playerIndex;
        public int score;

        public PlayerInfo(PlayerController tController, int tPlayerIndex) {
            controller = tController;
            playerIndex = tPlayerIndex;
        }
    }

    public int nbSteps;
    public float stepValue;
    [HideInInspector] public List<Vector3> spots;
    public List<PlayerInfo> playerInfos;
    public static GameManager instance { get; private set; }
    public Cinemachine.CinemachineVirtualCamera camera;

    private void Awake() {
        Time.timeScale = 1f;
        if (!instance) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
        playerInfos = new List<PlayerInfo>();
        spots = new List<Vector3>();

        float startPoint = -((nbSteps * stepValue) - stepValue / 2);

        for (int i = 0; i < nbSteps*2; ++i) {
            spots.Add(new Vector3(startPoint + i * stepValue, 0, 0));
        }
        camera = Camera.main.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int NewPlayer(PlayerController newPlayer) {
        int playerIndex = playerInfos.Count;

        if(playerIndex == 2) {
            Destroy(newPlayer);
            return 0;
        }

        switch (playerIndex) {
            case 0:
                newPlayer.transform.position = spots[nbSteps-1];
                newPlayer.facingLeft = false;
                break;
            case 1:
                newPlayer.transform.position = spots[nbSteps];
                newPlayer.facingLeft = true;
                newPlayer.opponent = playerInfos[0].controller;
                playerInfos[0].controller.opponent = newPlayer;
                break;
        }
        playerInfos.Add(new PlayerInfo(newPlayer, playerIndex));

        return playerIndex;
    }

    public void StrikeSuccessful(int playerIndex) {
        Camera.main.GetComponent<CameraShake>().Shake();
        ++ playerInfos[playerIndex].score;
    }

    public Vector3 GetDashPos(int playerIndex) {
        PlayerController pc = playerInfos[playerIndex].controller;
        if (pc.facingLeft) {
            return (spots[nbSteps + pc.currentSpotIndex]);
        } else {
            return (spots[nbSteps - pc.currentSpotIndex - 1]);
        }
    }
}

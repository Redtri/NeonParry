using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
    [HideInInspector] public List<Vector3> spotsLeft;
    [HideInInspector] public List<Vector3> spotsRight;
    public List<PlayerInfo> playerInfos;
    public static GameManager instance { get; private set; }

    private void Awake() {
        if (!instance) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
        playerInfos = new List<PlayerInfo>();
        spotsLeft = new List<Vector3>();
        spotsRight = new List<Vector3>();

        for (int i = 0; i < nbSteps; ++i) {
            spotsRight.Add(new Vector3(i * stepValue + (stepValue / 2), 0f, 0f));
            spotsLeft.Add(new Vector3((i * stepValue + (stepValue / 2)) * -1f, 0f, 0f));
        }
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
                newPlayer.transform.position = spotsLeft[0];
                newPlayer.facingLeft = false;
                break;
            case 1:
                newPlayer.transform.position = spotsRight[0];
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
}

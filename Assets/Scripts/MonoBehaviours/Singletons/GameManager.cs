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

    public List<PlayerInfo> playerInfos;
    public static GameManager instance { get; private set; }

    private void Awake() {
        if (!instance) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
        playerInfos = new List<PlayerInfo>();
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
                newPlayer.transform.position -= new Vector3(0.125f, 0, 0);
                newPlayer.facingLeft = false;
                break;
            case 1:
                newPlayer.transform.position += new Vector3(0.125f, 0, 0);
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

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

        playerInfos.Add(new PlayerInfo(newPlayer, playerIndex));

        return playerIndex;
    }

    public void StrikeSuccessful(int playerIndex) {
        ++ playerInfos[playerIndex].score;
    }
}

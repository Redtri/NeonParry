using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour
{
    public class PlayerInfo {
        [HideInInspector] public PlayerController controller;
        [HideInInspector] public int playerIndex;
        [HideInInspector] public int score;

        public PlayerInfo(PlayerController tController, int tPlayerIndex) {
            controller = tController;
            playerIndex = tPlayerIndex;
        }
    }

    public GameObject[] prefabs;
    public PlayerInputManager inputSystem;
    public int nbExchangeForARound;
    //public int nbRoundForAMatch;
    [HideInInspector] public int nbRoundForAMatch = 2; //set by the exercices, must be in maximu 3 rounds
    public List<int[]> score; //[0] will stock the number of round won by each player, next well be stock the number of exchange won by each player for each round
    public int currentRound;
    private bool startNewMatch;
    private float startStopTime;
    public int stopDuration;
    private bool isStop;
    public int nbSteps;
    public float stepValue;
    public float freezeFrameDuration;
    public Vector3 groundOffset;
    [HideInInspector] public List<Vector3> spots;
    public List<PlayerInfo> playerInfos;
    public static GameManager instance { get; private set; }
    public Cinemachine.CinemachineVirtualCamera camera;
    public InterpItem cameraCenterPoint;

    public delegate void Strike(int score);
    public Strike onStrike;

    private void Awake() {
        Time.timeScale = 1f;
        if (!instance) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
        initMatch();
        playerInfos = new List<PlayerInfo>();
        spots = new List<Vector3>();
        startNewMatch = false;
        isStop = false;

        float startPoint = -((nbSteps * stepValue) - stepValue / 2);

        for (int i = 0; i < nbSteps*2; ++i) {
            spots.Add(new Vector3(startPoint + i * stepValue, 0, 0) - groundOffset);
        }
        camera = Camera.main.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update() {
        //isStopGame();
        if (startNewMatch)
        {
            resetMatch();
        }
    }

    public int NewPlayer(PlayerController newPlayer) {
        int playerIndex = playerInfos.Count;

        if(playerIndex == 2) {
            Destroy(newPlayer);
            return 0;
        }

        switch (playerIndex) {
            case 0:
                placePlayer(newPlayer, playerIndex); newPlayer.facingLeft = false;
                inputSystem.playerPrefab = prefabs[1];
                break;
            case 1:
                placePlayer(newPlayer, playerIndex);
                newPlayer.facingLeft = true;
                newPlayer.opponent = playerInfos[0].controller;
                playerInfos[0].controller.opponent = newPlayer;
                cameraCenterPoint.startObject = playerInfos[0].controller.transform;
                cameraCenterPoint.endObject = newPlayer.transform;
                break;
        }
        playerInfos.Add(new PlayerInfo(newPlayer, playerIndex));

        return playerIndex;
    }

    public void placePlayer(PlayerController player, int playerIndex)
    {
        switch (playerIndex)
        {
            case 0:
                player.transform.position = spots[nbSteps - 1];
                break;
            case 1:
                player.transform.position = spots[nbSteps];
                break;
        }
    }

    public int StrikeSuccessful(int playerIndex) { // strike succesfull return 0, at the end of a round return 1, at the end of the match return 2
        Camera.main.GetComponent<CameraShake>().Shake(16f, 16f, 1.5f);
        ++score[currentRound][playerIndex];
        AudioManager.instance.UpdateMusic(1);
        StartCoroutine(StopFreezeFrame());
        if (score[currentRound][playerIndex] >= nbExchangeForARound)
        {
            ++score[0][playerIndex];
            if (score[0][playerIndex] >= nbRoundForAMatch)
            {
                startNewMatch = true;
                return 2;
            }
            else
            {
                score.Add(new int[2] { 0, 0 });
                ++currentRound;

                return 1;
            }
        } else
        {
            return 0;
        }
    }

    public void initMatch()
    { //initilize the score
        //Debug.Log("match Start");
        currentRound = 1;
        score = new List<int[]>();
        score.Add(new int[2] { 0, 0 }); //for round won count
        score.Add(new int[2] { 0, 0 }); //for exchange won count
    }

    public void resetMatch()
    { //reste the score to 0/0
        //Debug.Log("match reset");
        score.Clear();
        initMatch();

        //yes no for() because it's more understable this way
        placePlayer(playerInfos[0].controller, 0); //replace player 1
        placePlayer(playerInfos[1].controller, 1); //replace player 2
        playerInfos[0].controller.onNeutral(); //stop at Neutral for player 1
        playerInfos[1].controller.onNeutral(); //stop at Neutral for player 2
        playerInfos[0].controller.fury.resetFury();
        playerInfos[1].controller.fury.resetFury();
        startNewMatch = false;
    }

    private IEnumerator StopFreezeFrame() {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(freezeFrameDuration);
        Time.timeScale = 1f;
    }

    public void isStopGame()
    {
        StartCoroutine(Unstop());
    }

    private IEnumerator Unstop()
    {
        Debug.Log("STOP");
        playerInfos[0].controller.isStop = true;
        playerInfos[1].controller.isStop = true;
        yield return new WaitForSecondsRealtime(stopDuration);
        Debug.Log("OK");
        playerInfos[0].controller.isStop = false;
        playerInfos[1].controller.isStop = false;
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

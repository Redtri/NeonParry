using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
    public AnimationCurve freezeFrameCurve;
    public Vector3 groundOffset;
    [HideInInspector] public List<Vector3> spots;
    public static GameManager instance { get; private set; }
    public Cinemachine.CinemachineVirtualCamera camera;
    public InterpItem cameraCenterPoint;

    private int currentNbPlayers = 0;

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
        spots = new List<Vector3>();
        startNewMatch = false;
        isStop = false;

        float startPoint = -((nbSteps * stepValue) - stepValue / 2);

        for (int i = 0; i < nbSteps*2; ++i) {
            spots.Add(new Vector3(startPoint + i * stepValue, 0, 0) - groundOffset);
        }
        camera = Camera.main.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();


        if (SceneManager.GetActiveScene().name != "IntroScene") {
            LoadPlayerInfos();
        }
    }

    // Update is called once per frame
    void Update() {
        //isStopGame();
        if (startNewMatch) {
            resetMatch();
        }
    }

    public void LoadPlayerInfos() {
        inputSystem.JoinPlayer(0, 0, "Gamepad", GameInfos.playerInfos[0].device);
        inputSystem.playerPrefab = prefabs[1];
        inputSystem.JoinPlayer(1, 0, "Gamepad", GameInfos.playerInfos[1].device);
    }

    public int NewPlayer(PlayerController newPlayer) {

        GameInfos.playerInfos[currentNbPlayers].controller = newPlayer;
        newPlayer.GetComponent<SpriteSwaper>().PickSkin(GameInfos.playerInfos[currentNbPlayers].skin);

        switch (currentNbPlayers) {
            case 0:
                placePlayer(newPlayer, currentNbPlayers);
                newPlayer.facingLeft = false;
                break;
            case 1:
                placePlayer(newPlayer, currentNbPlayers);
                newPlayer.facingLeft = true;
                newPlayer.opponent = GameInfos.playerInfos[0].controller;
                GameInfos.playerInfos[0].controller.opponent = newPlayer;
                cameraCenterPoint.startObject = GameInfos.playerInfos[0].controller.transform;
                cameraCenterPoint.endObject = newPlayer.transform;
                break;
        }
        //GameInfos.playerInfos.Add(new PlayerInfo(newPlayer, playerIndex, ));

        ++currentNbPlayers;
        return currentNbPlayers-1;
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
        StartCoroutine(FreezeFrame(freezeFrameDuration));
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

    private IEnumerator FreezeFrame(float duration) {
        float refreshTime = Time.unscaledTime;

        while (Time.unscaledTime - refreshTime < duration) {
            Time.timeScale = freezeFrameCurve.Evaluate((Time.unscaledTime - refreshTime) / duration);
            yield return new WaitForSecondsRealtime(0.01f);
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
        StartCoroutine(MatchReset());
        //yes no for() because it's more understable this way
        GameInfos.playerInfos[0].controller.onNeutral(); //stop at Neutral for player 1
        GameInfos.playerInfos[1].controller.onNeutral(); //stop at Neutral for player 2
        startNewMatch = false;
    }

    private IEnumerator MatchReset() {
        yield return new WaitForSecondsRealtime(6f);
        score.Clear();
        initMatch();
        GameInfos.playerInfos[0].controller.fury.resetFury();
        GameInfos.playerInfos[1].controller.fury.resetFury();
        GameInfos.playerInfos[0].controller.currentSpotIndex = 0;
        GameInfos.playerInfos[1].controller.currentSpotIndex = 0;
        placePlayer(GameInfos.playerInfos[0].controller, 0); //replace player 1
        placePlayer(GameInfos.playerInfos[1].controller, 1); //replace player 2
    }

    public void isStopGame()
    {
        StartCoroutine(Unstop());
    }

    private IEnumerator Unstop()
    {
        Debug.Log("STOP");
        GameInfos.playerInfos[0].controller.isStop = true;
        GameInfos.playerInfos[1].controller.isStop = true;
        yield return new WaitForSecondsRealtime(stopDuration);
        Debug.Log("OK");
        GameInfos.playerInfos[0].controller.isStop = false;
        GameInfos.playerInfos[1].controller.isStop = false;
    }


    public Vector3 GetDashPos(int playerIndex) {
        PlayerController pc = GameInfos.playerInfos[playerIndex].controller;
        if (pc.facingLeft) {
            return (spots[nbSteps + pc.currentSpotIndex]);
        } else {
            return (spots[nbSteps - pc.currentSpotIndex - 1]);
        }
    }
}

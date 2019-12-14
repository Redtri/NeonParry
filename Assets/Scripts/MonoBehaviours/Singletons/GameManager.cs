using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

public enum eGAME_PHASE { TITLE, MENU, GAME, SCORE}

public delegate void Counter(int count, int max);
public delegate void tEvent();

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
    public AnimationCurve[] freezeFrameCurve;
    public Vector3 groundOffset;
    [HideInInspector] public List<Vector3> spots;
    public static GameManager instance { get; private set; }
    public Cinemachine.CinemachineVirtualCamera camera;
    public InterpItem cameraCenterPoint;
    public eGAME_PHASE currentPhase;
    public Volume postProcess;

    public GameTransition onRoundEnd;
    public GameTransition onRoundStart;
    public GameTransition onMatchEnd;
    public tEvent onScore;

    private int currentNbPlayers = 0;

    public delegate void Strike(int[] score);
    public Strike onStrike;
    public Counter onCountdown;

    private void Awake() {
        if (!instance) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
        Time.timeScale = 1f;
        spots = new List<Vector3>();
        float startPoint = -((nbSteps * stepValue) - stepValue / 2);

        for (int i = 0; i < nbSteps * 2; ++i) {
            spots.Add(new Vector3(startPoint + i * stepValue, 0, 0) - groundOffset);
        }
        camera = Camera.main.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();

    }

    private void Start() {
        if (SceneManager.GetActiveScene().name != "IntroScene") {
            LoadPlayerInfos();
            StartCoroutine(MatchReset(true));
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
        if(player != null) {
            switch (playerIndex) {
                case 0:
                    player.transform.position = spots[nbSteps - 1];
                    player.currentSpotIndex = 0;
                    break;
                case 1:
                    player.transform.position = spots[nbSteps];
                    player.currentSpotIndex = 0;
                    break;
            }
        }
    }

    public void CameraShake(float shake, float frequency, float duration) {
        Camera.main.GetComponent<CameraShake>().Shake(shake, frequency, duration);
    }

    public int StrikeSuccessful(int playerIndex) { // strike succesfull return 0, at the end of a round return 1, at the end of the match return 2
        CameraShake(16f, 16f, 1.5f);
        ++score[currentRound][playerIndex];
        onScore?.Invoke();
        onStrike?.Invoke(score[currentRound]);
        PostProcessManager.instance.Glitch(1, .5f, true);
        StartCoroutine(FreezeFrame(0, freezeFrameDuration));
        if (score[currentRound][playerIndex] >= nbExchangeForARound) {
            ++score[0][playerIndex];
            if (score[0][playerIndex] >= nbRoundForAMatch) {
                StartCoroutine(MatchStop());
                return 2;
            } else {
                score.Add(new int[2] { 0, 0 });
                StartCoroutine(RoundStop());
                ++currentRound;
                AudioManager.instance.NewRound();

                return 1;
            }
        } else {
            return 0;
        }
    }

    private bool freezing;

    public void Freeze(int curveIndex, float duration) {
        StartCoroutine(FreezeFrame(curveIndex, duration));
    }

    public IEnumerator FreezeFrame(int curveIndex, float duration) {
        if (!freezing) {
            float refreshTime = Time.unscaledTime;
            freezing = true;

            while (Time.unscaledTime - refreshTime < duration) {
                Time.timeScale = freezeFrameCurve[curveIndex].Evaluate((Time.unscaledTime - refreshTime) / duration);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            freezing = false;
        } else {
            yield return null;
        }
    }

    public IEnumerator RoundStop() {
        StopPlayers(true);
        onRoundEnd?.Invoke(true);
        yield return new WaitForSeconds(stopDuration);
        StopPlayers(false);
        StartCoroutine(RoundReset());
        yield return null;
    }

    public IEnumerator RoundReset(bool newGame = false)
    {
        //  print("RoundReset");
        float refreshTime = Time.unscaledTime;
        float duration = 3;
        int temp = 0;

        StopPlayers(true);
        onRoundStart?.Invoke(false);
        placePlayer(GameInfos.playerInfos[0].controller, 0); //replace player 1
        placePlayer(GameInfos.playerInfos[1].controller, 1); //replace player 2

        //Otherwise it would throw errors about animator reference not set
        if (!newGame) {
            GameInfos.playerInfos[0].controller.onNeutral(); //stop at Neutral for player 1
            GameInfos.playerInfos[1].controller.onNeutral(); //stop at Neutral for player 2
        }
        while (temp < duration) {
            onCountdown?.Invoke(temp, (int)duration - 1);
            PostProcessManager.instance.Glitch(temp, .1f, true, true);
            yield return new WaitForSeconds(1f);
            ++temp;
        }
        StopPlayers(false);
        yield return null;
    }


    public IEnumerator MatchStop() {
        int count = 0;
        StopPlayers(true);
        while (count < 2) {
            if(count > 0) {
                onMatchEnd?.Invoke(true);
            }
            yield return new WaitForSeconds(MenuManager.instance.gameStartTransition);
            ++count;
        }
        StopPlayers(false);
        GameInfos.playerInfos[0].controller.Unsubscribe();
        GameInfos.playerInfos[1].controller.Unsubscribe();
        SceneManager.LoadSceneAsync(0);
        yield return null;
    }

    private IEnumerator MatchReset(bool newGame = false) {
        if (!newGame) {
            yield return new WaitForSecondsRealtime(stopDuration);
        } else {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        NewMatch(newGame);
        StartCoroutine(RoundReset(newGame));
        yield return null;
    }

    public void NewMatch(bool newGame = false) {
        currentRound = 1;
        score = new List<int[]>();
        score.Add(new int[2] { 0, 0 }); //for round won count
        score.Add(new int[2] { 0, 0 }); //for exchange won count
        GameInfos.playerInfos[0].controller.fury.resetFury();
        GameInfos.playerInfos[1].controller.fury.resetFury();
        placePlayer(GameInfos.playerInfos[0].controller, 0); //replace player 1
        placePlayer(GameInfos.playerInfos[1].controller, 1); //replace player 2
        if (!newGame) {
            GameInfos.playerInfos[0].controller.onNew();
            GameInfos.playerInfos[1].controller.onNew();
        }
        GameInfos.playerInfos[0].controller.onNeutral(); //stop at Neutral for player 1
        GameInfos.playerInfos[1].controller.onNeutral(); //stop at Neutral for player 2
    }

    public void StopPlayers(bool stop) {
        GameInfos.playerInfos[0].controller.isStop = stop;
        GameInfos.playerInfos[1].controller.isStop = stop;
    }

    public void StopPlayers(float duration) {
        StartCoroutine(TempStop(duration));
    }

    public IEnumerator TempStop(float duration) {
        StopPlayers(true);
        yield return new WaitForSeconds(duration);
        StopPlayers(false);
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

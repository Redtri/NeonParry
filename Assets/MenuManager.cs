using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public delegate void GameTransition(bool fadeOut = true, float overrideDuration = 0f);

public class MenuManager : MonoBehaviour
{
    public GameObject[] prefabs;
    public PlayerInputManager inputSystem;
    public Camera menuCam;
    public GameObject skinSelectorPrefab;
    public float gameStartTransition;
    public int nbSteps;
    public float stepValue;
    public float freezeFrameDuration;
    public Vector3 groundOffset;
    private int nbPlayerReady;
    [HideInInspector] public List<Vector3> spots;
    public static MenuManager instance { get; private set; }

    public delegate void Strike(int score);
    public Strike onStrike;

    public GameTransition onGameStart;
    public GameTransition onScreen;

    private void Awake()
    {
        onScreen?.Invoke(false, .5f);
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
        spots = new List<Vector3>();

        float startPoint = -((nbSteps * stepValue) - stepValue / 2);

        for (int i = 0; i < nbSteps * 2; ++i)
        {
            spots.Add(new Vector3(startPoint + i * stepValue, 0, 0) - groundOffset);
        }
    }

    public void PlayerReady(int playerIndex) {
        if (playerIndex == 1) {
            StartCoroutine(BlockPlayer(playerIndex, GameInfos.playerInfos[playerIndex].controller.dashAction.currentActionDuration));
        }
        ++nbPlayerReady;
        if (nbPlayerReady == 2) {
            GameInfos.playerInfos[0].controller.Unsubscribe();
            GameInfos.playerInfos[1].controller.Unsubscribe();
            onGameStart?.Invoke(true);
            Invoke("LoadScene", gameStartTransition);
        }
    }

    private IEnumerator BlockPlayer(int index, float delay, bool block = true) {
        yield return new WaitForSeconds(delay);
        GameInfos.playerInfos[index].controller.isStop = block;
    }

    private void LoadScene() {
        SceneManager.LoadSceneAsync(1);
    }

    public int NewPlayer(PlayerController newPlayer) {
        menuCam.GetComponent<Animator>().SetTrigger("travelling");
        int playerIndex = GameInfos.playerInfos.Count;
        if(playerIndex == 0) {
            AudioManager.instance.specialEffets[0].Post(gameObject);
            StartCoroutine(BlockPlayer(playerIndex, 4f, false));
        } else {
            StartCoroutine(BlockPlayer(playerIndex, 1.5f, false));
        }

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
            SkinSelector selector = Instantiate(skinSelectorPrefab, newPlayer.transform.position + new Vector3(15f, 0f, 0f) * ((newPlayer.facingLeft) ? -1f : 1f) + GameManager.instance.groundOffset, Quaternion.identity, null).GetComponent<SkinSelector>();

            selector.Init(newPlayer.GetComponent<SpriteController>());
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

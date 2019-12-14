using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {
    public AK.Wwise.Event[] musicsMenu;
    public AK.Wwise.Event[] musicsGame;

    public AK.Wwise.Event[] ambient;
    public AK.Wwise.Event[] specialEffets;
    public AK.Wwise.Event[] countdown;
    public AK.Wwise.Event stopMusic;

    public static AudioManager instance;

    private void Awake() {
        if (!instance) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
    }

    private void Start() {
        GameManager.instance.onStrike += UpdateMusic;
        GameManager.instance.onCountdown += Countdown;
        switch (SceneManager.GetActiveScene().buildIndex) {
            case 0:
                musicsMenu[0].Post(gameObject);
                break;
            case 1:
                musicsGame[0].Post(gameObject);
                AkSoundEngine.SetState("roundState", "round_01");
                break;
        }
        ambient[SceneManager.GetActiveScene().buildIndex].Post(gameObject);
    }

    private void OnEnable() {
        if(GameManager.instance != null) {
            GameManager.instance.onStrike += UpdateMusic;
        }
    }

    private void OnDisable() {
        GameManager.instance.onStrike -= UpdateMusic;
        GameManager.instance.onCountdown -= Countdown;
    }    

    private void Countdown(int count, int max) {
        countdown[count].Post(gameObject);
    }

    public void NewRound() {
        switch (GameManager.instance.currentRound) {
            case 2:
                AkSoundEngine.SetState("roundState", "round_02");
                AkSoundEngine.PostEvent("Music_Reset", gameObject);
                break;
            case 3:
                AkSoundEngine.SetState("roundState", "round_03");
                AkSoundEngine.PostEvent("Music_Reset", gameObject);
                break;
            default:
                break;
        }
    }

    public void UpdateMusic(int[] index) {
        int highest = 0;


        for(int i = 0; i < index.Length; ++i) {
            if(index[i] > highest) {
                highest = index[i];
            }
        }
        switch (highest) {
            case 0:
                break;
            case 1:
                musicsGame[1].Post(gameObject);
                break;
            case 2:
                musicsGame[2].Post(gameObject);
                break;
            case 3:
                break;
        }
    }

    public void Death() {
        musicsGame[3].Post(gameObject);
    }
}

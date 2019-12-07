using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public AK.Wwise.Event[] events;

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
        events[0].Post(gameObject);
    }

    private void OnEnable() {
        if(GameManager.instance != null) {
            GameManager.instance.onStrike += UpdateMusic;
        }
    }

    private void OnDisable() {
        GameManager.instance.onStrike -= UpdateMusic;
    }    

    public void UpdateMusic(int index) {

        events[index].Post(gameObject);
    }
}

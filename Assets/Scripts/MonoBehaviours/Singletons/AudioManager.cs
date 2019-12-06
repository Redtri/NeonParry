using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AkEvent akEvent;

    private void Start() {
        GameManager.instance.onStrike += UpdateMusic;
    }

    private void OnEnable() {
        if(GameManager.instance != null) {
            GameManager.instance.onStrike += UpdateMusic;
        }
    }

    private void OnDisable() {
        GameManager.instance.onStrike -= UpdateMusic;
    }    

    private void UpdateMusic(int nbStrikes) {
        switch (nbStrikes) {
        case 1:

            break;
        case 2:
            break;
        }
    }
}

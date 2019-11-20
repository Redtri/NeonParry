using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;
    [SerializeField] private Text scoreLeft;
    [SerializeField] private Text scoreRight;

    private void Awake() {
        if (!instance) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }
    }

    private void Update() {
        RefreshScore();
    }

    private void RefreshScore() {
        scoreLeft.text = GameManager.instance.playerInfos[0].score.ToString();
        scoreRight.text = GameManager.instance.playerInfos[1].score.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;
    [SerializeField] private Text scoreLeft;
    [SerializeField] private Text scoreRight;
    public GameObject barLeft;
    public GameObject barRight;
    private float maxValue;
    private Vector3 barLowPosition;
    private float currentValueLeft;
    private float currentValueRight;

    private void Awake() {
        barLowPosition = barLeft.transform.localPosition; //barLeft.transform.position.y;

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
        if (GameManager.instance.playerInfos.Count > 1) {
            maxValue = GameManager.instance.playerInfos[0].controller.fury.highestValueOfFury;

            currentValueLeft = GameManager.instance.playerInfos[0].controller.fury.currentFury;
            currentValueRight = GameManager.instance.playerInfos[1].controller.fury.currentFury;
            barLeft.transform.localPosition = new Vector3(0, barLowPosition.y * (1-(currentValueLeft/maxValue)), 0);
            barRight.transform.localPosition = new Vector3(0, barLowPosition.y * (1 - (currentValueRight / maxValue)), 0);
            scoreLeft.text = GameManager.instance.playerInfos[0].score.ToString();
            scoreRight.text = GameManager.instance.playerInfos[1].score.ToString();
        }
    }
}

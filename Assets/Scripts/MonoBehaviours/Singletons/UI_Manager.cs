using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;
    [SerializeField] private Text scoreLeft;
    [SerializeField] private Text scoreRight;
    public Image fadeImage;
    public GameObject barLeft;
    public GameObject barRight;
    private float maxValue;
    private Vector3 barLowPosition;
    private float currentValueLeft;
    private float currentValueRight;

    private void Awake() {
        barLowPosition = barLeft.transform.localPosition; //barLeft.transform.position.y;
        Fade(false);
        if (!instance) {
            instance = this;
        } else {
            Destroy(this.gameObject);
        }

        MenuManager.instance.onGameStart += Fade;
        GameManager.instance.onRoundEnd += Fade;
        GameManager.instance.onMatchEnd += Fade;
        GameManager.instance.onRoundStart += Fade;
    }

    private void OnDisable() {
        MenuManager.instance.onGameStart -= Fade;
        GameManager.instance.onRoundEnd -= Fade;
        GameManager.instance.onMatchEnd -= Fade;
        GameManager.instance.onRoundStart -= Fade;
    }

    private void Update() {
        RefreshScore();
    }

    private void Fade(bool fadeOut = false) {
        StartCoroutine(Fading(fadeOut));
    }

    private IEnumerator Fading(bool fadeOut = true) {
        float refreshTime = Time.time;

        while (Time.time - refreshTime < MenuManager.instance.gameStartTransition) {
            if (fadeOut) {
                fadeImage.color = new Color(0, 0, 0, (Time.time - refreshTime) / MenuManager.instance.gameStartTransition);
            } else {
                fadeImage.color = new Color(0, 0, 0, 1-((Time.time - refreshTime) / MenuManager.instance.gameStartTransition));
            }
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }

    private void RefreshScore() {
        if (GameInfos.playerInfos.Count > 1) {
            maxValue = GameInfos.playerInfos[0].controller.fury.highestValueOfFury;

            currentValueLeft = GameInfos.playerInfos[0].controller.fury.currentFury;
            currentValueRight = GameInfos.playerInfos[1].controller.fury.currentFury;
            barLeft.transform.localPosition = new Vector3(0, barLowPosition.y * (1-(currentValueLeft/maxValue)), 0);
            barRight.transform.localPosition = new Vector3(0, barLowPosition.y * (1 - (currentValueRight / maxValue)), 0);
            scoreLeft.text = GameManager.instance.score[0][0].ToString() + " " + GameManager.instance.score[GameManager.instance.currentRound][0].ToString();
            scoreRight.text = GameManager.instance.score[0][1].ToString() + " " + GameManager.instance.score[GameManager.instance.currentRound][1].ToString();
        }
    }
}

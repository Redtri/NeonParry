using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;
    [SerializeField] private Text scoreLeft;
    [SerializeField] private Text scoreRight;
    [SerializeField] private Image countdownImage;
    [SerializeField] private Sprite[] countImages;
    public Image fadeImage;
    public Image creditsImage;
    public Image barLeft;
    public Image barRight;
    public Image barLeftSmooth;
    public Image barRightSmooth;
    public Image FuryJarLeft;
    public Image FuryJarRight;

    public Image FuryMaskLeft;
    public Image FuryMaskRight;

    private float maxValue;

    private float currentValueLeft;
    private float currentValueRight;

    public Sprite FullHP;     //FULL
    public Sprite LessHP;   //-1
    public Sprite LowHP;  //-2

    public Sprite FullHPMask;     //FULL
    public Sprite LessHPMask;   //-1
    public Sprite LowHPMask;  //-2

    public Image scoreLeft1;
    public Image scroreLeft2;
    public Image scoreRight1;
    public Image scoreRight2;

    public Sprite spriteOn;
    public Sprite spriteOff;

    public Transform BarTransform;
    public Transform[] joinMessage;


    private void Awake()
    {
        InitFury();
        StartCoroutine(BlackScreen(2f));
        /*FULLHP = Resources.Load<Sprite>("Assets/Images/UI/UI_3pv.png");      //FULL
        LESSHP = Resources.Load<Sprite>("Assets/Images/UI/UI_2pv.png");    //-1
        LESSERHP = Resources.Load<Sprite>("Assets/Images/UI/UI_1pv.png");  //-2
        */
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        MenuManager.instance.onGameStart += Fade;
        MenuManager.instance.onScreen += Fade;
        GameManager.instance.onRoundEnd += Fade;
        GameManager.instance.onMatchEnd += OnMatchEnd;
        GameManager.instance.onRoundStart += Fade;
        GameManager.instance.onCountdown += Countdown;
        GameManager.instance.onScore += RefreshScore;
    }

    private void Start() {
        MenuManager.instance.onJoin += PlayerJoined;
    }

    private void OnDisable()
    {
        MenuManager.instance.onGameStart -= Fade;
        MenuManager.instance.onScreen -= Fade;
        MenuManager.instance.onJoin -= PlayerJoined;
        GameManager.instance.onRoundEnd -= Fade;
        GameManager.instance.onMatchEnd -= OnMatchEnd;
        GameManager.instance.onRoundStart -= Fade;
        GameManager.instance.onCountdown -= Countdown;
        GameManager.instance.onScore -= RefreshScore;
    }

    private void Fade(bool fadeOut = false, float overrideDuration = 0f) {
        StartCoroutine(Fading(fadeOut, overrideDuration));
    }

    private IEnumerator BlackScreen(float duration) {
        fadeImage.color = new Color(0, 0, 0, 1f);
        yield return new WaitForSeconds(duration);
        Fade(false, .5f);
    }

    private IEnumerator Fading(bool fadeOut = true, float overrideDuration = 0f)
    {
        float refreshTime = Time.time;
        float duration = (overrideDuration == 0f) ? MenuManager.instance.gameStartTransition : overrideDuration;

        while (Time.time - refreshTime < (duration+0.1f))
        {
            if (fadeOut)
            {
                fadeImage.color = new Color(0, 0, 0, (Time.time - refreshTime) / duration);
            }
            else
            {
                fadeImage.color = new Color(0, 0, 0, 1 - ((Time.time - refreshTime) / duration));
            }
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }

    private void Update()
    {
        if (GameInfos.playerInfos.Count > 1)
        {
            maxValue = GameInfos.playerInfos[0].controller.fury.highestValueOfFury;

            currentValueLeft = GameInfos.playerInfos[0].controller.fury.currentFury;
            currentValueRight = GameInfos.playerInfos[1].controller.fury.currentFury;
            barLeft.fillAmount = currentValueLeft / maxValue;
            barRight.fillAmount = currentValueRight / maxValue;
            barLeftSmooth.fillAmount += (barLeft.fillAmount - barLeftSmooth.fillAmount) / 50;
            barRightSmooth.fillAmount += (barRight.fillAmount - barRightSmooth.fillAmount) / 50;
            if(GameManager.instance.score != null && GameManager.instance.score.Count > 0) {
                switch (GameManager.instance.score[0][0]) {
                    case 0:
                        scoreLeft1.sprite = spriteOff;
                        scroreLeft2.sprite = spriteOff;
                        break;
                    case 1:
                        scoreLeft1.sprite = spriteOn;
                        scroreLeft2.sprite = spriteOff;
                        break;
                    case 2:
                        scoreLeft1.sprite = spriteOn;
                        scroreLeft2.sprite = spriteOn;
                        break;

                }
                switch (GameManager.instance.score[0][1]) {
                    case 0:
                        scoreRight1.sprite = spriteOff;
                        scoreRight2.sprite = spriteOff;
                        break;
                    case 1:
                        scoreRight1.sprite = spriteOn;
                        scoreRight2.sprite = spriteOff;
                        break;
                    case 2:
                        scoreRight1.sprite = spriteOn;
                        scoreRight2.sprite = spriteOn;
                        break;

                }
            }
        }
    }

    private void RefreshScore(int playerIndex)
    {
        switch (playerIndex) {
            case 0:
                StartCoroutine(ShakeBar(FuryJarRight.transform, .5f));
                break;
            case 1:
                StartCoroutine(ShakeBar(FuryJarLeft.transform, .5f));
                break;
        }
        if (GameInfos.playerInfos.Count > 1)
        {
            scoreLeft.text = GameManager.instance.score[0][0].ToString() + " " + GameManager.instance.score[GameManager.instance.currentRound][0].ToString();
            scoreRight.text = GameManager.instance.score[0][1].ToString() + " " + GameManager.instance.score[GameManager.instance.currentRound][1].ToString();

            switch (GameManager.instance.score[GameManager.instance.currentRound][1])
                {
                    case 0:
                        
                        FuryJarLeft.sprite = FullHP;
                        FuryMaskLeft.sprite = FullHPMask;
                    break;
                    case 1:


                        FuryJarLeft.sprite = LessHP;
                    FuryMaskLeft.sprite = LessHPMask;

                    break;
                default:
                    FuryJarLeft.sprite = LowHP;
                    FuryMaskLeft.sprite = LowHPMask;
                    break;

                }
            switch (GameManager.instance.score[GameManager.instance.currentRound][0])
            {
                case 0:

                    FuryJarRight.sprite = FullHP;
                    FuryMaskRight.sprite = FullHPMask;
                    break;
                case 1:
                        FuryJarRight.sprite = LessHP;
                    FuryMaskRight.sprite = LessHPMask;

                    break;
                default:
                    FuryJarRight.sprite = LowHP;
                    FuryMaskRight.sprite = LowHPMask;

                    break;
            }

        }
    }

    private void Countdown(int count, int max) {
        if(count < max) {
            if (!countdownImage.gameObject.activeInHierarchy) {
                countdownImage.gameObject.SetActive(true);
            }
        } else {
            StartCoroutine(StopCountdown());
        }
        countdownImage.sprite = countImages[count];
        countdownImage.GetComponent<Animator>().SetTrigger("count");
    }

    private IEnumerator StopCountdown() {
        yield return new WaitForSeconds(1f);
        countdownImage.gameObject.SetActive(false);
    }

    private void PlayerJoined(int playerIndex) {
        joinMessage[playerIndex].gameObject.SetActive(false);
    }

    private void InitFury()
    {
        barLeft.fillAmount = 0f;
        barRight.fillAmount = 0f;
        barLeftSmooth.fillAmount = 0f;
        barRightSmooth.fillAmount = 0f;
    }

    private IEnumerator ShakeBar(Transform bar, float duration, float range = 8f, float pace = 0.01f) {
        Vector2 initialPosition = bar.position;

        float playback = 0.0f;

        while (playback < duration) {
            yield return new WaitForSecondsRealtime(pace);
            playback += pace;
            Vector2 v = Random.insideUnitCircle;
            bar.position = initialPosition + v * range;
        }
        bar.position = initialPosition;
        yield return null;
    }

    private void OnMatchEnd(bool fadeOut, float overrideDuration) {
        StartCoroutine(Credits(fadeOut, overrideDuration));
    }

    private IEnumerator Credits(bool fadeOut, float overrideDuration) {
        float refreshTime = Time.time;
        float duration = (overrideDuration == 0f) ? MenuManager.instance.gameStartTransition : overrideDuration;

        while (Time.time - refreshTime < (duration + 0.1f)) {
            if (fadeOut) {
                creditsImage.color = new Color(1, 1, 1, (Time.time - refreshTime) / duration);
            } else {
                creditsImage.color = new Color(1, 1, 1, 1 - ((Time.time - refreshTime) / duration));
            }
            yield return new WaitForSeconds(0.01f);
        }
        if (fadeOut) {
            yield return new WaitForSeconds(4f);
            Fade(true, 2f);
        }
        yield return null;
    }
}

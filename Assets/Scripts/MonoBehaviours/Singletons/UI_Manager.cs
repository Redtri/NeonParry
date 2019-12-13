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

    private void Awake()
    {
        InitFury();

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
        GameManager.instance.onMatchEnd += Fade;
        GameManager.instance.onRoundStart += Fade;
    }

    private void OnDisable()
    {
        MenuManager.instance.onGameStart -= Fade;
        MenuManager.instance.onScreen -= Fade;
        GameManager.instance.onRoundEnd -= Fade;
        GameManager.instance.onMatchEnd -= Fade;
        GameManager.instance.onRoundStart -= Fade;
    }

    private void Update()
    {
        RefreshScore();
    }

    private void Fade(bool fadeOut = false, float overrideDuration = 0f)
    {
        StartCoroutine(Fading(fadeOut, overrideDuration));
    }

    private IEnumerator Fading(bool fadeOut = true, float overrideDuration = 0f)
    {
        float refreshTime = Time.time;
        float duration = (overrideDuration == 0f) ? MenuManager.instance.gameStartTransition : overrideDuration;

        while (Time.time - refreshTime < (duration))
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

    private void RefreshScore()
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
            scoreLeft.text = GameManager.instance.score[0][0].ToString() + " " + GameManager.instance.score[GameManager.instance.currentRound][0].ToString();
            scoreRight.text = GameManager.instance.score[0][1].ToString() + " " + GameManager.instance.score[GameManager.instance.currentRound][1].ToString();


                switch (GameManager.instance.score[GameManager.instance.currentRound][1])
                {
                    case 0:
                        
                        FuryJarLeft.sprite = FullHP;
                        FuryMaskLeft.sprite = FullHPMask;
                    break;
                    case 1:

                            Debug.Log("OK ???");
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
                        Debug.Log("OK ???");
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

    private void InitFury()
    {
        barLeft.fillAmount = 0f;
        barRight.fillAmount = 0f;
        barLeftSmooth.fillAmount = 0f;
        barRightSmooth.fillAmount = 0f;
    }
}

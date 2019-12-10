using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private PlayerController controller;
    private SpriteSwaper sprSwaper;


    // Start is called before the first frame update
    void Start()
    {
        sprSwaper = GetComponent<SpriteSwaper>();
        controller = GetComponent<PlayerController>();
        controller.onStrike += StrikeEvent;
    }

    private void OnDisable() {
        controller.onStrike -= StrikeEvent;
    }

    private void StrikeEvent(eDIRECTION direction, float delay) {
        StartCoroutine(SwapSkinCall(direction, delay));
    }

    private IEnumerator SwapSkinCall(eDIRECTION direction, float delay) {
        yield return new WaitForSeconds(delay);
        GameInfos.playerInfos[controller.playerIndex].skin = (int)direction - 1;
        sprSwaper.PickSkin((int)direction-1);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FX_Manager : MonoBehaviour
{
    public static FX_Manager instance;
    public GameObject clashFxPrefab;

    private void Awake() {
        if (!instance) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void ClashFX() {
        if (!clashFxPrefab.activeInHierarchy) {
            AudioManager.instance.specialEffets[1].Post(gameObject);
            StartCoroutine(DelayedFX(clashFxPrefab, .2f, true, 1.5f));
        }
    }

    public void PerfectParryFX() {

    }

    private IEnumerator DelayedFX(GameObject fxPrefab, float tDuration, bool disable = false, float tDisableDelay = 1f) {

        yield return new WaitForSeconds(tDuration);

        fxPrefab.SetActive(true);
        if (disable) {
            StartCoroutine(DisableFX(fxPrefab, tDisableDelay));
        }
    }

    private IEnumerator DisableFX(GameObject fxPrefab, float tDuration = 1f) {

        yield return new WaitForSeconds(tDuration);
        
        fxPrefab.SetActive(false);
    }
}

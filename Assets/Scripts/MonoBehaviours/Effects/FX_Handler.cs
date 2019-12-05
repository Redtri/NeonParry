using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FX_Handler : MonoBehaviour
{
    public GameObject parryFxPrefab;
    public GameObject hitFxPrefab;
    public GameObject furySteamFxPrefab;
    public GameObject furyElecFxPrefab;

    private void Start() {
        var main = furyElecFxPrefab.GetComponent<ParticleSystem>().main;
        main.startSize = new ParticleSystem.MinMaxCurve(0.0f, 0.0f);
    }

    public void UpdateFuryFX(float furyPercent) {
        //0.45 0.87
        print(furyPercent);
        var main = furyElecFxPrefab.GetComponent<ParticleSystem>().main;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f * furyPercent, 0.1f * furyPercent);

        if(furyPercent > 0) {
            //TODO : Handle fury percent with FX feedback amount
            print("Hello FXs");
            furySteamFxPrefab.SetActive(true);
            furyElecFxPrefab.SetActive(true);
        } else {
            print("Bye Fxs");
            furySteamFxPrefab.SetActive(false);
            furyElecFxPrefab.SetActive(false);
        }
    }

    public void SpawnFX(ePLAYER_STATE type) {
        switch (type) {
            case ePLAYER_STATE.PARRY:
                parryFxPrefab.SetActive(true);
                StartCoroutine(DisableFX(parryFxPrefab));
                break;
            case ePLAYER_STATE.STRIKE:
                hitFxPrefab.SetActive(true);
                StartCoroutine(DisableFX(hitFxPrefab));
                break;
        }
    }

    private IEnumerator DisableFX(GameObject fxPrefab) {
        float duration = 0f;
        
        duration = fxPrefab.GetComponent<ParticleSystem>().main.duration;

        yield return new WaitForSeconds(duration);
        
        fxPrefab.SetActive(false); 
    }
}

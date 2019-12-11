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

        var main = furyElecFxPrefab.GetComponent<ParticleSystem>().main;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f * furyPercent, 0.1f * furyPercent);

        if(furyPercent > 0) {
            //TODO : Handle fury percent with FX feedback amount
            furySteamFxPrefab.SetActive(true);
            furyElecFxPrefab.SetActive(true);
        } else {
            furySteamFxPrefab.SetActive(false);
            furyElecFxPrefab.SetActive(false);
        }
    }

    public void SpawnFX(ePLAYER_STATE type, eDIRECTION direction = eDIRECTION.NONE) {
        float zRot = 0f;

        if(direction != eDIRECTION.NONE) {
            zRot = -45f * (int)direction;
        }

        switch (type) {
            case ePLAYER_STATE.PARRY:

                parryFxPrefab.SetActive(true);
                //parryFxPrefab.transform.rotation = Quaternion.Euler(0, 0, zRot);
                StartCoroutine(DisableFX(parryFxPrefab));
                break;
            case ePLAYER_STATE.STRIKE:
                hitFxPrefab.SetActive(true);
                hitFxPrefab.transform.rotation = Quaternion.Euler(0, 0, zRot);
                StartCoroutine(DisableFX(hitFxPrefab));
                break;
        }
    }

    private IEnumerator DisableFX(GameObject fxPrefab, float tDuration = 1f) {

        yield return new WaitForSeconds(tDuration);
        
        fxPrefab.SetActive(false); 
    }
}

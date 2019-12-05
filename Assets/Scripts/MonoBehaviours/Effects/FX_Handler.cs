using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FX_Handler : MonoBehaviour
{
    public GameObject parryFxPrefab;
    public GameObject hitFxPrefab;
    
    public void SpawnFX(ePLAYER_STATE type) {
        switch (type) {
            case ePLAYER_STATE.PARRY:
                parryFxPrefab.SetActive(true);
                break;
            case ePLAYER_STATE.STRIKE:
                hitFxPrefab.SetActive(true);
                break;
        }
        StartCoroutine(DisableFX(type));
    }

    private IEnumerator DisableFX(ePLAYER_STATE type) {
        float duration = 0f;

        switch (type) {
            case ePLAYER_STATE.PARRY:
                duration = parryFxPrefab.GetComponent<ParticleSystem>().main.duration;
                break;
            case ePLAYER_STATE.STRIKE:
                duration = hitFxPrefab.GetComponent<ParticleSystem>().main.duration;
                break;
        }


        yield return new WaitForSeconds(duration);


        switch (type) {
            case ePLAYER_STATE.PARRY:
                parryFxPrefab.SetActive(false);
                break;
            case ePLAYER_STATE.STRIKE:
                hitFxPrefab.SetActive(false);
                break;
        }
    }
}

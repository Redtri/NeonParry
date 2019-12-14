using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FX_StartSize {
    public ParticleSystem ps;
    [HideInInspector] public ParticleSystem.MainModule main;
    [HideInInspector] public ParticleSystem.MinMaxCurve minMaxCurve;
}

public class FX_Handler : MonoBehaviour
{
    public GameObject parryFxPrefab;
    public GameObject hitFxPrefab;
    public GameObject furySteamFxPrefab;
    public FX_StartSize furyElecFx;
    private List<FX_StartSize> elecFXChilds;

    private void Start() {
        elecFXChilds = new List<FX_StartSize>();
            
        foreach(ParticleSystem child in furyElecFx.ps.transform.GetComponentsInChildren<ParticleSystem>()) {

            FX_StartSize fxTemp = new FX_StartSize();

            fxTemp.ps = child;
            fxTemp.main = child.main;
            fxTemp.minMaxCurve = new ParticleSystem.MinMaxCurve(fxTemp.main.startSize.constantMin, fxTemp.main.startSize.constantMax);

            elecFXChilds.Add(fxTemp);
        }
    }

    public void UpdateFuryFX(float furyPercent) {
        //0.45 0.87

        foreach(FX_StartSize fx in elecFXChilds) {
            fx.main.startSize = new ParticleSystem.MinMaxCurve(fx.minMaxCurve.constantMin * furyPercent, fx.minMaxCurve.constantMax * furyPercent);

            if (furyPercent > 0) {
                //TODO : Handle fury percent with FX feedback amount
                furySteamFxPrefab.SetActive(true);
                fx.ps.gameObject.SetActive(true);
            } else {
                furySteamFxPrefab.SetActive(false);
                fx.ps.gameObject.SetActive(false);
            }
        }
    }

    public void SpawnFX(ePLAYER_STATE type, eDIRECTION direction = eDIRECTION.NONE, bool facingLeft = false) {
        float zRot = 0f;

        if(direction != eDIRECTION.NONE) {
            if (!facingLeft) {
                zRot = -135f / (int)direction;
            } else {
                zRot = 135f / (int)direction;
            }
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

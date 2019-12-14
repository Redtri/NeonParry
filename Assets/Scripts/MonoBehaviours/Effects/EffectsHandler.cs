using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraShakeInfos {
    public float amount;
    public float frequency;
    public float duration;
}

public class EffectsHandler : MonoBehaviour
{
    public CameraShakeInfos shakeInfos;

    public void ShakeCam() {
        GameManager.instance.CameraShake(shakeInfos.amount, shakeInfos.frequency, shakeInfos.duration);
    }
}

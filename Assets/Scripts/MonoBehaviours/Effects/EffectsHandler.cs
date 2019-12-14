using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsHandler : MonoBehaviour
{

    public void ShakeCam() {
        GameManager.instance.CameraShake(16, 16, 0.2f);
    }

    public void Glitch() {
        PostProcessManager.instance.Glitch(1, 0.2f);
    }
}

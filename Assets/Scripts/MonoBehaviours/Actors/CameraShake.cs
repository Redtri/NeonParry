using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {
    // How long the object should shake for.
    public float shakeDuration = 0f;
    private bool shaking;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;

    Vector3 originalPos;

    void Awake() {
        shaking = false;
        originalPos = transform.localPosition;
    }

    void Update() {
        if (shaking) {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
        }
    }

    public void Shake() {
        shaking = true;
        Invoke("StopShake", shakeDuration);
    }
    
    private void StopShake() {
        shaking = false;
        transform.localPosition = originalPos;
    }
}

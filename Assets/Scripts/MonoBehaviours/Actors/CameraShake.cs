using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour {
    // How long the object should shake for.
    public float shakeDuration = 0f;
    private bool shaking;
    private CinemachineVirtualCamera camera;
    private Cinemachine.CinemachineBasicMultiChannelPerlin multiChannel;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;

    Vector3 originalPos;

    void Awake() {
        camera = this.GetComponent<CinemachineVirtualCamera>();
        multiChannel = camera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        shaking = false;
        originalPos = transform.localPosition;
    }

    void Update() {

    }

    public void Shake() {
        shaking = true;
        multiChannel.m_AmplitudeGain = shakeAmount;
        Invoke("StopShake", shakeDuration);
    }
    
    private void StopShake() {
        shaking = false;
        multiChannel.m_AmplitudeGain = 0f;
    }
}

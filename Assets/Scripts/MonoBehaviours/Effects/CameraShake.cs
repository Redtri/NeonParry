using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour {
    // How long the object should shake for.
    public float shakeDuration = 0f;
    [SerializeField] Transform parent;
    private bool shaking;
    private CinemachineVirtualCamera camera;
    private Cinemachine.CinemachineBasicMultiChannelPerlin multiChannel;
    
    private float baseShakeAmount;
    private float baseFrequency;
    private bool trueShaking;

    Vector3 originalPos;

    void Awake() {
        camera = this.GetComponent<CinemachineVirtualCamera>();
        multiChannel = camera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();

        baseShakeAmount = multiChannel.m_AmplitudeGain;
        baseFrequency = multiChannel.m_FrequencyGain;
        originalPos = transform.parent.localPosition;
    }

    void Update() {
    }

    public void TrueShake() {
        trueShaking = true;
        Invoke("StopTrueShaking", shakeDuration);
    }

    private void StopTrueShaking() {
        trueShaking = false;
        parent.localPosition = originalPos;
    }

    public void Shake(float shake, float frequency, float duration) {
        multiChannel.m_AmplitudeGain = shake;
        multiChannel.m_FrequencyGain = frequency;
        Invoke("StopShake", shakeDuration);
    }
    
    private void StopShake() {
        multiChannel.m_AmplitudeGain = baseShakeAmount;
        multiChannel.m_FrequencyGain = baseFrequency;
    }
}

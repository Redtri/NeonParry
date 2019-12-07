using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AudioSampler", menuName = "Scriptable/AudioSampler")]
public class AudioSampler : ScriptableObject
{
    public AK.Wwise.Event[] actionSounds;
    public AK.Wwise.Event[] additionalSounds;
}

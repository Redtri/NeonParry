using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public AK.Wwise.Event[] sfxEvents;
    
    public void SFX_Event(int index)
    {
        sfxEvents[index].Post(gameObject);
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class PostProcessManager : MonoBehaviour {

    public Volume postProcess;
    private bool glitching = false;
    public static PostProcessManager instance;
    private LensDistortion lensDist;
    private ChannelMixer chanMix;
    private Bloom bloom;

    private void Awake() {
        if (!instance) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
        if (postProcess) {
            postProcess.profile.TryGet(out lensDist);
            postProcess.profile.TryGet(out chanMix);
            postProcess.profile.TryGet(out bloom);
            if (!lensDist) {
                lensDist = postProcess.profile.Add<LensDistortion>();
            }
            if (!chanMix) {
                chanMix = postProcess.profile.Add<ChannelMixer>();
            }
            chanMix.redOutBlueIn.overrideState = true;
            chanMix.greenOutBlueIn.overrideState = true;
            chanMix.blueOutBlueIn.overrideState = true;
            lensDist.intensity.overrideState = true;
            lensDist.xMultiplier.overrideState = true;
            lensDist.yMultiplier.overrideState = true;
            lensDist.center.overrideState = true;
            lensDist.scale.overrideState = true;
        }
    }

    public void Glitch(int tCount, float duration, bool realtime = false, bool force = false) {
        if (!glitching || force) {
            StartCoroutine(Glitching(tCount, duration, realtime));
        }
    }

    private IEnumerator Glitching(int tCount, float duration, bool realtime = false) {
        float count = 0.0f;
        glitching = true;
        lensDist.active = true;
        chanMix.active = false;
        lensDist.intensity.overrideState = true;
        lensDist.xMultiplier.overrideState = true;
        lensDist.yMultiplier.overrideState = true;
        lensDist.center.overrideState = true;
        lensDist.scale.overrideState = true;

        while (count < duration) {
            if (realtime) {
                yield return new WaitForSecondsRealtime(0.01f);
            } else {
                yield return new WaitForSeconds(0.01f);
            }
            switch (tCount) {
                case 0:
                    lensDist.intensity.value = -(1 - (count / duration));
                    lensDist.xMultiplier.value = (count / duration);
                    bloom.intensity.value = 1 - (count / duration);
                    break;
                case 1:
                    lensDist.intensity.value = Mathf.Lerp(1, 0, (count / duration));
                    lensDist.xMultiplier.value = (count / duration);
                    bloom.intensity.value = 1 - (count / duration);
                    break;
                case 2:
                    lensDist.intensity.value = -(1 - (count / duration));
                    lensDist.xMultiplier.value = 1 - (count / duration);
                    lensDist.scale.value = Mathf.Lerp(0.5f, 1, (count / duration));
                    bloom.intensity.value = Mathf.Lerp(1, 0.067f, (count / duration));
                    break;
            }
            count += 0.01f;
        }
        //bloom.intensity.value = 0.067f;
        lensDist.active = false;
        chanMix.active = false;
        glitching = false;
        yield return null;
    }
}

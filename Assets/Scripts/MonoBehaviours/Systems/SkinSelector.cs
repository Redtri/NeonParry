using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinSelector : MonoBehaviour
{
    public SpriteRenderer[] glyphes;
    private SpriteController controller;
    private int current = 0;

    public void Init(SpriteController sprController) {
        controller = sprController;
        glyphes[current].GetComponent<Animator>().SetTrigger("select");
        controller.onSwap += SwapGlyph;
    }

    private void OnDisable() {
        controller.onSwap -= SwapGlyph;
    }

    private void SwapGlyph(eDIRECTION direction, float overrideDuration = 0f) {
	if((int)direction-1 != current){
        	glyphes[current].GetComponent<Animator>().SetTrigger("deselect");
        	current = (int)direction-1;
        	glyphes[current].GetComponent<Animator>().SetTrigger("select");
	}
    }
}

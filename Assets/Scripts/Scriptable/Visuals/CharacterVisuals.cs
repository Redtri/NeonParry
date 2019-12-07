using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterVisuals", menuName = "Scriptable/CharacterVisuals")]
public class CharacterVisuals : ScriptableObject
{
    public Sprite[] bodyParts;

    public void SwapBodyParts(SpriteRenderer[] renders) {
        for(int i = 0; i < renders.Length; ++i){
            renders[i].sprite = bodyParts[i];
        }
    }
}

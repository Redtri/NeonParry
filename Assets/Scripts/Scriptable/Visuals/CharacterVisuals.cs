using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadeColor {
    public eFX_FIELD_MODIFIER field;
    public Color[] colors;
    public ShadeColor[] childs;
}

public enum eFX_FIELD_MODIFIER { STARTCOLOR_CONST, STARTCOLOR_RAND, TRAIL, RENDER}

[CreateAssetMenu(fileName = "New CharacterVisuals", menuName = "Scriptable/CharacterVisuals")]
public class CharacterVisuals : ScriptableObject
{
    public Sprite[] bodyParts;
    public ShadeColor fxHitGround;
    public ShadeColor fxTrailDash;
    public ShadeColor fxFury;
    public ShadeColor fxShield;
    public ShadeColor fxFuryAura;
    public ShadeColor fxLowAttack;
    public ShadeColor fxHighAttack;
    public ShadeColor fxMidAttack;
    public ShadeColor fxEyeTrail1;
    public ShadeColor fxEyeTrail2;

    public void SwapBodyParts(SpriteRenderer[] renders) {
        for(int i = 0; i < renders.Length; ++i){
            renders[i].sprite = bodyParts[i];
        }
    }
}

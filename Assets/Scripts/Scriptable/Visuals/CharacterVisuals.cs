using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShadeColor {
    public Color colorMin;
    public Color colorMax;
}

public enum eFX_FIELD_MODIFIER { STARTCOLOR_CONST, STARTCOLOR_RAND, TRAIL, RENDER}

[CreateAssetMenu(fileName = "New CharacterVisuals", menuName = "Scriptable/CharacterVisuals")]
public class CharacterVisuals : ScriptableObject {
    public Sprite[] bodyParts;
    public ShadeColor[] fxHitGround;
    public ShadeColor fxFury;
    public Color shieldMesh;
    public ShadeColor fxShieldAura;
    public Color fxHighAttack;
    public Color fxMidAttack;
    public Color fxLowAttack;
    public Color fxEyesTrail;
    public ShadeColor fxAxeAura;

    public void SwapBodyParts(SpriteRenderer[] renders) {
        for(int i = 0; i < renders.Length; ++i){
            renders[i].sprite = bodyParts[i];
        }
    }
}

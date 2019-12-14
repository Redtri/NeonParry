using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwaper : MonoBehaviour
{
    public CharacterVisuals[] skins;
    public SpriteRenderer[] bodyParts;
    public Transform root;

    public ParticleSystem[] fxHitGround;
    public ParticleSystem fxFury;
    public MeshRenderer shieldMesh;
    public ParticleSystem fxShieldAura;
    public ParticleSystem fxHighAttack;
    public ParticleSystem fxMidAttack;
    public ParticleSystem fxLowAttack;
    public TrailRenderer[] fxEyesTrail;
    public ParticleSystem fxAxeAura;

    private void Awake() {
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            PickSkin(1);
        } else if (Input.GetKeyDown(KeyCode.H)) {
            PickSkin(0);
        }
    }

    public void PickSkin(int index) {
        skins[index].SwapBodyParts(bodyParts);
        ChangeColor(skins[index].fxHighAttack, fxHighAttack.main);
        ChangeColor(skins[index].fxMidAttack, fxMidAttack.main);
        ChangeColor(skins[index].fxLowAttack, fxLowAttack.main);
        ChangeColor(skins[index].fxFury.colorMin, skins[index].fxFury.colorMax, fxFury.main);
        for(int i = 0; i < fxHitGround.Length; ++i) {
            ChangeColor(skins[index].fxHitGround[i].colorMin, skins[index].fxHitGround[i].colorMax, fxHitGround[i].main);
        }
        ChangeColor(skins[index].fxShieldAura.colorMin, skins[index].fxShieldAura.colorMax, fxShieldAura.main);
        shieldMesh.material.SetColor("_Color",skins[index].shieldMesh);
        //shieldMesh.material.shader.
        for(int i = 0; i < fxEyesTrail.Length; ++i) {
            ChangeColor(skins[index].fxEyesTrail, fxEyesTrail[i]);
        }
        ChangeColor(skins[index].fxAxeAura.colorMin, skins[index].fxAxeAura.colorMax, fxAxeAura.main);
    }

    private void ChangeColor(Color minCol, Color maxCol, ParticleSystem.MainModule mainModule) {
        mainModule.startColor = new ParticleSystem.MinMaxGradient(minCol, maxCol);
    }

    private void ChangeColor(Color color, ParticleSystem.MainModule mainModule) {
        mainModule.startColor = color;
    }

    private void ChangeColor(Color color, TrailRenderer trailRender) {
        trailRender.startColor = color;
    }
}

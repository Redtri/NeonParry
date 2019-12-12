using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwaper : MonoBehaviour
{
    public CharacterVisuals[] skins;
    public SpriteRenderer[] bodyParts;

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
    }
}

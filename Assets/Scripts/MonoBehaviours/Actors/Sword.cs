using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [HideInInspector] public Sword overlappedSword;
    public PlayerController owner { get; private set; }

    public void Initialize(PlayerController tOwner) {
        owner = tOwner;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Sword")) {
            overlappedSword = collision.GetComponent<Sword>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Sword")) {
            overlappedSword = null;
        }
    }
}

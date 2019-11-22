using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [HideInInspector] public PlayerController overlappedOpponent;
    public PlayerController owner { get; private set; }

    public void Initialize(PlayerController tOwner) {
        owner = tOwner;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            PlayerController tmpPC = collision.GetComponent<PlayerController>();
            if(tmpPC != owner) {
                overlappedOpponent = tmpPC;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            PlayerController tmpPC = collision.GetComponent<PlayerController>();
            if (tmpPC != owner) {
                overlappedOpponent = null;
            }
        }
    }
}

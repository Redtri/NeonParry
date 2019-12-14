using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInfo {
    [HideInInspector] public PlayerController controller;
    [HideInInspector] public int playerIndex;
    [HideInInspector] public int score;
    [HideInInspector] public InputDevice[] device;
    [HideInInspector] public int skin;


    public PlayerInfo(PlayerController tController, int tPlayerIndex, params InputDevice[] tDevice) {
        controller = tController;
        playerIndex = tPlayerIndex;
        device = tDevice;
    }
}

public static class GameInfos {

    public static List<PlayerInfo> playerInfos = new List<PlayerInfo>();
}

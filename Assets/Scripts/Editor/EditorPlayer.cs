using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController))]
public class EditorPlayer : Editor {

    PlayerController pc;

    private void OnSceneGUI() {
        pc = (PlayerController)target;

        if(pc != null) {
            for (int i = 1; i < 4; ++i) {
                Handles.DrawLine(pc.transform.position, pc.transform.position + new Vector3(Mathf.Sin((130/i) * Mathf.Deg2Rad), Mathf.Cos((130/i) * Mathf.Deg2Rad) * 50f, 0f));
            }
        }
    }
}

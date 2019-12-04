using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class EditorGM : Editor
{
    GameManager gm;

    private void OnSceneGUI() {
        gm = (GameManager)target;
        if(gm != null) {
            Handles.color = Color.red;
            
            if(gm.spots.Count > 0) {
                for (int i = 0; i < gm.spots.Count; ++i) {
                    Handles.DrawLine(gm.spots[i], gm.spots[i] + Vector3.up * 35f);
                }
            } else {
                float stepValue = gm.stepValue;
                int nbSteps = gm.nbSteps;

                float startPoint = -((nbSteps * stepValue) - stepValue / 2);

                for (int i = 0; i < nbSteps * 2; ++i) {
                    Handles.DrawLine(new Vector3(startPoint + i * stepValue, 0, 0) - gm.groundOffset, new Vector3(startPoint + i * stepValue, 0, 0) - gm.groundOffset + Vector3.up * 35f);
                }
            }


        } else {
            Debug.Log("hé ba null");
        }
    }
}

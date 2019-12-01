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
            Handles.color = Color.blue;
            float stepValue = gm.stepValue;
            float startPoint = 0f;
            int nbSteps = gm.nbSteps;

            for (int i = 0; i < nbSteps; ++i) {
                Vector3 tmp = new Vector3(startPoint + i*stepValue + (stepValue/2), 0f, 0f);
                Handles.DrawLine(tmp, tmp + Vector3.up*1f);
                tmp = new Vector3((startPoint + i * stepValue + (stepValue / 2)) * -1f, 0f, 0f);
                Handles.DrawLine(tmp, tmp + Vector3.up * 1f);
            }
        }
    }
}

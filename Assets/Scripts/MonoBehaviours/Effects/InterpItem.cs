using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpItem : MonoBehaviour
{
    public Transform startObject;
    public Transform endObject;
    
    void Update()
    {
        if(startObject && endObject) {
            transform.position = new Vector3(endObject.position.x - Vector3.Distance(startObject.position, endObject.position)/2, transform.position.y, transform.position.z);
        }
    }
}

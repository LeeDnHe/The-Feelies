using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoWalk : MonoBehaviour
{
    public float speed = 1.5f; 

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}

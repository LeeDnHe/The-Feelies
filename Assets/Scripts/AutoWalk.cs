using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoWalk : MonoBehaviour
{
    public float speed = 1.5f; // 걷는 속도 (여기서 조절 가능)

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}

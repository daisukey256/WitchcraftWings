using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TsRotateCamera : MonoBehaviour
{
    // ‰ñ“]‘¬“x
    public float rotationSpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // YŽ²Žü‚è‚É‰ñ“]‚³‚¹‚é
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}

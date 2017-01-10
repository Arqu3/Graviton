using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResetBox : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (col.gameObject.GetComponent<ControllerPlayer>())
            {
                col.gameObject.GetComponent<ControllerPlayer>().ResetPosition();
            }
        }
    }
}

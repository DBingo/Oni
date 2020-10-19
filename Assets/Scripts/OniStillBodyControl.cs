using UnityEngine;
using System.Collections;

public class OniStillBodyControl : MonoBehaviour
{
    public OniEmitterControl emitter_control;

    void Start()
    {

    }
    
    void Update()
    {

    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "OniYama")
        {
            this.emitter_control.PlayHitSound();
        }

        if(other.gameObject.tag == "Floor")
        {
            Destroy(this.GetComponent<Rigidbody>());
        }
    }
}

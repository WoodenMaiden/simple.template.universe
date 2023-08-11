using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float speed = 5.0f;
   

    private void FixedUpdate()
    {

        Vector3 vectF = Camera.main.transform.forward;
        vectF.y = 0;
        Vector3 vectR = Camera.main.transform.right;
        vectR.y = 0;
        transform.position += (vectF * speed * Time.fixedDeltaTime * Input.GetAxis("Vertical"));
        transform.position += (vectR * speed * Time.fixedDeltaTime * Input.GetAxis("Horizontal"));
        //transform.Translate(vectF * speed * Time.fixedDeltaTime * Input.GetAxis("Vertical"));
        //transform.Translate(vectR * speed * Time.fixedDeltaTime * Input.GetAxis("Horizontal"));
    }
}
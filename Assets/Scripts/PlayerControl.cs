using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerControl : MonoBehaviour
{


    public float bustersSpeed = 2f;
    public float rotationSpeed = 5f;
    public Vector2 direccion;
    public float rotacion;
    public float rotEnRad;
    public float pi;



public Rigidbody2D rb;

public float movement;
float rotador; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    rotador = Input.GetAxisRaw("Horizontal");
    movement = Input.GetAxisRaw("Vertical");
        
    }

    void FixedUpdate()
    {
        rotacion = rb.rotation;
        pi = Mathf.PI;
        rotEnRad = rotacion * pi / 180;

        direccion = new Vector2(-1 * Mathf.Sin(rotEnRad), Mathf.Cos(rotEnRad));

        if (Input.GetAxisRaw("Horizontal") != 0)
        {


            rb.rotation = rb.rotation + rotador * rotationSpeed;
        }

        else if (Input.GetAxisRaw("Horizontal") == 0)
        {

        }








        rb.AddForce(direccion * movement * bustersSpeed);
    }

}

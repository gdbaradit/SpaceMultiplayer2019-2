using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

public class controlDeMovimiento : MonoBehaviour
{
    // Start is called before the first frame update
    public float bustersSpeed = 2f;
    public float rotationSpeed = 5f;
    public Vector2 direccion;

    public float rotacion;
    public float rotEnRad; 
    public float pi; 

    

    public Rigidbody2D rb;

    public float movement;
    float rotador; 

        

    // Update is called once per frame
    // UPDATE PARA INPUTS
    void Update()
    {
        rotador = Input.GetAxisRaw("Horizontal");
        movement = Input.GetAxisRaw("Vertical"); 
    }

    // FIXED UPDATE PARA LAS FÍSICAS
    void FixedUpdate()
    {
        rotacion = rb.rotation;
        pi = Mathf.PI;
        rotEnRad = rotacion * pi / 180; 

        direccion = new Vector2 (-1*Mathf.Sin(rotEnRad), Mathf.Cos(rotEnRad));

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

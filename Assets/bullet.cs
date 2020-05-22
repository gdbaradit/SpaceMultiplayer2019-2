using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public GameObject hitEffect;
    public GameObject objeto;

    
    void OnCollisionEnter2D(Collision2D col)
    {
        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(effect, 5f); 
        Destroy(gameObject);
        objeto = col.gameObject;
        Debug.Log("Está chocando con " + objeto);
    }

    


}

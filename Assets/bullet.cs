using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    public GameObject hitEffect;
    public GameObject objeto;
    public int id;
    public Rigidbody rigidBody;
    public int thrownByPlayer;
    ///public Vector3 initialForce;
    /// public float explosionRadius = 1.5f;
    /// public float explosionDamage = 75f;


    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, thrownByPlayer);

        ///rigidBody.AddForce(initialForce);
       /// StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(effect, 5f); 
        Destroy(gameObject);
        objeto = col.gameObject;
        Debug.Log("Está chocando con " + objeto);
    }

    


}

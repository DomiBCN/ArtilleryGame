﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class Bomb : MonoBehaviour
{
    public float bombRadius = 10f;          // Radius within which enemies are killed.
    public float bombForce = 100f;          // Force that enemies are thrown from the blast.
    public AudioClip boom;                  // Audioclip of explosion.
    public AudioClip fuse;                  // Audioclip of fuse.
    public float fuseTime = 1.5f;
    public GameObject explosion;			// Prefab of explosion effect.
    public int radius;

    private LayBombs layBombs;              // Reference to the player's LayBombs script.
    private PickupSpawner pickupSpawner;    // Reference to the PickupSpawner script.
    private ParticleSystem explosionFX;     // Reference to the particle system of the explosion effect.

    void Awake()
    {
        // Setting up references.
        explosionFX = GameObject.FindGameObjectWithTag("ExplosionFX").GetComponent<ParticleSystem>();
        pickupSpawner = GameObject.Find("pickupManager").GetComponent<PickupSpawner>();
        if (GameObject.FindGameObjectWithTag("Player"))
            layBombs = GameObject.FindGameObjectWithTag("Player").GetComponent<LayBombs>();
    }

    void Start()
    {

        // If the bomb has no parent, it has been laid by the player and should detonate.
        if (transform.root == transform)
            StartCoroutine(BombDetonation());
    }


    IEnumerator BombDetonation()
    {
        // Play the fuse audioclip.
        AudioSource.PlayClipAtPoint(fuse, transform.position);

        // Wait for 2 seconds.
        yield return new WaitForSeconds(fuseTime);

        // Explode the bomb.
        Explode();
    }


    public void Explode()
    {
        // The player is now free to lay bombs when he has them.
        layBombs.bombLaid = false;

        // Make the pickup spawner start to deliver a new pickup.
        pickupSpawner.StartCoroutine(pickupSpawner.DeliverPickup());

        // Find all the colliders on the Enemies layer within the bombRadius.
        //Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, bombRadius, 1 << LayerMask.NameToLayer("Enemies"));
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2f);

        Collider2D enemyCollider = hitColliders.Where(h => h.tag == "Enemy").FirstOrDefault();
        if(enemyCollider != null)
        {
            Rigidbody2D rb = enemyCollider.GetComponent<Rigidbody2D>();
            // Find the Enemy script and set the enemy's health to zero.
            rb.gameObject.GetComponent<Enemy>().HP = 0;

            // Find a vector from the bomb to the enemy.
            Vector3 deltaPos = rb.transform.position - transform.position;

            // Apply a force in this direction with a magnitude of bombForce.
            Vector3 force = deltaPos.normalized * bombForce;
            rb.AddForce(force);
        }

        Collider2D obstacleHit = hitColliders.Where(h => h.tag != "Bullet" && h.tag != "Player" && h.tag != "Bullet" && h.tag != "BombPickup" && h.tag != "PlatformEnd" && !h.isTrigger).FirstOrDefault();

        if (obstacleHit != null)
        {
            BoxCollider2D boxCollider = obstacleHit.gameObject.GetComponent<BoxCollider2D>();
            //Creates explosion crater setting pixels to alpha 0
            transform.GetComponent<PixelsToAlpha>().UpdateTexture(new Vector2(transform.position.x, transform.position.y), boxCollider.gameObject, boxCollider, radius, Gun.Weapons.Bomb);
        }
        else
        {
            FinalExplosion();
        }
        

        //// For each collider...
        //foreach (Collider2D en in hitColliders)
        //{
        //    // Check if it has a rigidbody (since there is only one per enemy, on the parent).
        //    Rigidbody2D rb = en.GetComponent<Rigidbody2D>();
        //    if (rb != null && rb.tag == "Enemy")
        //    {
        //        // Find the Enemy script and set the enemy's health to zero.
        //        rb.gameObject.GetComponent<Enemy>().HP = 0;

        //        // Find a vector from the bomb to the enemy.
        //        Vector3 deltaPos = rb.transform.position - transform.position;

        //        // Apply a force in this direction with a magnitude of bombForce.
        //        Vector3 force = deltaPos.normalized * bombForce;
        //        rb.AddForce(force);
        //    }
        //    else if (en.tag != "Bullet" && en.tag != "Player" && en.tag != "Bullet" && en.tag != "BombPickup" && en.tag != "PlatformEnd" && !en.isTrigger)
        //    {
        //        BoxCollider2D boxCollider = en.gameObject.GetComponent<BoxCollider2D>();

        //        //explodePixels = false;
        //        //Creates explosion crater setting pixels to alpha 0
        //        //PixelsToAlpha.UpdateTexture(new Vector2(transform.position.x, transform.position.y), en.gameObject, boxCollider, radius);

        //    }
        //}
        
    }

    public void FinalExplosion()
    {
        explosionFX.transform.position = transform.position;
        explosionFX.Play();

        // Instantiate the explosion prefab.
        Instantiate(explosion, transform.position, Quaternion.identity);

        // Play the explosion sound effect.
        AudioSource.PlayClipAtPoint(boom, transform.position);

        // Destroy the bomb.
        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class RangeWeapon : MonoBehaviour
{
    [Header("Parameters")]
    public float moveSpeed = 1;
    public DoubleFloat damageRange = new DoubleFloat(10, 20);
    protected int damage;
    protected LayerMask targetLayer;

    private void Start()
    {
        StartCoroutine(DestroyByTime()); //Timer to destroy
    }

    private void FixedUpdate()
    {
        Move();
    }

    // Move method
    protected virtual void Move()
    {
        transform.Translate(Vector2.up * moveSpeed * Time.deltaTime); // Move
    }

    public virtual void OnTriggerEnter2D(Collider2D collider) // If in contact with an obstacle
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Obstacle")) // If obstacle
        {
            Destroying();
        }
    }

    // Destroy method
    public void Destroying()
    {
        Destroy(gameObject);
    }

    IEnumerator DestroyByTime()
    {
        yield return new WaitForSeconds(5); // Destroy gameobject after 5 sec
        Destroy(gameObject);
    }
}

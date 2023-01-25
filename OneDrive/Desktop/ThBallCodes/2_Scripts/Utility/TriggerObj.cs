using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerObj : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            collider.isTrigger = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }
    }
}

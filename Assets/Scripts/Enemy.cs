using UnityEngine;
using System.Collections; 

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public Material hitMat;
        private Rigidbody rb;
    private Renderer rend;
    private Material originalMaterial;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material; //Goes back to original material after bliking 
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Damage")
        {
            health -= 10;

            if (health <= 0)
            {
                Die();
            }
            else 
            {
                StartCoroutine(Blink());
            }
        }
    }

    void Die()
    {
        if (!this.enabled) return;

        rb.freezeRotation = false; //Enemy falls over
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z + 5f); // 
        this.enabled = false; // Enemy script is no longer active
    }

    IEnumerator Blink()
    {
        rend.material = hitMat;
        yield return new WaitForSeconds(0.1f);
        rend.material = originalMaterial;

    }
}

using UnityEngine;

public class BolaController : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Aquí puedes agregar efectos de sonido o partículas al chocar
        if (collision.gameObject.CompareTag("Bolo"))
        {
            // Efectos al golpear bolo
        }
    }
}
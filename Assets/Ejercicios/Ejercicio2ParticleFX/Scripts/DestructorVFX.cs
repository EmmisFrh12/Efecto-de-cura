using UnityEngine;

public class DestructorVFX : MonoBehaviour
{
    [Tooltip("Tiempo en segundos antes de borrar la explosión de la escena")]
    public float tiempoDeVida = 2f; 

    void Start()
    {
        // Esto destruye el objeto (la explosión) automáticamente después de X segundos
        Destroy(gameObject, tiempoDeVida);
    }
}
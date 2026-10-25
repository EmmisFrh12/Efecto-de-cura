using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Asegúrate de usar el namespace correcto de tu versión

public class LanzadorGranadas : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject granadaPrefab;
    public Transform puntoDeDisparo;
    
    [Header("Animación")]
    public Animator animadorPersonaje;

    [Header("Cámara Showcase")]
    public CinemachineCamera camaraShowcase; // Arrastra tu cámara aquí en el inspector

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (animadorPersonaje != null)
            {
                animadorPersonaje.SetTrigger("Lanzar");
            }
            DispararGranada();
        }
    }

    void DispararGranada()
    {
        if (granadaPrefab != null && puntoDeDisparo != null)
        {
            GameObject nuevaGranada = Instantiate(granadaPrefab, puntoDeDisparo.position, puntoDeDisparo.rotation);

            // Le decimos a la cámara que siga y mire a la nueva granada
            if (camaraShowcase != null)
            {
                camaraShowcase.Target.TrackingTarget = nuevaGranada.transform;
                // Si usas una versión anterior de Cinemachine, sería:
                // camaraShowcase.Follow = nuevaGranada.transform;
                // camaraShowcase.LookAt = nuevaGranada.transform;
            }
        }
    }
}
using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic; // Necesario para usar Listas

public class GranadaElemental : MonoBehaviour
{
    [Header("Fuerza y Dirección")]
    public float fuerzaLanzamiento = 15f;
    public float anguloLanzamiento = 45f;

    [Header("Indicadores de Rebote (VFX)")]
    public GameObject indicadorRebote1Prefab; // Prefab para el 1er toque
    public GameObject indicadorRebote2Prefab; // Prefab para el 2do toque
    public GameObject indicadorExplosionPrefab; // Prefab de Área de Efecto para el 3er toque (donde explota)

    [Header("Explosión Final (VFX)")]
    public GameObject explosionFinalPrefab; // Tu sistema de partículas y distorsión

    [Header("Ajustes de Predicción")]
    [Tooltip("Debe ser igual al Bounciness de tu Physic Material (ej. 0.8)")]
    public float bouncinessSimulado = 0.8f;
    
    [Tooltip("Ajusta este valor (ej. 1.05 a 1.2) para alargar SOLO el último rebote sin dañar los anteriores.")]
    public float compensacionUltimoRebote = 1.1f; // <-- VARIABLE EXCLUSIVA PARA EL FINAL

    private Rigidbody rb;
    private int rebotesActuales = 0;
    
    // Lista para guardar todos los indicadores y poder borrarlos al final
    private List<GameObject> indicadoresInstanciados = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Lanzar();
    }

    void Lanzar()
    {
        Vector3 direccion = (transform.forward + (Vector3.up * (anguloLanzamiento / 90f))).normalized;
        Vector3 impulsoInicial = direccion * fuerzaLanzamiento;

        PredecirPuntoDeCaida(impulsoInicial);

        rb.AddForce(impulsoInicial, ForceMode.Impulse);
    }

    void PredecirPuntoDeCaida(Vector3 velocidadInicial)
    {
        Vector3 posicionSimulada = transform.position;
        Vector3 velocidadSimulada = velocidadInicial / rb.mass;
        float pasoDeTiempo = 0.05f; // Mayor resolución para mayor precisión en los rebotes
        int rebotesSimulados = 0;

        // Aumentamos los pasos porque ahora volamos más lejos
        for (int i = 0; i < 300; i++) 
        {
            velocidadSimulada += Physics.gravity * pasoDeTiempo;
            Vector3 posicionSiguiente = posicionSimulada + (velocidadSimulada * pasoDeTiempo);

            if (Physics.Linecast(posicionSimulada, posicionSiguiente, out RaycastHit hit))
            {
                rebotesSimulados++;

                // Elegir el prefab según el número de rebote
                GameObject prefabAUsar = null;
                if (rebotesSimulados == 1) prefabAUsar = indicadorRebote1Prefab;
                else if (rebotesSimulados == 2) prefabAUsar = indicadorRebote2Prefab;
                else if (rebotesSimulados == 3) prefabAUsar = indicadorExplosionPrefab;

                // Instanciar el indicador y guardarlo en la lista
                if (prefabAUsar != null)
                {
                    GameObject nuevoIndicador = Instantiate(prefabAUsar, hit.point + new Vector3(0, 0.1f, 0), Quaternion.Euler(90, 0, 0));
                    indicadoresInstanciados.Add(nuevoIndicador);
                }

                // Si ya predecimos el 3er impacto (la explosión), detenemos la simulación matemática
                if (rebotesSimulados >= 3)
                {
                    break;
                }

                // MATEMÁTICA ORIGINAL: Funciona perfecto para los rebotes 1 y 2
                velocidadSimulada = Vector3.Reflect(velocidadSimulada, hit.normal) * bouncinessSimulado;
                
                // --- EL PARCHE PARA EL TERCER INDICADOR ---
                // Si acabamos de calcular el 2do rebote, el siguiente trayecto es el de la explosión.
                // Aquí aplicamos el ajuste de avance para corregir el desfase, sin tocar el salto.
                if (rebotesSimulados == 2)
                {
                    Vector3 velVertical = Vector3.Project(velocidadSimulada, hit.normal);
                    Vector3 velHorizontal = velocidadSimulada - velVertical;
                    
                    // Solo multiplicamos la parte horizontal
                    velocidadSimulada = velVertical + (velHorizontal * compensacionUltimoRebote);
                }
                
                // Movemos la posición simulada un pelín arriba del suelo para evitar quedarnos pegados
                posicionSimulada = hit.point + hit.normal * 0.05f; 
            }
            else
            {
                posicionSimulada = posicionSiguiente;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        rebotesActuales++;

        if (rebotesActuales >= 3)
        {
            Explotar();
        }
    }

    void Explotar()
    {
        // 1. Instanciar la explosión y los rayos finales
        if (explosionFinalPrefab != null)
        {
            Instantiate(explosionFinalPrefab, transform.position, Quaternion.identity);
        }

        // 2. Desvincular la cámara
        CinemachineCamera camara = FindAnyObjectByType<CinemachineCamera>();
        if (camara != null && camara.Target.TrackingTarget == transform)
        {
            camara.Target.TrackingTarget = null; 
        }

        // 3. Limpiar TODOS los indicadores del piso (los destruye)
        foreach (GameObject indicador in indicadoresInstanciados)
        {
            if (indicador != null)
            {
                Destroy(indicador);
            }
        }
        indicadoresInstanciados.Clear(); // Vaciamos la lista
        
        // 4. Destruimos la granada
        Destroy(gameObject);
    }
}
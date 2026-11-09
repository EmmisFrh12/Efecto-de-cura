using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MasterDemoManager : MonoBehaviour
{
    [Header("Contenedores de los Efectos")]
    [SerializeField] private GameObject[] effectContainers;

    [Header("UI Controles")]
    [Tooltip("Arrastra aquí el GameObject del Botón de Pausal")]
    [SerializeField] private Button playPauseButton;

    private TMP_Text playPauseBtnText;
    private int currentEffectIndex = 0;
    private bool isPaused = false;

    private void Awake()
    {
        Debug.Log("[MasterDemoManager] 1. Awake iniciado.");

        if (playPauseButton != null)
        {
            playPauseBtnText = playPauseButton.GetComponentInChildren<TMP_Text>();
            Debug.Log($"[MasterDemoManager] Botón de pausa detectado: '{playPauseButton.name}'. Texto TMP encontrado: {(playPauseBtnText != null)}");
        }
        else
        {
            Debug.LogWarning("[MasterDemoManager] ALERTA: No se ha asignado el 'playPauseButton' en el Inspector.");
        }
    }

    private void Start()
    {
        Debug.Log($"[MasterDemoManager] 2. Start iniciado. Total de contenedores asignados: {(effectContainers != null ? effectContainers.Length : 0)}");
        SelectEffect(0);
    }

    public void SelectEffect(int index)
    {
        Debug.Log($"[MasterDemoManager] ---> SelectEffect() LLAMADO con índice: {index}");

        if (effectContainers == null || effectContainers.Length == 0)
        {
            Debug.LogError("[MasterDemoManager] ERROR: El arreglo 'effectContainers' está vacío o es null. Revisa el Inspector.");
            return;
        }

        if (index < 0 || index >= effectContainers.Length)
        {
            Debug.LogError($"[MasterDemoManager] ERROR: El índice {index} está fuera de rango. El arreglo tiene tamaño {effectContainers.Length}.");
            return;
        }

        currentEffectIndex = index;
        Time.timeScale = 1f;
        isPaused = false;

        if (playPauseBtnText != null) playPauseBtnText.text = "Pausar";

        for (int i = 0; i < effectContainers.Length; i++)
        {
            if (effectContainers[i] != null)
            {
                bool shouldActivate = (i == index);
                effectContainers[i].SetActive(shouldActivate);
                Debug.Log($"[MasterDemoManager] Contenedor [{i}] ('{effectContainers[i].name}') -> SetActive({shouldActivate})");
            }
            else
            {
                Debug.LogWarning($"[MasterDemoManager] El elemento [{i}] de effectContainers es NULL en el Inspector.");
            }
        }
    }

    public void TogglePlayPause()
    {
        Debug.Log($"[MasterDemoManager] ---> TogglePlayPause() LLAMADO. Estado previo isPaused = {isPaused}");
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (playPauseBtnText != null)
            playPauseBtnText.text = isPaused ? "Reanudar" : "Pausar";

        Debug.Log($"[MasterDemoManager] Nuevo Time.timeScale = {Time.timeScale}");
    }

    public void ReplayCurrentEffect()
    {
        Debug.Log($"[MasterDemoManager] ---> ReplayCurrentEffect() LLAMADO para efecto {currentEffectIndex}");
        if (effectContainers[currentEffectIndex] != null)
        {
            effectContainers[currentEffectIndex].SetActive(false);
            effectContainers[currentEffectIndex].SetActive(true);
        }

        Time.timeScale = 1f;
        isPaused = false;
        if (playPauseBtnText != null) playPauseBtnText.text = "Pausar";
    }
}
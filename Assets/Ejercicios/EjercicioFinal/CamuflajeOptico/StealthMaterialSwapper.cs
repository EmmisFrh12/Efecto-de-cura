using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class StealthMaterialSwapper : MonoBehaviour
{
    [Header("Referencias de Malla y Animación")]
    [SerializeField] private SkinnedMeshRenderer characterRenderer;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private Material stealthMaterial;

    [Header("Referencias de Post-Procesado")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float maxVignetteIntensity = 0.45f;

    [Header("Configuración de Control e Inputs")]
    [SerializeField] private KeyCode toggleKey = KeyCode.E;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private string baseColorPropertyName = "_BaseColor";
    [SerializeField] private string crouchBoolName = "IsCrouching";

    private Material[] originalMaterials;
    private MaterialPropertyBlock propBlock;
    private int baseColorID;
    private bool isStealthActive = false;
    private Coroutine transitionCoroutine;
    private Vignette vignetteComponent;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        baseColorID = Shader.PropertyToID(baseColorPropertyName);

        if (characterRenderer == null)
            characterRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>();

        if (characterRenderer != null)
        {
            originalMaterials = characterRenderer.sharedMaterials;
        }
    }

    private void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out Vignette vignette))
        {
            vignetteComponent = vignette;
            vignetteComponent.intensity.value = 0f;
        }
    }

    private void OnEnable()
    {
        // Al encender el GameObject (seleccionar Efecto 1 en UI), arrancar en estado NORMAL
        isStealthActive = false;

        if (characterAnimator != null)
        {
            characterAnimator.SetBool(crouchBoolName, false);
        }

        if (characterRenderer != null && originalMaterials != null)
        {
            characterRenderer.materials = originalMaterials;
        }

        if (vignetteComponent != null)
        {
            vignetteComponent.intensity.value = 0f;
        }
    }

    private void Update()
    {
        // Detección dual de input (Input clásico + New Input System)
        bool isKeyPressed = Input.GetKeyDown(toggleKey);
        if (!isKeyPressed && Keyboard.current != null)
        {
            isKeyPressed = Keyboard.current.eKey.wasPressedThisFrame;
        }

        // Al presionar E, alternar el estado del camuflaje
        if (isKeyPressed)
        {
            ToggleStealth();
        }
    }

    [ContextMenu("Probar Camuflaje (Toggle)")]
    public void ToggleStealth()
    {
        isStealthActive = !isStealthActive;

        // 1. Activar / Desactivar la animación de agachado en el Animator
        if (characterAnimator != null)
        {
            characterAnimator.SetBool(crouchBoolName, isStealthActive);
        }

        // 2. Iniciar la corrutina de transición visual (Shader + Vignette)
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(ToggleStealthRoutine(isStealthActive));
    }

    private IEnumerator ToggleStealthRoutine(bool enableStealth)
    {
        if (enableStealth)
        {
            // Asignar el material de camuflaje en todos los slots
            Material[] activeStealthMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < activeStealthMaterials.Length; i++)
            {
                activeStealthMaterials[i] = stealthMaterial;
            }
            characterRenderer.materials = activeStealthMaterials;

            // Desvanecer Alfa de 1 a 0 y subir Vignette de 0 a Max
            yield return AnimateStealth(1f, 0f, 0f, maxVignetteIntensity);
        }
        else
        {
            // Regresar Alfa de 0 a 1 y bajar Vignette a 0
            yield return AnimateStealth(0f, 1f, maxVignetteIntensity, 0f);

            // Restaurar los materiales base del personaje
            characterRenderer.materials = originalMaterials;
        }
    }

    private IEnumerator AnimateStealth(float startAlpha, float targetAlpha, float startVignette, float targetVignette)
    {
        float elapsedTime = 0f;
        Color baseColor = stealthMaterial != null ? stealthMaterial.GetColor(baseColorID) : Color.white;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // Interpolación del Shader Graph (Alfa)
            characterRenderer.GetPropertyBlock(propBlock);
            baseColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            propBlock.SetColor(baseColorID, baseColor);
            characterRenderer.SetPropertyBlock(propBlock);

            // Interpolación de Post-Procesado (Vignette)
            if (vignetteComponent != null)
            {
                vignetteComponent.intensity.value = Mathf.Lerp(startVignette, targetVignette, t);
            }

            yield return null;
        }
    }
}
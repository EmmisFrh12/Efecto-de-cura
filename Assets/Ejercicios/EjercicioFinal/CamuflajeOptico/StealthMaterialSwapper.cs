using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class StealthMaterialSwapper : MonoBehaviour
{
    [Header("Referencias de Malla y Animación")]
    [SerializeField] private SkinnedMeshRenderer characterRenderer;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private Material stealthMaterial;

    [Header("Post-Procesado: Sigilo Base")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float maxVignetteIntensity = 0.45f;

    [Header("Post-Procesado: Glitch Digital")]
    [SerializeField] private float glitchChromaIntensity = 1f;
    [SerializeField] private float glitchLensDistortion = -0.5f;
    [SerializeField] private Image[] glitchImageOverlays; // <-- Ahora es un arreglo de imágenes
    [SerializeField] private Sprite[] glitchSprites;

    [Header("Configuración del Bucle")]
    [SerializeField] private float initialDelay = 1.0f;
    [SerializeField] private float stealthDuration = 3.0f;
    [SerializeField] private float normalDuration = 2.0f;
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Configuración del Shader/Animator")]
    [SerializeField] private string baseColorPropertyName = "_BaseColor";
    [SerializeField] private string crouchBoolName = "IsCrouching";

    private Material[] originalMaterials;
    private MaterialPropertyBlock propBlock;
    private int baseColorID;
    private bool isStealthActive = false;
    private Coroutine transitionCoroutine;
    private Coroutine loopCoroutine;
    
    private Vignette vignetteComponent;
    private ChromaticAberration chromaComponent;
    private LensDistortion lensComponent;

    // Arreglo para guardar las posiciones originales que pongas en el Canvas
    private Vector2[] originalGlitchPositions; 

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        baseColorID = Shader.PropertyToID(baseColorPropertyName);

        if (characterRenderer == null) characterRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();
        if (characterRenderer != null) originalMaterials = characterRenderer.sharedMaterials;

        // Guardar las posiciones originales de todas las imágenes de glitch
        if (glitchImageOverlays != null)
        {
            originalGlitchPositions = new Vector2[glitchImageOverlays.Length];
            for (int i = 0; i < glitchImageOverlays.Length; i++)
            {
                if (glitchImageOverlays[i] != null)
                {
                    originalGlitchPositions[i] = glitchImageOverlays[i].rectTransform.anchoredPosition;
                }
            }
        }
    }

    private void Start()
    {
        if (globalVolume != null)
        {
            if (globalVolume.profile.TryGet(out Vignette vignette)) vignetteComponent = vignette;
            if (globalVolume.profile.TryGet(out ChromaticAberration chroma)) chromaComponent = chroma;
            if (globalVolume.profile.TryGet(out LensDistortion lens)) lensComponent = lens;
            
            ResetPostProcessing();
        }
    }

    private void OnEnable()
    {
        isStealthActive = false;
        if (characterAnimator != null) characterAnimator.SetBool(crouchBoolName, false);
        if (characterRenderer != null && originalMaterials != null) characterRenderer.materials = originalMaterials;
        
        ResetPostProcessing();

        if (loopCoroutine != null) StopCoroutine(loopCoroutine);
        loopCoroutine = StartCoroutine(AutoStealthLoop());
    }

    private void OnDisable()
    {
        if (loopCoroutine != null) StopCoroutine(loopCoroutine);
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
    }

    private void ResetPostProcessing()
    {
        if (vignetteComponent != null) vignetteComponent.intensity.value = 0f;
        if (chromaComponent != null) chromaComponent.intensity.value = 0f;
        if (lensComponent != null) lensComponent.intensity.value = 0f;
        
        if (glitchImageOverlays != null)
        {
            for (int i = 0; i < glitchImageOverlays.Length; i++)
            {
                if (glitchImageOverlays[i] != null)
                {
                    glitchImageOverlays[i].color = new Color(1f, 1f, 1f, 0f);
                    if (originalGlitchPositions != null && originalGlitchPositions.Length > i)
                        glitchImageOverlays[i].rectTransform.anchoredPosition = originalGlitchPositions[i];
                }
            }
        }
    }

    private IEnumerator AutoStealthLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            ToggleStealth();
            yield return new WaitForSeconds(stealthDuration);
            ToggleStealth();
            yield return new WaitForSeconds(normalDuration);
        }
    }

    public void ToggleStealth()
    {
        isStealthActive = !isStealthActive;
        if (characterAnimator != null) characterAnimator.SetBool(crouchBoolName, isStealthActive);

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(ToggleStealthRoutine(isStealthActive));
    }

    private IEnumerator ToggleStealthRoutine(bool enableStealth)
    {
        if (enableStealth)
        {
            Material[] activeStealthMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < activeStealthMaterials.Length; i++) activeStealthMaterials[i] = stealthMaterial;
            characterRenderer.materials = activeStealthMaterials;

            yield return AnimateStealth(1f, 0f, maxVignetteIntensity);
        }
        else
        {
            yield return AnimateStealth(0f, 1f, 0f);
            characterRenderer.materials = originalMaterials;
        }
    }

    private IEnumerator AnimateStealth(float startAlpha, float targetAlpha, float targetVignette)
    {
        float elapsedTime = 0f;
        Color baseColor = stealthMaterial != null ? stealthMaterial.GetColor(baseColorID) : Color.white;
        float startVignette = vignetteComponent != null ? vignetteComponent.intensity.value : 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // 1. Interpolación de Shader Graph
            characterRenderer.GetPropertyBlock(propBlock);
            baseColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            propBlock.SetColor(baseColorID, baseColor);
            characterRenderer.SetPropertyBlock(propBlock);

            // 2. Interpolación del Vignette
            if (vignetteComponent != null)
                vignetteComponent.intensity.value = Mathf.Lerp(startVignette, targetVignette, t);

            // 3. Generación del Glitch en URP
            float glitchCurve = Mathf.Sin(t * Mathf.PI); 
            float chaoticNoise = Random.Range(0.5f, 1f); 
            float currentGlitch = glitchCurve * chaoticNoise;

            if (chromaComponent != null) chromaComponent.intensity.value = currentGlitch * glitchChromaIntensity;
            if (lensComponent != null) lensComponent.intensity.value = currentGlitch * glitchLensDistortion;

            // 4. TEXTURA DE PANTALLA MÚLTIPLE (MARCOS)
            if (glitchImageOverlays != null && glitchImageOverlays.Length > 0)
            {
                if (glitchCurve > 0.5f)
                {
                    for (int i = 0; i < glitchImageOverlays.Length; i++)
                    {
                        Image img = glitchImageOverlays[i];
                        if (img == null) continue;

                        // Seleccionar sprite aleatorio diferente para cada imagen
                        if (glitchSprites != null && glitchSprites.Length > 0)
                            img.sprite = glitchSprites[Random.Range(0, glitchSprites.Length)];

                        // Opacidad aleatoria para cada uno
                        Color glitchColor = Color.white;
                        glitchColor.a = (glitchCurve - 0.5f) * 2f * Random.Range(0.2f, 1f);
                        img.color = glitchColor;

                        // Temblor físico: vibra sobre su posición ORIGINAL en el canvas
                        float randomX = Random.Range(-25f, 25f);
                        float randomY = Random.Range(-25f, 25f);
                        img.rectTransform.anchoredPosition = originalGlitchPositions[i] + new Vector2(randomX, randomY);
                    }
                }
                else
                {
                    ResetPostProcessing(); // Apaga y devuelve a su lugar
                }
            }

            yield return null;
        }

        ResetPostProcessing();
    }
}

using System.Collections;
using UnityEngine;

public class trigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggername;
    [SerializeField] private ParticleSystem efecto;

    [Header("Configuración")]
    [SerializeField] private string idleStateName = "Idle";

    private bool animationPlaying = false;

    public void PlayAnimation()
    {
        if (animationPlaying)
        {
            return;
        }

        StartCoroutine(PlayAnimationCoroutine());
    }

    private IEnumerator PlayAnimationCoroutine()
    {
        animationPlaying = true;

        animator.ResetTrigger(triggername);
        animator.SetTrigger(triggername);

        if (efecto != null)
        {
            efecto.Play();
        }

        // Esperar a que el Animator salga de Idle
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName))
        {
            yield return null;
        }

        // Esperar a que regrese a Idle
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName))
        {
            yield return null;
        }

        animationPlaying = false;
    }
}

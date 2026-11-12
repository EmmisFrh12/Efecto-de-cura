
using UnityEngine;

public class volume : MonoBehaviour
{
    [Header("Movimiento")]
    public float movespeed = 2f;
    public float distance = 3f;

    [Header("Regreso")]
    public float returnDelay = 2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public bool activatemove;

    private bool returning;
    private bool waiting;
    private float timer;

    public void move()
    {
        startPosition = transform.position;

        targetPosition = startPosition + Vector3.forward * distance;

        activatemove = true;
        returning = false;
        waiting = false;
        timer = 0f;
    }

    void Update()
    {
        // Avanzar
        if (activatemove)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                movespeed * Time.deltaTime
            );

            if (transform.position == targetPosition)
            {
                activatemove = false;
                waiting = true;
                timer = 0f;
            }
        }

        // Esperar antes de regresar
        if (waiting)
        {
            timer += Time.deltaTime;

            if (timer >= returnDelay)
            {
                waiting = false;
                returning = true;
            }
        }

        // Regresar a la posición inicial
        if (returning)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                movespeed * Time.deltaTime
            );

            if (transform.position == startPosition)
            {
                returning = false;
            }
        }
    }
}

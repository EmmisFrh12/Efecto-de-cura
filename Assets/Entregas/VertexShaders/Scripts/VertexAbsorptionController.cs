using UnityEngine;

[ExecuteInEditMode]
public class VertexAbsorptionController : MonoBehaviour
{

    [SerializeField] private Material target;
    [SerializeField] private float radius;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        target.SetVector("_Center", transform.position);
        target.SetFloat("_Radius", radius);
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    #endif
}

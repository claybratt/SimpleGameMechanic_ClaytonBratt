using UnityEngine;

public class RaycastCollisionHandler : MonoBehaviour
{
    public float range = 7;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Raycast Script Started");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = Vector3.forward;
        Ray playerRay = new Ray(transform.position, transform.TransformDirection(direction * range));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction * range), Color.red);

        if(Physics.Raycast(playerRay, out RaycastHit hit, range))
        {
            if (hit.collider.CompareTag("Static"))
            {
                Debug.Log("Hit Static");
            }
            else if (hit.collider.CompareTag("Enemy")) 
            {
                Debug.Log("Hit Enemy");
            }

            // New functionality: Change the hit object's color to a random color
            Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
            if (hitRenderer != null) // Ensure the object has a Renderer component
            {
                hitRenderer.material.color = GetRandomColor();
            }
        }
    }
    Color GetRandomColor() 
    {
        return new Color(Random.value, Random.value, Random.value);
    }
}

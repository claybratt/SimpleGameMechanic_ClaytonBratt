using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TriggerObject"))
        {
            // Destroy Collision Object
            // Destroy(other.gameObject);
            // Debug.Log("Trigger Object Removed");
            Debug.Log("ENTER");
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("TriggerObject"))
        {
            // Destroy Collision Object
            // Destroy(other.gameObject);
            // Debug.Log("Trigger Object Removed");
            Debug.Log("INSIDE");
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TriggerObject"))
        {
            // Destroy Collision Object
            // Destroy(other.gameObject);
            // Debug.Log("Trigger Object Removed");
            Debug.Log("EXIT");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

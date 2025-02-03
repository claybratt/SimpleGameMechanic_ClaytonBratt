using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class RaycastLaser : MonoBehaviour
{
    public float debugLaserTime = 5f;
    private float objectIOR = 1.5f;         // Default IOR of glass
    private float enviornmentIOR = 1.0f;    // IOR of air
    public float maxRayDistance = 100f;    // Max distance for raycast
    public float rayWidth = 0.20f;          // Width of the ray

    private LineRenderer lineRenderer;

    void Start()
    {
        // Setup LineRenderer for visible rays
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.positionCount = 2; // Default to a single segment
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic visible shader
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.enabled = false; // Hide initially
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // Holding down the left mouse button
        {
            CastRay();
        }
        else
        {
            lineRenderer.enabled = false; // Hide ray when button is released
        }
    }

    void CastRay()
    {
        Vector3 rayOrigin = transform.position + Vector3.down * 0.5f; // Move ray down by 0.5 units
        Ray laserRay = new Ray(rayOrigin, transform.forward);
        RaycastHit enterHit;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 1; // Reset ray path
        lineRenderer.SetPosition(0, rayOrigin); // Start at player

        if (Physics.Raycast(laserRay, out enterHit, maxRayDistance))
        {
            //Debug.Log("Hit 1: " + enterHit.collider.name);
            //Debug.DrawRay(rayOrigin, transform.forward * enterHit.distance, Color.red, debugLaserTime);

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(1, enterHit.point); // Show the first segment

            RefractiveObject refractiveObj = enterHit.collider.GetComponent<RefractiveObject>(); // Check if the object hit is refractive
            if (refractiveObj != null)
            {
                objectIOR = refractiveObj.indexOfRefraction;

                // Compute refracted ray 
                Vector3 refractedInDir;

                if (Refract(laserRay.direction, enterHit.normal, enviornmentIOR, objectIOR, out refractedInDir))
                {
                    //Debug.DrawRay(enterHit.point, refractedInDir * 5, Color.blue, debugLaserTime);
                    //Debug.DrawRay(enterHit.point + refractedInDir * 5, -refractedInDir * 5, Color.blue, debugLaserTime);

                    Ray insideRay = new Ray(enterHit.point + refractedInDir * 5, -refractedInDir * 5);

                    RaycastHit exitHit;

                    if (enterHit.collider.Raycast(insideRay, out exitHit, maxRayDistance))
                    {
                        //Debug.Log("Hit 2: " + exitHit.collider.name);
                        lineRenderer.positionCount = 3;
                        lineRenderer.SetPosition(2, exitHit.point); // Add second segment (inside object)

                        // Compute refracted ray exiting the object
                        Vector3 refractedOutDir;
                        Vector3 exitNormal = exitHit.normal;

                        if (Vector3.Dot(refractedInDir, exitNormal) > 0) exitNormal = -exitNormal;

                        if (Refract(refractedInDir, exitNormal, objectIOR, enviornmentIOR, out refractedOutDir))
                        {
                            //Debug.DrawRay(exitHit.point, refractedOutDir * maxRayDistance, Color.green, debugLaserTime);

                            lineRenderer.positionCount = 4;
                            lineRenderer.SetPosition(3, exitHit.point + refractedOutDir * maxRayDistance); // Final outgoing ray
                        }else
                        {
                            //Debug.DrawRay(exitHit.point, refractedInDir * maxRayDistance, Color.green, debugLaserTime);

                            lineRenderer.positionCount = 4;
                            lineRenderer.SetPosition(3, exitHit.point + refractedInDir * maxRayDistance); // Final outgoing ray;
                        }
                    }
                }
            }
            ReflectiveObject reflectiveeObj = enterHit.collider.GetComponent<ReflectiveObject>(); // Check if the object hit is reflective
            if (reflectiveeObj != null)
            {
                Vector3 reflectedDir = Vector3.Reflect(laserRay.direction, enterHit.normal);
                Ray reflectedRay = new Ray(enterHit.point + enterHit.normal * 0.1f, reflectedDir);
                RaycastHit reflectedHit;
                if (Physics.Raycast(reflectedRay, out reflectedHit, maxRayDistance))
                {
                    Debug.Log("Hit 2: " + enterHit.collider.name);
                    Debug.DrawRay(reflectedRay.origin, reflectedRay.direction * 1f, Color.blue, debugLaserTime);
                    lineRenderer.positionCount = 3;
                    lineRenderer.SetPosition(2, reflectedHit.point); // Add second segment (reflected ray)
                }
            }
        }
        else
        {
            //Debug.DrawRay(rayOrigin, transform.forward * maxRayDistance, Color.red, debugLaserTime);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(1, rayOrigin + laserRay.direction * maxRayDistance); // Show the full ray
        }
    }

    bool Refract(Vector3 incident, Vector3 normal, float n1, float n2, out Vector3 refracted)
    {
        incident = incident.normalized;
        normal = normal.normalized;

        float n = n1 / n2;
        float cosI = -Vector3.Dot(normal, incident);
        float sinT2 = n * n * (1 - cosI * cosI);

        if (sinT2 > 1.0f) // Total Internal Reflection case
        {
            refracted = Vector3.zero;
            return false;
        }

        float cosT = Mathf.Sqrt(1 - sinT2);
        refracted = n * incident + (n * cosI - cosT) * normal;
        return true;
    }
}
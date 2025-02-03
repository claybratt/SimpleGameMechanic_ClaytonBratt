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
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 1; // Reset ray path
            CastLaserRay(transform.position + Vector3.down * 0.5f, transform.forward, maxRayDistance);
        }
        else
        {
            lineRenderer.enabled = false; // Hide ray when button is released
        }
    }

    void CastLaserRay(Vector3 rayOrigin, Vector3 rayDirection, float rayDistance)
    {
        Ray laserRay = new Ray(rayOrigin, rayDirection);
        RaycastHit enterHit;

        int linePosition = lineRenderer.positionCount - 1;
        //int linePosition = 0;
        lineRenderer.SetPosition(linePosition, rayOrigin); // Start at player

        if (Physics.Raycast(laserRay, out enterHit, rayDistance))
        {
            //Debug.Log("Hit 1: " + enterHit.collider.name);
            //Debug.DrawRay(rayOrigin, rayDirection * enterHit.distance, Color.red, debugLaserTime);

            lineRenderer.positionCount = linePosition + 2;
            lineRenderer.SetPosition(linePosition + 1, enterHit.point); // Show the first segment

            RefractiveObject refractiveObj = enterHit.collider.GetComponent<RefractiveObject>(); // Check if the object hit is refractive
            if (refractiveObj != null)
            {
                objectIOR = refractiveObj.indexOfRefraction;

                // Compute refracted ray 
                Vector3 refractedInDir;

                if (Refract(laserRay.direction, enterHit.normal, enviornmentIOR, objectIOR, out refractedInDir))
                {
                    //Debug.DrawRay(enterHit.point, refractedInDir * 5, Color.blue, debugLaserTime);
                    //Debug.DrawRay(enterHit.point + refractedInDir * rayDistance, -refractedInDir * rayDistance, Color.blue, debugLaserTime);

                    Ray insideRay = new Ray(enterHit.point + refractedInDir * rayDistance, -refractedInDir * rayDistance);

                    RaycastHit exitHit;

                    if (enterHit.collider.Raycast(insideRay, out exitHit, rayDistance))
                    {
                        //Debug.Log("Hit 2: " + exitHit.collider.name);
                        lineRenderer.positionCount = linePosition + 3;
                        lineRenderer.SetPosition(linePosition + 2, exitHit.point); // Add second segment (inside object)

                        // Compute refracted ray exiting the object
                        Vector3 refractedOutDir;
                        Vector3 exitNormal = exitHit.normal;

                        if (Vector3.Dot(refractedInDir, exitNormal) > 0) exitNormal = -exitNormal;

                        if (Refract(refractedInDir, exitNormal, objectIOR, enviornmentIOR, out refractedOutDir))
                        {
                            //Debug.DrawRay(exitHit.point, refractedOutDir * rayDistance, Color.green, debugLaserTime);

                            lineRenderer.positionCount = linePosition + 4;
                            lineRenderer.SetPosition(linePosition + 3, exitHit.point + refractedOutDir * rayDistance); // Final outgoing ray

                            CastLaserRay(exitHit.point, refractedOutDir, maxRayDistance);
                        }
                        else
                        {
                            //Debug.DrawRay(exitHit.point, refractedInDir * rayDistance, Color.green, debugLaserTime);

                            lineRenderer.positionCount = linePosition + 4;
                            lineRenderer.SetPosition(linePosition + 3, exitHit.point + refractedInDir * rayDistance); // Final outgoing ray;

                            CastLaserRay(exitHit.point, refractedInDir, maxRayDistance);
                        }
                    }
                }
            }
            ReflectiveObject reflectiveeObj = enterHit.collider.GetComponent<ReflectiveObject>(); // Check if the object hit is reflective
            if (reflectiveeObj != null)
            {
                Vector3 reflectedDir = Vector3.Reflect(laserRay.direction, enterHit.normal);
                Ray reflectedRay = new Ray(enterHit.point + enterHit.normal * 0.01f, reflectedDir);
                RaycastHit reflectedHit;
                if (Physics.Raycast(reflectedRay, out reflectedHit, rayDistance))
                {
                    //Debug.Log("Hit 2: " + enterHit.collider.name);
                    //Debug.DrawRay(reflectedRay.origin, reflectedRay.direction * rayDistance, Color.magenta, debugLaserTime);

                    lineRenderer.positionCount = linePosition + 3;
                    lineRenderer.SetPosition(linePosition + 2, reflectedHit.point); // Add second segment (reflected ray)

                    CastLaserRay(reflectedRay.origin, reflectedRay.direction, maxRayDistance);
                }
            }
        }
        else
        {
            //Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, debugLaserTime);
            lineRenderer.positionCount = linePosition + 2;
            lineRenderer.SetPosition(linePosition + 1, rayOrigin + laserRay.direction * rayDistance); // Show the full ray
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
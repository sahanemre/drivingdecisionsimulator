using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics.UI
{
    public class VehicleRadar : MonoBehaviour
    {
        public float detectionDistance = 10f; // Ray uzunluğu
        public LayerMask vehicleLayer;        // Hangi layer'lara çarpsın
        public float rayHeight = 1f;           // Yükseklik (y=1)

        void Update()
        {
            DetectAndDraw(Vector3.forward, Color.red, "FRONT");
            DetectAndDraw(-Vector3.forward, Color.blue, "BACK");
            DetectAndDraw(Vector3.right, Color.green, "RIGHT");
            DetectAndDraw(-Vector3.right, Color.yellow, "LEFT");
        }

        void DetectAndDraw(Vector3 direction, Color baseColor, string directionName)
        {
            RaycastHit hit;
            Vector3 origin = transform.position;
            origin.y += rayHeight; // Burada Y eksenine 1 ekliyoruz

            bool isHit = Physics.Raycast(origin, transform.TransformDirection(direction), out hit, detectionDistance, vehicleLayer);

            if (isHit)
            {
                Debug.Log($"Vehicle detected on {directionName}: {hit.collider.name}");
                Debug.DrawRay(origin, transform.TransformDirection(direction) * hit.distance, Color.magenta); // Çarptıysa mor
            }
            else
            {
                Debug.DrawRay(origin, transform.TransformDirection(direction) * detectionDistance, baseColor); // Çarpmazsa normal
            }
        }
    }
}

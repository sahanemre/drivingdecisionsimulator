using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehiclePhysics.UI
{
    public class VehicleRadar : MonoBehaviour
    {
        public float detectionDistance = 10f;
        public LayerMask vehicleLayer;
        public float rayHeight = 1f;
        public float sideOffset = 1f;     // Sağa-sola ne kadar kaydırılacak
        public float frontBackOffset = 1f; // Öne-arkaya ne kadar kaydırılacak

        void Update()
        {
            Vector3[] directions = { Vector3.forward, -Vector3.forward, Vector3.right, -Vector3.right };
            string[] directionNames = { "FRONT", "BACK", "RIGHT", "LEFT" };
            Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };

            for (int i = 0; i < directions.Length; i++)
            {
                DetectAndDrawMultipleRays(directions[i], colors[i], directionNames[i]);
            }
        }

        void DetectAndDrawMultipleRays(Vector3 direction, Color baseColor, string directionName)
        {
            // Merkezden
            Vector3 centerOrigin = transform.position + Vector3.up * rayHeight;
            CastRay(centerOrigin, direction, baseColor, directionName + " (CENTER)");

            // Sağ ve sol offsetli (yanlardan)
            Vector3 sideOffsetVector = transform.right * sideOffset;
            Vector3 leftOrigin = centerOrigin - sideOffsetVector;
            Vector3 rightOrigin = centerOrigin + sideOffsetVector;

            CastRay(leftOrigin, direction, baseColor * 0.8f, directionName + " (LEFT SIDE)");
            CastRay(rightOrigin, direction, baseColor * 0.8f, directionName + " (RIGHT SIDE)");

            // Önden ve arkadan offsetli (uzunlamasına)
            Vector3 frontBackOffsetVector = transform.forward * frontBackOffset;
            Vector3 frontOrigin = centerOrigin + frontBackOffsetVector;
            Vector3 backOrigin = centerOrigin - frontBackOffsetVector;

            CastRay(frontOrigin, direction, baseColor * 0.6f, directionName + " (FRONT EDGE)");
            CastRay(backOrigin, direction, baseColor * 0.6f, directionName + " (BACK EDGE)");
        }

        void CastRay(Vector3 origin, Vector3 direction, Color color, string logPrefix)
        {
            RaycastHit hit;
            Vector3 dirWorld = transform.TransformDirection(direction);
            if (Physics.Raycast(origin, dirWorld, out hit, detectionDistance, vehicleLayer))
            {
                float hitDistance = hit.distance;
                Debug.Log($"[{logPrefix}] Araç algılandı: {hit.collider.name}, Mesafe: {hitDistance:F2} metre");
                Debug.DrawRay(origin, dirWorld * hitDistance, Color.magenta);
            }
            else
            {
                Debug.DrawRay(origin, dirWorld * detectionDistance, color);
            }
        }

    }
}

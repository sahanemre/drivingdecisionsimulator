using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LensFlareComponentSRP))]
public class RTC_SRPLensflare : MonoBehaviour {

    private Light _lightSource;

    /// <summary>
    /// Light source.
    /// </summary>
    private Light LightSource {

        get {

            if (_lightSource == null)
                _lightSource = GetComponent<Light>();

            return _lightSource;

        }

    }

    /// <summary>
    /// SRP Lens flare for URP / HDRP.
    /// </summary>
    private LensFlareComponentSRP lensFlare_SRP;

    /// <summary>
    /// Max flare brigthness of the light.
    /// </summary>
    [Range(0f, 10f)] public float flareBrightness = 1.5f;

    private Camera mCam;

    void Start() {

        lensFlare_SRP = GetComponent<LensFlareComponentSRP>();

        InvokeRepeating(nameof(FindMCamera), .1f, 1f);

    }

    private void FindMCamera() {

        mCam = Camera.main;

    }

    // Update is called once per frame
    void Update() {

        if (!lensFlare_SRP)
            return;

        //  If no main camera found, return.
        if (!mCam)
            return;

        // Calculate the distance factor based on distance to the camera
        float distanceToCamera = Vector3.Distance(LightSource.transform.position, mCam.transform.position);
        float distanceFactor = Mathf.Clamp01(1 - (distanceToCamera / 300f));

        // Calculate the angle factor based on the angle between the camera's and light's forward directions
        float angle = Vector3.Angle(LightSource.transform.forward, mCam.transform.forward);
        float angleFactor = Mathf.Clamp01(angle / 360f);  // Normalizes angleFactor from 1 at 180 degrees to 0 at 0 degrees

        // Calculate the new intensity based on light intensity, distance factor, and angle factor
        float newIntensity = Mathf.Clamp(LightSource.intensity * distanceFactor * angleFactor, 0, flareBrightness);

        lensFlare_SRP.intensity = newIntensity;
        lensFlare_SRP.attenuationByLightShape = false;

    }

}

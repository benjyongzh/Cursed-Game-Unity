using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Mirror;

public class PostProcessing : NetworkBehaviour
{
    GameObject postProcesser;
    PostProcessVolume postProcessVolume;
    ColorGrading colorgrading;
    Bloom bloom;
    Vignette vignette;
    UnitStats stats;

    [SerializeField] float criticalHealthThresholdUpper = 30f;
    [SerializeField] float criticalHealthThresholdLower = 15f;
    [SerializeField, Range(-100f, 0f)] float saturationMinimum = -100f;
    [SerializeField, Range(2f, 10f)] float bloomMaximumIntensity = 3.5f;
    [SerializeField, Range(0.48f, 0.65f)] float vignetteMaximumIntensity = 0.52f;
    [SerializeField, Range(0, 0.6f)] float vignetteHighestMinimumIntensity = 0.4f;
    [SerializeField, Range(0, 0.4f)] float vignetteDamageAddition = 0.15f;
    [SerializeField, Range(0, 1f)] float vignetteIntensityReduction = 0.04f;

    float vignetteMinValue = 0;

    private void Start()
    {
        postProcesser = GameObject.Find("/GameSystems/Camera Postprocessing");
        postProcessVolume = postProcesser.GetComponent<PostProcessVolume>();
        stats = GetComponent<UnitStats>();
        postProcessVolume.profile.TryGetSettings(out colorgrading);
        postProcessVolume.profile.TryGetSettings(out bloom);
        postProcessVolume.profile.TryGetSettings(out vignette);
        colorgrading.saturation.value = 0;
        bloom.intensity.value = 0;
        vignette.intensity.value = 0;
    }

    void Update()
    {
        if (!hasAuthority)
            return;

        //bloom only starts brightening up if health below a threshold. static value dependent on health left.
        //vignette lights up on taking damage AND if health below 30. health value determines minimum value of vignette. vignette decreases over time naturally.
        if (stats.GetCurrentHealth() <= criticalHealthThresholdUpper)
        {
            //below HP upper threshold
            if (stats.GetCurrentHealth() < criticalHealthThresholdLower)
            {
                //below HP lower threshold as well
                colorgrading.saturation.value = saturationMinimum;
                bloom.intensity.value = bloomMaximumIntensity;
                vignetteMinValue = vignetteHighestMinimumIntensity;
            }
            else
            {
                //in between upper and lower threshold
                float window = criticalHealthThresholdUpper - criticalHealthThresholdLower;
                float difference = window - (stats.GetCurrentHealth() - criticalHealthThresholdLower);
                colorgrading.saturation.value = (difference / window) * saturationMinimum;
                bloom.intensity.value = (difference / window) * bloomMaximumIntensity;
                vignetteMinValue = (difference / window) * vignetteHighestMinimumIntensity;
            }
        }
        else
        {
            //above HP upper threshold
            if (colorgrading.saturation.value > 0)
                colorgrading.saturation.value = 0f;
            if (bloom.intensity.value > 0)
                bloom.intensity.value = 0f;
            if (vignetteMinValue > 0)
                vignetteMinValue = 0f;
        }

        if (vignette.intensity.value > vignetteMinValue)
        {
            //cap intensity according to maxintensitysetting
            if (vignette.intensity.value > vignetteMaximumIntensity)
                vignette.intensity.value = vignetteMaximumIntensity;

            //decrease vignette intensity value over time
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, vignetteMinValue, vignetteIntensityReduction * Time.deltaTime);
        }
        else if (vignette.intensity.value < vignetteMinValue)
            vignette.intensity.value = vignetteMinValue;

    }

    public void VignetteDamage()
    {
        vignette.intensity.value += vignetteDamageAddition;
    }
}

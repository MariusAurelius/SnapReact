using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer MainAudioMixer;
    [SerializeField] private Slider VolumeSlider;
    public TMP_Dropdown GraphicsDropdown;
    public TMP_Dropdown ResolutionDropdown;
    private Resolution[] Resolutions;

    [SerializeField] private Toggle FullscreenToggle;

    private void Start()
    {
        Resolutions = Screen.resolutions;
        ResolutionDropdown.ClearOptions();

        List<string> options = new();
        int currentResolutionIndex = 0;
        for (int i = 0; i < Resolutions.Length; i++)
        {
            string option = Resolutions[i].width + " x " + Resolutions[i].height;
            options.Add(option);

            if (Resolutions[i].width == Screen.currentResolution.width &&
                Resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }


        // update UI with actual values

        ResolutionDropdown.AddOptions(options);
        ResolutionDropdown.value = currentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();

        VolumeSlider.value = GetVolume();

        FullscreenToggle.isOn = Screen.fullScreen;

        GraphicsDropdown.value = QualitySettings.GetQualityLevel();
        GraphicsDropdown.RefreshShownValue();

    }

    public void SetVolume(float volume)
    {
        MainAudioMixer.SetFloat("MainVolume", volume);
    }

    public float GetVolume()
    {
        float value;
        bool result = MainAudioMixer.GetFloat("MainVolume", out value);
        if (result)
        {
            return value;
        }
        else
        {
            return 0f;
        }
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = Resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
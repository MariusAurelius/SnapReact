using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckboxButton : MonoBehaviour
{
    public Image CheckmarkImage;
    public bool isChecked = false;

    void Awake()
    {
        CheckmarkImage.gameObject.SetActive(isChecked);
    }

    public void ToggleCheckmark()
    {
        isChecked = !isChecked;
        CheckmarkImage.gameObject.SetActive(isChecked); // Show / hide the checkmark
        if (isChecked && ScenesManager.Instance.GetCurrentScene() == ScenesManager.Scenes.AudioStimuliMenu) // play sfx if toggling checkbox to true
        {
            string stimulus_name = gameObject.name;
            Debug.Log(stimulus_name);
            
            stimulus_name = stimulus_name.Replace("Button", "");

            if (stimulus_name.Contains("Arrow"))
            {
                stimulus_name = stimulus_name.Replace("Arrow", "");
            }
            else if (stimulus_name.Contains("Color"))
            {
                stimulus_name = stimulus_name.Replace("Color", "");
            }
            else if (stimulus_name.Contains("Noise"))
            {
                stimulus_name = stimulus_name.Replace("Noise", "");
            }
            AudioManager.Instance.PlaySFXFromString(stimulus_name);
        }
    }
    
    public void ToggleTrue() {
        isChecked = true;
        CheckmarkImage.gameObject.SetActive(isChecked);
    }

    public void ToggleFalse() {
        isChecked = false;
        CheckmarkImage.gameObject.SetActive(isChecked);
    }

    /// <summary>
    /// Toggles checkmark to the value of <c><paramref name="toggle"/></c>.
    /// </summary>
    public void ToggleTo(bool toggle) 
    {
        if (toggle) ToggleTrue();
        else ToggleFalse();
    }
}
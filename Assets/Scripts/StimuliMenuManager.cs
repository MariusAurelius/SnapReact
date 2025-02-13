using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Dependencies.NCalc;
using Unity.Mathematics;

/// <summary>
/// Class handling the saving and loading of stimuli, and the Audio Stimuli Menu and Visual Stimuli Menu scenes. 
/// </summary>
public class StimuliMenuManager : MonoBehaviour
{
    private const string VISUAL_STIMULI_FILENAME = "SavedVisualStimuli.json";
    private const string AUDIO_STIMULI_FILENAME = "SavedAudioStimuli.json";
    private ScenesManager.Scenes? currentScene;

        // Visual stimuli gameobjects
    [SerializeField] private GameObject Visual_Arrows;
    [SerializeField] private GameObject Visual_Colors;
    [SerializeField] private GameObject Visual_Numbers;
    [SerializeField] private GameObject Visual_Letters;

        // Audio stimuli gameobjects
    [SerializeField] private GameObject Audio_Directions;
    [SerializeField] private GameObject Audio_Colors;
    [SerializeField] private GameObject Audio_Numbers;
    [SerializeField] private GameObject Audio_Letters;
    [SerializeField] private GameObject Audio_Noises;


    /// <summary>
    /// List of selected visual stimuli to load to and from a JSON file.
    /// </summary>
    private List<string> SelectedVisualStimuli = new(); 

    /// <summary>
    /// List of selected audio stimuli to load to and from a JSON file.
    /// </summary>
    private List<string> SelectedAudioStimuli = new(); 

    /// <summary>
    /// Retrieves checked stimuli.
    /// </summary>
    private void Start() {
        currentScene = ScenesManager.Instance.GetCurrentScene();

        if (currentScene == ScenesManager.Scenes.AudioStimuliMenu)
        {
            Debug.Log("In Audio Stimuli menu");
            LoadSavedAudioStimuli();
            UpdateAudioStimuliCheckboxes();
        }
        else if (currentScene == ScenesManager.Scenes.VisualStimuliMenu)
        {
            Debug.Log("In Visual Stimuli menu");
            LoadSavedVisualStimuli();
            UpdateVisualStimuliCheckboxes();
        }
        else if (currentScene == ScenesManager.Scenes.ReactionTest)
        {
            
        }
    }

    /// <summary>
    /// Custom function called on 'Previous' button press to update list before object destruction.
    /// </summary>
    public void ToPreviousScene()
    {
        Debug.Log("calling przv scene");
        if (currentScene == ScenesManager.Scenes.VisualStimuliMenu)
        {
            UpdateVisualStimuliList(); // Update before transitioning
            SaveVisualStimuli();      // Save changes
            ScenesManager.Instance.LoadMainMenu();
        }
        else if (currentScene == ScenesManager.Scenes.AudioStimuliMenu)
        {
            UpdateAudioStimuliList();
            SaveAudioStimuli();
            ScenesManager.Instance.LoadVisualStimuliMenu();
        }
        else
        {
            Debug.LogWarning("Function ToPreviousScene() called in a scene it shouldn't have been.");
        }
    }

    /// <summary>
    /// Custom function called on 'Next' button press to update list before object destruction.
    /// </summary>
    public void ToNextScene()
    {
        Debug.Log("calling next scene");
        if (currentScene == ScenesManager.Scenes.VisualStimuliMenu)
        {
            UpdateVisualStimuliList(); // Update before transitioning
            SaveVisualStimuli();      // Save changes
            ScenesManager.Instance.LoadAudioStimuliMenu();
        }
        else if (currentScene == ScenesManager.Scenes.AudioStimuliMenu)
        {
            UpdateAudioStimuliList();
            SaveAudioStimuli();
            ScenesManager.Instance.LoadTimeControlsMenu();
        }
        else
        {
            Debug.LogWarning("Function ToNextScene() called in a scene it shouldn't have been.");
        }
    }

    /// <summary>
    /// Updates the visual stimuli list based on if each checkbox is toggled on or off.
    /// </summary>
    public void UpdateVisualStimuliList() {
        var categories = new List<Transform> {Visual_Arrows.transform, Visual_Colors.transform, Visual_Numbers.transform, Visual_Letters.transform};
        foreach (var category in categories)
        {
            foreach (Transform transform in category)
            {
                if (transform.gameObject.GetComponent<CheckboxButton>().isChecked)
                {
                    AddVisualStimuli(transform.gameObject.GetComponent<CheckboxButton>().name);
                }
                else
                {
                    RemoveVisualStimuli(transform.gameObject.GetComponent<CheckboxButton>().name);
                }
            }
        }
    }

    /// <summary>
    /// Updates the audio stimuli list based on if each checkbox is toggled on or off.
    /// </summary>
    public void UpdateAudioStimuliList() {
        var categories = new List<Transform> {Audio_Directions.transform, Audio_Colors.transform, Audio_Numbers.transform, Audio_Letters.transform, Audio_Noises.transform};
        foreach (var category in categories)
        {
            foreach (Transform transform in category)
            {
                if (transform.gameObject.GetComponent<CheckboxButton>().isChecked)
                {
                    AddAudioStimuli(transform.gameObject.GetComponent<CheckboxButton>().name);
                }
                else
                {
                    RemoveAudioStimuli(transform.gameObject.GetComponent<CheckboxButton>().name);
                }
            }
        }
    }

    /// <summary>
    /// Adds a visual <c><paramref name="stimulus"/></c> to the list.
    /// </summary>
    public void AddVisualStimuli(string stimulus) {
        if (!SelectedVisualStimuli.Contains(stimulus))
        {
            SelectedVisualStimuli.Add(stimulus);
        }
    }

    /// <summary>
    /// Removes a visual <c><paramref name="stimulus"/></c> from the list.
    /// </summary>
    public void RemoveVisualStimuli(string stimulus) {
        if (SelectedVisualStimuli.Contains(stimulus))
        {
            SelectedVisualStimuli.Remove(stimulus);
        }
    }

    /// <summary>
    /// Adds an audio <c><paramref name="stimulus"/></c> to the list.
    /// </summary>
    public void AddAudioStimuli(string stimulus) {
        if (!SelectedAudioStimuli.Contains(stimulus))
        {
            SelectedAudioStimuli.Add(stimulus);
        }
    }

    /// <summary>
    /// Removes an audio <c><paramref name="stimulus"/></c> from the list.
    /// </summary>
    public void RemoveAudioStimuli(string stimulus) {
        if (SelectedAudioStimuli.Contains(stimulus))
        {
            SelectedAudioStimuli.Remove(stimulus);
        }
    }

    /// <summary>
    /// Loads the saved visual stimuli from the JSON file.
    /// </summary>
    public void LoadSavedVisualStimuli() {
        SelectedVisualStimuli = FileHandler.ReadListFromJSON<string>(VISUAL_STIMULI_FILENAME);
        // Debug.Log("Saved stimuli loaded: ");
        // foreach (var item in SelectedVisualStimuli)
        // {
        //     Debug.Log(item);
        // }
    }

    /// <summary>
    /// Returns the saved visual stimuli from the JSON file.
    /// </summary>
    public static List<string> GetSavedVisualStimuli() {
        return FileHandler.ReadListFromJSON<string>(VISUAL_STIMULI_FILENAME);
    }


    /// <summary>
    /// Loads the saved visual stimuli from the JSON file.
    /// </summary>
    public void LoadSavedAudioStimuli() {
        SelectedAudioStimuli = FileHandler.ReadListFromJSON<string>(AUDIO_STIMULI_FILENAME);
        // Debug.Log("Saved stimuli loaded: ");
        // foreach (var item in SelectedAudioStimuli)
        // {
        //     Debug.Log(item);
        // }
    }

    /// <summary>
    /// Returns the saved audio stimuli from the JSON file.
    /// </summary>
    public static List<string> GetSavedAudioStimuli() {
        return FileHandler.ReadListFromJSON<string>(AUDIO_STIMULI_FILENAME);
    }


    /// <summary>
    /// Saves the checked visual stimuli to the JSON file. 
    /// </summary>
    public void SaveVisualStimuli() {
        FileHandler.SaveToJSON<string>(SelectedVisualStimuli, VISUAL_STIMULI_FILENAME);
        Debug.Log("Saving stimuli: ");
        foreach (var item in SelectedVisualStimuli)
        {
            Debug.Log(item);
        }
    }

    /// <summary>
    /// Saves the checked audio stimuli to the JSON file. 
    /// </summary>
    public void SaveAudioStimuli() {
        FileHandler.SaveToJSON<string>(SelectedAudioStimuli, AUDIO_STIMULI_FILENAME);
        Debug.Log("Saving stimuli: ");
        foreach (var item in SelectedAudioStimuli)
        {
            Debug.Log(item);
        }
    }

    /// <summary>
    /// Toggles all the checkboxes to true if <c><paramref name="isChecked"/></c>, else toggles all to false.
    /// </summary>
    /// <param name="isChecked"></param>
    public void ToggleAllVisualStimuli(bool isChecked) {
        var categories = new List<Transform> {Visual_Arrows.transform, Visual_Colors.transform, Visual_Numbers.transform, Visual_Letters.transform};
        foreach (var category in categories)
        {
            foreach (Transform transform in category)
            {
                transform.gameObject.GetComponent<CheckboxButton>().ToggleTo(isChecked);
            }
        }
    }

    /// <summary>
    /// Toggles all the checkboxes to true if <c><paramref name="isChecked"/></c>, else toggles all to false.
    /// </summary>
    public void ToggleAllAudioStimuli(bool isChecked) {
        var categories = new List<Transform> {Audio_Directions.transform, Audio_Colors.transform, Audio_Numbers.transform, Audio_Letters.transform, Audio_Noises.transform};
        foreach (var category in categories)
        {
            foreach (Transform transform in category)
            {
                transform.gameObject.GetComponent<CheckboxButton>().ToggleTo(isChecked);
            }
        }
    }

    /// <summary>
    /// Updates the checkboxes by toggling all the checkboxes of the stimuli in the list. 
    /// </summary>
    public void UpdateVisualStimuliCheckboxes() {
        ToggleAllVisualStimuli(false);

        foreach (var stimulus in SelectedVisualStimuli)
        {
            if (stimulus.Contains("Arrow"))
            {
                Visual_Arrows.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
            else if (stimulus.Contains("Color"))
            {
                Visual_Colors.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
            else if (stimulus.Contains("Number"))
            {
                Visual_Numbers.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
            else if (stimulus.Contains("Letter"))
            {
                Visual_Letters.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
        }
    }

    /// <summary>
    /// Updates the checkboxes by toggling all the checkboxes of the stimuli in the list. 
    /// </summary>
    public void UpdateAudioStimuliCheckboxes() {
        ToggleAllAudioStimuli(false);

        foreach (var stimulus in SelectedAudioStimuli)
        {
            if (stimulus.Contains("Arrow"))
            {
                Audio_Directions.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
            else if (stimulus.Contains("Color"))
            {
                Audio_Colors.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
            else if (stimulus.Contains("Number"))
            {
                Audio_Numbers.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
            else if (stimulus.Contains("Letter"))
            {
                Audio_Letters.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
            else if (stimulus.Contains("Noise"))
            {
                Audio_Noises.transform.Find(stimulus).GetComponent<CheckboxButton>().ToggleTrue();
            }
        }
    }
}
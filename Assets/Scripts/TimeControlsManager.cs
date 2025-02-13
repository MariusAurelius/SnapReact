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
using Unity.VisualScripting;

/// <summary>
/// Class handling the saving and loading of time control settings in the Time Controls Menu scene. 
/// </summary>
public class TimeControlsManager : MonoBehaviour
{
    private const string TIME_CONTROLS_FILENAME = "SavedTimeControls.json";
    private ScenesManager.Scenes? currentScene;

    [SerializeField] private TMP_Text DurationText;
    [SerializeField] private Slider DurationSlider;
    [SerializeField] private Toggle RandomizeDurationToggle;
    [SerializeField] private TMP_Text MinDurationText;
    [SerializeField] private TMP_Text MaxDurationText;

    [SerializeField] private TMP_Text DelayText;
    [SerializeField] private Slider DelaySlider;
    [SerializeField] private Toggle RandomizeDelayToggle;
    [SerializeField] private TMP_Text MinDelayText;
    [SerializeField] private TMP_Text MaxDelayText;

    [SerializeField] private TMP_Text RoundsText;
    [SerializeField] private Toggle UnlimitedRoundsToggle;

    [SerializeField] private Toggle ShowDelayCountdownToggle;
    [SerializeField] private Toggle ShowDurationCountdownToggle;
    [SerializeField] private Toggle ShowNumberOfRoundsLeftToggle;

    [SerializeField] private Toggle SimultaneousVisualAudioStimuliToggle;

    private Color originalDurationSliderHandleColor;
    private Color originalDurationSliderFillColor;

    private Color originalDelaySliderHandleColor;
    private Color originalDelaySliderFillColor;

    private TimeControls timeControls;


    /// <summary>
    /// determines current scene. Start instead of Awake to make sure ScenesManager Instance exists.
    /// </summary>
    private void Start()
    {
        timeControls = new();
        currentScene = ScenesManager.Instance.GetCurrentScene();
        if (currentScene == ScenesManager.Scenes.TimeControlsMenu)
        {
            originalDurationSliderHandleColor = GetSliderHandleColor(DurationSlider);
            originalDurationSliderFillColor = GetSliderFillColor(DurationSlider);
            originalDelaySliderHandleColor = GetSliderHandleColor(DelaySlider);
            originalDelaySliderFillColor = GetSliderFillColor(DelaySlider);

            LoadSavedTimeControls();
            UpdateTimeControlsUI();
        }
        else if (currentScene == ScenesManager.Scenes.ReactionTest)
        {
            
        }
    }

    /// <summary>
    /// Custom function called on 'Previous' button press to update time controls before object destruction.
    /// </summary>
    public void ToPreviousScene()
    {
        if (currentScene == ScenesManager.Scenes.TimeControlsMenu)
        {
            UpdateTimeControls(); // Update before transitioning
            SaveTimeControls();      // Save changes
            ScenesManager.Instance.LoadAudioStimuliMenu();
        }
        else
        {
            Debug.LogWarning("Function ToPreviousScene() called in a scene it shouldn't have been.");
        }
    }

    /// <summary>
    /// Custom function called on 'Next' button press to update time controls before object destruction.
    /// </summary>
    public void ToNextScene()
    {
        if (currentScene == ScenesManager.Scenes.TimeControlsMenu)
        {
            UpdateTimeControls();
            SaveTimeControls();
            ScenesManager.Instance.LoadReactionTest();
        }
        else
        {
            Debug.LogWarning("Function ToNextScene() called in a scene it shouldn't have been.");
        }
    }


    public Color GetSliderFillColor(Slider slider)
    {
        Image fillImage = slider.fillRect.GetComponent<Image>();
        return fillImage.color;
    }

    public Color GetSliderHandleColor(Slider slider)
    {
        return slider.colors.normalColor;
    }

    public void SetSliderFillColor(Slider slider, Color newColor)
    {
        Image fillImage = slider.fillRect.GetComponent<Image>();
        fillImage.color = newColor;
    }

    public void SetSliderHandleColor(Slider slider, Color newColor)
    {
        var sliderColors = slider.colors;
        sliderColors.normalColor = newColor;
        slider.colors = sliderColors;
    }


    // functions used to update slider and text values, used by the functions that are called on +/- button click

    /// <summary>
    /// Increases the time value of the slider and the text by 0.1s
    /// </summary>
    /// <param name="slider">Slider to change the time value.</param>
    /// <param name="text">TMP_Text showing the current time value.</param>
    public void IncreaseTime(Slider slider, TMP_Text text)
    {
        if (slider.value > 599) // cap at 60s
        {
            return;
        }

        slider.value += 1f;

        float seconds = slider.value / 10f;

        text.text = seconds.ToString("F1") + "s";
    }

    /// <summary>
    /// Increases the time value of the text by 0.1s
    /// </summary>
    /// <param name="text">TMP_Text showing the current time value.</param>
    public void IncreaseTime(TMP_Text text)
    {
        string txt = text.text.Replace("s", "").Trim(); // remove the 's' at the end to get a float
        if (float.TryParse(txt, out float seconds))
        {
            if (seconds <= 59.9f)
            {
                seconds += 0.1f;
            }
            text.text = seconds.ToString() + "s";
        }
        else
        {
            Debug.LogError("Error: the seconds text object did not contain an integer.");
        }
    }

    /// <summary>
    /// Decreases the time value of the slider and the text by 0.1s
    /// </summary>
    /// <param name="slider">Slider to change the time value.</param>
    /// <param name="text">TMP_Text showing the current time value.</param>
    public void DecreaseTime(Slider slider, TMP_Text text)
    {
        if (slider.value < 1)
        {
            return;
        }

        slider.value -= 1f;

        float seconds = slider.value / 10f;

        text.text = seconds.ToString("F1") + "s";
    }

    /// <summary>
    /// Decreases the time value of the text by 0.1s
    /// </summary>
    /// <param name="text">TMP_Text showing the current time value.</param>
    public void DecreaseTime(TMP_Text text)
    {
        string txt = text.text.Replace("s", "").Trim(); // remove the 's' at the end to get a float
        if (float.TryParse(txt, out float seconds))
        {
            if (seconds >= 0.1f)
            {
                seconds -= 0.1f;
            }
            text.text = seconds.ToString() + "s";
        }
        else
        {
            Debug.LogError("Error: the seconds text object did not contain an integer.");
        }
    }


    // change specific value for duration and delay (on +/- button click)

    /// <summary>
    /// Increases the on-screen duration of the stimulus by 0.1s, and updates the UI.
    /// </summary>
    public void IncreaseDuration()
    {
        IncreaseTime(DurationSlider, DurationText);
    }

    /// <summary>
    /// Decreases the on-screen duration of the stimulus by 0.1s, and updates the UI.
    /// </summary>
    public void DecreaseDuration()
    {
        DecreaseTime(DurationSlider, DurationText);
    }

    /// <summary>
    /// Increases the delay between the stimulus by 0.1s, and updates the UI.
    /// </summary>
    public void IncreaseDelay()
    {
        IncreaseTime(DelaySlider, DelayText);
    }

    /// <summary>
    /// Decreases the delay between the stimulus by 0.1s, and updates the UI.
    /// </summary>
    public void DecreaseDelay()
    {
        DecreaseTime(DelaySlider, DelayText);
    }


    // function used for min and max change

    /// <summary>
    /// Checks if a string <c><paramref name="a"/></c> is inferior or equal to a string <c><paramref name="b"/></c>, where 
    /// both strings contain a float. 
    /// </summary>
    /// <param name="a">String of format X.Ys</param>
    /// <param name="b">String of format X.Ys</param>
    public bool IsInferiorEqualTo(string a, string b)
    {
        a = a.Replace("s", "").Trim(); // remove the 's' at the end to get a float
        b = b.Replace("s", "").Trim(); // remove the 's' at the end to get a float
        if (float.TryParse(a, out float a_seconds) && float.TryParse(b, out float b_seconds))
        {
            return a_seconds <= b_seconds;
        }
        Debug.LogError("Error: one of the 2 strings did not contain an integer.");
        return false;
    }


    // change Min and Max Duration (on +/- button click)
    public void IncreaseMinDuration()
    {
        if (IsInferiorEqualTo(MaxDurationText.text, MinDurationText.text)) // cant increase min if min >= max <=> max <= min
        {
            return; // or MinDurationText.text = MaxDurationText.text but no reason for min to be superior
        }
        IncreaseTime(MinDurationText);
    }

    public void DecreaseMinDuration()
    {
        DecreaseTime(MinDurationText);
    }

    public void IncreaseMaxDuration()
    {
        IncreaseTime(MaxDurationText);
    }

    public void DecreaseMaxDuration()
    {
        if (IsInferiorEqualTo(MaxDurationText.text, MinDurationText.text)) // cant decrease if max <= min
        {
            return; // or MaxDurationText.text = MinDurationText.text but no reason for max to be inferior
        }
        DecreaseTime(MaxDurationText);
    }


    // change Min and Max Delay (on +/- button click)
    public void IncreaseMinDelay()
    {
        if (IsInferiorEqualTo(MaxDelayText.text, MinDelayText.text)) // cant increase min if min >= max <=> max <= min
        {
            return; // or MinDelayText.text = MaxDelayText.text but no reason for min to be superior
        }
        IncreaseTime(MinDelayText);
    }

    public void DecreaseMinDelay()
    {
        DecreaseTime(MinDelayText);
    }

    public void IncreaseMaxDelay()
    {
        IncreaseTime(MaxDelayText);
    }

    public void DecreaseMaxDelay()
    {
        if (IsInferiorEqualTo(MaxDelayText.text, MinDelayText.text)) // cant decrease if max <= min
        {
            return; // or MaxDelayText.text = MinDelayText.text but no reason for max to be inferior
        }
        DecreaseTime(MaxDelayText);
    }


    // update time text via slider

    /// <summary>
    /// Updates the UI duration text via the slider.
    /// </summary>
    public void UpdateDurationText(float duration)
    {
        float seconds = duration / 10f;

        DurationText.text = seconds.ToString("F1") + "s";
    }

    /// <summary>
    /// Updates the UI delay text via the slider.
    /// </summary>
    public void UpdateDelayText(float delay)
    {
        float seconds = delay / 10f;

        DelayText.text = seconds.ToString("F1") + "s";
    }


    // change number of rounds (on +/- button click)

    /// <summary>
    /// Increases the number of rounds by 1.
    /// </summary>
    public void IncreaseRounds()
    {
        if (int.TryParse(RoundsText.text, out int rounds))
        {
            if (rounds < 999) // arbitrary limit at 999 rounds so that not too many digits that would ruin UI
            {
                rounds++;
            }
            RoundsText.text = rounds.ToString();
        }
        else
        {
            Debug.LogError("Error: the rounds text object did not contain an integer.");
        }
    }

    /// <summary>
    /// Decreases the number of rounds by 1.
    /// </summary>
    public void DecreaseRounds()
    {
        if (int.TryParse(RoundsText.text, out int rounds))
        {
            if (rounds >= 2)
            {
                rounds--;
            }
            RoundsText.text = rounds.ToString();
        }
        else
        {
            Debug.LogError("Error: the rounds text object did not contain an integer.");
        }
    }


    // change UI appearance based on whether randomize is toggled on/off

    /// <summary>
    /// "Minimizes" the slider and text appearance by changing their colors to gray, OR the Min and Max texts 
    /// if <c><paramref name="randomize"/></c> is true.
    /// </summary>
    /// <param name="category">"Duration" or "Delay"</param>
    /// <param name="randomize"> True if the 'randomize' part must be minimized, else false.</param>
    /// <remarks>
    /// If category is "Rounds" then <c><paramref name="randomize"/></c> is to be considered rather as "unlimited". 
    /// (It is easier to use the logic here for it instead of creating a new function, despite an inaccurate parameter name.)
    /// </remarks>
    public void MinimizeObjectAppearance(string category, bool randomize)
    {
        if (category == "Duration")
        {
            if (randomize) // randomize part of Duration to be minimized
            {
                MinDurationText.color = Color.gray;
                MaxDurationText.color = Color.gray;
            }
            else // specific value part (text and slider) to be minimized
            {
                // Apply gray filter to the slider
                SetSliderHandleColor(DurationSlider, Color.gray);
                SetSliderFillColor(DurationSlider, Color.gray);

                /*             // Apply gray filter to the button
                            var buttonColors = button.colors;
                            originalButtonColor = buttonColors.disabledColor; // Save original
                            buttonColors.normalColor = Color.gray;
                            // buttonColors.disabledColor = Color.gray;
                            button.colors = buttonColors; */

                // Apply gray filter to the text
                DurationText.color = Color.gray;
            }
        }
        else if (category == "Delay")
        {
            if (randomize) // randomize part of Delay to be minimized
            {
                MinDelayText.color = Color.gray;
                MaxDelayText.color = Color.gray;
            }
            else // specific value part (text and slider) to be minimized
            {
                // Apply gray filter to the slider
                SetSliderHandleColor(DelaySlider, Color.gray);
                SetSliderFillColor(DelaySlider, Color.gray);

                /*             // Apply gray filter to the button
                            var buttonColors = button.colors;
                            originalButtonColor = buttonColors.disabledColor; // Save original
                            buttonColors.normalColor = Color.gray;
                            // buttonColors.disabledColor = Color.gray;
                            button.colors = buttonColors; */

                // Apply gray filter to the text
                DelayText.color = Color.gray;
            }
        }
        else if (category == "Rounds")
        {
            /*             // Apply gray filter to the button
                        var buttonColors = button.colors;
                        originalButtonColor = buttonColors.disabledColor; // Save original
                        buttonColors.normalColor = Color.gray;
                        // buttonColors.disabledColor = Color.gray;
                        button.colors = buttonColors; */

            // Apply gray filter to the text
            RoundsText.color = Color.gray;
        }
        else
        {
            Debug.LogWarning("Wrong parameter passed");
        }
    }

    /// <summary>
    /// "Maximizes" the slider and text appearance by reverting their colors back to their original ones, OR the Min and Max texts 
    /// if <c><paramref name="randomize"/></c> is true.
    /// </summary>
    /// <param name="category">"Duration" or "Delay"</param>
    /// <param name="randomize"> True if the 'randomize' part must be maximized, else false.</param>
    /// <remarks>
    /// If category is "Rounds" then <c><paramref name="randomize"/></c> is to be considered rather as "unlimited". 
    /// (It is easier to use the logic here for it instead of creating a new function, despite an inaccurate parameter name.)
    /// </remarks>
    public void MaximizeObjectAppearance(string category, bool randomize)
    {
        if (category == "Duration")
        {
            if (randomize) // randomize part of Duration to be maximized
            {
                MinDurationText.color = Color.white;
                MaxDurationText.color = Color.white;
            }
            else // specific value part (text and slider) to be maximized
            {
                // Restore original colors
                SetSliderHandleColor(DurationSlider, originalDurationSliderHandleColor);
                SetSliderFillColor(DurationSlider, originalDurationSliderFillColor);

                /*             var buttonColors = button.colors;
                            buttonColors.normalColor = originalButtonColor;
                            buttonColors.disabledColor = originalButtonColor;
                            button.colors = buttonColors; */

                DurationText.color = Color.white;
            }
        }
        else if (category == "Delay")
        {

            if (randomize) // randomize part of Delay to be maximized
            {
                MinDelayText.color = Color.white;
                MaxDelayText.color = Color.white;
            }
            else // specific value part (text and slider) to be maximized
            {
                // Restore original colors
                SetSliderHandleColor(DelaySlider, originalDelaySliderHandleColor);
                SetSliderFillColor(DelaySlider, originalDelaySliderFillColor);

                /*             var buttonColors = button.colors;
                            buttonColors.normalColor = originalButtonColor;
                            buttonColors.disabledColor = originalButtonColor;
                            button.colors = buttonColors; */

                DelayText.color = Color.white;
            }
        }
        else if (category == "Rounds")
        {
            /*             var buttonColors = button.colors;
                        buttonColors.normalColor = originalButtonColor;
                        buttonColors.disabledColor = originalButtonColor;
                        button.colors = buttonColors; */

            RoundsText.color = Color.white;
        }
        else
        {
            Debug.LogWarning("Wrong parameter passed");
        }
    }

    /// <summary>
    /// Called when the 'Randomize duration' checkbox is toggled: minimizes / maximizes the appropriate UI elements.
    /// </summary>
    /// <param name="randomize">The checkbox state after toggling.</param>
    public void OnRandomizeDurationToggle(bool randomize)
    {
        if (randomize == true) // minimize the specific value UI elements, maximize the random value UI elements.
        {
            MinimizeObjectAppearance("Duration", false);
            MaximizeObjectAppearance("Duration", true);
        }
        else // maximize the specific value UI elements, minimize the random value UI elements.
        {
            MaximizeObjectAppearance("Duration", false);
            MinimizeObjectAppearance("Duration", true);
        }
    }

    /// <summary>
    /// Called when the 'Randomize delay' checkbox is toggled: minimizes / maximizes the appropriate UI elements.
    /// </summary>
    /// <param name="randomize">The checkbox state after toggling.</param>
    public void OnRandomizeDelayToggle(bool randomize)
    {
        if (randomize == true) // minimize the specific value UI elements, maximize the random value UI elements.
        {
            MinimizeObjectAppearance("Delay", false);
            MaximizeObjectAppearance("Delay", true);
        }
        else // maximize the specific value UI elements, minimize the random value UI elements.
        {
            MaximizeObjectAppearance("Delay", false);
            MinimizeObjectAppearance("Delay", true);
        }
    }

    /// <summary>
    /// Called when the 'Unlimited rounds' checkbox is toggled: minimizes / maximizes the number of rounds UI text element.
    /// </summary>
    /// <param name="unlimited">The checkbox state after toggling.</param>
    public void OnUnlimitedRoundsToggle(bool unlimited)
    {
        if (unlimited == true) // minimize the number of rounds text
        {
            MinimizeObjectAppearance("Rounds", unlimited);
        }
        else // maximize the number of rounds text
        {
            MaximizeObjectAppearance("Rounds", unlimited);
        }
    }

    // changing min and max makes random delay checked, minimizes toggle and text, and maximizes itself ; and vice versa

    public float GetDuration()
    {
        return DurationSlider.value / 10f;
    }

    public float GetDelay()
    {
        return DelaySlider.value / 10f;
    }

    /// <summary>
    /// Gets the float seconds from the TMP_Text.text string with the format "X.Ys".
    /// </summary>
    /// <param name="text">TMP_Text.text string with the format "X.Ys".</param>
    /// <returns>The amount of seconds.</returns>
    public float GetSecondsFromText(string text, float default_value)
    {
        text = text.Replace("s", "").Trim(); // remove the 's' at the end to get a float
        if (float.TryParse(text, out float seconds))
        {
            return seconds;
        }
        else
        {
            Debug.LogError("Error: the text object did not contain a float.");
            return default_value; // return default value
        }
    }

    public float GetMinDuration()
    {
        return GetSecondsFromText(MinDurationText.text, 3f);
    }

    public float GetMaxDuration()
    {
        return GetSecondsFromText(MaxDurationText.text, 7f);
    }

    public float GetMinDelay()
    {
        return GetSecondsFromText(MinDelayText.text, 3f);
    }

    public float GetMaxDelay()
    {
        return GetSecondsFromText(MaxDelayText.text, 7f);
    }

    public bool GetRandomizeDuration()
    {
        return RandomizeDurationToggle.isOn;
    }

    public bool GetRandomizeDelay()
    {
        return RandomizeDelayToggle.isOn;
    }

    public bool GetUnlimitedRounds()
    {
        return UnlimitedRoundsToggle.isOn;
    }

    public bool GetShowDelayCountdown()
    {
        return ShowDelayCountdownToggle.isOn;
    }

    public bool GetShowDurationCountdown()
    {
        return ShowDurationCountdownToggle.isOn;
    }

    public bool GetShowNumberOfRoundsLeftCountdown()
    {
        return ShowNumberOfRoundsLeftToggle.isOn;
    }

    public bool GetSimultaneousVisualAudioStimuli()
    {
        return SimultaneousVisualAudioStimuliToggle.isOn;
    }

    public int GetNumberOfRounds()
    {
        string text = RoundsText.text.Trim();
        if (int.TryParse(text, out int rounds))
        {
            return rounds;
        }
        else
        {
            Debug.LogError("Error: the rounds text object did not contain an integer.");
            return 12; // return default value
        }
    }

    /// <summary>
    /// Updates the time controls data members based on the value of each corresponding UI element.
    /// </summary>
    public void UpdateTimeControls()
    {
        timeControls.Duration = GetDuration();
        timeControls.MinDuration = GetMinDuration();
        timeControls.MaxDuration = GetMaxDuration();
        timeControls.Delay = GetDelay();
        timeControls.MinDelay = GetMinDelay();
        timeControls.MaxDelay = GetMaxDelay();
        timeControls.RandomizeDuration = GetRandomizeDuration();
        timeControls.RandomizeDelay = GetRandomizeDelay();
        timeControls.UnlimitedRounds = GetUnlimitedRounds();
        timeControls.ShowDelayCountdown = GetShowDelayCountdown();
        timeControls.ShowDurationCountdown = GetShowDurationCountdown();
        timeControls.ShowNumberOfRoundsLeft = GetShowNumberOfRoundsLeftCountdown();
        timeControls.SimultaneousVisualAudioStimuli = GetSimultaneousVisualAudioStimuli();
        timeControls.NumberOfRounds = GetNumberOfRounds();
    }

    /// <summary>
    /// Saves the time controls to the JSON file.
    /// </summary>
    public void SaveTimeControls()
    {
        FileHandler.SaveToJSON<TimeControls>(timeControls, TIME_CONTROLS_FILENAME);
        Debug.Log("Saving time controls: ");
        timeControls.Log();
    }

    /// <summary>
    /// Loads the saved time controls from the JSON file into the <c>timeControls</c> field.
    /// </summary>
    public void LoadSavedTimeControls()
    {
        timeControls = FileHandler.ReadFromJSON<TimeControls>(TIME_CONTROLS_FILENAME);
        if (timeControls != null)
        {
            Debug.Log("Saved time controls loaded: ");
            timeControls.Log();
        }
        else
        {
            timeControls = new();
        }
    }

    /// <summary>
    /// Returns the saved time controls from the JSON file.
    /// </summary>
    public static TimeControls GetSavedTimeControls() {
        return FileHandler.ReadFromJSON<TimeControls>(TIME_CONTROLS_FILENAME);
    }

    public void SetDurationUI() {
        DurationSlider.value = timeControls.Duration * 10;
        DurationText.text = timeControls.Duration.ToString("F1") + "s";
    }

    public void SetDelayUI() {
        DelaySlider.value = timeControls.Delay * 10;
        DelayText.text = timeControls.Delay.ToString("F1") + "s";
    }

    public void SetTextSeconds(TMP_Text tmp_text, float seconds){
        tmp_text.text = seconds.ToString("F1") + "s";
    }

    /// <summary>
    /// Updates the UI elements of the TimeControls scene with the loaded time controls. 
    /// </summary>
    public void UpdateTimeControlsUI()
    {
        
        if (timeControls != null)
        {    
            timeControls.Log();
            SetDurationUI();
            SetTextSeconds(MinDurationText, timeControls.MinDuration);
            SetTextSeconds(MaxDurationText, timeControls.MaxDuration);
            SetDelayUI();
            SetTextSeconds(MinDelayText, timeControls.MinDelay);
            SetTextSeconds(MaxDelayText, timeControls.MaxDelay);
            RandomizeDurationToggle.isOn = timeControls.RandomizeDuration;
            RandomizeDelayToggle.isOn = timeControls.RandomizeDelay;
            UnlimitedRoundsToggle.isOn = timeControls.UnlimitedRounds;
            ShowDelayCountdownToggle.isOn = timeControls.ShowDelayCountdown;
            ShowDurationCountdownToggle.isOn = timeControls.ShowDurationCountdown;
            ShowNumberOfRoundsLeftToggle.isOn = timeControls.ShowNumberOfRoundsLeft;
            SimultaneousVisualAudioStimuliToggle.isOn = timeControls.SimultaneousVisualAudioStimuli;

            RoundsText.text = timeControls.NumberOfRounds.ToString();
        }
    }

}

public class TimeControls
{
    public float Duration, MinDuration, MaxDuration, Delay, MinDelay, MaxDelay;
    public bool RandomizeDuration, RandomizeDelay, UnlimitedRounds, ShowDelayCountdown, ShowDurationCountdown, ShowNumberOfRoundsLeft, SimultaneousVisualAudioStimuli;
    public int NumberOfRounds;

    /// <summary>
    /// Initialize all the data members.
    /// </summary>
    public TimeControls()
    {
        Duration = Delay = 5f;
        MinDuration = MinDelay = 3f;
        MaxDuration = MaxDelay = 7f;
        RandomizeDuration = RandomizeDelay = UnlimitedRounds = ShowDelayCountdown = ShowDurationCountdown = ShowNumberOfRoundsLeft = SimultaneousVisualAudioStimuli = false;
        NumberOfRounds = 12;
    }

    /// <summary>
    /// Log all the fields to the console.
    /// </summary>
    public void Log()
    {
        Debug.Log($"on screen duration: {Duration} | min duration: {MinDuration} | max duration: {MaxDuration} | delay: {Delay} | min delay: {MinDelay} | max delay: {MaxDelay} | randomize duration: {RandomizeDuration} | randomize delay: {RandomizeDelay} | unlimited rounds: {UnlimitedRounds} | show delay countdown: {ShowDelayCountdown} | show duration countdown: {ShowDurationCountdown} | show # of rounds left: {ShowNumberOfRoundsLeft} | rounds: {NumberOfRounds} | Audio and Visal stimuli simultaneously: {SimultaneousVisualAudioStimuli}");
    }
}
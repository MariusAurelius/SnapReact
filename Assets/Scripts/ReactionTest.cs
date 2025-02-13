using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using Unity.Properties;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// Class handling the Reaction Test scene.
/// </summary>
public class ReactionTest : MonoBehaviour
{
    /// <summary>
    /// The scene's camera to change the color of. 
    /// </summary>
    [SerializeField] private Camera MainCamera;

    /// <summary>
    /// Will contain the number / letter / arrow symbol. If it is arrow, then its sprite will change; 
    /// if it's a number or letter, its text value will change.
    /// </summary>
    [SerializeField] private Button SymbolButton;
    private TMP_Text SymbolText;
    [SerializeField] private Sprite ArrowSprite;

    /// <summary>
    /// Text counting down the delay until the next stimulus.
    /// </summary>
    [SerializeField] private TMP_Text DelayCountdownText;

    /// <summary>
    /// Text counting down the duration left of the current stimulus.
    /// </summary>
    [SerializeField] private TMP_Text DurationCountdownText;

    /// <summary>
    /// Text showing the number of rounds left.
    /// </summary>
    [SerializeField] private TMP_Text RoundsLeftText;


    // for pausing
    [SerializeField] private Button PauseButton;
    [SerializeField] private Sprite PauseSprite;
    [SerializeField] private Sprite ResumeSprite;
    [SerializeField] private Button ToggleDelayCountdownButton;
    [SerializeField] private Button ToggleDurationCountdownButton;
    [SerializeField] private Button ToggleRoundsLeftButton;
    [SerializeField] private Button RestartButton;
    [SerializeField] private Button QuitButton;

    private bool isPaused = false;
    private bool isCountingDown = true;
    private const float BEGIN_COUNTDOWN = 3.0f;

    /// <summary>
    /// A countdown timer at the beginning of the scene and for whenever the scene is un-paused. 
    /// </summary>
    private float beginTimer = BEGIN_COUNTDOWN;

    /// <summary>
    /// The Text component showing the beginning / un-pausing timer.
    /// </summary>
    [SerializeField] private TMP_Text beginTimerText;

    /// <summary>
    /// Text displaying general messages like when the test is over, or potentially errors.
    /// </summary>
    [SerializeField] private TMP_Text generalMessageText;

    /// <summary>
    /// The current round, incremented after every stimulus stops being displayed.
    /// </summary>
    private int currentRound = 0;


    /// <summary>
    /// Is there a visual stimulus currently being shown on screen?
    /// </summary>
    private bool isShowingVisualStimulus = false;

    /// <summary>
    /// Is there an audio stimulus currently being played?
    /// </summary>
    private bool isPlayingAudioStimulus = false;

    /// <summary>
    /// The remaining amount of time to show / play the stimulus.
    /// </summary>
    private float stimulusDurationRemaining = 5.0f;

    /// <summary>
    /// The remaining amount of time before showing / playing the next stimulus.
    /// </summary>
    private float stimuliDelayRemaining = 5.0f;


    /// <summary>
    /// The next visual stimulus to show.
    /// </summary>
    private string visualStimulusToShow = null;

    /// <summary>
    /// The next audio stimulus to play.
    /// </summary>
    private string audioStimulusToPlay = null;

    public static List<string> VisualStimuli;
    public static List<string> AudioStimuli;
    private static TimeControls timeControls;

    public static Dictionary<string, Color> Colors = new()
    {
        {"", new Color32(202, 178, 152, 255)},
        {"RedColorButton", Color.red},
        {"GreenColorButton", Color.green},
        {"OrangeColorButton", new Color32(255, 165, 0, 255)},
        {"BlueColorButton", Color.blue},
        {"YellowColorButton", Color.yellow},
        {"PinkColorButton", new Color32(255, 134, 155, 255)}

    };
    public static Dictionary<string, float> ArrowDirectionZ = new()
    {
        {"LeftArrowButton", 180f},
        {"RightArrowButton", 0f},
        {"UpArrowButton", 90f},
        {"DownArrowButton", -90f},
        {"UpLeftArrowButton", 135f},
        {"UpRightArrowButton", 45f},
        {"DownLeftArrowButton", -135f},
        {"DownRightArrowButton", -45f}
    };


    void Awake()
    {
        SymbolText = SymbolButton.GetComponentInChildren<TMP_Text>();
        SymbolButton.image.sprite = ArrowSprite;

        timeControls = TimeControlsManager.GetSavedTimeControls();
        SetShowDelayCountdown(timeControls.ShowDelayCountdown);
        SetShowDurationCountdown(timeControls.ShowDurationCountdown);
        SetShowRoundsLeft(timeControls.ShowNumberOfRoundsLeft);
        timeControls.Log();
        ResetDurationRemaining();
        ResetDelayRemaining();
        VisualStimuli = StimuliMenuManager.GetSavedVisualStimuli();
        AudioStimuli = StimuliMenuManager.GetSavedAudioStimuli();
        GenerateNextStimuli();

        if (!timeControls.UnlimitedRounds) RoundsLeftText.text = timeControls.NumberOfRounds.ToString() + 
                                                        ((timeControls.NumberOfRounds == 1) ? " round left" : " rounds left");
    }

    /// <summary>
    /// Changes the camera's bg color to the color of the button <c><paramref name="color_button_name"/></c>
    /// </summary>
    public void ChangeColorTo(string color_button_name)
    {
        MainCamera.backgroundColor = Colors[color_button_name];
    }

    /// <summary>
    /// Hides the image (arrow) and the text (letter, number).
    /// </summary>
    private void ClearVisualSymbol()
    {
        SymbolButton.image.enabled = false;
        SymbolButton.image.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0); // reset z rotation
        SymbolText.text = "";
    }

    /// <summary>
    /// Shows the visual symbol (arrow / letter / number) <c><paramref name="symbol_name"/></c>. 
    /// </summary>
    /// <param name="symbol_name">the name of the stimulus.</param>
    public void ChangeSymbolTo(string symbol_button_name)
    {
        if (symbol_button_name.Contains("Arrow"))
        { // set new z, show arrow
            float direction_z = ArrowDirectionZ[symbol_button_name];
            SymbolButton.image.gameObject.transform.rotation = Quaternion.Euler(0, 0, direction_z);
            SymbolButton.image.enabled = true;
        }
        else if (symbol_button_name.Contains("Letter") || symbol_button_name.Contains("Number"))
        {
            SymbolText.text = symbol_button_name[^1].ToString(); // gets the last character which is the letter or number
        }
        else
        {
            Debug.LogWarning("Invalid symbol name (string) passed.");
        }
    }

    /// <summary>
    /// Shows the stimulus <c><paramref name="stimulus_name"/></c>. 
    /// </summary>
    /// <param name="stimulus_name">The next stimulus to show.</param>
    public void ShowVisualStimulus(string stimulus_name)
    {
        if (stimulus_name.Contains("Color"))
        {
            ChangeColorTo(stimulus_name);
        }
        else
        {
            ChangeSymbolTo(stimulus_name);
        }

    }

    /// <summary>
    /// Clears the visual stimulus by resetting the background color and symbol button.
    /// </summary>
    public void HideVisualStimuli()
    {
        ChangeColorTo("");
        ClearVisualSymbol();
    }

    /// <summary>
    /// Plays the stimulus <c><paramref name="stimulus_name"/></c>. 
    /// </summary>
    /// <param name="stimulus_name">The next stimulus to play.</param>
    public void PlayAudioStimulus(string stimulus_name)
    {
        AudioManager.Instance.PlaySFXFromString(stimulus_name);
    }


    /// <summary>
    /// Returns the name of a random visual stimulus.
    /// </summary>
    public string RandomVisualStimulus()
    {
        if (VisualStimuli.Count == 0)
        {
            return null;
        }
        else if (VisualStimuli.Count == 1)
        {
            return VisualStimuli[0];
        }
        else
        {
            int index = Random.Range(0, VisualStimuli.Count);
            return VisualStimuli[index];
        }
    }

    /// <summary>
    /// Returns the name of a random audio stimulus.
    /// </summary>
    public string RandomAudioStimulus()
    {
        string stimulus_name;
        if (AudioStimuli.Count == 0)
        {
            return null;
        }
        else if (AudioStimuli.Count == 1)
        {
            stimulus_name = AudioStimuli[0];
        }
        else
        {
            int index = Random.Range(0, AudioStimuli.Count);
            stimulus_name = AudioStimuli[index];
        }
        
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
        return stimulus_name;
    }


    /// <summary>
    /// Generates the next stimulus or stimuli to show / play by setting <c>visualStimulusToShow</c> and <c>audioStimulusToPlay</c>.
    /// </summary>
    public void GenerateNextStimuli()
    {
        visualStimulusToShow = audioStimulusToPlay = null; // reset stimulus to none

        if (VisualStimuli.Count == 0 && AudioStimuli.Count == 0)
        {
            Debug.LogWarning("need at least 1 visual or audio stimulus");
            return;
        }

        if (timeControls.SimultaneousVisualAudioStimuli) // at same time
        {
            visualStimulusToShow = RandomVisualStimulus(); // null or name of stimulus
            audioStimulusToPlay = RandomAudioStimulus(); // null or name of stimulus
        }
        else // one or other
        {
            int number_of_stimuli = VisualStimuli.Count + AudioStimuli.Count;
            int stimulus_index = Random.Range(0, number_of_stimuli); // for 0 or 1 - range 0, 2
            bool is_visual = stimulus_index < VisualStimuli.Count;

            if (is_visual) // visual only
            {
                Debug.Log("choosing visual stim");
                visualStimulusToShow = RandomVisualStimulus();
            }
            else // audio only
            {
                Debug.Log("choosing audio stim");
                audioStimulusToPlay = RandomAudioStimulus();
            }
        }
    }


    // pause menu functions

    /// <summary>
    /// Toggles showing / hiding the countdown on the screen to the next stimulus.
    /// Updates the pause menu button text.
    /// </summary>
    public void ToggleDelayCountdown()
    {
        DelayCountdownText.transform.parent.gameObject.SetActive(!DelayCountdownText.transform.parent.gameObject.activeSelf);
        ToggleDelayCountdownButton.GetComponentInChildren<TMP_Text>().text = (DelayCountdownText.transform.parent.gameObject.activeSelf) ? "Hide countdown" : "Show countdown";
    }

    /// <summary>
    /// Shows the delay countdown timer if <c><paramref name="show_countdown"/></c> is true, else hides it. 
    /// Updates the pause menu button text.
    /// </summary>
    public void SetShowDelayCountdown(bool show_countdown)
    {
        DelayCountdownText.transform.parent.gameObject.SetActive(show_countdown);
        ToggleDelayCountdownButton.GetComponentInChildren<TMP_Text>().text = (show_countdown) ? "Hide countdown" : "Show countdown";
    }


    /// <summary>
    /// Toggles showing / hiding the countdown on the screen to the stimulus exiting the screen.
    /// Updates the pause menu button text.
    /// </summary>
    public void ToggleDurationCountdown()
    {
        DurationCountdownText.transform.parent.gameObject.SetActive(!DurationCountdownText.transform.parent.gameObject.activeSelf);
        ToggleDurationCountdownButton.GetComponentInChildren<TMP_Text>().text = (DurationCountdownText.transform.parent.gameObject.activeSelf) ? "Hide countdown" : "Show countdown";
    }


    /// <summary>
    /// Shows the duration countdown timer if <c><paramref name="show_countdown"/></c> is true, else hides it. 
    /// Updates the pause menu button text.
    /// </summary>
    public void SetShowDurationCountdown(bool show_countdown)
    {
        DurationCountdownText.transform.parent.gameObject.SetActive(show_countdown);
        ToggleDurationCountdownButton.GetComponentInChildren<TMP_Text>().text = (show_countdown) ? "Hide countdown" : "Show countdown";
    }


    /// <summary>
    /// Toggles showing / hiding the number of rounds left text.
    /// Updates the pause menu button text.
    /// </summary>
    public void ToggleRoundsLeft()
    {
        RoundsLeftText.transform.parent.gameObject.SetActive(!RoundsLeftText.transform.parent.gameObject.activeSelf);
        ToggleRoundsLeftButton.GetComponentInChildren<TMP_Text>().text = (RoundsLeftText.transform.parent.gameObject.activeSelf) ? "Hide rounds" : "Show rounds";
    }

    /// <summary>
    /// Shows the number of rounds left if <c><paramref name="show_rounds"/></c> is true, else hides it. 
    /// Updates the pause menu button text.
    /// </summary>
    public void SetShowRoundsLeft(bool show_rounds)
    {
        RoundsLeftText.transform.parent.gameObject.SetActive(show_rounds);
        ToggleRoundsLeftButton.GetComponentInChildren<TMP_Text>().text = (show_rounds) ? "Hide rounds" : "Show rounds";
    }


    /// <summary>
    /// Called on the pause/resume button press. Pauses/resumes the reaction test, opens pause menu (UI), and changes the pause button sprite.
    /// </summary>
    public void OnPause()
    {
        if (!enabled) // end of scene: pause disabled
        {
            return;

        }
        if (isPaused) // if game is currently paused > un-pausing
        {
            RestartButton.gameObject.SetActive(false);
            QuitButton.gameObject.SetActive(false);
            ToggleDelayCountdownButton.gameObject.SetActive(false);
            ToggleDurationCountdownButton.gameObject.SetActive(false);
            ToggleRoundsLeftButton.gameObject.SetActive(false);
            PauseButton.image.sprite = PauseSprite;

            // start a 3 sec countdown to prepare for un-pausing
            StartBeginTimer();
        }
        else // if game is currently un-paused > pausing
        {
            RestartButton.gameObject.SetActive(true);
            QuitButton.gameObject.SetActive(true);
            ToggleDelayCountdownButton.gameObject.SetActive(true);
            ToggleDurationCountdownButton.gameObject.SetActive(true);
            ToggleRoundsLeftButton.gameObject.SetActive(true);
            PauseButton.image.sprite = ResumeSprite;
        }
        isPaused = !isPaused;
    }

    /// <summary>
    /// Starts the begin timer and updates the UI.
    /// </summary>
    private void StartBeginTimer()
    {
        beginTimer = BEGIN_COUNTDOWN;
        isCountingDown = true;
        beginTimerText.transform.parent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Stops the begin timer and updates the UI.
    /// </summary>
    private void StopBeginTimer()
    {
        isCountingDown = false;
        beginTimerText.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Resets the stimulus duration to <c>timeControls.Duration</c>, or a random duration between <c>timeControls.MinDuration</c> 
    /// and <c>timeControls.MaxDuration</c>. If there is an audio stimulus ready to play, and its length is superior to the duration, then its length becomes the duration.
    /// </summary>
    private void ResetDurationRemaining()
    {
        float audio_duration = 0f;
        if (audioStimulusToPlay != null)
        {
            Sound sound = AudioManager.Instance.GetSoundFromString(audioStimulusToPlay); 
            if (sound != null)
            {
                audio_duration = sound.Clip.length;
            }
        }

        if (timeControls.RandomizeDuration)
        {
            stimulusDurationRemaining = Math.Max(audio_duration, Random.Range(timeControls.MinDuration, timeControls.MaxDuration));
        }
        else
        {
            stimulusDurationRemaining = Math.Max(audio_duration, timeControls.Duration);
        }
    }

    /// <summary>
    /// Resets the stimulus delay to <c>timeControls.Delay</c>, or a random duration between <c>timeControls.MinDelay</c> and <c>timeControls.MaxDelay</c>.
    /// </summary>
    private void ResetDelayRemaining()
    {
        if (timeControls.RandomizeDelay)
        {
            stimuliDelayRemaining = Random.Range(timeControls.MinDelay, timeControls.MaxDelay);
        }
        else
        {
            stimuliDelayRemaining = timeControls.Delay;
        }
    }

    /// <summary>
    /// Called when Restart button is pressed at the end of the test or in the pause menu. 
    /// </summary>
    public void OnRestart()
    {
        ScenesManager.Instance.LoadReactionTest();
    }

    /// <summary>
    /// Called when Quit button is pressed at the end of the test or in the pause menu. 
    /// </summary>
    public void OnQuit()
    {
        ScenesManager.Instance.LoadMainMenu();
    }

    /// <summary>
    /// Increments the current round and updates the 'rounds left' text.
    /// </summary>
    private void IncrementCurrentRound() {
        currentRound++;
        if (currentRound <= timeControls.NumberOfRounds) // update text if not last round (otherwise would write -1 rounds left)
        {
            int rounds_left = timeControls.NumberOfRounds - currentRound;
            RoundsLeftText.text = rounds_left.ToString() + ((rounds_left == 1) ? " round left" : " rounds left");
        }
    }

    void Update()
    {
        if (!timeControls.UnlimitedRounds && currentRound >= timeControls.NumberOfRounds)
        {
            generalMessageText.text = $"Test over: {timeControls.NumberOfRounds} rounds completed.";
            generalMessageText.gameObject.SetActive(true);
            RestartButton.gameObject.SetActive(true);
            QuitButton.gameObject.SetActive(true);
            enabled = false;
        }

        else if (!isPaused && !isCountingDown) // only update if game isnt paused and isnt counting down
        {
            if (!timeControls.SimultaneousVisualAudioStimuli)// one or the other at a time
            {
                if (isShowingVisualStimulus) // stimulus currently being shown
                {
                    stimulusDurationRemaining -= Time.deltaTime;
                    if (stimulusDurationRemaining <= 0.0f) // stop showing stimulus, set next duration, and set next stimulus
                    {
                        if (!timeControls.UnlimitedRounds)
                        {
                            IncrementCurrentRound();
                        }

                        Debug.Log("stop showing visual stimulus");
                        isShowingVisualStimulus = false;
                        HideVisualStimuli();

                        DurationCountdownText.text = "";

                        ResetDurationRemaining(); // only for visual
                        GenerateNextStimuli();
                    }
                    else
                    {
                        DurationCountdownText.text = "Stimulus ends in: " + stimulusDurationRemaining.ToString("F1") + "s";
                    }
                }
                else if (isPlayingAudioStimulus)
                {
                    // if mixer done playing audio: 
                    if (!AudioManager.Instance.IsPlaying())
                    {
                        if (!timeControls.UnlimitedRounds)
                        {
                            IncrementCurrentRound();
                        }
                       
                        Debug.Log("stop playing audio stimulus");
                        isPlayingAudioStimulus = false;
                        
                        GenerateNextStimuli();
                    }
                    // no show duration bc no need: when audio ends (audio = short)
                }
                else // no stimulus is playing / being shown
                {
                    stimuliDelayRemaining -= Time.deltaTime;
                    if (stimuliDelayRemaining <= 0.0f) // delay over: show / play stimulus, set next delay
                    {
                        Debug.Log("delay over");
                        DelayCountdownText.text = "";

                        if (visualStimulusToShow != null)
                        {
                            Debug.Log("going to show visual stim: " + visualStimulusToShow);
                            isShowingVisualStimulus = true;
                            ShowVisualStimulus(visualStimulusToShow);
                        }
                        if (audioStimulusToPlay != null)
                        {
                            Debug.Log("going to play audio stim: " + audioStimulusToPlay);
                            isPlayingAudioStimulus = true;
                            PlayAudioStimulus(audioStimulusToPlay);
                        }
                        ResetDelayRemaining();
                    }
                    else
                    {
                        DelayCountdownText.text = "Next stimulus in: " + stimuliDelayRemaining.ToString("F1") + "s";
                    }
                }
            }

            else // both visual and audio stimuli at same time
            {
                if (!isShowingVisualStimulus && !isPlayingAudioStimulus) // if no stimulus is playing / being shown
                {
                    stimuliDelayRemaining -= Time.deltaTime;
                    if (stimuliDelayRemaining <= 0.0f) // delay over: show / play stimuli, set next delay
                    {
                        Debug.Log("delay over");
                        DelayCountdownText.text = "";

                        if (visualStimulusToShow != null)
                        {
                            Debug.Log("going to show visual stim: " + visualStimulusToShow);
                            isShowingVisualStimulus = true;
                            ShowVisualStimulus(visualStimulusToShow);
                        }
                        if (audioStimulusToPlay != null)
                        {
                            Debug.Log("going to play audio stim: " + audioStimulusToPlay);
                            isPlayingAudioStimulus = true;
                            PlayAudioStimulus(audioStimulusToPlay);
                        }
                        ResetDelayRemaining();
                    }
                    else // update delay text
                    {
                        DelayCountdownText.text = "Next stimuli in: " + stimuliDelayRemaining.ToString("F1") + "s";
                    }
                }
                else // something is playing
                {
                    if (isShowingVisualStimulus) // stimulus currently being shown
                    {
                        stimulusDurationRemaining -= Time.deltaTime;
                        if (stimulusDurationRemaining <= 0.0f) // stop showing stimulus, set next duration
                        {
                            if (!isPlayingAudioStimulus) // if audio also done playing: set next stimuli
                            {
                                if (!timeControls.UnlimitedRounds)
                                {
                                    IncrementCurrentRound();
                                }

                                HideVisualStimuli();
                                GenerateNextStimuli();
                            }
                            DurationCountdownText.text = "";
                            ResetDurationRemaining(); // only for visual
                            Debug.Log("stop showing visual stimulus");
                            isShowingVisualStimulus = false;
                        }
                        else
                        {
                            DurationCountdownText.text = "Stimulus ends in: " + stimulusDurationRemaining.ToString("F1") + "s";
                        }
                    }
                    else if (isPlayingAudioStimulus) // audio stimulus currently being played
                    {
                        // if mixer done playing audio: 
                        if (!AudioManager.Instance.IsPlaying())
                        {
                            if (!isShowingVisualStimulus)
                            {
                                if (!timeControls.UnlimitedRounds)
                                {
                                    IncrementCurrentRound();
                                }

                                HideVisualStimuli();
                                GenerateNextStimuli();
                            }
                            else
                            {
                                DurationCountdownText.text = "Stimulus ends in: " + stimulusDurationRemaining.ToString("F1") + "s";
                            }
                            Debug.Log("stop playing audio stimulus");
                            isPlayingAudioStimulus = false;        
                        }
                    }
                }
            }
        }

        else if (isCountingDown) // if counting down (scene start or un-pausing)
        {
            if (!isPaused) // counting down while game isn't paused
            {
                beginTimer -= Time.deltaTime;
                if (beginTimer <= 0.0f) // stop counting down
                {
                    StopBeginTimer();
                }
                beginTimerText.text = "Starting in: " + beginTimer.ToString("F1") + "s";
            }
            else // game is paused : stop counting down
            {
                StopBeginTimer();
            }
        }
    }
}
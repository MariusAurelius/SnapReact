using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager Instance;

    private void Awake() {
        Instance = this;
    }

    public enum Scenes { // have to be in same order as build
        MainMenu,
        SettingsMenu,
        VisualStimuliMenu,
        AudioStimuliMenu,
        TimeControlsMenu,
        ReactionTest
    }

    public void LoadScene(Scenes scene) {
        SceneManager.LoadScene(scene.ToString());
    }

    public void LoadMainMenu() {
        SceneManager.LoadScene(Scenes.MainMenu.ToString());
    }

    public void LoadSettingsMenu() {
        SceneManager.LoadScene(Scenes.SettingsMenu.ToString());
    }

    public void LoadVisualStimuliMenu() {
        SceneManager.LoadScene(Scenes.VisualStimuliMenu.ToString());
    }

    public void LoadAudioStimuliMenu() {
        SceneManager.LoadScene(Scenes.AudioStimuliMenu.ToString());
    }

    public void LoadTimeControlsMenu() {
        SceneManager.LoadScene(Scenes.TimeControlsMenu.ToString());
    }
    
    public void LoadReactionTest() {
        SceneManager.LoadScene(Scenes.ReactionTest.ToString());
    }

    public bool IsSceneVisualStimuliMenu() {
        return SceneManager.GetActiveScene().name == Scenes.VisualStimuliMenu.ToString();
    }

    public bool IsSceneAudioStimuliMenu() {
        return SceneManager.GetActiveScene().name == Scenes.AudioStimuliMenu.ToString();
    }

    public bool IsSceneReactionTest() {
        return SceneManager.GetActiveScene().name == Scenes.ReactionTest.ToString();
    }

    /// <summary>
    /// Gets the current scene as a Scenes enum value.
    /// </summary>
    public Scenes? GetCurrentScene() {
        string sceneName = SceneManager.GetActiveScene().name;
        if (Enum.TryParse(sceneName, out Scenes sceneEnum))
        {
            return sceneEnum;
        }
        else
        {
            Debug.LogWarning($"current scene '{sceneName}' not in the Scenes enum.");
            return null;       
        }
    }

    public void QuitGame() {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
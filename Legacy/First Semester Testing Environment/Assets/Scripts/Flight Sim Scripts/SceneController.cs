using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    //This class controls a number of the UI elements, such as canvases and buttons. This is used throughout all scenes.

    // The UI canvases within the scene
    public GameObject UIPause; //the UI canvas which is active when the game is paused.
    public GameObject UIMain; //The main UI canvas which is active when the game isn't paused.
    public GameObject UIOptions; //The UI canvas which contains the scenes settings

    public static bool isCrashed;    //Has the plane crashed?

    private bool isPaused; //Has the game been paused?
    private bool isOptions; //Is the options menu active?
    private float originalTimeScale; //The timescale when the scene is loaded

    void Start ()
    {
        //Sets which UI canvases should be active when the scene loads.
        UIPause.SetActive(false);
        UIMain.SetActive(true);

        if (UIOptions != null) //As not all scenes have an options menu, this is used to check. If the scene contains an option menu, then it is deactivated when the scene loads.
        {
            UIOptions.SetActive(false); //Deactivates the options UI cawnvas when the scene is loaded
        }

        //Diactivates the crashed and paused UI canvases
        isCrashed = false;
        isPaused = false;

        originalTimeScale = Time.timeScale; //Sets the originalTimeScale to the time scale when the scene is loaded.
    }

    void Update ()
    {
        //Allows the user to pause the game by pressing the 'Cancel' button (default: escape)
        if (Input.GetButtonDown("Cancel") && !isPaused && !isCrashed) //Checks that the scene isn't already paused and the aircraft has not crashed. 
        {
            Time.timeScale = 0f; //Freezes the scene 
            UIMain.SetActive(false); //Disables the main UI canvas
            UIPause.SetActive(true); //Enables the pause UI canvas
            isPaused = true; //Reports that the scene is paused
        }
        else if (Input.GetButtonDown("Cancel") && isPaused && !isCrashed && !isOptions) //Checks that the scene is already paused and the aircraft has not crashed.
        {
            Time.timeScale = originalTimeScale; //Un-freezes the scene to the timescale that the scene started with
            UIMain.SetActive(true); //Re-activates the main UI canvas
            UIPause.SetActive(false); //Disables the pause UI canvas
            
            isPaused = false; //Reports the the scene is no longer paused
        }
    }

    //Functions which are used by buttons throughout the project

    public void Restart() //restarts the main flying arena scene
    {
        Time.timeScale = originalTimeScale; //sets the timescale back to the timescale that the scene started with
        SceneManager.LoadScene("Main"); //loads the main flying arena scene
    }

    public void RestartTest () //restarts the wind tunnel scene
    {
        Time.timeScale = originalTimeScale;  //sets the timescale back to the timescale that the scene started with
        SceneManager.LoadScene("Tunnel"); //loads the wind tunnel scene
    }

    public void QuitApplication () //Quits the application
    {
        Application.Quit();
    }

    public void QuitToSetup () //Returns to the aircraft setup scne
    {
        SceneManager.LoadScene("Menu");
    }

    public void Options () //Activates the options menu UI canvas
    {
        isOptions = true; //Reports that the options menu is activated
        UIMain.SetActive(false); //Deactivates the main UI
        UIPause.SetActive(false); //Deactivates the Pause UI
        UIOptions.SetActive(true); //Activates the options UI
    }
    public void ReturnToPause () //Exits the options menu to the pause menu
    {
        isOptions = false; //Reports that the options menu is no longer activated
        UIMain.SetActive(false); //Deactivates the main UI
        UIPause.SetActive(true); //Activates the Pause UI
        UIOptions.SetActive(false); //Deactivates the options UI
    }

}

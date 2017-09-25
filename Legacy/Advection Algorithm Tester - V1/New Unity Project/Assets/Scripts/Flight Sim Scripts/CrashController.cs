using UnityEngine;
using System.Collections;

public class CrashController : MonoBehaviour
{
    //This class controls what occurs when the aircraft has collided with the ground

    public GameObject UIMain;
    public GameObject UICrash;

    public GameObject explosion; //The explosion that will be instantiated upon the aircrafts collision with the ground
    public GameObject aircraft; //The aircraft

    //The aircraft's audio sources
    public AudioSource audioExplosion;
    public AudioSource audioJet;

    //The cameras in the scene
    public GameObject crashCam;
    public GameObject chaseCam;
    public GameObject lockedCam;

    float timer; //Timer which counts how long since the crash occured
    
    void Start ()
    {
        UICrash.SetActive(false);
        crashCam.SetActive(false);

        timer = 0; //Sets the timer to zero at the start of the scene
    }

    void Update ()
    {
        //Enables the crash UI menu 3 seconds after the aircraft has crashed
        if (SceneController.isCrashed && timer < 3)
        {
            timer += Time.deltaTime; //Increases the timer value 
            
        }
        else if (timer > 3 && Time.timeScale != 0)
        {
            UICrash.SetActive(true); //Activates the crash UI canvas
        }
        
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag("Ground") && !SceneController.isCrashed) //Checks whether the collider that the aircraft has made contact with has the correct tag
        {
            SceneController.isCrashed = true; //Sets the static variable, isCrashed, to true when the aircraft has collided with the ground. This restricts the user from pausing the game
            UIMain.SetActive(false); //Deactivates the main UI canvas

            //Activates the crash camera and deactivates all other cameras
            crashCam.SetActive(true); 
            chaseCam.SetActive(false);
            lockedCam.SetActive(false);

            crashCam.transform.position = transform.position + new Vector3(0, 20, -30); //Positions the crash camera 

            //Enables the explosion sound effect and disables the aircraft's audio
            audioExplosion.enabled = true;
            audioJet.enabled = false;

            Instantiate(explosion, transform.position, Quaternion.identity); //Produces the explosion prefab at the position of impact
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class PressureVisualisationController : MonoBehaviour
{
    //This class controls which lift visualisation option is enabled

    public Dropdown liftViewOptions; //The dropdown which contains all of the visualisation options

    // All of the gameobjects which display a lift, or pressure visualisation
    public GameObject rightWingArrows;
    public GameObject leftWingArrows;
    public GameObject tailArrows;
    public GameObject planeImage;
    public GameObject rightWingEmitter;
    public GameObject leftWingEmitter;
    public GameObject rightWingTip;
    public GameObject leftWingTip;
    public GameObject rightWingEmitterMesh;
    public GameObject leftWingEmitterMesh;

    void Start ()
    {
        LiftCheck(); //Checks the selected visualisation option when the scene is loaded. This is set to case 5 in the dropdown inspector menu
    }

    public void LiftCheck () //Checks the current value of the visualisation dropdown menu, and runs the appropriate function.
    {
        switch (liftViewOptions.value)
        {
            case 0:
                ComplexLift();
                break;
            case 1:
                SimpleLift();
                break;
            case 2:
                CondensationLift();
                break;
            case 3:
                CondensationLiftMesh();
                break;
            case 4:
                BothLift();
                break;
            case 5:
                NoneLift();
                break;
        }
    }

    void ComplexLift () //Activates the lift arrows on the wings, and deactivates all other visualisation methods
    {
        rightWingArrows.SetActive(true);
        leftWingArrows.SetActive(true);
        tailArrows.SetActive(true);
        planeImage.SetActive(false);
        rightWingEmitter.SetActive(false);
        leftWingEmitter.SetActive(false);
        rightWingTip.SetActive(false);
        leftWingTip.SetActive(false);
        rightWingEmitterMesh.SetActive(false);
        leftWingEmitterMesh.SetActive(false);
    }

    void SimpleLift () //Activates the lift visualisation image, and deactivates all other visualisation methods
    {
        rightWingArrows.SetActive(false);
        leftWingArrows.SetActive(false);
        tailArrows.SetActive(false);
        planeImage.SetActive(true);
        rightWingEmitter.SetActive(false);
        leftWingEmitter.SetActive(false);
        rightWingTip.SetActive(false);
        leftWingTip.SetActive(false);
        rightWingEmitterMesh.SetActive(false);
        leftWingEmitterMesh.SetActive(false);
        
    }

    void CondensationLift() //Activates the condensation lift visualisation, and deactivates all other visualisation methods
    {
        rightWingArrows.SetActive(false);
        leftWingArrows.SetActive(false);
        tailArrows.SetActive(false);
        planeImage.SetActive(false);
        rightWingEmitter.SetActive(true);
        leftWingEmitter.SetActive(true);
        rightWingTip.SetActive(true);
        leftWingTip.SetActive(true);
        rightWingEmitterMesh.SetActive(false);
        leftWingEmitterMesh.SetActive(false);
    }

    void CondensationLiftMesh()  //Activates the mesh condensation lift visualisation, and deactivates all other visualisation methods
    {
        rightWingArrows.SetActive(false);
        leftWingArrows.SetActive(false);
        tailArrows.SetActive(false);
        planeImage.SetActive(false);
        rightWingEmitter.SetActive(false);
        leftWingEmitter.SetActive(false);
        rightWingTip.SetActive(true);
        leftWingTip.SetActive(true);
        rightWingEmitterMesh.SetActive(true);
        leftWingEmitterMesh.SetActive(true);
    }

    void BothLift () //Activates all lift visualisation methods
    {
        rightWingArrows.SetActive(true);
        leftWingArrows.SetActive(true);
        tailArrows.SetActive(true);
        planeImage.SetActive(true);
        rightWingEmitter.SetActive(true);
        leftWingEmitter.SetActive(true);
        rightWingTip.SetActive(true);
        leftWingTip.SetActive(true);
        rightWingEmitterMesh.SetActive(true);
        leftWingEmitterMesh.SetActive(true);
    }

    void NoneLift () //Deactivates all lift visualisation methods
    {
        rightWingArrows.SetActive(false);
        leftWingArrows.SetActive(false);
        tailArrows.SetActive(false);
        planeImage.SetActive(false);
        rightWingEmitter.SetActive(false);
        leftWingEmitter.SetActive(false);
        rightWingTip.SetActive(false);
        leftWingTip.SetActive(false);
        rightWingEmitterMesh.SetActive(false);
        leftWingEmitterMesh.SetActive(false);
    }

}

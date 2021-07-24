using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using TMPro;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update

    private MLInput.Controller controller;
    public GameObject controllerInput;
    public GameObject HeadlockedCanvas;

    void Start()
    {
        controller = MLInput.GetController(MLInput.Hand.Right);
    }

    // Update is called once per frame
    void Update()
    {
        if(controller.TriggerValue > 0.5)
        {
            RaycastHit hit;
            if(Physics.Raycast(controllerInput.transform.position, controllerInput.transform.forward, out hit))
            {
                if(hit.transform.gameObject.name == "settings")
                {
                    goSettings();
                }

                
                if(hit.transform.gameObject.name == "quit")
                {
                    leaveGame();
                }

                if(hit.transform.gameObject.name == "playSingle") {
                    playSingle();
                }
            }
        }
    }

    public void goSettings() {
        SceneManager.LoadScene("Settings");
    }

    public void playSingle() {
        SceneManager.LoadScene("Single Player");
    }

    public void leaveGame() {
        Application.Quit();
    }

}

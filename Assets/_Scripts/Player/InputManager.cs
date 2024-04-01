using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        InputDownHorizon();
        InputDownVertical();
        InputUpVertical();
    }

    public int InputDownHorizon()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            return 1;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public int InputDownVertical()
    {
        if (Input.GetKeyDown("w") || Input.GetKey("w") || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetButtonDown("Jump") || Input.GetButton("Jump"))
        {
            return 1;
        }
        else if (Input.GetKeyDown("s") || Input.GetKey("s") || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKey(KeyCode.DownArrow))
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public int InputUpVertical()
    {
        if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.UpArrow))
        {
            return 1;
        }
        else if (Input.GetKeyUp("s") || Input.GetKeyUp(KeyCode.DownArrow))
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}

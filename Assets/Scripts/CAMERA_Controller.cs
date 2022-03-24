using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAMERA_Controller : MonoBehaviour
{
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject IngameMenu;
    [SerializeField] GameObject gameManager;

    [Header("Movement settings")]
    [SerializeField] int movementSpeed;
    [SerializeField] int boostSpeed;
    [SerializeField] int normalSpeed;

    [Header("Mouse settings")]
    [SerializeField] float mouseSensitivity;

    Vector2 mouseInput;
    bool canControlCamera;
    bool inMenu;

    private void Start()
    {
        inMenu = true;
        Cursor.lockState = CursorLockMode.None;
        canControlCamera = false;

        MainMenu.SetActive(false);
        IngameMenu.SetActive(false);
    }

    private void Update()
    {
        if (canControlCamera)
        {
            CameraControls();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inMenu = !inMenu;
        }

        if (inMenu){
          OpenMenu();
        }else{
          CloseMenu();  
        }
    }

    void OpenMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        canControlCamera = false;
        if (!MainMenu.activeSelf)
        {
            MainMenu.SetActive(true);

            if (IngameMenu.activeSelf)
            {
                IngameMenu.SetActive(false);
            }
        }
    }

    void CloseMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        canControlCamera = true;
        if (MainMenu.activeSelf)
        {
            MainMenu.SetActive(false);
            if (!IngameMenu.activeSelf)
            {
                IngameMenu.SetActive(true);
            }
        }
    }

    void CameraControls()
    {
        movementSpeed = BoostPressed() ? boostSpeed : normalSpeed;

        Vector3 moveDir = GetMovementDirection().normalized;
        Vector3 moveAmount = moveDir * movementSpeed * Time.deltaTime;


        transform.TransformDirection(moveAmount);
        transform.Translate(moveAmount);

        transform.Rotate(Vector3.up * GetMouseInput().x, Space.World);
        transform.localEulerAngles = new Vector3(GetMouseInput().y, GetMouseInput().x, 0);
    }

    Vector3 GetMovementDirection()
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        moveDirection.z = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.E))
        {
            moveDirection.y = 1;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            moveDirection.y = -1;
        }

        return moveDirection;
    }

    bool BoostPressed()
    {
        return Input.GetKey(KeyCode.LeftShift); 
    }

    Vector2 GetMouseInput()
    {
        mouseInput.x += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseInput.y += -Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseInput.y = Mathf.Clamp(mouseInput.y, -90f, 90f);
        return mouseInput;
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
}

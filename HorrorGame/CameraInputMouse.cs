using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInputMouse : MonoBehaviour
{
    InteractionManager IM;
    public Camera playerCam;

    public float speedH = 2.0f;
    public float speedV = 2.0f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    public float M1MinInput = 1, M2MinInput = 1, InteractionReachDistance = 10;
    float M1currentInput, M1prevInput, M2currentInput, M2prevInput;
    Vector3 cachedCursorPos;
    Ray laserPointer;
    RaycastHit rh;
    public Vector3 improveAim = new Vector3(0, 0.25f, 0);

    float scrollAmt, lastScrollUp, lastScrollDown;
    public float scrollDelay = 0.75f;

    bool cursorLockedToCenter;
    public KeyCode unlockMouseWithKey = KeyCode.Escape;
    public KeyCode relockMouseWithKey = KeyCode.Alpha1;

    public bool showDebugLine = false;

    private void Awake()
    {
        IM = GetComponent<InteractionManager>();
        if (IM == null)
            Debug.LogWarning("Interaction manager not found. no gameplay will occur");
    }

    void Start()
    {
        if (playerCam == null)
            Debug.LogError("no camera assigned to input manager");
        else
            SetMouseConstraints(true);
    }

    void Update()
    {
        {
            if (Input.GetKeyDown(unlockMouseWithKey))
                SetMouseConstraints(false);
            if (Input.GetKeyDown(relockMouseWithKey))
                SetMouseConstraints(true);

            if (cursorLockedToCenter)
                RotateCamera();

            if (Input.mousePosition != cachedCursorPos)
            {
                laserPointer.origin = playerCam.transform.position;
                laserPointer.direction = playerCam.transform.forward;
                Physics.Raycast(laserPointer, out rh, InteractionReachDistance, IM.clickableLayers.value);
            }

            GetM1Input();
            GetM2Input();
            cachedCursorPos = Input.mousePosition;
        }

        {
            scrollAmt = Input.GetAxis("Mouse ScrollWheel");

            if (Time.time > lastScrollUp + scrollDelay && scrollAmt > 0)
            {
                lastScrollUp = Time.time;
                IM.SetScrollInput(scrollAmt);
            }

            if (Time.time > lastScrollDown + scrollDelay && scrollAmt < 0)
            {
                lastScrollDown = Time.time;
                IM.SetScrollInput(scrollAmt);
            }
        }
    }

    void GetM1Input()
    {
        M1currentInput = Input.GetAxisRaw("Fire1");

        if (M1currentInput >= M1MinInput) //holding down button
        {
            if (showDebugLine)
                Debug.DrawLine(laserPointer.origin, laserPointer.GetPoint(InteractionReachDistance), Color.red);

            if (M1prevInput < M1MinInput) //started holding
                IM.EngageWithObjectStart(rh.transform);
            else //still held
                IM.EngageWithObjectContinued(rh.transform);
        }
        else //not holding
        {
            if (showDebugLine)
                Debug.DrawLine(laserPointer.origin, laserPointer.GetPoint(InteractionReachDistance), Color.blue);

            if (M1prevInput >= M1MinInput) //let go
                IM.MightEngageWithObjectStart(rh.transform);
            else  //still not holding
                IM.MightEngageWithObjectContinued(rh.transform);
        }

        M1prevInput = M1currentInput;
    }

    void GetM2Input()
    {
        M2currentInput = Input.GetAxisRaw("Fire2");

        if (M2currentInput >= M1MinInput) //holding down button
        {
            if (M2prevInput < M2MinInput)
                IM.AimNavPointerStart();
            else
                IM.AimNavPointerContinued(playerCam.transform.forward + improveAim);
        }
        else //not holding
        {
            if (M2prevInput >= M2MinInput) //let go
                IM.ReleasedNavPointer();
        }

        M2prevInput = M2currentInput;
    }

    void RotateCamera()
    {
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch, -90, 90);

        playerCam.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    void SetMouseConstraints(bool mouseLockedToCenter)
    {
        if (mouseLockedToCenter)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorLockedToCenter = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorLockedToCenter = false;
        }
    }
}

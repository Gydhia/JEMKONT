﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PortalsFX2_MouseOrbit : MonoBehaviour
{
    public GameObject target;
    public float distance = 10.0f;

    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20;
    public float yMaxLimit = 80;

    float x = 0.0f;
    float y = 0.0f;

    void Start()
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    float prevDistance;

    void LateUpdate()
    {
        if (distance < 2) distance = 2;
        if (target && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
        {
            var pos = Mouse.current.position.ReadValue();
            var dpiScale = 1f;
            if (Screen.dpi < 1) dpiScale = 1;
            if (Screen.dpi < 200) dpiScale = 1;
            else dpiScale = Screen.dpi/200f;

            if (pos.x < 380*dpiScale && Screen.height - pos.y < 250*dpiScale) return;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;


            y = ClampAngle(y, yMinLimit, yMaxLimit);
            var rotation = Quaternion.Euler(y, x, 0);
            var position = rotation* new Vector3(0.0f, 0.0f, -distance) + target.transform.position;
            transform.rotation = rotation;
            transform.position = position;

        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            //Screen.lockCursor = false;
        }

        if (Math.Abs(prevDistance - distance) > 0.001f)
        {
            prevDistance = distance;
            var rot = Quaternion.Euler(y, x, 0);
            var po = rot*new Vector3(0.0f, 0.0f, -distance) + target.transform.position;
            transform.rotation = rot;
            transform.position = po;
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
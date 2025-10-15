using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class playerGun : MonoBehaviour
{
    private float distanceToClosestGun;
    public string targTag;
    private GameObject ClosestGun;
    public Transform body;
    public bool hasGun;
    private GameObject myGun;

    public grab leftHand, rightHand;
    public arms leftArm;

    [Tooltip("Assign the camera's custom script component (e.g. your 'camera' script) here.")]
    public Component camComponent; // assign the component (your custom camera script) in Inspector

    // Reflection cache
    private object camInstance;
    private FieldInfo aimingField;
    private PropertyInfo aimingProp;

    private void Awake()
    {
        // If user assigned a component in inspector, cache it and look for an 'aiming' field/property
        if (camComponent != null)
        {
            camInstance = camComponent;
            var type = camInstance.GetType();

            aimingField = type.GetField("aiming", BindingFlags.Public | BindingFlags.Instance);
            if (aimingField == null)
                aimingProp = type.GetProperty("aiming", BindingFlags.Public | BindingFlags.Instance);
        }
        else
        {
            // Try to auto-find a component on the main camera that has a public 'aiming' field/property
            GameObject camGo = Camera.main != null ? Camera.main.gameObject : GameObject.FindWithTag("MainCamera");

            if (camGo != null)
            {
                Component[] comps = camGo.GetComponents<Component>();
                foreach (var c in comps)
                {
                    var t = c.GetType();
                    var f = t.GetField("aiming", BindingFlags.Public | BindingFlags.Instance);
                    var p = t.GetProperty("aiming", BindingFlags.Public | BindingFlags.Instance);

                    if (f != null || p != null)
                    {
                        camComponent = c;
                        camInstance = c;
                        aimingField = f;
                        aimingProp = p;
                        break;
                    }
                }
            }
        }
    }

    private bool GetCamAiming()
    {
        if (camInstance == null) return false;
        if (aimingField != null) return (bool)aimingField.GetValue(camInstance);
        if (aimingProp != null) return (bool)aimingProp.GetValue(camInstance, null);
        return false;
    }

    private void SetCamAiming(bool value)
    {
        if (camInstance == null) return;

        if (aimingField != null)
            aimingField.SetValue(camInstance, value);
        else if (aimingProp != null)
            aimingProp.SetValue(camInstance, value, null);
    }

    private void Update()
    {
        findClosestGun();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (hasGun)
            {
                throwGun();
            }
            else
            {
                if (distanceToClosestGun < 5.5)
                {
                    pickUpGun();
                }
            }
        }

        if (hasGun)
        {
            leftHand.canGrab = false;
            rightHand.canGrab = false;
            leftArm.canUse = false;

            if (Input.GetMouseButton(1))
            {
                SetCamAiming(true);
            }
            else
            {
                SetCamAiming(false);
            }
        }
        else
        {
            leftHand.canGrab = true;
            rightHand.canGrab = true;
            leftArm.canUse = true;
            SetCamAiming(false);
        }

        if (GetCamAiming())
        {
            if (myGun == null) return;

            gun myGunsScript = myGun.GetComponent<gun>();
            if (myGunsScript == null) return;

            if (myGunsScript.oneShotAtATime)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    myGunsScript.shoot(null);
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    myGunsScript.shoot(null);
                }
            }
        }
    }

    void findClosestGun()
    {
        distanceToClosestGun = Mathf.Infinity;
        GameObject[] Guns = GameObject.FindGameObjectsWithTag(targTag);

        foreach (GameObject Gun in Guns)
        {
            float distanceToGun = Vector3.Distance(Gun.transform.position, body.position);
            if (distanceToGun < distanceToClosestGun)
            {
                distanceToClosestGun = distanceToGun;
                ClosestGun = Gun;
            }
        }
    }

    void pickUpGun()
    {
        myGun = ClosestGun;
        hasGun = true;
        myGun.GetComponent<gun>().Activate(rightHand.transform);
    }

    void throwGun()
    {
        if (myGun != null)
            myGun.GetComponent<gun>().DeActivate();

        myGun = null;
        hasGun = false;
    }
}

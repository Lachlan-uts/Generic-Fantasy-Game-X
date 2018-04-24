﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEquip : MonoBehaviour {

    GameObject currentWeapon;
    bool swordActive, axeActive, maceActive;

    [SerializeField]
    List<GameObject> weapons;

    // Use this for initialization
    void Start () {
        currentWeapon = weapons[0];
        for (int i = 0; i < weapons.Count; i++)
        {
            Collider detector = weapons[i].GetComponent<Collider>();
            detector.enabled = false;
            Rigidbody rigidbody = weapons[i].GetComponentInChildren<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
            
        }
        swordActive = true;
        axeActive = false;
        maceActive = false;
        EquipSword();
        EquipAxe();
        EquipMace();
    }
	
	// Update is called once per frame
	void Update () {
		
    }

    void EquipSword()
    {
        if (swordActive == true)
        {
            weapons[0].SetActive(true);
        }
        else weapons[0].SetActive(false);
    }

    void EquipAxe()
    {
        if (axeActive == true)
        {
            weapons[1].SetActive(true);
        }
        else weapons[1].SetActive(false);
    }

    void EquipMace()
    {
        if (maceActive == true)
        {
            weapons[2].SetActive(true);
        }
        else weapons[2].SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitch : MonoBehaviour {

    private bool canSwitch;
    private WeaponEquip weaponEquip;

	// Use this for initialization
	void Start () {
        canSwitch = false;
        weaponEquip = this.GetComponentInChildren<WeaponEquip>();
	}
	
	// Update is called once per frame
	void Update () { 
       
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "WeaponDrop")
        {
            Debug.Log("Can Switch to " + other);
            canSwitch = true;
        }
    }

    void OnTriggerStay (Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (other.name == "Sword00_prefab(Clone)")
            {
                Debug.Log("Sword Equipped!");
                weaponEquip.EquipSword();
            }
            else if (other.name == "Axe00_prefab(Clone)")
                {
                Debug.Log("Axe Equipped");
                weaponEquip.EquipAxe();
            }
            else
            {
                Debug.Log("Mace Equipped");
                weaponEquip.EquipMace();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "WeaponDrop")
        {
            Debug.Log("Can't Switch to " + other);
            canSwitch = false;
        }
    }
}

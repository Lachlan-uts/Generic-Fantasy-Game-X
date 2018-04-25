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

    void DropWeapon(Vector3 other)
    {
        Debug.Log("" + other);
        if (weaponEquip.GetName() == "Sword00_prefab")
        {
            Debug.Log("Sword Swap");
            CreateDroppedWeapon(other, 0);
        }
        else if (weaponEquip.GetName() == "Axe00_prefab")
        {
            Debug.Log("Axe Swap");
            CreateDroppedWeapon(other, 1);
        }
        else
        {
            Debug.Log("Mace Swap");
            CreateDroppedWeapon(other, 2);
        }
    }

    void CreateDroppedWeapon(Vector3 other, int num)
    {
        Quaternion weaponRotation = Quaternion.AngleAxis(90, Vector3.forward);
        Vector3 newPosition = new Vector3(other.x, 2f, other.z);
        GameObject weaponHolder = weaponEquip.GetWeapon();
        Rigidbody rigidbody = weaponHolder.GetComponentInChildren<Rigidbody>();
        rigidbody.useGravity = true;
        rigidbody.constraints = RigidbodyConstraints.None;
        GameObject swappedWeapon = Instantiate(weaponHolder, newPosition, weaponRotation);
        swappedWeapon.tag = "WeaponDrop";
        Collider swappedCollider = swappedWeapon.GetComponent<Collider>();
        swappedCollider.enabled = true;
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
                DropWeapon(other.transform.position);
                weaponEquip.EquipSword();
            }
            else if (other.name == "Axe00_prefab(Clone)")
                {
                Debug.Log("Axe Equipped");
                DropWeapon(other.transform.position);
                weaponEquip.EquipAxe();
            }
            else
            {
                Debug.Log("Mace Equipped");
                DropWeapon(other.transform.position);
                weaponEquip.EquipMace();
            }
            Destroy(other.gameObject);
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

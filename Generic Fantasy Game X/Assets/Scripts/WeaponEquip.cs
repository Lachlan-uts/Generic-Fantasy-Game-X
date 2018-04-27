using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEquip : MonoBehaviour {
    
    public bool swordActive, axeActive, maceActive;

    private GameObject currentWeapon;

    [SerializeField]
    List<GameObject> weapons;

    // Use this for initialization
    void Start () {
        currentWeapon = weapons[0];
        for (int i = 0; i < weapons.Count; i++)
        {
            Collider detector = weapons[i].GetComponent<Collider>();
            detector.enabled = false;
            ResetContraints(i);     
        }
        swordActive = true;
        axeActive = false;
        maceActive = false;
        EquipSword();
    }
	
	// Update is called once per frame
	void Update () {
		
    }

    public void EquipSword()
    {
        WeaponActive(0);
        weapons[0].SetActive(true);
        weapons[1].SetActive(false);
        weapons[2].SetActive(false);
        for (int i = 0; i < weapons.Count; i++)
        {
            ResetContraints(i);
        }
    }

    public void EquipAxe()
    {
        WeaponActive(1);
        weapons[0].SetActive(false);
        weapons[1].SetActive(true);
        weapons[2].SetActive(false);
        for (int i = 0; i < weapons.Count; i++)
        {
            ResetContraints(i);
        }
    }

    public void EquipMace()
    {
        WeaponActive(2);
        weapons[0].SetActive(false);
        weapons[1].SetActive(false);
        weapons[2].SetActive(true);
        for (int i = 0; i < weapons.Count; i++)
        {
            ResetContraints(i);
        }
    }

    void ResetContraints(int i)
    {
        Rigidbody rigidbody = weapons[i].GetComponentInChildren<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }

    void WeaponActive(int i)
    {
        switch (i)
        {
            case 0:
                swordActive = true;
                axeActive = false;
                maceActive = false;
                break;
            case 1:
                swordActive = false;
                axeActive = true;
                maceActive = false;
                break;
            case 2:
                swordActive = false;
                axeActive = false;
                maceActive = true;
                break;
            default:
                Debug.Log("Switch Statement Error");
                break;
        }
    }

    public string GetName()
    {
        Debug.Log(swordActive);
        if (swordActive) return weapons[0].name;
        else if (axeActive) return weapons[1].name;
        else return weapons[2].name;
    }

    public GameObject GetWeapon()
    {
        if (swordActive) return weapons[0];
        else if (axeActive) return weapons[1];
        else return weapons[2];
    }
}

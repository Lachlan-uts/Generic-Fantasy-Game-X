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
            Rigidbody rigidbody = weapons[i].GetComponentInChildren<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
            
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
        weapons[0].SetActive(true);
        weapons[1].SetActive(false);
        weapons[2].SetActive(false);
    }

    public void EquipAxe()
    {
        weapons[0].SetActive(false);
        weapons[1].SetActive(true);
        weapons[2].SetActive(false);
    }

    public void EquipMace()
    {
        weapons[0].SetActive(false);
        weapons[1].SetActive(false);
        weapons[2].SetActive(true);
    }

    public int GetActive()
    {
        if (!swordActive) return 0;
        else if (!axeActive) return 1;
        else return 2;
    }
}

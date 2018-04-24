using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyWeaponSpawn : MonoBehaviour {

    [SerializeField]
    List<GameObject> weapons;
    
    private List<GameObject> clones = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            Rigidbody floorWeapon;
            floorWeapon = weapons[i].GetComponentInChildren<Rigidbody>();
            floorWeapon.useGravity = true;
            floorWeapon.constraints = RigidbodyConstraints.None;
        }

        //simulating dropped weapons
        Quaternion weaponRotation = Quaternion.AngleAxis(90, Vector3.forward);
        clones.Add(Instantiate(weapons[0], new Vector3(10f, 0.1f, 10f), weaponRotation));
        clones.Add(Instantiate(weapons[0], new Vector3(-10f, 0.1f, 10f), weaponRotation));
        clones.Add(Instantiate(weapons[1], new Vector3(10f, 0.1f, -10f), weaponRotation));
        clones.Add(Instantiate(weapons[1], new Vector3(-10f, 0.1f, -10f), weaponRotation));
        clones.Add(Instantiate(weapons[2], new Vector3(20f, 0.1f, 20f), weaponRotation));
        clones.Add(Instantiate(weapons[2], new Vector3(-20f, 0.1f, -20f), weaponRotation));

        foreach (GameObject drop in clones)
        {
            drop.tag = "WeaponDrop";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }   
}

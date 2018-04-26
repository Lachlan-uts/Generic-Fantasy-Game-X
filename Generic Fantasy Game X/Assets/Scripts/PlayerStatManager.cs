using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatManager : MonoBehaviour {

    PlayerManager playerManager;

    PlayerInformation slot1 = new PlayerInformation();
    public GameObject thisPlayer;
    

    void Start()
    {


        //playerManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<PlayerManager>();
        //slot1.ThisPlayer = GameObject.FindGameObjectWithTag("Player");
        //slot1.Name = "Slot 1 Player";
        //slot1.level = playerManager.currentLevel;
        //slot1.Exp = playerManager.currentExp;
        //slot1.CurrentHP = playerManager.currentHP;
        //slot1.MaxHP = playerManager.HPUp[playerManager.currentLevel];
        //slot1.Damage = playerManager.currentAttack;
        //slot1.IsActive = true;

        //Debug.Log("first character: " + slot1.Name +
        //  " " + slot1.exp +
        //  " " + slot1.level +
        //  " " + slot1.currentHP +
        //  " " + slot1.maxHP +
        //  " " + slot1.damage +
        //  " " + slot1.isActive);
    }
    void Update()
    {
       

    }
   

}

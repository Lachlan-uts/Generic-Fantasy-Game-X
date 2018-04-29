using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class PlayerInformation 
{
    //From PlayerInfo Script, moved here
        public GameObject thisPlayer;
        public string name;
        public int level;
        public int exp;
        public int currentHP;
        public int maxHP;
        public int damage;
        public bool isActive;

    public GameObject ThisPlayer
    {
        get
        {
            return thisPlayer;
        }
        set
        {
            thisPlayer = value;
        }
    }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Level
        {
            get { return level; }
            set { level = value; }
        }
        public int Exp
        {
            get { return exp; }
            set { exp = value; }
        }
        public int CurrentHP
        {
            get { return currentHP; }
            set { currentHP = value; }
        }
        public int MaxHP
        {
            get { return maxHP; }
            set { maxHP = value; }
        }
        public int Damage
        {
            get { return damage; }
            set { damage = value; }
        }
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

    
}




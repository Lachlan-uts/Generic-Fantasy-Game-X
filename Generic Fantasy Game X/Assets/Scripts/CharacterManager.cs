using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {

	void Start()
	{
		PlayerInformation firstPlayer = new PlayerInformation ();

		firstPlayer.Name = "First Hero";
		firstPlayer.Exp = 0;
		firstPlayer.Level = (firstPlayer.Exp / 100);
		firstPlayer.HP = 100;
		firstPlayer.Damage = 10;
		firstPlayer.isActive = true;

		Debug.Log ("First Character: " 
			+ firstPlayer.Name + " " 
			+ firstPlayer.Exp + " " 
			+ firstPlayer.Level + " " 
			+ firstPlayer.HP + " " 
			+ firstPlayer.Damage + " " 
			+ firstPlayer.isActive);
	}

}

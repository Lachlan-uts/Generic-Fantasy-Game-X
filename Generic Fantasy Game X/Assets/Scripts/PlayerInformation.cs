using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInformation : MonoBehaviour {
	public string Name { get; set; }
	public int Level { get; set; }
	public int Exp { get; set; }
	public int HP { get; set; }
	public int Damage { get; set; }
	public bool isActive { get; set; }

	public PlayerInformation()
	{
		this.Name = "No Name";
		this.Exp = 0;
		this.Level = ( Exp / 100);
		this.HP = 100;
		this.Damage = 10;
		isActive = false;
	}
}

	public class Slot1 : PlayerInformation
	{
	public Slot1()
	{
		Name = "First Hero";
		Exp = 0;
		Level = ( Exp / 100 );
		HP = 100;
		Damage = 10;
		isActive = true;

		Debug.Log ("First Character: " + Name + " " + Exp + " " + Level + " " + HP + " " + Damage + " " + isActive);
	}
	}



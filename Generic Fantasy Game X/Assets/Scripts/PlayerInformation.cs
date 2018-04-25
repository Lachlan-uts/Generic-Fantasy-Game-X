using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class PlayerInformation : MonoBehaviour {
	public string Name { get; set; }
	public int Level { get; set; }
	public int Exp { get; set; }
	public int CurrentHP { get; set; }
	public int MaxHP { get; set; }
	public int Damage { get; set; }
	public bool isActive { get; set; }


	public PlayerInformation()
	{
//		this.Name = "No Name";
//		this.Exp = 0;
//		this.Level = ( Exp / 100);
//		this.CurrentHP = 0;
//		this.MaxHP = 0;
//		this.Damage = 0;
//		isActive = false;
		Name = "First Hero";
//		Exp = 100;
//		Level = ( Exp / 100 );
//		MaxHP = 0;
//		CurrentHP = 0;
//		Damage = 0;
		isActive = true;
		
		Debug.Log ("First Character: " + Name + " " + Exp + " " + Level + " " + CurrentHP + " " + Damage + " " + isActive);
	}
}

//	public class Slot1 : PlayerInformation
//	{
//	public Slot1()
//	{
//		Name = "First Hero";
//		Exp = 0;
//		Level = ( Exp / 100 );
//		HP = 100;
//		Damage = 10;
//		isActive = true;
//
//		Debug.Log ("First Character: " + Name + " " + Exp + " " + Level + " " + HP + " " + Damage + " " + isActive);
//	}
	//}



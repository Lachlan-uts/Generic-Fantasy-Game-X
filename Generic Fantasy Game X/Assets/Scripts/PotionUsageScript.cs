using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionUsageScript : MonoBehaviour {

	// public properties
	public enum potionSize { Tiny, Small, Medium, Large };

	[SerializeField]
	private potionSize size;

	public int fluidAmount { get; private set; }
	public int fluidLimit { get; private set; }

	// Use this for initialization
	void Start () {
		if (size == null) {
			size = potionSize.Tiny;
			fluidAmount = 5;
			fluidLimit = 25;
		} else {
			if (size.Equals (potionSize.Small)) {
				fluidLimit = 120;
			} else if (size.Equals (potionSize.Medium)) {
				fluidLimit = 500;
			} else {
				fluidLimit = 1400;
			}
			fluidAmount = fluidLimit;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GenerateStatistics(int floorNumber) {
		int potionSelect = Random.Range (0, 101);
		int chanceSmall, chanceMed, chanceLarge;

		if (floorNumber < 5) {
			chanceSmall = 95;
			chanceMed = 100;
			chanceLarge = 101;
		} else if (floorNumber < 15) {
			chanceSmall = 45;
			chanceMed = 90;
			chanceLarge = 100;
		} else {
			chanceSmall = 10;
			chanceMed = 50;
			chanceLarge = 100;
		}

		if (potionSelect < chanceSmall) {
			size = potionSize.Small;
			fluidLimit = 120;
		} else if (potionSelect < chanceMed) {
			size = potionSize.Medium;
			fluidLimit = 500;
		} else {
			size = potionSize.Large;
			fluidLimit = 1400;
		}

		fluidAmount = fluidLimit;

	}

	public int Quaff(int amountNeeded) { // 'Quaff' an amount needed, so can conserve potions. Integer returned is the amount healed
		int fluidConsumed = 0;
		if (amountNeeded > fluidAmount) {
			fluidConsumed = fluidAmount;
			fluidAmount = 0;
		} else {
			fluidConsumed = amountNeeded;
			fluidAmount -= amountNeeded;
		}

		return fluidConsumed;
	}

	public int Refill(int amountRefilled) { // 'Refills' the amount poured in. Integer returned is the amount leftover.
		int fluidRefilled = 0;
		int fluidLeft = 0;
		if (amountRefilled > (fluidLimit - fluidAmount)) {
			fluidRefilled = fluidLimit - fluidAmount;
			fluidAmount = fluidLimit;
			fluidLeft = amountRefilled - fluidRefilled;
		} else {
			fluidRefilled = amountRefilled;
			fluidAmount += fluidRefilled;
			fluidLeft = 0;
		}

		return fluidLeft;
	}

	public void Refill() { // The simpler version, which returns nothing
		fluidAmount = fluidLimit;
	}

}

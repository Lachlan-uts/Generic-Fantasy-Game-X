﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionUsageScript : MonoBehaviour {

	// public properties
	public enum potionSize { Empty, Tiny = 25, Small = 120, Medium = 500, Large = 1400 };

	[SerializeField]
	private potionSize size;

	public int fluidAmount { get; private set; }

	// Use this for initialization
	void Start () {
		if (size == potionSize.Empty)
			size = potionSize.Tiny;
		fluidAmount = (int) size;
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
		} else if (potionSelect < chanceMed) {
			size = potionSize.Medium;
		} else {
			size = potionSize.Large;
		}
		fluidAmount = (int) size;
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
		if (amountRefilled > ((int) size - fluidAmount)) {
			fluidRefilled = (int) size - fluidAmount;
			fluidAmount = (int)size;
			fluidLeft = amountRefilled - fluidRefilled;
		} else {
			fluidRefilled = amountRefilled;
			fluidAmount += fluidRefilled;
			fluidLeft = 0;
		}

		return fluidLeft;
	}

	public void Refill() { // The simpler version, which returns nothing
		fluidAmount = (int) size;
	}

}
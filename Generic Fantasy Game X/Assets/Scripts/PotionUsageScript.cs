using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionUsageScript : MonoBehaviour {

	// private variables
	public int fluidAmount { get; private set; }
	public int fluidLimit { get; private set; }

	// Use this for initialization
	void Start () {
		fluidAmount = 5; // Placeholder
		fluidLimit = 15; // Placeholder
	}
	
	// Update is called once per frame
	void Update () {
		
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

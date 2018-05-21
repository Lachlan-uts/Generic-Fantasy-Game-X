using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISlotScript : MonoBehaviour {

	public Text healthText;
	public Slider healthBar;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Can use the funcitonality of this script to further improve the UI functionality
	public void updateHealthStatus(int curHealth, int maxHealth) {
		healthText.text = "" + curHealth + "/" + maxHealth + "";
		// Insert code for updating the slider
	}
}

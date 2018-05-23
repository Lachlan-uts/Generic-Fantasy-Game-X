using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PowerGridInventory;


public class EntityStatisticsScript : MonoBehaviour {

	public enum entitySlots { Helmet, Chestplate, Greaves, RightHand, LeftHand, Potion };

	// public variables
	public GameObject nameUI; // placeholder
	public GameObject healthUI; // placeholder

	public PGIModel inventory;
	//public GameObject inventoryGO;


	// public properties
	[SerializeField]
	public int curHealth { get { 
			return curHealth;
		}
		private set { 
			curHealth = value;
			//healthUI.GetComponent<Text> ().text = "" + curHealth + "/" + maxHealth; // UI update everytime the current health is affected
		} }
	[SerializeField]
	public int maxHealth { get { 
			return maxHealth; 
		} 
		private set { 
			maxHealth = value; 
		} }
	[SerializeField]
	public int level { get; private set; }
	[SerializeField]
	public int experience { get { 
			return experience;
		}
		private set { 
			experience = value;
		} }

	[SerializeField]
	public int statStrength { get; private set; }
	[SerializeField]
	public int statVitality { get; private set; }

	// "Equipment Slots" for items - where they go when they are equipped
	[SerializeField]
	private GameObject rightHand;
	[SerializeField]
	private GameObject leftHand;
	[SerializeField]
	private GameObject chest;

	public Dictionary<string, GameObject> e = new Dictionary<string, GameObject>();

	public Dictionary<entitySlots, GameObject> equippedItems = new Dictionary<entitySlots, GameObject> ();

	//public List<GameObject> inventoryItems;

	public GameObject target;
	public string targetContext;

	// Use this for initialization
	void Start () {
		equippedItems.Add (entitySlots.Helmet, null);
		equippedItems.Add (entitySlots.Chestplate, null);
		equippedItems.Add (entitySlots.Greaves, null);
		equippedItems.Add (entitySlots.RightHand, null);
		equippedItems.Add (entitySlots.LeftHand, null);
		equippedItems.Add (entitySlots.Potion, null);
	}
	
	// Update is called once per frame
	void Update () {
		if (target != null) {
			if (targetContext == "Item") {
				if (Vector3.Distance (this.gameObject.transform.position, target.transform.position) < 0.2f) {
					//Pickup (target);
				}
			} else if (targetContext == "Furniture") {
				if (Vector3.Distance (this.gameObject.transform.position, target.transform.position) < 0.2f) {
					Interact (target);
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.P)) {
			Debug.Log ("attempting to find item");
			Pickup (GameObject.Find ("ExampleDrop(Clone)"));
			Pickup (GameObject.Find ("Sword(x)"));
		}
	}

	public void InstigateCommand (string context, GameObject other) {
		targetContext = context;
		target = other;
		this.gameObject.GetComponent<EntityNavigationScript> ().SetDestination (target.transform.position, this.gameObject);
	}

	public void Interact (GameObject other) {
		other.GetComponent<FurnitureScript> ().Interact ();
		target = null;
		this.gameObject.GetComponent<EntityNavigationScript> ().CancelMovement ();
	}

	public void Pickup (GameObject other) {
		other.GetComponent<DropScript> ().Pickup (inventory.transform);
		target = null;
		this.gameObject.GetComponent<EntityNavigationScript> ().CancelMovement ();
	}

	/*public void Pickup (GameObject other) {
		other.transform.SetParent (inventoryGO.transform);
		inventoryItems.Add (other);
		target = null;
		this.gameObject.GetComponent<EntityNavigationScript> ().CancelMovement ();
	}

	public void Drop (GameObject other) {
		inventoryItems.Remove (other);
		other.transform.parent = null;
	}*/

	public void Equip(PGISlotItem item, PGIModel model, PGISlot slot) {
		switch (item.GetComponent<ItemTypeScript> ().itemType) {
		case entitySlots.RightHand:
			item.gameObject.transform.position = rightHand.transform.position;
			item.gameObject.transform.rotation = rightHand.transform.rotation;
			item.gameObject.transform.Rotate (0.0f, 90.0f, 90.0f);
			item.gameObject.transform.SetParent (rightHand.transform);
			item.gameObject.transform.Translate (0.052f, -0.011f, -0.086f);
			break;
		default:
			break;
		}
	}

	public void Quaff () {
//		GameObject equippedPotion;
//		if (equippedItems.TryGetValue (entitySlots.Potion, out equippedPotion)) {
//
//		}
		int healAmount = inventory.Equipment[0].Item.GetComponent<PotionUsageScript>().Quaff(maxHealth - curHealth);
		Heal (healAmount);
	}

	private void LevelUp () {
		statStrength++;
		statVitality++;
		UpdateMaxHealth ();
	}

	public void Heal (int amount) {
		curHealth += amount;
	}

	private void FullHeal () {
		curHealth = maxHealth;
	}

	private void UpdateMaxHealth () {
		int prevMaxHealth = maxHealth;
		maxHealth = (level * 15) + (statVitality * 20);
		int healthDiff = maxHealth - prevMaxHealth;
		curHealth += healthDiff;
	}

	public void EquipItem (entitySlots slot, GameObject item) {
		// Equipping Items
		equippedItems[slot] = item;
	}

	public void GenerateStats (int floor, int category) {
		if (this.gameObject.tag.Equals ("Enemy")) {
			if (category == 0) {
				statStrength = 1 + level;
				statVitality = 0 + level;
			} else if (category == 1) {
				statStrength = 2 + level;
				statVitality = 3 + level;
			} else {
				statStrength = 2 * level + 4;
				statVitality = 3 * level - 1;
			}
			UpdateMaxHealth ();
			FullHeal ();
		} else {
			statStrength = 5;
			statVitality = 7;
			UpdateMaxHealth ();
			FullHeal ();
		}
	}

	public void GainExperience(int gainedExp) {
		experience += gainedExp;
	}

	public int ExperienceNeeded() { // Determines how much experience is needed in order to level up
		int expNeeded = 0;
		if (level < 5) {
			expNeeded = ((level - 1) * 20) + 80;
		} else if (level < 15) {
			expNeeded = ((level + 2) * 40) + 40;
		} else {
			expNeeded = ((level + 3) * 50) + 30;
		}

		return expNeeded;
	}

	public void TakeDamage(int damage) { // Determines how much damage is to be taken
		curHealth -= damage;
		if (curHealth <= 0) {
			curHealth = 0;
			if (this.gameObject.tag.Equals ("Enemy")) {
				foreach (GameObject hero in GameObject.FindGameObjectsWithTag("Hero")) {
					hero.GetComponent<EntityStatisticsScript> ().GainExperience (10 * level);
				}
			}

			// Invoke "death" here

		} else if (curHealth <= ((int) 0.3f * maxHealth)) {
			if (inventory.Equipment [0].Item != null) {
				if (inventory.Equipment [0].Item.GetComponent<PotionUsageScript> ().fluidAmount > 0) {
					Quaff ();
				}
			}
		}


	}
}

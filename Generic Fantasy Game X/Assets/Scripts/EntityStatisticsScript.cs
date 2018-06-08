using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PowerGridInventory;


public class EntityStatisticsScript : MonoBehaviour {

	public enum entitySlots { Helmet, Chestplate, Greaves, RightHand, LeftHand, Potion };
	public enum entityScripts { Navigation, Targeting, Selection };

	public enum entityTargetContexts { None, Item, Furniture, Enemy };

	//enums to check current order state and entity default behaviour
	public enum entityOrderState { None, Script, Human };
	public enum entityResponseState { Neutral, Defensive, Aggressive };

	//Entity states for orders and auto response.
	public entityOrderState orderState { get; private set; }
	[SerializeField]
	public entityResponseState responseState { get; private set; }

	//Starting Weapon
	[SerializeField]
	private GameObject startingWeapon;

	//All the different components that the entity statisics script controls
	[SerializeField]
	private EntityTargetScript targeting;
	private EntityNavigationScript navigation;
	private EntitySelectedScript selection;

	// public variables
	public GameObject nameUI; // placeholder
	public Slider healthUI; // placeholder

	public PGIModel inventory;
	//public GameObject inventoryGO;


	//private stuff
	private int curHealth_, maxHealth_,experience_;

	// public properties
	[SerializeField]
	public int curHealth { get {
			return curHealth_;
		}
		private set {
			curHealth_ = value;
			healthUI.value = value;
			if (staticHealthUI != null) {
				staticHealthUI.value = value;
			}
			//healthUI.GetComponent<Text> ().text = "" + curHealth + "/" + maxHealth; // UI update everytime the current health is affected
		} }
	[SerializeField]
	public int maxHealth { get {
			return maxHealth_;
		}
		private set {
			maxHealth_ = value;
			healthUI.maxValue = value;
			if (staticHealthUI != null) {
				staticHealthUI.maxValue = value;
			}
		} }
	[SerializeField]
	public int level { get; private set; }
	[SerializeField]
	public int experience { get {
			return experience_;
		}
		private set {
			experience_ = value;
			if (staticXPUI != null) {
				staticXPUI.value = value;
			}
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
	private GameObject shieldPos;
	[SerializeField]
	private GameObject chest;

	public Dictionary<entityScripts, GameObject> e = new Dictionary<entityScripts, GameObject>();

	public Dictionary<entitySlots, GameObject> equippedItems = new Dictionary<entitySlots, GameObject> ();

	//public List<GameObject> inventoryItems;

	public GameObject targetThing;
	public entityTargetContexts targetContext;

	//UI Stuff
	public Slider staticHealthUI;
	public Text staticHealthText;
	public Slider staticXPUI;
	public Text staticXPText;
	public Text currentLevelText;

	//use awake for certain variables
	void Awake() {
		orderState = entityOrderState.None;
		responseState = entityResponseState.Defensive;

		// Add this entity to a relevent list.
		LevelGenerationScript.entityLists [this.gameObject.tag].Add (this.gameObject.transform);
	}

	// Use this for initialization
	void Start () {
		equippedItems.Add (entitySlots.Helmet, null);
		equippedItems.Add (entitySlots.Chestplate, null);
		equippedItems.Add (entitySlots.Greaves, null);
		equippedItems.Add (entitySlots.RightHand, null);
		equippedItems.Add (entitySlots.LeftHand, null);
		equippedItems.Add (entitySlots.Potion, null);

		//assign default graphics
		healthUI = GetComponentInChildren<Slider> ();
		healthUI.minValue = 0;

		//A series of magic numbers
		level = 1;
		statVitality = 5;
		statStrength = 3;

		// UI updating as a result of "magic numbers"
		if (staticXPUI != null) {
			staticXPUI.maxValue = ExperienceNeeded ();
		}

		//set health
		maxHealth = 0;
		UpdateMaxHealth ();

		//get the selection
		targeting = GetComponent<EntityTargetScript> ();
		navigation = GetComponent<EntityNavigationScript> ();
		selection = GetComponentInChildren<EntitySelectedScript> ();

		// Equipping the starting weapon
		GameObject initialWeapon = Instantiate(startingWeapon, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
		//InstigateCommand (entityTargetContexts.Item, initialWeapon);
		Pickup (initialWeapon);
		//inventory.Equip (initialWeapon.GetComponent<PGISlotItem> (), 1, true);

		//the below is all garbage!

//		isSelected
//		Debug.Log (GetComponentInChildren<EntitySelectedScript> ().GetType ());
//		e.Add(entityScripts.Navigation,GetComponentInChildren<EntitySelectedScript>());
//		MonoBehaviour temp = null;
//		if (e.TryGetValue (entityScripts.Navigation, out temp)) {
//			Debug.Log();
//		}
	}

	// Update is called once per frame
	void Update () {
		if (targetThing != null) {
			if (targetContext == entityTargetContexts.Item) {
				if (Vector3.Distance (this.gameObject.transform.position, targetThing.transform.position) < 0.2f) {
					Pickup (targetThing);
				}
			} else if (targetContext == entityTargetContexts.Furniture) {
				if (Vector3.Distance (this.gameObject.transform.position, targetThing.transform.position) < 0.2f) {
					Interact (targetThing);
				}
			}
		}

//		if (Input.GetKeyDown (KeyCode.G)) {
//			if (this.gameObject.CompareTag("Hero")) {
//				Debug.Log ("attempting to find nearest item");
//				//Pickup (GameObject.Find ("ExampleDrop(Clone)"));
//				//Pickup (GameObject.Find ("Sword(x)"));
//				List<GameObject> pickupItems = new List<GameObject>();
//				pickupItems.AddRange (GameObject.FindGameObjectsWithTag ("Items"));
//
//				if (pickupItems.Count > 0) {
//					Pickup (GameObject.Find (pickupItems[0].name));
//				}
//			}
//
//		}

		//used for testing the health UI.
		if (Input.GetKeyDown (KeyCode.M)) {
			TakeDamage (20);
		}
//		if (staticHealthText != null && currentLevelText != null && staticXPText != null) {
//			staticHealthText.text = "HP: " + curHealth + "/" + maxHealth;
//			staticXPText.text = "Exp: " + experience + "/" + ExperienceNeeded ();
//			currentLevelText.text = "Level: " + level;
//		}

		if (staticHealthText != null) {
			staticHealthText.text = "HP: " + curHealth + "/" + maxHealth;
		}

		if (staticXPText != null) {
			staticXPText.text = "Exp: " + experience + "/" + ExperienceNeeded ();
		}

		if (currentLevelText != null) {
			currentLevelText.text = "Level: " + level;
		}

	}

	public void InstigateCommand (entityTargetContexts context, GameObject other) {
		if (Vector3.Distance (other.transform.position, this.transform.position) > 1.0f) {
			targetContext = context;
			targetThing = other;
			this.gameObject.GetComponent<EntityNavigationScript> ().SetDestination (targetThing.transform.position, this.gameObject);
		} else {
			Pickup (other);
		}
	}

	public void Interact (GameObject other) {
		other.GetComponent<FurnitureScript> ().Interact ();
		targetThing = null;
		targetContext = entityTargetContexts.None;
		this.gameObject.GetComponent<EntityNavigationScript> ().CancelMovement ();
	}

	public void Pickup (GameObject other) {
		other.GetComponent<DropScript> ().Pickup (inventory.transform);
		targeting = null;
		targetContext = entityTargetContexts.None;
		this.gameObject.GetComponent<EntityNavigationScript> ().CancelMovement ();
	}

	/*public void Pickup (GameObject other) {
		other.transform.SetParent (inventoryGO.transform);
		inventoryItems.Add (other);
		targetThing = null;
		this.gameObject.GetComponent<EntityNavigationScript> ().CancelMovement ();
	}

	public void Drop (GameObject other) {
		inventoryItems.Remove (other);
		other.transform.parent = null;
	}*/

	public void Equip(PGISlotItem item, PGIModel model, PGISlot slot) {
		switch (item.GetComponent<ItemTypeScript> ().itemType) {
		case entitySlots.Potion:
			item.gameObject.transform.position = leftHand.transform.position;
			item.gameObject.transform.rotation = leftHand.transform.rotation;
			item.gameObject.transform.SetParent (leftHand.transform);
			break;
		case entitySlots.RightHand:
			item.gameObject.transform.position = rightHand.transform.position;
			item.gameObject.transform.rotation = rightHand.transform.rotation;
			item.gameObject.transform.SetParent (rightHand.transform);
//			equippedItems.Add (entitySlots.RightHand, item.gameObject);
			//item.gameObject.transform.Rotate (0.0f, 90.0f, 90.0f);
			//item.gameObject.transform.Translate (0.052f, -0.011f, -0.086f);
			break;
		default:
			break;
		}
	}

	public void Dequip(PGISlotItem item, PGIModel model, PGISlot slot) {
		switch (item.GetComponent<ItemTypeScript> ().itemType) {
		case entitySlots.Potion:
			item.gameObject.transform.position = inventory.gameObject.transform.position;
			item.gameObject.transform.rotation = inventory.gameObject.transform.rotation;
			//item.gameObject.transform.SetParent (leftHand.transform);
			break;
		case entitySlots.RightHand:
			item.gameObject.transform.position = inventory.gameObject.transform.position;
			item.gameObject.transform.rotation = inventory.gameObject.transform.rotation;
//			equippedItems.Remove (entitySlots.RightHand);
			//item.gameObject.transform.Rotate (0.0f, 90.0f, 90.0f);
			//item.gameObject.transform.SetParent (rightHand.transform);
			//item.gameObject.transform.Translate (0.052f, -0.011f, -0.086f);
			break;
		default:
			break;
		}
	}

	public void DropItem(PGISlotItem item, PGIModel model) {
		item.gameObject.GetComponent<DropScript> ().Drop ();
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
		level++;
		statStrength++;
		statVitality++;
		UpdateMaxHealth ();
		if (staticXPUI != null) {
			staticXPUI.maxValue = ExperienceNeeded ();
		}
	}

	public void Heal (int amount) {
		curHealth += amount;
	}

	private void FullHeal () {
		curHealth = maxHealth;
	}

	private void UpdateMaxHealth () {
		int prevMaxHealth = maxHealth;
		if (this.gameObject.CompareTag ("Hero")) {
			maxHealth = 5 + (level * 15) + (statVitality * 20);
		} else {
			maxHealth = (level * 15) + (statVitality * 20);
		}
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
		if (curHealth != 0) {
			experience += gainedExp;
			if (experience >= ExperienceNeeded ()) {
				experience -= ExperienceNeeded ();
				LevelUp ();
			}
		}
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

	public int ExperienceNeeded(int theoreticalLevel) { // Determines how much experience is needed in order to level up
		int expNeeded = 0;
		if (theoreticalLevel < 5) {
			expNeeded = ((theoreticalLevel - 1) * 20) + 80;
		} else if (theoreticalLevel < 15) {
			expNeeded = ((theoreticalLevel + 2) * 40) + 40;
		} else {
			expNeeded = ((theoreticalLevel + 3) * 50) + 30;
		}

		return expNeeded;
	}

	public void TakeDamage(int damage) { // Determines how much damage is to be taken
		if (curHealth == 0) {
			this.gameObject.GetComponent<EntityTargetScript> ().Die ();
			return;
		}

		int actualDamage = damage - statStrength;
		if (actualDamage < 1) {
			actualDamage = 1;
		}
		curHealth -= actualDamage;

		if (curHealth <= 0) {
			curHealth = 0;
			if (this.gameObject.tag.Equals ("Enemy")) {
				foreach (GameObject hero in GameObject.FindGameObjectsWithTag("Hero")) {
					hero.GetComponent<EntityStatisticsScript> ().GainExperience (3 + 12 * level);
					Debug.Log ("Gain experience...?");
				}

				int lootChance = 30;
				if (Random.Range (0, 101) < lootChance) {
					GameObject.Find ("DropManager").GetComponent<DropManagerScript> ().generateLoot (this.gameObject.transform);
				}

			}

			// Invoke "death" here
			this.gameObject.GetComponent<EntityTargetScript>().Die();
			//GameObject.Find("LevelGenerator").GetComponent<LevelGenerationScript>().entityLists[this.gameObject.tag].Remove(this.gameObject.transform);
			LevelGenerationScript.entityLists [this.gameObject.tag].Remove (this.gameObject.transform);
			GetComponentInChildren<Canvas> ().enabled = false;

		} else if (curHealth <= Mathf.RoundToInt(0.3f * maxHealth)) {
			if (inventory.Equipment [0].Item != null) {
				if (inventory.Equipment [0].Item.GetComponent<PotionUsageScript> ().fluidAmount > 0) {
					Quaff ();
				}
			}
		}
	}

	public void SelectionToggle() {
		selection.enabled = !selection.enabled;
	}

//	private IEnumerator IsSelected() {
//	}

	/*
	 * This section will be all about getting the attack system to work with equiped weapons. 
	 * 
	 */
	public void ToggleWeaponCollider(string state) {
//		GameObject weapon = inventory.Equipment [1].gameObject;
//		GameObject weapon = inventory.Equipment [1].Item.gameObject;
		WeaponScript weaponScript = GetComponentInChildren<WeaponScript> ();
		if (weaponScript != null) {
			if (state.Contains ("true")) {
				weaponScript.ToggleCollider (true);
			} else {
				weaponScript.ToggleCollider (false);
			}
		}
	}

	private void Die() {

		if (this.gameObject.CompareTag ("Hero")) {
			//			Invoke ("PlayerDeath", 3.0f);
		} else {
			GameObject.Find ("SampleExit(Clone)").GetComponent<HatchScript> ().IncrementEnemyKills ();
		}
		targeting.CleanUp ();
		navigation.CleanUp ();
		GetComponent<Animator> ().enabled = false;

		GetComponentInChildren<WeaponScript> ().enabled = false;

		//all heros are dead
		bool allDead = true;

		foreach (var hero in GameObject.FindGameObjectsWithTag("Hero")) {
			if (hero != this.gameObject && !hero.GetComponent<EntityTargetScript>().enabled) {

			} else
				allDead = false;
		}

		if (allDead)
			Invoke ("PlayerDeath", 3.0f);

		this.enabled = false;
	}
}
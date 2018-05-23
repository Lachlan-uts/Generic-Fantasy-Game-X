using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityStatisticsScript : MonoBehaviour {

	public enum entitySlots { Helmet, Chestplate, Greaves, RightHand, LeftHand, Potion };
	public enum entityScripts { Navigation, Targeting, Selection };

	//All the different components that the entity statisics script controls
	[SerializeField]
	private EntityTargetScript target;
	private EntityNavigationScript navigation;
	private EntitySelectedScript selected;

	// public variables
	public GameObject nameUI; // placeholder
	public GameObject healthUI; // placeholder

	public GameObject inventoryGO;

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

	public Dictionary<entityScripts, MonoBehaviour> e = new Dictionary<entityScripts, MonoBehaviour>();

	public Dictionary<entitySlots, GameObject> equippedItems = new Dictionary<entitySlots, GameObject> ();

	public List<GameObject> inventoryItems;

	public GameObject targetThing;
	public string targetContext;

	// Use this for initialization
	void Start () {
		equippedItems.Add (entitySlots.Helmet, null);
		equippedItems.Add (entitySlots.Chestplate, null);
		equippedItems.Add (entitySlots.Greaves, null);
		equippedItems.Add (entitySlots.RightHand, null);
		equippedItems.Add (entitySlots.LeftHand, null);
		equippedItems.Add (entitySlots.Potion, null);
		//get the selection
		target = GetComponent<EntityTargetScript> ();
		navigation = GetComponent<EntityNavigationScript> ();
		selected = GetComponentInChildren<EntitySelectedScript> ();
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
			if (targetContext == "Item") {
				if (Vector3.Distance (this.gameObject.transform.position, targetThing.transform.position) < 0.2f) {
					Pickup (targetThing);
				}
			} else if (targetContext == "Furniture") {
				if (Vector3.Distance (this.gameObject.transform.position, targetThing.transform.position) < 0.2f) {
					Interact (targetThing);
				}
			}
		}
	}

	public void InstigateCommand (string context, GameObject other) {
		targetContext = context;
		targetThing = other;
		this.gameObject.GetComponent<EntityNavigationScript> ().SetDestination (targetThing.transform.position, this.gameObject);
	}

	public void Interact (GameObject other) {
		other.GetComponent<FurnitureScript> ().Interact ();
		targetThing = null;
		this.gameObject.GetComponent<EntityNavigationScript> ().CancelMovement ();
	}

	public void Pickup (GameObject other) {
		other.transform.SetParent (inventoryGO.transform);
		inventoryItems.Add (other);
		targetThing = null;
		this.gameObject.GetComponent<EntityNavigationScript> ().CancelMovement ();
	}

	public void Drop (GameObject other) {
		inventoryItems.Remove (other);
		other.transform.parent = null;
	}

	public void Quaff () {
		GameObject equippedPotion;
		if (equippedItems.TryGetValue (entitySlots.Potion, out equippedPotion)) {

		}
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

		}


	}
//	private IEnumerator IsSelected() {
//	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataCollector : MonoBehaviour {
	[SerializeField]
	private GameObject scoreScreen;

	public Text timerText;
	public Text killText;
	public Text collectableText;

	public int scoreValue;
	public int levelTimer;
	public float secondsCount;
	public float minuteCount;
	public int hourCount;
	public int killCount;

	public int collectCount;
	public int collectMax;

	public bool timeOn;

	// Use this for initialization
	void Start () {
		timeOn = true;
		//scoreScreen = GameObject.FindGameObjectWithTag("ScoreScreen");
		//ScoreScreen variables from DataCollector
		timerText = GameObject.FindGameObjectWithTag("TimerScore").GetComponent<Text> ();
		killText =  GameObject.FindGameObjectWithTag("KillScore").GetComponent<Text> ();
		collectableText = GameObject.FindGameObjectWithTag("CollectableScore").GetComponent<Text> ();


		killCount = 0;
		collectCount = 0;
		collectMax = 1;

			}

	void Awake()
	{
		

	}
	
	// Update is called once per frame
	void Update () {
		TimerOn ();
		GetScore ();

	}

	public void GetScore()
	{
		//Score Updates
		killText.text = "Enemies Defeated: " + killCount;
		collectableText.text = "Collected: " + collectCount + "/" + collectMax;
		timerText.text = "Level Completed in: " + hourCount + ":" + minuteCount + ":" + (int)secondsCount + "s";

	}


	public void TimerOn(){
		
		Debug.Log (hourCount + "h:" + minuteCount + "m:" + (int)secondsCount + "s");
		//levelTimer += Time.deltaTime;

		secondsCount += Time.deltaTime;

		if (secondsCount >= 60) {
			minuteCount++;
			secondsCount = 0;
		} else if (minuteCount >= 60) {
			hourCount++;
			minuteCount = 0;

		}
	}

	public float StopTimer()
	{
		timeOn = false;
		return secondsCount;

	}
		
	public void AddCollectableScore(int collected)
	{
		collectCount += collected;
	}

	public void AddPoints(int score)
	{
		killCount += score;
	}

	public void resetScore()
	{
		scoreValue = 0;
		levelTimer = 0;
		killCount = 0;
	}
}

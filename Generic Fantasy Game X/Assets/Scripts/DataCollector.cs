using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataCollector : MonoBehaviour {
	[SerializeField]
	private GameObject scoreScreen;
	private Text timerText;
	private Text killText;
	private Text collectableText;

	public int scoreValue;
	public int levelTimer;
	private float secondsCount;
	private float minuteCount;
	private int hourCount;
	public int killCount;

	public int collectCount;
	public int collectMax;

	public bool timeOn;

	// Use this for initialization
	void Start () {
		
		//scoreScreen = GameObject.FindGameObjectWithTag("ScoreScreen");
		timerText = GameObject.FindGameObjectWithTag("TimerScore").GetComponent<Text> ();
		killText =  GameObject.FindGameObjectWithTag("KillScore").GetComponent<Text> ();
		collectableText = GameObject.FindGameObjectWithTag("CollectableScore").GetComponent<Text> ();
	}

	void Awake()
	{
		timeOn = true;
		killCount = 0;
		collectCount = 0;
		collectMax = 1;

	}
	
	// Update is called once per frame
	void Update () {
		if (timeOn) {
			TimerOn ();
		}


	}

	public void GetScore()
	{
		KillScore ();
		CollectableScore ();
		TimeScore ();
	}
	public void TimerOn(){
		
		Debug.Log (hourCount + "h:" + minuteCount + "m:" + (int)secondsCount + "s");
		//levelTimer += Time.deltaTime;

		secondsCount += Time.deltaTime;




	}

	public float StopTimer()
	{
		timeOn = false;
		return secondsCount;

	}

	public void TimeScore()
	{

		if (secondsCount >= 60) {
			minuteCount++;
			secondsCount = 0;
		} else if (minuteCount >= 60) {
			hourCount++;
			minuteCount = 0;

		}
		timerText.text = "Level Completed in: " + hourCount + ":" + minuteCount + ":" + (int)secondsCount + "s";
	}

	public void KillScore()
	{
		killText.text = "Enemies Defeated: " + killCount;

	}

	public void CollectableScore()
	{
		collectableText.text = "Collected: " + collectCount + "/" + collectMax;
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

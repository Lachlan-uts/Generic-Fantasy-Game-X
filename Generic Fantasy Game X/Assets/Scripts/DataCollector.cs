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

	// Use this for initialization
	void Start () {
		scoreScreen = GameObject.FindGameObjectWithTag ("ScoreScreen");
		timerText = GameObject.FindGameObjectWithTag("TimerScore").GetComponent<Text> ();
		killText =  GameObject.FindGameObjectWithTag("KillScore").GetComponent<Text> ();
		collectableText = GameObject.FindGameObjectWithTag("CollectableScore").GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		TimerScore ();
		KillScore ();
		CollectableScore ();

	}

	public void TimerScore()
	{
		Debug.Log (hourCount + "h:" + minuteCount + "m:" + (int)secondsCount + "s");
		//levelTimer += Time.deltaTime;
		secondsCount += Time.deltaTime;
		timerText.text = "Level Completed in: " + hourCount + ":" + minuteCount + ":" + (int)secondsCount + "s";
		if (secondsCount >= 60) {
			minuteCount++;
			secondsCount = 0;
		} else if (minuteCount >= 60) {
			hourCount++;
			minuteCount = 0;

		}
	}

	public void KillScore()
	{
		killText.text = "Enemies Defeated: " + killCount;

	}

	public void CollectableScore()
	{
		collectableText.text = "Collected: " + collectCount + "/" + collectMax;
	}

	public void AddPoints()
	{
		killCount += 1;
	}

	public void resetScore()
	{
		scoreValue = 0;
		levelTimer = 0;
		killCount = 0;
	}
}

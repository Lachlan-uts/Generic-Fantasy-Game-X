using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataCollector : MonoBehaviour {
	//[SerializeField]
	public GameObject scoreScreen;
	public Text timerText;
	public Text killText;
	public Text collectableText;
	public Text onScreenTimeText;
	public AudioSource timerAudio;


    public int keysFound = 0;

    //StatusScreen
    public Text killStatus;
    public Text keyStatus;
    public Text objectiveText;


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
		collectMax = 3;
    }
	
	// Update is called once per frame
	void Update () {
		TimerScore ();
		KillScore ();
		CollectableScore ();
        addQuest();

	}

	void FixedUpdate(){
		//InvokeRepeating ("playTimeAudio", 1f, 100f); 
	}

	public void playTimeAudio()
	{
		timerAudio.Play ();
	}
	public void TimerScore()
	{
		Debug.Log (hourCount + "h:" + minuteCount + "m:" + (int)secondsCount + "s");
		//levelTimer += Time.deltaTime;
		secondsCount += Time.deltaTime;



		timerText.text = "Level Completed in: " + hourCount + ":" + minuteCount + ":" + (int)secondsCount + "s";
		onScreenTimeText.text = hourCount + ":" + minuteCount + ":" + (int)secondsCount + "s";

		if (secondsCount >= 60) {
			minuteCount++;
			secondsCount = 0;

		} else if (minuteCount >= 60) {
			hourCount++;
			minuteCount = 0;

		}
	}

    public void addQuest()
    {
        objectiveText.text = "Collect at least 2 keys to enter the next level";
    }
	public void KillScore()
	{
		killText.text = "Enemies Defeated: " + killCount;
        killStatus.text = killText.text;
    }

	public void CollectableScore()
	{

		collectableText.text = "Keys: " + keysFound + "/" + collectMax;
        keyStatus.text = collectableText.text;
    }

	public void AddPoints()
	{
		killCount += 1;
	}

    public void AddKey()
    {
        keysFound += 1;
    }

	public void resetScore()
	{
		scoreValue = 0;
		levelTimer = 0;
		killCount = 0;
	}
}

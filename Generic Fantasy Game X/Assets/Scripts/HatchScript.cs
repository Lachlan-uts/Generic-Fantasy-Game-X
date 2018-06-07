using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HatchScript : MonoBehaviour {

    // private variables
    private float hatchRange = 6.0f;

    private float missionObjective;
    private bool enemyCondition = false;
    private bool obtainKeyCondition = false;

    private int enemyKills;

    public static int keysFound = 0;

    //Get ScreenScore from UIManager
    //private UIManager uiManager;

    // Data Collection Setup
    private static string filePath = "Assets/AcquiredData/SessionData.txt";

    // Use this for initialization
    void Start () {

        //uiManager = GameObject.FindGameObjectWithTag ("GameManagers").GetComponent<UIManager> ();

        missionObjective = Random.Range(0, 2);

        //For the moment, we'll just make the objective to kill a certain number of enemies
        missionObjective = 0;

        //For Later when creating different objectives
        if (missionObjective == 0)
        {
            enemyCondition = true;
        }
        else if (missionObjective == 1)
        {
            obtainKeyCondition = true;
        }

        if (enemyCondition)
        {
            SetEnemyObjective(1);
        }

        if (obtainKeyCondition)
        {
            SetKeyObjective();
        }
    }
    
    // Update is called once per frame
    void Update () {
        

    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Hero")
        {
            //Debug.Log("Hero has reached Exit");
            if (CanLeave())
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    GameObject.FindGameObjectWithTag ("GameManagers").GetComponent<UIManager> ().ScoreScreen ();
                    Debug.Log("Level Complete");
                    StartCoroutine (ScoreScreenExit ());
//                    ExitLevel();
                }
            }
            else
            {
                Debug.Log("Mission Incomplete");
            }
            
        }
    }

    private bool CanLeave()
    {
        bool canLeave = false;

        if(keysFound > 1){
            canLeave = true;
        }

        return canLeave;
        //Check Objective Completion
        // bool canLeave = true;
        // if (CompleteKeyObjective() || CompleteEnemyObjective())
        // {
        //     GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
        //     foreach (GameObject hero in heroes)
        //     {
        //         if (Vector3.Distance(hero.transform.position, gameObject.transform.position) < hatchRange)
        //         {
        //             canLeave = true;
        //         }
        //     }
        // }
        // else
        // {
        //     canLeave = false;
        // }
        // return canLeave;
    }

    public void IncrementEnemyKills() {
        enemyKills++;
    }

    private IEnumerator ScoreScreenExit() {
        yield return new WaitForSeconds (2.0f);
        yield return new WaitUntil (() => !GameObject.FindGameObjectWithTag ("GameManagers").GetComponent<UIManager> ().scoreScreenStatus);
        ExitLevel();
        yield return null;
    }

    public void ExitLevel()
    {
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
        foreach (GameObject hero in heroes)
        {
            //At the moment if using this will increase one hero per scene load
            //DontDestroyOnLoad(hero);
        }

        // Data Collection
        StreamWriter writer = new StreamWriter(filePath, true);
        string objectiveString = "";
        if (enemyCondition) {
            objectiveString += "Kill Enemies";
        } else {
            objectiveString += "Find Key";
        }
        writer.WriteLine ("Floor: " + GameObject.Find("LevelGenerator").GetComponent<LevelGenerationScript>().floorNumber 
            + " Objective: " + objectiveString 
            + " Enemy Kills: " + enemyKills
            + " Total Enemies: " + GameObject.Find("LevelGenerator").GetComponent<LevelGenerationScript>().GetEnemyCount());
        writer.Close ();

        // All done on Data Collection

        //Load Next Level
        GameObject gameManager = GameObject.Find("GameManagers");
        SceneController sceneLoader = gameManager.GetComponent<SceneController>();
        Debug.Log(sceneLoader);
        sceneLoader.LoadNextLevel();
        //Create End Screen
    }

    void CheckObjective()
    {

    }

    private void SetEnemyObjective(int i)
    {
        //Kill No. Of Enemies based on i value
    }

    private bool CompleteEnemyObjective()
    {
        return true;
    }

    private void SetKeyObjective()
    {
        //Deliver Key to Exit Room
        //Check if Team has Key
    }

    private bool CompleteKeyObjective()
    {
        // if(keysFound > 1){
        //     return true;
        // }else{
        //     return false;
        // }
        return false;
    }
}

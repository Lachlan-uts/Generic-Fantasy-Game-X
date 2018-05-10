using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatchScript : MonoBehaviour {

    // private variables
	private float hatchRange = 6.0f;

    private float missionObjective;
    private bool enemyCondition = false;
    private bool obtainKeyCondition = false;

    private int enemyKills;

	// Use this for initialization
	void Start () {
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

        Debug.Log(missionObjective);
        Debug.Log(enemyCondition);
        Debug.Log(obtainKeyCondition);
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
                    Debug.Log("Level Complete");
                    ExitLevel();
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
        //Check Objective Completion
        bool canLeave = true;
        if (CompleteKeyObjective() || CompleteEnemyObjective())
        {
            GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
            foreach (GameObject hero in heroes)
            {
                if (Vector3.Distance(hero.transform.position, gameObject.transform.position) > hatchRange)
                {
                    canLeave = false;
                }
            }
        }
        else
        {
            canLeave = false;
        }
        return canLeave;
    }

    public void ExitLevel()
    {
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
        foreach (GameObject hero in heroes)
        {
            //At the moment if using this will increase one hero per scene load
            //DontDestroyOnLoad(hero);
        }

        //Load Next Level
        GameObject gameManager = GameObject.Find("GameManagers");
        LevelManager sceneLoader = gameManager.GetComponent<LevelManager>();
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
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour {
	public static GameLogic gameInstance;
	public Transform maxLeft;
	public Transform maxRight;
	public Transform maxDown;
	public Transform maxUp;
	public Transform shipGameLine;
	public GameObject asteroid;
	public GameObject[] patterns;

	void Start()
	{
		gameInstance = this;
		Reset();
    StartAttractMode();
	}
	void Reset()
	{
		foreach(Ship ship in GameObject.FindObjectsOfType<Ship>())
		{
			ship.ResetShip();
		}
	}
	public void SpawnAsteroid()
	{
		GameObject patternHolder = Instantiate(patterns[Random.Range(0,patterns.Length)], Vector3.zero, Quaternion.identity);
		Transform plotter = patternHolder.GetComponentInChildren<Plotter>().transform;
		GameObject asteroid = GameObjectPooler.Get(this.asteroid, plotter.position, Quaternion.identity);
		asteroid.transform.parent = plotter;
	}
	void StartAttractMode()
	{
		foreach(var ship in GameObject.FindObjectsOfType<Ship>())
		{
			ship.StartAttractMode();
		}
		SpawnAsteroid();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour {
	public int playerId = 0; // The Rewired player id of this character
	public float moveSpeed = 3.0f;
	public float bulletSpeed = 15.0f;
	public GameObject bulletPrefab;
	bool initialized;
	[SerializeField]
	Vector3 moveVector;
	public Animator animator;
	bool fire;
	public Transform muzzle;
	bool canbank;
	string currentlyPlaying;
	public enum SHIPSTATE
	{
		hidden,
		enter,
		ready,
		active,
		activeAI,
		exit,
		idle,
	}
	public SHIPSTATE shipState;
	public bool changingState;
	bool verticalMove;
	public Vector2 startPosition;
	public bool inPlay;
	public Material originalMaterial;
	public Material flash;
	SpriteRenderer spriteRenderer;
	public bool canShoot;

	void Initialize() {
		initialized = true;
		spriteRenderer = GetComponent<SpriteRenderer>();
		originalMaterial = spriteRenderer.material;
		canShoot = true;
	}
	void Update() {
		if(!initialized) Initialize(); // Reinitialize after a recompile in the editor

		ProcessInput();

			if (moveVector.x < 0 && canbank)
			{
				if (currentlyPlaying != "left")
				{
//					animator.Play("bank left");
					currentlyPlaying = "left";
				}
			}
			else if (moveVector.x > 0 && canbank)
			{
				if (currentlyPlaying != "right")
				{
//					animator.Play("bank right");
					currentlyPlaying = "right";
				}
			}
			else
			{
				if (currentlyPlaying != "idle")
				{
//					animator.Play("idle");
					currentlyPlaying = "idle";
				}
			}
	}
	IEnumerator RandomMove()
	{
		moveVector.x = Random.Range(-1,2);
		yield return new WaitForSeconds(Random.Range(0.1f,1f));
		if (shipState == SHIPSTATE.activeAI)
		{
			StartCoroutine( RandomMove());
		}
	}
	IEnumerator RandomShoot()
	{
		Shoot();
		yield return new WaitForSeconds(Random.Range(0.025f,1f));
		if (shipState == SHIPSTATE.activeAI)
		{
			StartCoroutine( RandomShoot());
		}
		
	}
	public void ResetShip()
	{
		StopAllCoroutines();
		transform.position = startPosition;
		moveVector = Vector3.zero;
		inPlay = false;
		changingState = false;
		canShoot = true;
	}
	void ProcessInput() {
		if(moveVector.x != 0.0f) {
			if (moveVector.x < 0 && transform.position.x < GameLogic.gameInstance.maxLeft.position.x)
			{
				canbank = false;
			}
			else if (moveVector.x > 0 && transform.position.x > GameLogic.gameInstance.maxRight.position.x)
			{
				canbank = false;
			}
			else
			{
				canbank = true;
				transform.Translate(moveVector * moveSpeed * Time.deltaTime, 0);
			}
		}
	}
	void Shoot()
	{
		if (!canShoot) return;
		GameObject bullet = GameObjectPooler.Get(bulletPrefab, muzzle.position, Quaternion.identity);
		bullet.GetComponent<Rigidbody2D>().AddForce(transform.up * bulletSpeed);
		GameObjectPooler.Destroy(bullet, 2);
		canShoot = false;
		StartCoroutine(ShootPacer());
	}
	IEnumerator ShootPacer()
	{
		yield return new WaitForSeconds(0.08f);
		canShoot = true;
	}
	public void StartAttractMode()
	{
		canShoot = true;
		transform.position = new Vector2(transform.position.x, GameLogic.gameInstance.shipGameLine.position.y);
		shipState = SHIPSTATE.activeAI;
		StartCoroutine(RandomMove());
    string[] args = System.Environment.GetCommandLineArgs();
    int result = System.Array.FindIndex(args, s => s.Equals("--shoot", System.StringComparison.OrdinalIgnoreCase));
    if (result >= 0) {
  		StartCoroutine(RandomShoot());
    }
	}
	
}

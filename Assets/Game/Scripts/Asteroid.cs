using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour {

	CircleCollider2D circleCollider;

  void Start()
  {
		circleCollider = GetComponent<CircleCollider2D>();
  	circleCollider.enabled = true;
	}
  
}

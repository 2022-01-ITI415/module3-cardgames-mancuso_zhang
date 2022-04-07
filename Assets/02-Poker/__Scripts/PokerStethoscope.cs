using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerStethoscope : MonoBehaviour
{
    public Vector3 mousePosition;
	// Use this for initialization
	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update()
	{
		mousePosition = Input.mousePosition;
	}
}

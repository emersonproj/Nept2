using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class BouncyShoot : MonoBehaviour
{


    	public static readonly Color colorZero = Color.clear;
        public static List<BallClass> balls;

        public GameObject BallInstYesPrefab;
        public GameObject BallInstNoPrefab;

        public GameObject spherePrefab;

        public Vector3 mousePos;

        private bool ballOnCool;

		public Camera camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        	balls = new List<BallClass>();
            BallInstYesPrefab = spherePrefab;

			 camera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // next: figure out how to do "is currently pressed down"
        if (Keyboard.current.fKey.isPressed)
            {


                if (!ballOnCool)
			    {
				
				StartCoroutine(ballCD(Static.ballCoolTime));
			    }
            }

            // mousePos = GetComponent<Camera>().ScreenToWorldPoint(new Vector3 (Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z + 1));

			Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

			Vector3 mousePos = camera.ScreenToWorldPoint(
				new Vector3(mouseScreenPos.x, mouseScreenPos.y, 1f)
			);
		
		    transform.LookAt(mousePos);
 
			// if (!ballOnCool)
			// {
				
			// 	StartCoroutine(ballCD(Static.ballCoolTime));
			// }
        
    }


    void FixedUpdate()
    {
        addBallForces();
    }

	public void addBallForces()
	{
		foreach (BallClass ball in balls)
		{
			ball.rB.AddForce(ball.totalForceToAdd);
			ball.totalForceToAdd = Vector3.zero;
		}
	}



    public BallClass ballFire(GameObject shootingObj, Vector3 addedVect, Color color)
	{
		//audio.Play ();
//		print (mousePos);
		//get camera position, add something to it in forward direction, then add the x and y position in respect to that.

		
		//HOTFIX CHANGE THIS BACK: THIS MAKES IT SO ALL COLORS ARE WHITE
		//if (color != Color.white)
		{
		//	color = Color.white;
		}


		GameObject newBall;
		//true is here to always make it a gpu instanced no-color ball because the images he sent
		//were not true black 
		if (color == Color.clear  || color == colorZero || color == Color.black)
		{
			newBall = Instantiate(BallInstYesPrefab, shootingObj.transform.position + addedVect, Quaternion.identity);
		}
		else
		{
			newBall = Instantiate(BallInstNoPrefab, shootingObj.transform.position + addedVect, Quaternion.identity);
		}
		BallClass ballClass = new BallClass();
		ballClass.ball = newBall;
		ballClass.bS = newBall.GetComponent<BallScript>();
		ballClass.rB = newBall.GetComponent<Rigidbody>();
		ballClass.relativeStartPos = addedVect;
		
		balls.Add(ballClass);

		
		//false is here to always make it a gpu instanced no-color ball because the images he sent
		//were not true black. up there its true and here its false because the statements ar ereverse
		if (color != colorZero  && color != Color.black)
		{
			newBall.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
		}
		else
		{
		//check box for this
			//obj.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
		}
		if (Static.velocityDirectToggle)
		{
			ballClass.rB.linearVelocity = shootingObj.transform.forward * Static.ballVelocity;
		}
		else
		{
			Vector3 forceAddedVect = shootingObj.transform.forward * Static.ballForce;
			ballClass.totalForceToAdd += forceAddedVect;
		}
		// int randomSound = Random.Range(0, bounceSounds.Length - 1);
		//obj.GetComponent<AudioSource>().clip = bounceSounds[randomSound];
		newBall.transform.localScale = new Vector3(Static.ballSize * newBall.transform.localScale.x, Static.ballSize * newBall.transform.localScale.y, Static.ballSize * newBall.transform.localScale.z);

		return ballClass;

	}



    	public IEnumerator ballCD(float cdTime)
	{
		ballOnCool = true;
		ballFire(this.gameObject, Vector3.zero, colorZero);
		yield return new WaitForSeconds(cdTime);
		ballOnCool = false;
	}
}

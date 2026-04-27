using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BouncyShoot : MonoBehaviour
{


    	public static readonly Color colorZero = Color.clear;
        public static List<BallClass> balls;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        	balls = new List<BallClass>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

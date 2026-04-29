using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;



//going to put the color changing in a really crappy way
public class BallScript : MonoBehaviour
{
	private AudioSource audio;
	private MeshRenderer meshR;
	private Color originalColor;
	private float startTime;
	private bool colorChangingOn;
	
	private float colorChangeSpeed = 1f;
	private Color red = new Color(0.64f, 0, 0);
	Color green = new Color(0, 0.47f, 0);
	Color blue = new Color(0, 0, 0.96f);
	private Color violet = new Color(0.3f, 0, 0.96f);
	// Use this for initialization
	void Start ()
	{
		audio = GetComponent<AudioSource>();
		meshR = GetComponent<MeshRenderer>();
		startTime = Time.time;
	}
	
	// Update is called once per frame
	/*void Update () {
		
		
		if (Vector3.Distance(transform.position, Vector3.zero) > 3000)
		{
			
			destroyBall();
		}

		if (colorChangingOn)
		{
			
				float t = Mathf.Abs(Mathf.Sin(Time.time * .025f - startTime) * colorChangeSpeed);
				
				meshR.material.color = Color.Lerp(violet, blue, t);

				
			}
			
		
	}*/

	private void destroyBall()
	{
		BallClass ball = BouncyShoot.balls.FirstOrDefault(b => b.ball == gameObject);
		BouncyShoot.balls.Remove(ball); 
		Debug.Log("Destroyed");
		GameObject.Destroy(gameObject);
	}

	public void colorChangeToggle()
	{
		
		colorChangingOn = !colorChangingOn;
		
	}
}

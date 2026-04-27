using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Static 
{

	public static float ballSize = 1;
	public static float ballForce = 500;
	public static float ballSideForce = 200;
	public static float returnForce = 1;
	public static float bounciness = 1;
	public static float friction = 0;
	public static float ballVelocity = 20;
	public static float ballMass = 1;
	public static float wallZPos = 130;
	
	public static int matrixD1 = 50;
	public static int matrixD2 = 50;
	public static int matrixD3 = 1;
	
	public static int bHmatrixD1 = 1;
	public static int bHmatrixD2 = 1;
	public static int bHmatrixD3 = 1;

	public static int ballColorR = 100;
	public static int ballColorG = 100;
	public static int ballColorB = 100;
	public static float ballColorA = 1;
	public static float ballMetallic = 1;
	public static float ballSmoothness = 1;
	public static float blackOpacity = .05f;
	
	public static float ballSeperatness = 8;
	public static float bHSeparateness = 10;

	public static float gAddedPre = 0;
	public static float gAddedPost = 0;
	public static float gMult = 30;
	public static float gExp = 2;
	public static float bDist = 50;
	public static float bHIncrement;
	public static float camHeight = 0;
	public static float ballCoolTime = 1;
	public static float timeStep = 1;

	public static bool velocityDirectToggle;

	public static bool toggleWalls;
	public static bool toggleGravity;
	public static bool toggleCollision;
	public static bool toggleBHGravity = true;
	public static bool toggleMountains;
	public static bool toggleOldMath = false;
	public static bool massOverride = true;
	public static bool expOverride = true;
	public static bool toggleSuperMode = false;
	public static bool toggleSolidColor = false;

	public static float blackHoleRadius = 300;
	//public static float Game
	
	//Image Data
	public static int imgIndex;
	public static Texture2D currentImage;
	public static int imageDivideBy = 4;
	public static int currentBallShape;

	//model Data
	public static int modelIndex;

	public static int vertModelIndex;


	//public static float distance = 10;
}

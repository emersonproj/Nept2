using UnityEngine;

namespace DefaultNamespace
{
    public class BallClass
    {

        public GameObject ball;
        public Rigidbody rB;
        public BallScript bS;
        public Vector3 relativeStartPos;
        public Vector3 relativePosToCenter;
        public Vector3 totalForceToAdd = Vector3.zero;
        
    }
}
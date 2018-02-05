using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;

namespace AltSrc.ProceduralCity.Example01
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        protected float zoomFactor = 1200f;

        [SerializeField]
        protected bool zoomReverse = false;

        [SerializeField]
        protected float movementFactor = 30f;

        protected void Update()
        {
            float horizontalAxis = Input.GetAxisRaw("Horizontal");
            float scrollWheelAxis = -Input.GetAxis("Mouse ScrollWheel");
            float verticalAxis = Input.GetAxisRaw("Vertical");

            transform.position += new Vector3(
                horizontalAxis * movementFactor, 
                scrollWheelAxis * zoomFactor * (zoomReverse ? -1f : 1f), 
                verticalAxis * movementFactor);
        }
    }
}

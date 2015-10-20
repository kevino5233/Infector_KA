using UnityEngine;
using System.Collections;

public class CameraControlScript : MonoBehaviour {

	private float deadzone = .1f;
	private float MAX_VEL = .2f;

	private uint frame_counter = 0;

	// Update is called once per frame
	void Update () {
		frame_counter++;
		if (frame_counter % 2 == 0){
			Vector3 viewport = Camera.main.ScreenToViewportPoint(Input.mousePosition);
			Vector3 cam_vel = Vector3.zero;
			if (viewport.y < deadzone){
				cam_vel += Vector3.down;
			} else if (viewport.y > 1 - deadzone){
				cam_vel += Vector3.up;
			}
			if (viewport.x < deadzone){
				cam_vel += Vector3.left;
			} else if (viewport.x > 1 - deadzone){
				cam_vel += Vector3.right;
			}
			transform.position += cam_vel * MAX_VEL;
		}
	}
}

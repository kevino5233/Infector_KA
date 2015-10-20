using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class User : MonoBehaviour, IComparable<User>{

	// User's unique ID.
	private int user_id;
	public int UID {
		get {
			return user_id;
		}
		set {
			user_id = value;
		}
	}
	// User's coach's ID, -1 if this user does not have a coach.
	private int coach_id;
	public int Coach {
		get {
			return coach_id;
		}
		set {
			coach_id = value;
			UpdateTexture();
		}
	}
	// Unique ID for feature run on this user.
	private int feature_id;
	public int Feature {
		get {
			return feature_id;
		}
		set {
			feature_id = value;
			UpdateTexture();
		}
	}

	// Distance from arbitrary center node;
	private int depth;
	public int Depth {
		get {
			return depth;
		}
		set {
			depth = value;
		}
	}

	public Vector3 Position {
		get {
			return transform.position;
		}
	}
    // used to draw the arrow points
    private int theta_prime = 5;
    private float arrow_len = .5f;
    // Sets this user's position in the scene and draws the line to
    // their coach, if they exist.
    // Spaghettios.
    public void SetPosition(Vector3 origin, float length, int angle){
        float cos =	Mathf.Cos(Mathf.Deg2Rad * angle);
        float sin =	Mathf.Sin(Mathf.Deg2Rad * angle);
        Vector3 unit = new Vector3(cos, sin, 0);
        Vector3 offset = unit * length;
        Vector3 newpos = origin + offset;
        transform.position = newpos;
        if (coach_id != -1){
            float cos_prime_1 = Mathf.Cos(Mathf.Deg2Rad * (angle - theta_prime));
            float cos_prime_2 = Mathf.Cos(Mathf.Deg2Rad * (angle + theta_prime));
            float sin_prime_1 = Mathf.Sin(Mathf.Deg2Rad * (angle - theta_prime));
            float sin_prime_2 = Mathf.Sin(Mathf.Deg2Rad * (angle + theta_prime));
            Vector3 unit_prime_1 =
                new Vector3(cos_prime_1, sin_prime_1, 0);
            Vector3 unit_prime_2 =
                new Vector3(cos_prime_2, sin_prime_2, 0);
            Vector3 arrow1 = unit_prime_1 * (length - arrow_len);
            arrow1 += origin;
            Vector3 arrow2 = unit_prime_2 * (length - arrow_len);
            arrow2 += origin;
            GetComponent<LineRenderer>().SetPosition(0, origin);
            GetComponent<LineRenderer>().SetPosition(1, newpos);
            GetComponent<LineRenderer>().SetPosition(2, arrow1);
            GetComponent<LineRenderer>().SetPosition(3, newpos);
            GetComponent<LineRenderer>().SetPosition(4, arrow2);
        }
    }

	// Contains UIDs of this user's students. This will be empty if
	// this user is not a coach.
	private List<int> students;
	public List<int> Students {
		get {
			return students;
		}
	}

	// Hueristic for determing infection.
	public int Priority {
		get {
			return students.Count;
		}
	}

	public void Awake(){
		user_id = -1;
		coach_id = -1;
		feature_id = 0;
		depth = 0;
		students = new List<int>();
	}

	public void AddStudent(int uid){
		students.Add(uid);
		UpdateTexture();
	}

	private void UpdateTexture(){
		GetComponent<SpriteRenderer>().color = UserManager.instance.GetColor(feature_id); 
		if (students.Count == 0){
			GetComponent<SpriteRenderer>().sprite = 
				UserManager.instance.sprite_student;
		} else {
			GetComponent<SpriteRenderer>().sprite = 
				UserManager.instance.sprite_coach;
		}
	}

	public int CompareTo(User other){
		return this.Priority.CompareTo(other.Priority);
	}
}

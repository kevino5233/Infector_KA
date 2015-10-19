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
		set {
			transform.position = value;
			if (coach_id != -1){
				GetComponent<LineRenderer>().SetPosition(0, value);
				GetComponent<LineRenderer>().SetPosition(
					1,
					UserManager.instance.GetUser(coach_id).transform.position);
			}
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

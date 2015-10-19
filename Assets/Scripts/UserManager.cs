using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UserManager : MonoBehaviour {
	public static UserManager instance = null;

	public Transform user_trans;

	public Sprite sprite_student;
	public Sprite sprite_coach;

	private Color[] colors = 
		{Color.cyan, Color.gray, Color.green, 
		Color.magenta, Color.white, Color.yellow};

	private enum TraverseModes {
		Initial,
		Infect,
		Render
	}

	private int feature_count = 0;
	private int center = -1;
	private int max_depth = -1;
	private int angle = 0;
	private int angle_d = 44;
	private float length_scale = 5f;

	private bool busy = false;
	public bool Busy {
		get {
			return busy;
		}
	}
	
	private Dictionary<int, User> users;

	private List<int> independent_users;

	public void Awake() {
		users = new Dictionary<int, User>();
		independent_users = new List<int>();
		if (instance == null){
			instance = this;
		} else if (instance != this){
			Destroy(this.gameObject);
		}
		DontDestroyOnLoad(this.gameObject);
	}

	public Color GetColor(int index){
		return colors[index %colors.Length];
	}

	public User GetUser(int uid){
		return users[uid];
	}

	// Adds users. Prints logs if user already exists.
	public void AddUser(int uid){
		users[uid] = 
			((Transform)Instantiate(user_trans)).gameObject.GetComponent<User>();
		users[uid].UID = uid;
		independent_users.Add(uid);
	}

	// Creates student-teacher relation between two users.
	public void SetStudent(int uid, int cid){
		if (uid == cid){
			Debug.Log("User can not be their own coach.");
			return;
		}
		users[uid].Coach = cid;
		users[cid].AddStudent(uid);
		if (center == -1 ||
				(cid != center &&
				users[cid].Priority > users[center].Priority)){
			center = cid;
		}
		independent_users.Remove(uid);
	}

	// read from JSON file
	public void Load(){
	}

	private void TraverseGraph(TraverseModes mode){
		// set this later
		bool limited = false;
		Queue<User> bfs_queue = new Queue<User>();
		float ind_dist = Mathf.Pow(2, max_depth) * length_scale;
		foreach (int uid in independent_users){
			float offset_x = Mathf.Cos(Mathf.Deg2Rad * angle) * ind_dist;
			float offset_y = Mathf.Sin(Mathf.Deg2Rad * angle) * ind_dist;
			users[uid].transform.position = new Vector3(offset_x, offset_y, -5);
			bfs_queue.Enqueue(users[uid]);
			angle += angle_d;
			angle %= 360;
		}
		float[] powers = null;
		if (mode == TraverseModes.Render){
			powers = new float[max_depth];
			for (int i = 0; i < max_depth; i++){
				powers[max_depth - 1 - i] = Mathf.Pow(2, i);
			}
		}
		busy = true;
		while (bfs_queue.Count > 0){
			User curr = bfs_queue.Dequeue();
			curr.Feature = feature_count;
			List<User> students = new List<User>();
			foreach(int uid in curr.Students){
				students.Add(users[uid]);
			}
			if (limited){
				students.Sort();
			}
			foreach (User user in students){
				if (mode == TraverseModes.Initial){
					user.Depth = curr.Depth + 1;
					max_depth = Mathf.Max(max_depth, user.Depth);
				} else if (mode == TraverseModes.Render){
					float offset_x = 
						length_scale *
						powers[user.Depth - 1] *
						Mathf.Cos(Mathf.Deg2Rad * angle);
					float offset_y =
						length_scale *
						powers[user.Depth - 1] *
						Mathf.Sin(Mathf.Deg2Rad * angle);
					Vector3 offset = new Vector3(offset_x, offset_y, 0);
					Vector3 origin = users[user.Coach].transform.position;
					user.Position = offset + origin;
					angle += angle_d;
					angle %= 360;
				}
				bfs_queue.Enqueue(user);
			}
		}
		busy = false;
	}

	public void RenderGraph(){
		// Calculate max_depth of tree
		TraverseGraph(TraverseModes.Initial);
		// Reorganizes users
		TraverseGraph(TraverseModes.Render);
		Vector3 cam_pos = Camera.main.transform.position;
		Vector3 user_pos = users[independent_users[0]].transform.position;
		Vector3 new_pos = new Vector3(user_pos.x, user_pos.y, cam_pos.z);
		Camera.main.transform.position = new_pos;
	}

	public void StartInfection(int patient_zero){
		TraverseGraph(TraverseModes.Infect);
		feature_count++;
	}

}

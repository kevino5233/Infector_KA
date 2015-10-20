using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class UserManager : MonoBehaviour {
	public static UserManager instance = null;

    public Text text_quota;

	public Transform user_trans;

	public Sprite sprite_student;
	public Sprite sprite_coach;

    private bool infect_limited = false;
    private bool infect_quota = false;
    private int quota = -1;

    private int jump_count = 0;

	private Color[] colors = 
		{Color.cyan, Color.red, Color.green,
		Color.magenta, Color.blue, Color.yellow};

	private enum TraverseModes {
		Initial,
		Render,
		Infect
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
    public void ToggleLimited(){
        infect_limited = !infect_limited;
    }
    public void ToggleQuota(){
        infect_quota = !infect_quota;
    }
    public void SetQuota(){
        try {
            quota = Int32.Parse(text_quota.text);
        } catch (Exception e){
            Debug.Log("Quota must be an integer");
        }
    }
    // spaghettios.
	private void TraverseGraph(TraverseModes mode, int uid_start){
        Dictionary<int, bool> visited = new Dictionary<int, bool>();
        float ind_dist = Mathf.Pow(2, max_depth + 1) * length_scale;
		Queue<User> bfs_queue = new Queue<User>();
        foreach (int uid in independent_users){
            if (mode == TraverseModes.Render){
                users[uid].SetPosition(new Vector3(0, 0, -5), ind_dist, angle);
                angle += angle_d;
                angle %= 360;
            } else if (mode != TraverseModes.Initial){
                bfs_queue.Enqueue(users[uid_start]);
                break;
            }
            bfs_queue.Enqueue(users[uid]);
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
            if (visited.ContainsKey(curr.UID) && visited[curr.UID]){
                continue;
            }
            visited[curr.UID] = true;
            if (infect_limited){
                int old = curr.Feature;
                curr.Feature = feature_count;
                if (curr.Coach != -1){
                    if(curr.Priority > 0 &&
                        users[curr.Coach].Feature == feature_count){
                        curr.Feature = old;
                    } else if (curr.Priority == 0){
                        curr.Feature = users[curr.Coach].Feature;
                    }
                }
            } else {
                curr.Feature = feature_count;
            }
            if (!(curr.Coach == -1 ||
                    (visited.ContainsKey(curr.Coach) && visited[curr.Coach]))){
                bfs_queue.Enqueue(users[curr.Coach]);
            }
            if (curr.Priority == 0){
                continue;
            }
			List<User> students = new List<User>();
			foreach(int uid in curr.Students){
				students.Add(users[uid]);
			}
			foreach (User user in students){
				if (mode == TraverseModes.Initial){
					user.Depth = curr.Depth + 1;
					max_depth = Mathf.Max(max_depth, user.Depth);
				} else if (mode == TraverseModes.Render){
                    float length = length_scale * powers[user.Depth - 1];
                    Vector3 origin = users[user.Coach].Position;
                    user.SetPosition(origin, length, angle);
					angle += angle_d;
					angle %= 360;
				}
				if (!(visited.ContainsKey(user.UID) && visited[user.UID])){
                    bfs_queue.Enqueue(user);
                }
			}
		}
		busy = false;
	}

    public void Jump(){
		Vector3 cam_pos = Camera.main.transform.position;
		Vector3 user_pos = 
            users[independent_users[jump_count]].transform.position;
		Vector3 new_pos = new Vector3(user_pos.x, user_pos.y, cam_pos.z);
		Camera.main.transform.position = new_pos;
        jump_count++;
        jump_count %= independent_users.Count;
    }

	public void RenderGraph(){
		// Calculate max_depth of tree
		TraverseGraph(TraverseModes.Initial, -1);
		// Reorganizes users
		TraverseGraph(TraverseModes.Render, -1);
        Jump();
	}

    public void Infect(int uid){
        if (infect_limited && infect_quota && quota == -1){
            return;
        }
        ++feature_count;
        TraverseGraph(TraverseModes.Infect, uid);
    }
}

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class UserManager : MonoBehaviour {
    // Static instance of the User Manager that will be accessible
    // across classes.
	public static UserManager instance = null;

    // Text UI object that contains the quota.
    public Text text_quota;

    // Transform of User prefab.
	public Transform user_trans;

    // Sprites for student and coach users.
	public Sprite sprite_student;
	public Sprite sprite_coach;

    // Infection modes. Quota not implemented.
    private bool infect_limited = false;
    private bool infect_quota = false;
    private int quota = -1;

    // Tracks which graph to jump to.
    private int jump_count = 0;

    // Colors of the users, represents version of the software.
	private Color[] colors = 
		{Color.cyan, Color.red, Color.green,
		Color.magenta, Color.blue, Color.yellow};

    // Differing traverse modes. This will affect the behavior of
    // TraverseGraph()
	private enum TraverseModes {
		Initial,
		Render,
		Infect
	}

    // Which feature we are currently deploying.
	private int feature_count = 0;
    // User with the highest "Priority" (number of students)
	private int center = -1;
    // Farthest distance from the center.
	private int max_depth = -1;
    // Angle to move the Users in the visualization.
	private int angle = 0;
    // Increment angle by this much every time we move a User.
	private int angle_d = 44;
    // Multiply the determined length of a line by this much.
	private float length_scale = 5f;
    // Currently traversing graph.
	private bool busy = false;
	public bool Busy {
		get {
			return busy;
		}
	}
    // Map of users. Maps unique IDs to Users.
	private Dictionary<int, User> users;
    // Users with no coaches. Denotes entry points for independent graphs.
	private List<int> independent_users;
    // Initialization. Creates static instance of Usermanager. Makes
    // sure only one exists at a given time during the application.
	public void Awake() {
		if (instance == null){
			instance = this;
		} else if (instance != this){
			Destroy(this.gameObject);
		}
		DontDestroyOnLoad(this.gameObject);
		users = new Dictionary<int, User>();
		independent_users = new List<int>();
	}
    // Gets a color for the given index. Index generally represents
    // feature number.
	public Color GetColor(int index){
		return colors[index % colors.Length];
	}
    // Gets a user by UID.
	public User GetUser(int uid){
		return users[uid];
	}

	// Adds user and instantiates it using User prefab.
	public void AddUser(int uid){
		users[uid] = 
			((Transform)Instantiate(
                user_trans)).gameObject.GetComponent<User>();
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
    // Toggle traverse modes.
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
    // Traverses graph with render mode and start node. If the mode is
    // Initial or Render, then the algorithm traverses all nodes. The
    // traversal algorithm is BFS and takes care of graph depth, feature
    // implementation, and positioning of users in the camera view.
    //
    // spaghettios.
	private void TraverseGraph(TraverseModes mode, int uid_start){
        // Tracks whether a node has been visited.
        Dictionary<int, bool> visited = new Dictionary<int, bool>();
        // Used if we are in Render mode. Initial placing of
        // graph entry points.
        float ind_dist = Mathf.Pow(2, max_depth + 1) * length_scale;
        // Queue for BFS.
		Queue<User> bfs_queue = new Queue<User>();
        // Enqueues all graph entry points if we are in Initial or Render
        // mode, else we only enqueue users[uid_start]
        foreach (int uid in independent_users){
            if (mode == TraverseModes.Render){
                users[uid].SetPosition(
                        new Vector3(0, 0, -5), ind_dist, angle);
                angle += angle_d;
                angle %= 360;
            } else if (mode != TraverseModes.Initial){
                bfs_queue.Enqueue(users[uid_start]);
                break;
            }
            bfs_queue.Enqueue(users[uid]);
        }
        // Used if we are in Render mode for placing the Users. The deeper
        // a User is in the Graph, the shorter it's distance between other
        // Users of the same depth is.
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
            // Already visited.
            if (visited.ContainsKey(curr.UID) && visited[curr.UID]){
                continue;
            }
            visited[curr.UID] = true;
            if (infect_limited){
                // Store former feature.
                int old = curr.Feature;
                curr.Feature = feature_count;
                // If this User is a coach and this User's coach uses
                // the current feature_count, then we restore the old
                // feature for this user.
                //
                // If the user is only a student, set it's feature to
                // their Coach's feature.
                if (curr.Coach != -1){
                    if(curr.Priority > 0 &&
                        users[curr.Coach].Feature == feature_count){
                        curr.Feature = old;
                    }
                    // Enqueue Coach if not visited yet.
                    // Prioritized over this User's students.
                    // Use Coach's feature only if we have already
                    // visited the Coach
                    if (!(visited.ContainsKey(curr.Coach) &&
                            visited[curr.Coach])){
                        bfs_queue.Enqueue(users[curr.Coach]);
                    } else if (curr.Priority == 0){
                        curr.Feature = users[curr.Coach].Feature;
                    }
                }
            } else {
                // Total infection
                curr.Feature = feature_count;
                // Mom's spaghetti.
                if (curr.Coach != -1 &&
                        !(visited.ContainsKey(curr.Coach) &&
                        visited[curr.Coach])){
                    bfs_queue.Enqueue(users[curr.Coach]);
                }
            }
            // Continue if this User has no other students.
            if (curr.Priority == 0){
                continue;
            }
            // Enqueues users if they haven't been visited yet.
            // Sets their position if we are in Render mode,
            // calculating depth if we are in Initial mode.
			foreach (int uid in curr.Students){
                User user = users[uid];
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
    // Jumps to a graph entry point. Increments the jump counter.
    public void Jump(){
		Vector3 cam_pos = Camera.main.transform.position;
		Vector3 user_pos = 
            users[independent_users[jump_count]].transform.position;
		Vector3 new_pos = new Vector3(user_pos.x, user_pos.y, cam_pos.z);
		Camera.main.transform.position = new_pos;
        jump_count++;
        jump_count %= independent_users.Count;
    }
    // Runs TraverseGraph in Initial mode, then Render mode. Jumps to
    // the first graph entry point.
	public void RenderGraph(){
		// Calculate max_depth of tree
		TraverseGraph(TraverseModes.Initial, -1);
		// Reorganizes users
		TraverseGraph(TraverseModes.Render, -1);
        Jump();
	}
    // Calls TraverseGraph in Infect mode with starting location uid.
    // Increments feature count before TraverseGraph so we can roll
    // out a new feature.
    public void Infect(int uid){
        if (infect_limited && infect_quota && quota == -1){
            return;
        }
        ++feature_count;
        TraverseGraph(TraverseModes.Infect, uid);
    }
}

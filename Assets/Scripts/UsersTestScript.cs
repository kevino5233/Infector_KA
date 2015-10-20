using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class UsersTestScript : MonoBehaviour {

    private Dictionary<int, bool> users;
    private JSONObject file_data;

    // instantiate stuff;
    void Awake(){
        users = new Dictionary<int, bool>();
    }

	// Update is called once per frame. Destroy this
    // after the test case is created because it
    // is no longer needed.
	void Update () {
		if (UserManager.instance){
			InstantiateTest();
			Destroy(this.gameObject);
		}
	}
    // Loads json file data. Returns whether success or not.
    bool Load(string filename){
        try {
            string line;
            string data = "";
            StreamReader reader = new StreamReader(filename, Encoding.Default);
            using(reader){
                line = reader.ReadLine();
                while (line != null){
                    data += line;
                    line = reader.ReadLine();
                }
                reader.Close();
                file_data = new JSONObject(data);
                return true;
            }
        } catch (Exception e) {
            return false;
        }
    }
    // Default test case if load fails.
	void LoadDefault(){
		UserManager.instance.AddUser(1);
		UserManager.instance.AddUser(2);
		UserManager.instance.AddUser(3);
		UserManager.instance.AddUser(4);
		UserManager.instance.AddUser(5);
		UserManager.instance.AddUser(6);
		UserManager.instance.AddUser(7);
		UserManager.instance.AddUser(8);
		UserManager.instance.AddUser(9);
		UserManager.instance.AddUser(10);
		UserManager.instance.SetStudent(2, 1);
		UserManager.instance.SetStudent(6, 2);
		UserManager.instance.SetStudent(7, 2);
		UserManager.instance.SetStudent(3, 1);
		UserManager.instance.SetStudent(4, 1);
		UserManager.instance.SetStudent(8, 4);
		UserManager.instance.SetStudent(5, 1);
		UserManager.instance.SetStudent(9, 5);
		UserManager.instance.SetStudent(10, 5);
	}
    // Creates test. Attempts to load from file.
    // If it fails, it loads a default test case.
    void InstantiateTest(){
        if (Load("data.json")){
            // file_data is instantiated if this is true
            foreach (JSONObject user in file_data.list){
                int uid = (int)user.list[0].n;
                if (!users.ContainsKey(uid)){
                    users[uid] = true;
                    UserManager.instance.AddUser(uid);
                }
                Debug.Log(uid);
                if (user.list.Count == 2){
                    foreach(JSONObject student in user.list[1].list){
                        int sid = (int)student.n;
                        if (!users.ContainsKey(sid)){
                            users[sid] = true;
                            UserManager.instance.AddUser(sid);
                            UserManager.instance.SetStudent(sid, uid);
                        }
                    }
                }
            }
        } else {
            LoadDefault();
        }
		UserManager.instance.RenderGraph();
        Destroy(this.gameObject);
    }
}

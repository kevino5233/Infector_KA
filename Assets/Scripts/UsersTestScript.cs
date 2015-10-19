using UnityEngine;
using System.Collections;

public class UsersTestScript : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (UserManager.instance){
			InstantiateTest();
			Destroy(this.gameObject);
		}
	}
	void InstantiateTest(){
		UserManager UserMan = UserManager.instance;
		UserMan.AddUser(1);
		UserMan.AddUser(2);
		UserMan.AddUser(3);
		UserMan.AddUser(4);
		UserMan.AddUser(5);
		UserMan.AddUser(6);
		UserMan.SetStudent(2, 1);
		UserMan.SetStudent(4, 1);
		UserMan.SetStudent(5, 4);
		UserMan.SetStudent(3, 2);
		UserMan.RenderGraph();
	}
}

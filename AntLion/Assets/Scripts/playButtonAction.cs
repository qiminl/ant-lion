using UnityEngine;
using System.Collections;

public class playButtonAction : MonoBehaviour {

	public Object button;
	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI(){
		if (GUI.Button (new Rect (550, 250, 50, 50), this.guiTexture.texture))
			Debug.Log("Clicked the button with an image");
	}
}

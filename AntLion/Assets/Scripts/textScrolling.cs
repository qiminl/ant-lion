using UnityEngine;
using System.Collections;

public class textScrolling : MonoBehaviour {

	float charactersPerSecond= 8.0f;
	string text = "Here is some text to scroll";
	Rect rect;
	
	private string textUsing;
	private string scrollBasis;
	private string scrollText;
	
	private int currChar = 0;
	private float timer = 0.0f;
	
	void Start () {
		this.guiText.text = "If Ant lion guess your movement right,\n" +
			"you will get caught \n" +
			"and be dragged down for two blocks if you choose to move vertically\n" +
			"or be dragged down for one block if you move horizontally\n" +
			"climb to the top of the screen to get rid of the ant lion\n" +
			"so be careful and have fun!\n";
	}
	/*
	void Update () {
		if (textUsing != text)
			NewText();
		
		float secondsPerCharacter  = 1.0f / charactersPerSecond;
		if (timer > secondsPerCharacter) {
			int iT  = Mathf.FloorToInt(timer / secondsPerCharacter);
			currChar = (currChar + iT) % textUsing.Length;
			timer -= (float)iT * secondsPerCharacter;
			scrollText = scrollBasis.Substring(currChar, textUsing.Length);  
		}
		timer += (float)Time.deltaTime;
		this.guiText.text = textUsing;
	}
	
	void OnGUI() {
		GUI.Label(rect, scrollText);
	}
	
	void NewText() {
		textUsing = text;
		scrollBasis = textUsing+textUsing;
		currChar = 0;
		scrollText = scrollBasis.Substring(currChar, textUsing.Length);
		timer = 0.0f;
	}*/
}


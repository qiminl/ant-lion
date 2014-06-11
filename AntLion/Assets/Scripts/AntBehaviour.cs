using UnityEngine;
using System.Collections;


public class AntBehaviour : MonoBehaviour {
	static private float textSize = 0.3f;

	private GameObject grid;
	private GridMaker gridMaker;
	private GlobalState globalState;
	private GameObject cameraObject;
	private GameObject sand;
	private Bounds antBounds;
	private GameObject rightProbObject;
	private GameObject leftProbObject;
	private GameObject upProbObject;
	public GameObject rightSqr;
	public GameObject leftSqr;
	public GameObject upSqr;

	Vector2 antPos;
	Vector2 targetPos;
	enum MoveType {NONE,UP, LEFT, DOWN, RIGHT, LANDSLIDE, DRAG};

	struct SlideProbs {public float up; public float down; public float right; public float left;}
	SlideProbs probabilities = new SlideProbs { right = 0.1f, left = 0.1f, up = 0.5f, down = 0.0f};
	SlideProbs landslideProb = new SlideProbs{right = Random.value, left = Random.value, up = Random.value, down = 0};
	MoveType movementType = MoveType.NONE;
	bool inSuspense = false;
	float movementStartTime;
	float suspenseStartTime;
	public float movementDurationSeconds = 1.0f;

	
	TextMesh rightText;
	TextMesh leftText;
	TextMesh upText;

	// Use this for initialization
	void Start () {
		//grab the grid maker
		grid = GameObject.Find ("Grid");
		DebugUtils.Assert(grid);
		gridMaker = grid.GetComponent<GridMaker> ();
		DebugUtils.Assert(gridMaker);
		sand = GameObject.Find ("SandBackground");
		DebugUtils.Assert (sand);
		cameraObject = GameObject.Find ("Main Camera");
		DebugUtils.Assert(cameraObject);
		rightProbObject = GameObject.Find ("rightText");
		leftProbObject = GameObject.Find("leftText");
		upProbObject = GameObject.Find("upText");


		//make sure we have a global state - create one if it's missing (probably started scene from within Unity editor)
		GameObject glob = GameObject.Find ("GlobalState");
		if (glob) {
			globalState = glob.GetComponent<GlobalState>();
		} else {
			globalState = grid.AddComponent<GlobalState>();
			Debug.LogWarning("Creating global state object on the fly - assuming game is running in editor");
		}

		//size the ant to fill one grid cell, and put it in a bottom centre cell
		Sprite sprite = GetComponent<SpriteRenderer> ().sprite;
		antBounds = sprite.bounds;
		transform.localScale = new Vector3 (1.0f/antBounds.size.x * gridMaker.gridCellWidth, 1.0f/antBounds.size.y*gridMaker.gridCellHeight, 1.0f);
		Debug.Log("scaled");
		antPos.x = (int)(gridMaker.gridSizeHorizontal / 2.0f);
		targetPos = antPos;

		// set the size of texts
		rightText = rightProbObject.GetComponent<TextMesh> ();
		rightText.transform.localScale *= textSize;
		upText = upProbObject.GetComponent<TextMesh> ();
		upText.transform.localScale *= textSize;
		leftText = leftProbObject.GetComponent<TextMesh> ();
		leftText.transform.localScale *= textSize;

		string probString;
		if (probabilities.right > 0.6) {
			probString = "red";
		}
		else if (probabilities.right <= 0.6 && probabilities.right > 0.3) {
			probString = "yellow";		
		}
		else{
			probString = "green";
		}

		rightSqr = (GameObject) Instantiate(Resources.Load(probString, typeof(GameObject)));

		if (probabilities.left > 0.6) {
			probString = "red";
		}
		else if (probabilities.left <= 0.6 && probabilities.left > 0.3) {
			probString = "yellow";		
		}
		else{
			probString = "green";
		}
		leftSqr = (GameObject)Instantiate (Resources.Load (probString, typeof(GameObject)));

		if (probabilities.up > 0.6) {
			probString = "red";
		}
		else if (probabilities.up <= 0.6 && probabilities.up > 0.3) {
			probString = "yellow";		
		}
		else{
			probString = "green";
		}
		upSqr = (GameObject)Instantiate (Resources.Load (probString, typeof(GameObject)));
		ResetProbabilities();
		UpdateMovement ();
	}

	void InitializeMovement() {
		movementStartTime = Time.time;
	}

	//transform a grid-cell x,y coordinate into a world x,y,z coordinate
	private Vector3 GridPosToVector3(Vector2 pos) {
		return grid.transform.position + new Vector3 (gridMaker.gridCellWidth * pos.x, gridMaker.gridCellHeight * pos.y, 0.0f);
	}

	//initialize the landslide probabilities based on difficulty level
	void ResetProbabilities() {
	//	switch (globalState.difficulty) {
	//	case GlobalState.Difficulty.EASY: {
			probabilities.left = 0.1f;
			probabilities.up = 0.3f;
			probabilities.right = 0.1f;
	//		break;
	//	}
		/*
		case GlobalState.Difficulty.MEDIUM: {
			probabilities.horizontal = 0.1f;
			probabilities.vertical = 0.5f;
			break;
		}
		case GlobalState.Difficulty.HARD: {
			probabilities.horizontal = 0.2f;
			probabilities.vertical = 0.7f;
			break;
		}
		*/
	//	}
	}

	//calculate the current state of the game
	public GlobalState.GameState CheckWinLose() {
		if (antPos.y >= gridMaker.gridSizeVertical)
		{
			return GlobalState.GameState.WON;
		} else if (antPos.y < 0) {
			return GlobalState.GameState.LOST;
		}
		return GlobalState.GameState.PLAYING;
	}

	//determine the outcome of the current move action
	void CheckProbabilities() {
		switch (movementType) {
		case MoveType.RIGHT: {
			if (Random.value < probabilities.right) {
				MoveVertical(-1, true);
			} else {
				movementType = MoveType.NONE;
				probabilities.right += 0.1f;
				probabilities.up /= 1.5f;
			}
			break;
		}
		case MoveType.UP: {
			if (Random.value < probabilities.up) {
				MoveVertical(-2, true);
			} else {
				movementType = MoveType.NONE;
				probabilities.up *= 1.5f;
			}
			break;
		}
		
		case MoveType.LEFT:{
			if (Random.value < probabilities.left) {
				MoveVertical(-1, true);
			}
			else{
				movementType = MoveType.NONE;
				probabilities.left += 0.1f;
				probabilities.up /= 1.5f;
			}
			break;
		}
		case MoveType.DRAG: {
			movementType = MoveType.NONE;
		//	ResetProbabilities();
			break;
		}
		}
	}

	//update the ant location, and handle the current movement
	void UpdateMovement() {
		Vector3 movement = Vector3.zero;
		if (inSuspense) {
			if (suspenseStartTime + 1.0f < Time.time) {
				CheckProbabilities();
				GetComponent<SpriteRenderer> ().color = Color.white;
				inSuspense = false;
			}
		}
		if (!inSuspense && movementType != MoveType.NONE) {
			float elapsed = (Time.time-movementStartTime)/movementDurationSeconds;
			if (elapsed > 1) {
				antPos = targetPos;
				movement = Vector3.zero;
				if (CheckWinLose() != GlobalState.GameState.PLAYING)
				{
				} else {
					suspenseStartTime = Time.time;
					inSuspense = true;
					if (movementType != MoveType.DRAG) {
						GetComponent<SpriteRenderer> ().color = Color.blue;
					} else {
						GetComponent<SpriteRenderer> ().color = Color.red;
					}
				}
			} else {
				Vector3 start = GridPosToVector3(antPos);
				Vector3 target = GridPosToVector3(targetPos);
				float progress = 1 - Mathf.Pow( Mathf.Cos( elapsed * Mathf.PI / 2 ), 2 );
				movement = (target-start) * progress;
			}
		}
		transform.position = GridPosToVector3(antPos) + movement;

		Camera cam = cameraObject.GetComponent<Camera>();
		Bounds sandBounds = sand.GetComponent<SpriteRenderer>().bounds;
		float camx = Mathf.Max (sandBounds.min.x + cam.orthographicSize*cam.aspect,transform.position.x + antBounds.extents.x);
		camx = Mathf.Min (sandBounds.max.x - cam.orthographicSize*cam.aspect, camx);
		float camy = Mathf.Max (sandBounds.min.y + cam.orthographicSize, transform.position.y - 1.0f + antBounds.extents.y);
		camy = Mathf.Min (sandBounds.max.y - cam.orthographicSize, camy);
		cameraObject.transform.position = new Vector3(camx, camy, cameraObject.transform.position.z);



		Vector3 rightTextPos = GridPosToVector3 (new Vector2 (antPos.x + 1, antPos.y))+ new Vector3(0.4f, 0.4f, 0);
		rightText.transform.position = rightTextPos;
		rightText.text = "" + probabilities.right;
		Vector3 leftTextPos = GridPosToVector3 (new Vector2 (antPos.x - 1, antPos.y))+ new Vector3(0.4f, 0.4f, 0);
		leftText.transform.position = leftTextPos;
		leftText.text = "" + probabilities.left;
		Vector3 upTextPos = GridPosToVector3 (new Vector2 (antPos.x, antPos.y + 1))+ new Vector3(0.4f, 0.4f, 0);
		upText.transform.position = upTextPos;
		upText.text = "" + Mathf.Round(probabilities.up * 100.0f) / 100.0f;

		// rect updates
		string probString;
		if (probabilities.right > 0.6) {
			probString = "red";
		}
		else if (probabilities.right <= 0.6 && probabilities.right > 0.3) {
			probString = "yellow";		
		}
		else{
			probString = "green";
		}
		
		Sprite rightSprite = Resources.Load (probString, typeof(Sprite)) as Sprite;
		SpriteRenderer rightSpriteRenderer = rightSqr.GetComponent <SpriteRenderer>();
		rightSpriteRenderer.sprite = rightSprite;
		rightSqr.transform.position = GridPosToVector3 (new Vector2 (antPos.x + 1, antPos.y))+ new Vector3(0.4f, 0.4f, 0);
		if (probabilities.left > 0.6) {
			probString = "red";
		}
		else if (probabilities.left <= 0.6 && probabilities.left > 0.3) {
			probString = "yellow";		
		}
		else{
			probString = "green";
		}
		Sprite leftSprite = Resources.Load (probString, typeof(Sprite)) as Sprite;
		SpriteRenderer leftSpriteRenderer = leftSqr.GetComponent <SpriteRenderer>();
		leftSpriteRenderer.sprite = leftSprite;
		leftSqr.transform.position = GridPosToVector3 (new Vector2 (antPos.x - 1, antPos.y))+ new Vector3(0.4f, 0.4f, 0);

		if (probabilities.up > 0.6) {
			probString = "red";
		}
		else if (probabilities.up <= 0.6 && probabilities.up > 0.3) {
			probString = "yellow";		
		}
		else{
			probString = "green";
		}
		Sprite upSprite = Resources.Load (probString, typeof(Sprite)) as Sprite;
		SpriteRenderer upSpriteRenderer = upSqr.GetComponent <SpriteRenderer>();
		upSpriteRenderer.sprite = upSprite;
		upSqr.transform.position = GridPosToVector3 (new Vector2 (antPos.x, antPos.y + 1))+ new Vector3(0.4f, 0.4f, 0);

	}
	
	void MoveLandSlide(int steps) {
		MoveVertical(steps, true);
	}

	void MoveHorizontal(int steps) {
		targetPos = antPos;
		targetPos.x += steps;
		targetPos.x = Mathf.Max (targetPos.x, 0);
		targetPos.x = Mathf.Min (targetPos.x, gridMaker.gridSizeHorizontal - 1);
		if (targetPos != antPos && targetPos.x < antPos.x) {
			movementType = MoveType.LEFT;
			InitializeMovement();
		}
		else if(targetPos != antPos && targetPos.x > antPos.x){
			movementType = MoveType.RIGHT;
			InitializeMovement();
		}
	}

	void MoveVertical(int steps, bool isDrag) {
		targetPos = antPos;
		targetPos.y += steps;
		movementType = isDrag ? MoveType.DRAG : MoveType.UP;
		InitializeMovement();
	}

	// Update is called once per frame
	void Update () {
		//if not currently moving, allow new movement based on user input
		if (movementType == MoveType.NONE) {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				MoveVertical(1, false);
			} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
				MoveVertical(-1, false);
			} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				MoveHorizontal (-1);
			} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
				MoveHorizontal (1);
			}
		}
		/*
		rightProb.transform.position = new Vector3 (antPos.x + 1, antPos.y, 0.0f);
		rightProb.text = "" + probabilities.right;
*/

		//always update the ant movement
		UpdateMovement();
	}

	void OnGUI(){
	
	}
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <author>
/// Fernando Geraci
/// </author>
/// <summary>
/// Line tracing memory game. Difficulty progresses procedurally.
/// </summary>

public class GridGenerator : MonoBehaviour {

    public enum DIFFICULTY_LEVEL {
        EASY,
        MEDIUM,
        HARD,
        EXPERT
    }

    public UnityEngine.UI.Dropdown DifficultyOptions;
    public int level = 0;
    public DIFFICULTY_LEVEL DifficultyLevel = DIFFICULTY_LEVEL.EASY;

    // Game Difficulty settings
    private int                 X = 4, Y = 8;
    private float               startDelay = 0.01f;
    private float               routePreviewSpeed = 5f;
    private int                 pathMultiplier = 1;
    private Vector3             origMenuPosition, origBallPosition;
    private GameObject          gamePanel;
    private GameObject          quitButton;
    private GameObject          KillBall;
    private List<GameObject>    goalPath;
    private GameObject          currentNode;
    private int                 currentNodeIndex = 0;
    private Transform           Plane;
    private GameObject[]        vertices;
    private Material            nodeMaterial;
    private float               drawPathDelay = 2f;
    private bool                playing = false;
    private GameObject          playText, levelText, nodesText;
    private float               tileOffsetX, tileOffsetY;
    private GameObject          levelClearedPanel;
    private AudioSource         audioSource;
    private AudioClip           clickSound, badClickSound, exitSound, levelClearedSound;


    public void Awake() {
        goalPath = new List<GameObject>();
        KillBall = GameObject.FindGameObjectWithTag("Game.KillBall");
        origMenuPosition = GameObject.FindGameObjectWithTag("UI.MenuPanel").GetComponent<RectTransform>().position;
        origBallPosition = KillBall.transform.position;
        audioSource = GetComponent<AudioSource>();
        clickSound = Resources.Load("Sounds/Bubble_Pop") as AudioClip;
        exitSound = Resources.Load("Sounds/Exit_Game") as AudioClip;
        badClickSound = Resources.Load("Sounds/Bad_Pop") as AudioClip;
        levelClearedSound = Resources.Load("Sounds/Level_Cleared") as AudioClip;
        gamePanel = GameObject.FindGameObjectWithTag("UI.GamePanel");
        quitButton = GameObject.FindGameObjectWithTag("UI.QuitButton");
        playText = GameObject.FindGameObjectWithTag("UI.PlayText");
        levelText = GameObject.FindGameObjectWithTag("UI.LevelText");
        nodesText = GameObject.FindGameObjectWithTag("UI.NodesText");
        levelClearedPanel = GameObject.FindGameObjectWithTag("UI.LevelClearedPanel");
        levelClearedPanel.SetActive(false);
        StartCoroutine(NotifyPlay());
        gamePanel.SetActive(false);
    }

    public void FixedUpdate() {
        if (playing) {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 200.0f)) {
                    if(hit.collider.transform == goalPath[currentNodeIndex + 1].transform) {
                        PlayClip(clickSound);
                        currentNodeIndex++;
                        StartCoroutine(MoveKillBall(goalPath[currentNodeIndex].transform.position));
                        currentNode = goalPath[currentNodeIndex];
                        if (currentNodeIndex == (goalPath.Count - 1)) {
                            StartCoroutine(Congratulate());
                        }
                    } else {
                        PlayClip(badClickSound);
                    }
                }
            }
        }
    }

    private void PlayClip(AudioClip clip, bool interrupt = false) {
        if (!audioSource.isPlaying || interrupt) {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void SetDifficulty() {
        DifficultyLevel = (DIFFICULTY_LEVEL) DifficultyOptions.value;
    }

    public void StartGame() {
        StartCoroutine(MoveMenu(true));
    }

    private void SetLabelText(GameObject label, string text) {
        label.GetComponent<UnityEngine.UI.Text>().text = text;
    }

    private IEnumerator NotifyPlay() {
        while(true) {
            playText.SetActive(playing);
            yield return null;
        }
    }

    public void ExitGame() {
        PlayClip(exitSound);
        level = 0;
        currentNodeIndex = 0;
        playing = false;
        gamePanel.SetActive(false);
        foreach(GameObject go in vertices) {
            GameObject.DestroyImmediate(go);
        }
        StartCoroutine(MoveKillBall(origBallPosition));
        StartCoroutine(MoveMenu(false));
    }

    public IEnumerator MoveMenu(bool away) {
        float width = away ? Screen.width : Screen.width * -1;
        RectTransform panel = GameObject.FindGameObjectWithTag("UI.MenuPanel").GetComponent<RectTransform>();
        Vector3 currentPosition = panel.transform.position;
        Vector3 targetPosition = away ? currentPosition + new Vector3(currentPosition.x + (width / 2), 0, 0) : 
            new Vector3(Screen.width / 2, Screen.height / 2, 0);
        while(Vector3.Distance(currentPosition,targetPosition) > 10f) {
            panel.position = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * 4f);
            currentPosition = panel.position;
            yield return null;
        }
        panel.position = targetPosition;
        gamePanel.SetActive(away);
        if (away)
            LoadGame();
    }

    private void LoadGame() {
        switch (DifficultyLevel) {
            case DIFFICULTY_LEVEL.EASY:
                X = 2; Y = 4;
                break;
            case DIFFICULTY_LEVEL.MEDIUM:
                X = 4; Y = 8;
                break;
            case DIFFICULTY_LEVEL.HARD:
                X = 6; Y = 12;
                drawPathDelay *= 0.8f;
                break;
            case DIFFICULTY_LEVEL.EXPERT:
                X = 12; Y = 12;
                drawPathDelay *= 0.5f;
                break;
        }
        startDelay /= X;
        nodeMaterial = Resources.Load("Materials/Poly_2_mat") as Material;
        vertices = new GameObject[(X + 1) * (Y + 1)];
        Plane = GetComponent<Transform>();
        StartCoroutine(GenerateGrid());
    }

    private IEnumerator GenerateGrid() {
        float width = Plane.GetComponent<Renderer>().bounds.size.x;
        float height = Plane.GetComponent<Renderer>().bounds.size.z;
        tileOffsetX = width / X;
        tileOffsetY = height / Y;
        Vector3 topLeft = Plane.position + new Vector3(-1 * (width / 2), 1.5f, height / 2);
        WaitForSeconds wait = new WaitForSeconds(startDelay);
        int index = 0;
        for (int row = 0; row <= Y; row++) {
            for (int col = 0; col <= X; col++) {
                GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                node.GetComponent<Renderer>().material = nodeMaterial;
                node.transform.position = topLeft + new Vector3(tileOffsetX * col, 1.5f, -1 * (tileOffsetY * row));
                StartCoroutine(AnimateScale(node,new Vector3(3, 3, 3)));
                vertices[index] = node;
                index++;
                if ((index - 1) == vertices.Length / 2) {
                    StartCoroutine(MoveKillBall(vertices[vertices.Length / 2].transform.position));
                    currentNode = vertices[vertices.Length / 2];
                }
                yield return wait;
            }
        }
        StartLevel();
    }

    private IEnumerator MoveKillBall(Vector3 targetPosition) {
        Transform ball = GameObject.FindGameObjectWithTag("Game.KillBall").transform;
        Vector3 currentPosition = ball.position;
        while(Vector3.Distance(ball.position,targetPosition) > 0.25f) {
            ball.position = Vector3.Lerp(ball.position, targetPosition, Time.deltaTime * 8f);
            yield return null;
        }
        ball.position = targetPosition;
    }

    private IEnumerator AnimateScale(GameObject node,Vector3 target) {
        while (node.transform.localScale != target) {
            node.transform.localScale = Vector3.Lerp(node.transform.localScale, target, Time.deltaTime * Random.Range(5f,10f));
            if (Vector3.Distance(node.transform.localScale, target) <= 0.5f) {
                node.transform.localScale = target;
                break;
            }
            yield return null;
        }
        node.transform.localScale = target;
    }
    
    private IEnumerator DrawPath() {
        WaitForSeconds wait = new WaitForSeconds(drawPathDelay);
        for (int i = 0; i <  goalPath.Count; ++i) {
            if(i < goalPath.Count - 1) {
                GameObject myLine = new GameObject();
                myLine.transform.position = goalPath[i].transform.position;
                myLine.AddComponent<LineRenderer>();
                LineRenderer lr = myLine.GetComponent<LineRenderer>();
                Material mat = Resources.Load("Materials/Line_mat") as Material;
                lr.material = mat;
                lr.startColor = Color.green;
                lr.endColor = Color.red;
                lr.startWidth = lr.endWidth = 1f;
                lr.SetPosition(0, goalPath[i].transform.position);
                lr.SetPosition(1, goalPath[i+1].transform.position);
                myLine.transform.parent = this.transform;
                GameObject.Destroy(myLine, drawPathDelay);
                if(i  == goalPath.Count / 2) playing = true;
            }
            yield return wait;
        }
    }

    private IEnumerator Congratulate() {
        PlayClip(levelClearedSound, true);
        playing = false;
        levelClearedPanel.SetActive(true);
        WaitForSeconds seconds = new WaitForSeconds(2.5f);
        yield return seconds;
        levelClearedPanel.SetActive(false);
        StartLevel();
    }

    private void StartLevel() {
        currentNodeIndex = 0;
        level++;
        GenerateRoad();
        SetLabelText(levelText, "Level: " + level);
        SetLabelText(nodesText, "Edges: " + (goalPath.Count - 1));
        StartCoroutine(DrawPath());
    }

    private void GenerateRoad() {
        if (goalPath != null) goalPath.Clear();
        else goalPath = new List<GameObject>();
        goalPath.Add(currentNode);
        int path = level + 1;
        float extra = 1;
        switch(DifficultyLevel) {
            case DIFFICULTY_LEVEL.HARD:
                extra = 1.5f;
                break;
            case DIFFICULTY_LEVEL.MEDIUM:
                extra = 1.75f;
                break;
        }
        path = (int) (path * extra);
        
        for(int i = 1; i < path + 1; i++) {
            GameObject nextNode = vertices[Random.Range(0,vertices.Length - 1)];
            while(nextNode == currentNode) {
                nextNode = vertices[Random.Range(0, vertices.Length - 1)];
            }
            goalPath.Add(nextNode);
        }
    }
}

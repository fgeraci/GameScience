using System.Collections;
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

    public DIFFICULTY_LEVEL DifficultyLevel = DIFFICULTY_LEVEL.EASY;

    // Game Difficulty settings
    private int         X = 4, Y = 8;
    private float       startDelay = 0.01f;
    private float       routePreviewSpeed = 5f;
    private int         pathMultiplier = 1;
    private Vector3     origMenuPosition, origBallPosition;
    private GameObject  gamePanel;
    private GameObject  quitButton;

    private Transform Plane;
    private GameObject[] vertices;
    private Material nodeMaterial;

    private float tileOffsetX, tileOffsetY;

    public void Awake() {
        origMenuPosition = GameObject.FindGameObjectWithTag("UI.MenuPanel").GetComponent<RectTransform>().position;
        origBallPosition = GameObject.FindGameObjectWithTag("Game.KillBall").transform.position;
        gamePanel = GameObject.FindGameObjectWithTag("UI.GamePanel");
        quitButton = GameObject.FindGameObjectWithTag("UI.QuitButton");
        gamePanel.SetActive(false);
    }

    public void SetDifficulty() {
        DifficultyLevel = (DIFFICULTY_LEVEL) DifficultyOptions.value;
    }

    public void StartGame() {
        StartCoroutine(MoveMenu(true));
    }

    public void ExitGame() {
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
        if(away)
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
                break;
            case DIFFICULTY_LEVEL.EXPERT:
                X = 12; Y = 12;
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
                if ((index - 1) == vertices.Length / 2)
                    StartCoroutine(MoveKillBall(vertices[vertices.Length / 2].transform.position));
                yield return wait;
            }
        }
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
            if (Vector3.Distance(node.transform.localScale, target) <= 0.2f) {
                node.transform.localScale = target;
                break;
            }
            yield return null;
        }
    }
}

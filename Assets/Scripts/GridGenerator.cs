using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour {

    public enum DIFFICULTY_LEVEL {
        EASY,
        MEDIUM,
        HARD,
        EXPERT
    }

    public DIFFICULTY_LEVEL DifficultyLevel = DIFFICULTY_LEVEL.EASY;

    // Game Difficulty settings
    private int     X = 4, Y = 8;
    private float   startDelay = 0.01f;
    private float   routePreviewSpeed = 5f;
    private int     pathMultiplier = 1;

    private Transform Plane;
    private Vector3[] vertices;
    private Material nodeMaterial;

    private float tileOffsetX, tileOffsetY;
    
    public void StartGame() {
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
        vertices = new Vector3[(X + 1) * (Y + 1)];
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
        for (int row = 0; row <= Y; row++) {
            for (int col = 0; col <= X; col++) {
                GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                node.GetComponent<Renderer>().material = nodeMaterial;
                node.transform.position = topLeft + new Vector3(tileOffsetX * col, 1.5f, -1 * (tileOffsetY * row));
                StartCoroutine(AnimateScale(node,new Vector3(3, 3, 3)));
                yield return wait;
            }
        }
    }

    private IEnumerator AnimateScale(GameObject node,Vector3 target) {
        while (node.transform.localScale != target) {
            node.transform.localScale = Vector3.Lerp(node.transform.localScale, target, Time.deltaTime * Random.Range(5f,10f));
            yield return null;
        }
    }
}

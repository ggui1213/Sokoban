using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[ExecuteInEditMode]
public class GridMaker : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellWidth; //Width in unity units
    public Vector2 dimensions; //number of cells on the x and the y

    [Header("Line Settings")]
    public float lineWidth;
    public Color lineColor;
    public Material lineMaterial;

    //Info used to draw the grid and for GridObjects to place themselves
    private Vector2 _topLeft;
    private Vector2 _bottomRight;
    public Vector2 TopLeft { get { return _topLeft; } }
    public Vector2 BottomRight { get { return _bottomRight; } }

    //Singleton reference that GridObjects use
    public static GridMaker reference;

    private void OnEnable()
    {
        reference = this;
        CreateGrid();
    }

    [Button("Refresh Grid")]
    private void CreateGrid()
    {
        ClearGrid();

        //The grid is drawn with its center on the Grid gameobject's position
        //Doing some quick math here so I don't need to do it every iteration of the for loop
        _topLeft = Vector2.zero;
        _topLeft.x = this.transform.position.x - (dimensions.x * cellWidth * 0.5f);
        _topLeft.y = this.transform.position.y + (dimensions.y * cellWidth * 0.5f);

        _bottomRight = Vector2.zero;
        _bottomRight.x = this.transform.position.x + (dimensions.x * cellWidth * 0.5f);
        _bottomRight.y = this.transform.position.y - (dimensions.y * cellWidth * 0.5f);


        for (int x = 0; x <= dimensions.x; x++)
        {
            //Have to make a new gameobject for each linerenderer because
            //there can only be one linerenderer per gameobject apparently
            GameObject lineGO = new GameObject("x line " + x);
            lineGO.transform.parent = this.transform;
            lineGO.transform.localPosition = Vector3.zero;

            //Create a new line renderer on the child gameobject via code
            LineRenderer line = lineGO.AddComponent<LineRenderer>();

            //set the color and width
            line.startColor = lineColor;
            line.endColor = lineColor;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;

            line.numCapVertices = 5;

            //set the material
            line.material = lineMaterial;

            //set the positions of the line renderer
            float xPos = _topLeft.x + (x * cellWidth);
            line.SetPositions(new Vector3[] {new Vector3(xPos, _topLeft.y, 0), new Vector3(xPos, _bottomRight.y, 0)});
        }
        for (int y = 0; y <= dimensions.y; y++)
        {
            //Create a child gameobject that we're going to add all our LineRenderers to
            GameObject lineGO = new GameObject("y line " + y);
            lineGO.transform.parent = this.transform;
            lineGO.transform.localPosition = Vector3.zero;

            //Create a new line renderer on the child gameobject via code
            LineRenderer line = lineGO.AddComponent<LineRenderer>();

            //set the color and width
            line.startColor = lineColor;
            line.endColor = lineColor;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            
            line.numCapVertices = 5;

            //set the material
            line.material = lineMaterial;

            //set the positions of the line renderer
            float yPos = _topLeft.y - (y * cellWidth);
            line.SetPositions(new Vector3[] {new Vector3(_topLeft.x, yPos, 0), new Vector3(_bottomRight.x, yPos, 0)});
        }
    }

    private void ClearGrid()
    {
        while (this.transform.childCount > 0)
        {
            GameObject.DestroyImmediate(this.transform.GetChild(0).gameObject);
        }
    }
}

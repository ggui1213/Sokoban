using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[ExecuteInEditMode]
public class GridObject : MonoBehaviour
{
    public Vector2Int gridPosition;
    private Vector2Int prevGridPosition;

    private void Update()
    {
        //If our position hasn't been updated, don't 
        if (gridPosition == prevGridPosition)
            return;

        //Move to the new position
        UpdatePosition();

        //Keep track of our previous position
        prevGridPosition = gridPosition;
    }

    [Button("Update Position")]
    private void UpdatePosition()
    {
        float x = GridMaker.reference.TopLeft.x + GridMaker.reference.cellWidth * (gridPosition.x - 0.5f); 
        float y = GridMaker.reference.TopLeft.y - GridMaker.reference.cellWidth * (gridPosition.y - 0.5f);
        this.transform.position = new Vector3(x, y, 0); 
    }
}

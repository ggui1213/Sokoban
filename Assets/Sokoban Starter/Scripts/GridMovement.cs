using System;
using System.Collections.Generic;
using UnityEngine;

public enum GridObjectType
{
    Wall,
    Player,
    Smooth,
    Sticky,
    Clingy,
}

public class GridMovement : MonoBehaviour
{
    //record xy coordinates
    private GridObject gridObject;
    
    private static Dictionary<Vector2Int, GridMovement> gridDictionary = new();
    
    //avoid logic error
    private static HashSet<GridMovement> traveledThisFrame = new();

    public GridObjectType gridObjectType;
    
    private void Start()
    {
        gridObject = GetComponent<GridObject>(); 
        gridDictionary.Add(gridObject.gridPosition, this);
    }

    private void Update()
    {
        if (gridObjectType != GridObjectType.Player)
            return;

        Vector2Int direction = Vector2Int.zero;
        
        //inputs
        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = Vector2Int.down;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Vector2Int.up;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Vector2Int.left;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            direction = Vector2Int.right;
        }

        if (direction != Vector2Int.zero)
        {
            traveledThisFrame.Clear(); 
            AskMove(direction);
        }
    }

    //identifier 1
    public bool AskMove(Vector2Int direction)
    {
        if (gridObjectType == GridObjectType.Smooth)
            print(traveledThisFrame.Contains(this));
        if (traveledThisFrame.Contains(this))
            return true;
        
        switch (gridObjectType)
        {
            case GridObjectType.Player:
            case GridObjectType.Smooth:
            case GridObjectType.Sticky:
                return TryMove(direction);
            case GridObjectType.Clingy:
            case GridObjectType.Wall:
                return false;  
            default:
                return false;
        }
    }
    
    //identifier 2
    private bool TryMove(Vector2Int direction)
    {
        if (traveledThisFrame.Contains(this))
            return false;
        traveledThisFrame.Add(this);
        
        if (direction == Vector2Int.zero)
            return false;

        Vector2Int oldPos = gridObject.gridPosition;
        Vector2Int targetPos = oldPos + direction;
        
        Vector2 gridDims = GridMaker.reference.dimensions;
        if (targetPos.x < 1 || targetPos.x > gridDims.x ||
            targetPos.y < 1 || targetPos.y > gridDims.y)
        {
            return false; 
        }
        
        if (gridDictionary.TryGetValue(targetPos, out var existingGridObj))
        {
            if (existingGridObj.gridObjectType == GridObjectType.Wall)
                return false;
            
            if (!existingGridObj.AskMove(direction))
                return false;
        }
        
        gridDictionary.Remove(oldPos);
        gridObject.gridPosition = targetPos;
        gridDictionary[targetPos] = this;
        
        if (gridObjectType == GridObjectType.Sticky)
            OnStickyMove(oldPos, direction);
        else
            NotifyNeighborMove(oldPos, direction);

        return true;
    }
    Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
   
    //i hate this but i have to do this
    private void OnStickyMove(Vector2Int oldPos, Vector2Int moveDir)
    {
        foreach (var offset in offsets)
        {
            Vector2Int neighborPos = oldPos + offset;
            if (!gridDictionary.TryGetValue(neighborPos, out var neighbor))
                continue;  
            
            if (traveledThisFrame.Contains(neighbor))
                continue;
            
            switch (neighbor.gridObjectType)
            {
                case GridObjectType.Clingy:
                    if (offset == -moveDir)
                        neighbor.TryMove(moveDir);
                    break;
                
                case GridObjectType.Sticky:
                default:
                    neighbor.AskMove(moveDir);
                    break;
            }
        }
    }
    
    //movement
    private void NotifyNeighborMove(Vector2Int oldPos, Vector2Int moveDir)
    {
        //letting objects move
        foreach (var offset in offsets)
        {
            Vector2Int neighborPos = oldPos + offset;
            if (!gridDictionary.TryGetValue(neighborPos, out var neighbor))
                continue; 
            
            if (traveledThisFrame.Contains(neighbor))
                continue;
            
            switch (neighbor.gridObjectType)
            {
                case GridObjectType.Sticky:
                    neighbor.AskMove(moveDir);
                    break;

                case GridObjectType.Clingy:
                    if (offset == -moveDir)
                        neighbor.TryMove(moveDir);
                    break;
                default:
                    break;
            }
        }
    }

}

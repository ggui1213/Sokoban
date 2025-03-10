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
    private GridObject gridObject;
    
    // 全局字典：坐标 -> GridMovement 脚本
    private static Dictionary<Vector2Int, GridMovement> gridDictionary = new();
    // 记录本帧已移动过的对象，防止递归无限移动
    private static HashSet<GridMovement> traveledThisFrame = new();

    public GridObjectType gridObjectType;
    
    private void Start()
    {
        gridObject = GetComponent<GridObject>();
        // 初始化时将自己放入字典
        gridDictionary.Add(gridObject.gridPosition, this);
    }

    private void Update()
    {
        // 只有 Player 才根据输入来移动
        if (gridObjectType != GridObjectType.Player)
            return;

        Vector2Int direction = Vector2Int.zero;
 
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

    public bool AskMove(Vector2Int direction)
    {
        if (gridObjectType == GridObjectType.Smooth)
            print(traveledThisFrame.Contains(this));
        // 若本帧已移动过，直接允许（避免重复推动）
        if (traveledThisFrame.Contains(this))
            return true;

        // 根据类型来决定是否可移动
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

    private bool TryMove(Vector2Int direction)
    {
        // 防止递归无限移动
        if (traveledThisFrame.Contains(this))
            return false;
        traveledThisFrame.Add(this);

        // 若没有移动方向，直接返回
        if (direction == Vector2Int.zero)
            return false;

        // 目标位置
        Vector2Int oldPos = gridObject.gridPosition;
        Vector2Int targetPos = oldPos + direction;

        // 边界检查
        Vector2 gridDims = GridMaker.reference.dimensions;
        if (targetPos.x < 1 || targetPos.x > gridDims.x ||
            targetPos.y < 1 || targetPos.y > gridDims.y)
        {
            return false; // 出界，不移动
        }

        // 检查目标位置是否有对象
        if (gridDictionary.TryGetValue(targetPos, out var existingGridObj))
        {
            // 若是墙，无法移动
            if (existingGridObj.gridObjectType == GridObjectType.Wall)
                return false;

            // 如果对方无法移动，也返回 false
            if (!existingGridObj.AskMove(direction))
                return false;
        }

        // 移动自己到 targetPos
        gridDictionary.Remove(oldPos);
        gridObject.gridPosition = targetPos;
        gridDictionary[targetPos] = this;

        // 如果是 Smooth、Sticky、Clingy 继续执行下面的邻居通知
        // 如果是 Player，也可以同样通知邻居
        // 这里为了统一，大家都通知邻居
        if (gridObjectType == GridObjectType.Sticky)
            OnStickyMove(oldPos, direction);
        else
            NotifyNeighborMove(oldPos, direction);

        return true;
    }
    Vector2Int[] offsets = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    private void OnStickyMove(Vector2Int oldPos, Vector2Int moveDir)
    {
        foreach (var offset in offsets)
        {
            Vector2Int neighborPos = oldPos + offset;
            if (!gridDictionary.TryGetValue(neighborPos, out var neighbor))
                continue;  // 没邻居就跳过

            // 如果邻居已经移动过，跳过
            if (traveledThisFrame.Contains(neighbor))
                continue;

            // **根据“邻居”的类型**来决定它要不要动
            switch (neighbor.gridObjectType)
            {
                case GridObjectType.Clingy:
                    // Clingy 只在被拉时移动
                    // 即如果 offset == -moveDir，表示我离开了它
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

    private void NotifyNeighborMove(Vector2Int oldPos, Vector2Int moveDir)
    {
        // 我刚从 oldPos 移到 oldPos + moveDir，
        // 所以 oldPos 的四周邻居要看看是否需要跟着动
        

        foreach (var offset in offsets)
        {
            Vector2Int neighborPos = oldPos + offset;
            if (!gridDictionary.TryGetValue(neighborPos, out var neighbor))
                continue;  // 没邻居就跳过

            // 如果邻居已经移动过，跳过
            if (traveledThisFrame.Contains(neighbor))
                continue;

            // **根据“邻居”的类型**来决定它要不要动
            switch (neighbor.gridObjectType)
            {
                case GridObjectType.Sticky:
                    // Sticky 跟随移动者
                    neighbor.AskMove(moveDir);
                    break;

                case GridObjectType.Clingy:
                    // Clingy 只在被拉时移动
                    // 即如果 offset == -moveDir，表示我离开了它
                    if (offset == -moveDir)
                        neighbor.TryMove(moveDir);
                    break;
                // 其他类型(比如 Player、Smooth、Wall)，在这里可能不做额外处理
                default:
                    break;
            }
        }
    }

}

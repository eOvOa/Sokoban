using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Dictionary to store the positions of all blocks on the grid
    private Dictionary<Vector2Int, GameObject> blockPositions;

    void Start()
    {
        InitializeBlockPositions();
    }

    private void InitializeBlockPositions()
    {
        blockPositions = new Dictionary<Vector2Int, GameObject>();

        // Find all blocks in the scene and add them to the dictionary
        GridObject[] allBlocks = FindObjectsOfType<GridObject>(); // Assuming that all block objects have the GridObject component attached

        foreach (GridObject block in allBlocks)
        {
            Vector2Int gridPosition = WorldToGridPosition(block.transform.position);
            blockPositions[gridPosition] = block.gameObject;
        }
    }

    // Converts a world position to a grid position (Vector2Int)
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        // Calculate grid cell position based on GridMaker's cell width
        float cellWidth = GridMaker.reference.cellWidth;

        // Convert the world position to grid coordinates, considering the grid starts at (1,1)
        int x = Mathf.FloorToInt((worldPosition.x - GridMaker.reference.TopLeft.x) / GridMaker.reference.cellWidth) + 1;
        int y = Mathf.FloorToInt((GridMaker.reference.TopLeft.y - worldPosition.y) / GridMaker.reference.cellWidth) + 1;

        return new Vector2Int(x, y);
    }

    // Check if the target grid position is within the grid boundaries
    public bool IsWithinGridBounds(Vector2Int gridPosition)
    {
        // Ensure that the grid position respects the grid's starting point at (1,1)
        return gridPosition.x >= 1 && gridPosition.x <= GridMaker.reference.dimensions.x &&
               gridPosition.y >= 1 && gridPosition.y <= GridMaker.reference.dimensions.y;
    }


    // Method to check if a grid position is occupied
    public bool IsPositionOccupied(Vector2Int gridPosition)
    {
        return blockPositions.ContainsKey(gridPosition);
    }

    // Method to update the block's position in the dictionary
  public void UpdateBlockPosition(GameObject block, Vector2Int oldPosition, Vector2Int newPosition)
    {
        // Remove the block from its old position in the dictionary
        if (blockPositions.ContainsKey(oldPosition) && blockPositions[oldPosition] == block)
        {
            blockPositions.Remove(oldPosition);
        }

        // Add the block to its new position in the dictionary
        if (!blockPositions.ContainsKey(newPosition))
        {
            blockPositions[newPosition] = block;
        }
        else
        {
            Debug.LogError($"Collision error: Tried to move {block.name} to an occupied position {newPosition}.");
        }
    }

    public bool IsPositionOccupiedByWall(Vector2Int position)
    {
        // Check if the given grid position contains a wall object
        //return blockPositions.ContainsKey(position) && blockPositions[position].tag == "Wall";
        if (blockPositions.ContainsKey(position))
        {
            GameObject block = blockPositions[position];
            return block != null && block.CompareTag("Wall");
        }
        return false;
    }

    public GameObject GetBlockAtPosition(Vector2Int gridPosition)
    {
        if (blockPositions.ContainsKey(gridPosition))
        {
            return blockPositions[gridPosition];
        }
        return null;
    }
}

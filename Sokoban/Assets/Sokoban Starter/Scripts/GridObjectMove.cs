using System;
using UnityEngine;

namespace Sokoban_Starter.Scripts.Pseudocode
{
    public class GridObjectMove : MonoBehaviour
    {
        private GameManager gameManager;
        private GridObject gridObject;
        public MovementType movementType;

        public bool moved = false;

        public enum MovementType
        {
            Player,
            Smooth,
            Sticky,
            Clingy,
            Wall,
            PulledSticky,
        }

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            gridObject = GetComponent<GridObject>();
        }

        private void Update()
        {
            if (movementType != MovementType.Player)
                return;

            Vector2Int direction = Vector2Int.zero;


            if (Input.GetKeyDown(KeyCode.W))
                direction = Vector2Int.down; // up
            else if (Input.GetKeyDown(KeyCode.S))
                direction = Vector2Int.up; // down
            else if (Input.GetKeyDown(KeyCode.A))
                direction = Vector2Int.left; // left
            else if (Input.GetKeyDown(KeyCode.D))
                direction = Vector2Int.right; // right

            if (direction != Vector2Int.zero)
                TryMove(direction);
        }

        /// Return bool is necessary because perhaps this block can move, it pushes another block, and that block
        /// Cannot move thus this block cannot move, in this case, the block pushed this black cannot move anyway
        public bool TryMove(Vector2Int direction)
        {
            if (movementType == MovementType.Wall)
                return false;

            Vector2Int gridPosition = gridObject.gridPosition;
            Vector2Int targetPosition = gridPosition + direction;
            Vector2Int sideDirection1 = new Vector2Int(direction.x == 0 ? 1 : 0,
                direction.y == 0 ? 1 : 0);
            Vector2Int sideDirection2 = -sideDirection1;

            if (!gameManager.IsWithinGridBounds(targetPosition))
                return false;

            var objInFront = gameManager.GetBlockAtPosition(targetPosition);
            GridObjectMove blockInFront = null;
            if (objInFront)
            {
                blockInFront = objInFront.GetComponent<GridObjectMove>();

                if (!blockInFront)
                    return false;
                if (blockInFront.movementType == MovementType.Clingy)
                    return false;
                if (!blockInFront.TryMove(direction))
                    return false;
            }

            TakeMove(direction);

            //先移动再检测sticky，避免sticky再反过来检测当前物体导致无限循环，最后检测clingy

            GameObject sideBlock = null;
            GridObjectMove blockMove = null;

            switch (movementType)
            {
                case MovementType.Player:
                case MovementType.Smooth:
                case MovementType.Clingy:
                    sideBlock = gameManager.GetBlockAtPosition(gridPosition + sideDirection1);

                    if (sideBlock)
                    {
                        blockMove = sideBlock.GetComponent<GridObjectMove>();
                        if (blockMove.movementType == MovementType.Sticky)
                            blockMove.TryMove(direction);
                    }

                    sideBlock = gameManager.GetBlockAtPosition(gridPosition + sideDirection2);

                    if (sideBlock)
                    {
                        blockMove = sideBlock.GetComponent<GridObjectMove>();
                        if (blockMove.movementType == MovementType.Sticky)
                            blockMove.TryMove(direction);
                    }

                    break;

                case MovementType.Sticky:
                    moved = true;
                    sideBlock = gameManager.GetBlockAtPosition(gridPosition + sideDirection1);
                    if (sideBlock)
                    {
                        blockMove = sideBlock.GetComponent<GridObjectMove>();
                        if (blockMove.movementType != MovementType.Clingy)
                            blockMove.TryMove(direction);
                    }

                    sideBlock = gameManager.GetBlockAtPosition(gridPosition + sideDirection2);
                    blockMove = null;
                    if (sideBlock)
                    {
                        blockMove = sideBlock.GetComponent<GridObjectMove>();
                        if (blockMove.movementType != MovementType.Clingy)
                            blockMove.TryMove(direction);
                    }

                    break;
            }

            var blockBehind = gameManager.GetBlockAtPosition(gridPosition - direction);
            if (blockBehind)
            {
                var blockBehindMove = blockBehind.GetComponent<GridObjectMove>();
                if (blockBehindMove && (blockBehindMove.movementType == MovementType.Clingy ||
                                        blockBehindMove.movementType == MovementType.Sticky))
                    blockBehindMove.TryMove(direction);
            }

            return true;
        }

        private void LateUpdate()
        {
            moved = false;
        }

        private void TakeMove(Vector2Int direction)
        {
            if (moved)
                return;
            moved = true;
            gameManager.UpdateBlockPosition(gameObject, gridObject.gridPosition, gridObject.gridPosition + direction);
            gridObject.gridPosition += direction;
        }
    }
}
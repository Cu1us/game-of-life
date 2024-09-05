//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Dictionary<Vector2Int, Cell> Grid = new();
    List<Vector2Int> tickedCellsThisIteration = new();
    Camera mainCamera;

    [Header("Settings (hover on each for info)")]
    [Space]

    [SerializeField]
    bool gamePaused = false;

    [SerializeField]
    [Min(1)]
    [Tooltip("How often the game updates. Limited by the frame rate.")]
    int ticksPerSecond = 3;

    [SerializeField]
    [Tooltip("Controls the size of the camera.")]
    float gameSize = 8;

    [Space]

    [SerializeField]
    [Range(0, 0.5f)]
    [Tooltip("How close the mouse has to be to the screen border (in viewport coordinates) to make the camera move in that direction.")]
    float screenBorderWidth = 0.1f;
    [SerializeField]
    [Tooltip("How fast the camera moves per second per game size when the mouse is on the screen border.")]
    float cameraMoveSpeed = 1;
    [SerializeField]
    [Tooltip("How quickly the camera zooms in/out when you scroll the mouse wheel.")]
    float cameraZoomSpeed = 1;

    float timeSinceLastTick = 0;

    void Start()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        Application.targetFrameRate = Mathf.Max(30, ticksPerSecond);
        mainCamera.orthographicSize = gameSize;

        timeSinceLastTick += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
            gamePaused = !gamePaused;

        if (!gamePaused && timeSinceLastTick >= 1 / (float)ticksPerSecond)
        {
            timeSinceLastTick = 0;
            Tick();
        }

        if (Input.mousePresent)
        {
            if (Input.GetMouseButton(0))
                SpawnCellAtCursor();
            if (Input.GetMouseButton(1))
                KillCellAtCursor();
            MoveCameraTowardCursor();

            if (Input.mouseScrollDelta.y != 0 && gameSize >= 0.1f)
                gameSize = Mathf.Max(gameSize + Input.mouseScrollDelta.y * cameraZoomSpeed, 0.1f);
        }
        Cell.drawFilledBox = gameSize < 20; // if the cells are really small, draw them more cheaply because you can barely tell either way
        DrawAllCells();
    }

    bool IsCellAlive(Vector2Int position)
    {
        return Grid.ContainsKey(position);
    }

    [ContextMenu("Tick")]
    public void Tick()
    {
        tickedCellsThisIteration.Clear();
        Dictionary<Vector2Int, Cell> NewGrid = new(Grid);
        foreach (KeyValuePair<Vector2Int, Cell> gridEntry in Grid)
        {
            Vector2Int position = gridEntry.Key;
            //Cell cell = gridEntry.Value;
            foreach (Vector2Int adjacentPosition in GetAdjacentCoordinates(position, includeCenter: true))
            {
                if (!tickedCellsThisIteration.Contains(adjacentPosition))
                {
                    TickCell(ref NewGrid, adjacentPosition);
                    tickedCellsThisIteration.Add(adjacentPosition);
                }
            }
        }
        Grid = NewGrid;
    }
    void TickCell(ref Dictionary<Vector2Int, Cell> gridToModify, Vector2Int position)
    {
        int aliveAdjacentCells = CountAdjacentAliveCells(position);
        if (IsCellAlive(position))
        {
            if (aliveAdjacentCells < 2 || aliveAdjacentCells > 3)
            {
                gridToModify.Remove(position);
            }
        }
        else if (aliveAdjacentCells == 3)
        {
            if (!gridToModify.ContainsKey(position))
                gridToModify.Add(position, new Cell(position));
        }
    }
    int CountAdjacentAliveCells(Vector2Int center)
    {
        int aliveCellsNearby = 0;
        foreach (Vector2Int adjacentPosition in GetAdjacentCoordinates(center))
        {
            if (IsCellAlive(adjacentPosition))
                aliveCellsNearby++;
        }
        return aliveCellsNearby;
    }

    IEnumerable<Vector2Int> GetAdjacentCoordinates(Vector2Int center, bool includeCenter = false)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (!includeCenter && i == 0 && j == 0) continue;
                yield return center + new Vector2Int(i, j);
            }
        }
    }

    [ContextMenu("Draw all cells")]
    public void DrawAllCells()
    {
        foreach (KeyValuePair<Vector2Int, Cell> gridEntry in Grid)
        {
            gridEntry.Value.Draw();
        }
    }

    [ContextMenu("Spawn random cells")]
    void SpawnRandomCells()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2Int cellPos = new(Random.Range(-10, 10), Random.Range(-10, 10));
            if (!Grid.ContainsKey(cellPos))
            {
                Grid.Add(cellPos, new Cell(cellPos));
            }
        }
    }
    [ContextMenu("Kill all cells")]
    void KillAllCells()
    {
        Grid.Clear();
    }

    private Vector2Int GetCellPositionAtCursor()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2Int(Mathf.RoundToInt(mouseWorldPosition.x), Mathf.RoundToInt(mouseWorldPosition.y));
    }

    void SpawnCellAtCursor()
    {
        Vector2Int cellPosition = GetCellPositionAtCursor();
        if (!IsCellAlive(cellPosition))
        {
            Grid.Add(cellPosition, new Cell(cellPosition));
        }
    }
    void KillCellAtCursor()
    {
        Vector2Int cellPosition = GetCellPositionAtCursor();
        if (IsCellAlive(cellPosition))
        {
            Grid.Remove(cellPosition);
        }
    }
    void MoveCameraTowardCursor()
    {
        Vector2 mouseViewportPos = mainCamera.ScreenToViewportPoint(Input.mousePosition);
        Vector2 moveDelta = Vector2.zero;

        if (mouseViewportPos.x > 1 - screenBorderWidth / mainCamera.aspect && mouseViewportPos.x < 1)
        {
            moveDelta.x = 1;
        }
        if (mouseViewportPos.x < screenBorderWidth / mainCamera.aspect && mouseViewportPos.x > 0)
        {
            moveDelta.x = -1;
        }
        if (mouseViewportPos.y > 1 - screenBorderWidth && mouseViewportPos.y < 1)
        {
            moveDelta.y = 1;
        }
        if (mouseViewportPos.y < screenBorderWidth && mouseViewportPos.y > 0)
        {
            moveDelta.y = -1;
        }

        if (moveDelta != Vector2.zero)
        {
            moveDelta *= gameSize * cameraMoveSpeed * Time.deltaTime;
            mainCamera.transform.Translate(moveDelta);
        }
    }
}

public class Cell
{
    public readonly Vector2Int position;
    public static bool drawFilledBox = true;

    public Cell(int x, int y)
    {
        position = new Vector2Int(x, y);
    }
    public Cell(Vector2Int position)
    {
        this.position = position;
    }

    public void Draw()
    {
        Color color = Color.white;
        float size = 0.5f;
        Vector2 center = new(position.x, position.y);

        // Draw border
        Debug.DrawLine(center + new Vector2(-size, size), center + new Vector2(size, size), color);
        Debug.DrawLine(center + new Vector2(size, size), center + new Vector2(size, -size), color);
        Debug.DrawLine(center + new Vector2(size, -size), center + new Vector2(-size, -size), color);
        Debug.DrawLine(center + new Vector2(-size, -size), center + new Vector2(-size, size), color);

        if (drawFilledBox)
        {
            Debug.DrawLine(center + new Vector2(-size, -size), center + new Vector2(size, size), color);
            Debug.DrawLine(center + new Vector2(-size, size), center + new Vector2(size, -size), color);
        }
    }
}
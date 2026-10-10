using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    // --- Configuration ---
    [Header("Assets")]
    [SerializeField] private MazeCell _mazeCellPrefab;

    [Header("Dimensions")]
    [SerializeField] public int _mazeWidth = 30;
    [SerializeField] public int _mazeHeight = 30;
    [SerializeField] public int _mazeDepth = 30;
    [SerializeField] public float cellSizeX = 1.2f;
    [SerializeField] public float cellSizeY = 1.1f;
    [SerializeField] public float cellSizeZ = 1.2f;

    [Header("Seed")]
    [SerializeField] private int _seed = -1;
    private int _usedSeed;

    private void Awake()
    {
        // Check if the Manager exists (it survived from the Menu Scene)
        if (MazeGameManager.Instance != null)
        {
            _mazeWidth = MazeGameManager.Instance.MazeWidth;
            _mazeHeight = MazeGameManager.Instance.MazeHeight;
            _mazeDepth = MazeGameManager.Instance.MazeDepth;
            _seed = MazeGameManager.Instance.Seed;
            
            Debug.Log($"Loaded settings from Manager: {_mazeWidth}x{_mazeHeight}x{_mazeDepth}");
        }
    }

    // --- Getters ---
    public int GetUsedSeed()
    {
        return _usedSeed;
    }

    public float CellSizeX => cellSizeX;
    public float CellSizeY => cellSizeY;
    public float CellSizeZ => cellSizeZ;

    // --- Data Storage ---
    public struct MazeCellData
    {
        public bool IsVisited;
        public int Distance;
        // true = wall exists (closed), false = no wall (open)
        public bool WallLeft, WallRight, WallUp, WallDown, WallFront, WallBack;
    }

    private MazeCellData[,,] _mazeData;
    
    // Tracking for End Block
    private Vector3Int _furthestCellPos; 
    private int _maxDistanceFound = 0;

    public bool IsGenerationComplete { get; private set; } = false;

    // --- 1. DATA GENERATION (Math Only) ---
    public void InitializeAndGenerate()
    {
        // 1. Setup Seed
        if (_seed == -1)
            _usedSeed = System.DateTime.Now.ToString().GetHashCode();
        else
            _usedSeed = _seed;

        Random.InitState(_usedSeed);
        Debug.Log($"Generating Maze Data for {_mazeWidth}x{_mazeHeight}x{_mazeDepth} (Seed: {_usedSeed})...");

        // 2. Initialize Data Array
        _mazeData = new MazeCellData[_mazeWidth, _mazeHeight, _mazeDepth];
        
        // IMPORTANT: Set defaults. All walls start CLOSED (true).
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int y = 0; y < _mazeHeight; y++)
            {
                for (int z = 0; z < _mazeDepth; z++)
                {
                    _mazeData[x, y, z] = new MazeCellData
                    {
                        IsVisited = false,
                        Distance = 0,
                        WallLeft = true, WallRight = true,
                        WallUp = true, WallDown = true,
                        WallFront = true, WallBack = true
                    };
                }
            }
        }

        // 3. Run Algorithm
        GenerateHuntAndKillData(new Vector3Int(0, 0, 0));
        
        IsGenerationComplete = true;
        Debug.Log($"Maze Data Ready. End is at {_furthestCellPos} (Dist: {_maxDistanceFound})");
    }

    private void GenerateHuntAndKillData(Vector3Int startPos)
    {
        Vector3Int currentPos = startPos;
        _mazeData[currentPos.x, currentPos.y, currentPos.z].IsVisited = true;
        
        _furthestCellPos = currentPos;
        _maxDistanceFound = 0;

        bool unvisitedCellsRemain = true;

        // Optimization: Keep track of where we stopped hunting last time
        // to avoid scanning 0,0,0 to 30,30,30 every time.
        int lastHuntIndex = 0; 
        int totalCells = _mazeWidth * _mazeHeight * _mazeDepth;

        while (unvisitedCellsRemain)
        {
            // KILL Phase: Walk randomly
            Vector3Int? nextPos = GetRandomUnvisitedNeighbor(currentPos);

            if (nextPos.HasValue)
            {
                Vector3Int next = nextPos.Value;
                RemoveWallsBetweenData(currentPos, next);
                
                // Update Data
                int newDist = _mazeData[currentPos.x, currentPos.y, currentPos.z].Distance + 1;
                _mazeData[next.x, next.y, next.z].IsVisited = true;
                _mazeData[next.x, next.y, next.z].Distance = newDist;
                
                // Track furthest
                if (newDist > _maxDistanceFound)
                {
                    _maxDistanceFound = newDist;
                    _furthestCellPos = next;
                }
                
                currentPos = next;
            }
            else
            {
                // HUNT Phase: Scan for an unvisited cell with a visited neighbor
                Vector3Int? huntResult = HuntForNewCellData(ref lastHuntIndex, totalCells);
                
                if (huntResult.HasValue)
                {
                    currentPos = huntResult.Value;
                }
                else
                {
                    unvisitedCellsRemain = false; // No more cells found
                }
            }
        }
    }

    // --- 2. INSTANTIATION (Visuals) ---
    public MazeCell InstantiateCellAt(int x, int y, int z, Transform parent)
    {
        if (!IsWithinBounds(new Vector3Int(x, y, z))) return null;

        Vector3 pos = new Vector3(x * cellSizeX, y * cellSizeY, z * cellSizeZ);
        MazeCell cell = Instantiate(_mazeCellPrefab, pos, Quaternion.identity, parent);
        cell.name = $"Cell_{x}_{y}_{z}";

        cell.Visit();

        MazeCellData data = _mazeData[x, y, z];

        bool isLeftBoundary = (x == 0);
        bool isRightBoundary = (x == _mazeWidth - 1);
        bool isBackBoundary = (z == 0);
        bool isFrontBoundary = (z == _mazeDepth - 1);
        bool isBottomBoundary = (y == 0);

        // Render only 3 faces per cell (Right, Front, Up) to prevent double walls.
        // Boundary walls always stay intact to contain the maze.
        if (!isLeftBoundary) cell.ClearLeftWall();
        if (!isBackBoundary) cell.ClearBackWall();
        if (!isBottomBoundary) cell.ClearDownWall();
        if (!isRightBoundary && !data.WallRight) cell.ClearRightWall();
        if (!isFrontBoundary && !data.WallFront) cell.ClearFrontWall();
        if (!data.WallUp) cell.ClearTopWall();

        if (x == _furthestCellPos.x && y == _furthestCellPos.y && z == _furthestCellPos.z)
            cell.ActivateEndBlock();

        return cell;
    }

    // --- Helpers ---

    private Vector3Int? HuntForNewCellData(ref int startIndex, int totalCells)
    {
        // Optimized Scan: Treat 3D array as 1D list to resume scanning efficiently
        for (int i = startIndex; i < totalCells; i++)
        {
            // Convert 1D index back to 3D coordinates
            int z = i % _mazeDepth;
            int y = (i / _mazeDepth) % _mazeHeight;
            int x = i / (_mazeDepth * _mazeHeight);

            // If unvisited...
            if (!_mazeData[x, y, z].IsVisited)
            {
                // ...check if it has a VISITED neighbor to connect to
                List<Vector3Int> visitedNeighbors = GetVisitedNeighborCoords(new Vector3Int(x, y, z));
                
                if (visitedNeighbors.Count > 0)
                {
                    Vector3Int neighbor = visitedNeighbors[Random.Range(0, visitedNeighbors.Count)];
                    
                    // Connect them
                    RemoveWallsBetweenData(new Vector3Int(x, y, z), neighbor);
                    
                    // Mark as visited
                    _mazeData[x, y, z].IsVisited = true;
                    _mazeData[x, y, z].Distance = _mazeData[neighbor.x, neighbor.y, neighbor.z].Distance + 1;

                    // Update furthest point tracking even during Hunt phase
                    if (_mazeData[x, y, z].Distance > _maxDistanceFound)
                    {
                        _maxDistanceFound = _mazeData[x, y, z].Distance;
                        _furthestCellPos = new Vector3Int(x, y, z);
                    }

                    // Save index for next time (optimization)
                    startIndex = i + 1;
                    return new Vector3Int(x, y, z);
                }
            }
        }
        
        // If we reach here, we scanned everything and found nothing
        return null; 
    }

    private List<Vector3Int> GetVisitedNeighborCoords(Vector3Int pos)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();
        // Standard Directions: Right, Left, Up, Down, Forward, Back
        Vector3Int[] dirs = { 
            new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0), new Vector3Int(0,-1,0),
            new Vector3Int(0,0,1), new Vector3Int(0,0,-1)
        };

        foreach (var dir in dirs)
        {
            Vector3Int target = pos + dir;
            if (IsWithinBounds(target) && _mazeData[target.x, target.y, target.z].IsVisited)
                neighbors.Add(target);
        }
        return neighbors;
    }

    private Vector3Int? GetRandomUnvisitedNeighbor(Vector3Int pos)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();
        Vector3Int[] dirs = { 
            new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0), new Vector3Int(0,-1,0),
            new Vector3Int(0,0,1), new Vector3Int(0,0,-1)
        };

        foreach (var dir in dirs)
        {
            Vector3Int target = pos + dir;
            if (IsWithinBounds(target) && !_mazeData[target.x, target.y, target.z].IsVisited)
                neighbors.Add(target);
        }

        return neighbors.Count > 0 ? neighbors[Random.Range(0, neighbors.Count)] : (Vector3Int?)null;
    }

    private void RemoveWallsBetweenData(Vector3Int a, Vector3Int b)
    {
        Vector3Int dir = b - a;

        // Note: We are modifying structs in the array directly. 
        // We set the wall to FALSE to indicate it is Open.
        
        if (dir.x > 0) // b is to the RIGHT of a
        { 
            _mazeData[a.x, a.y, a.z].WallRight = false; 
            _mazeData[b.x, b.y, b.z].WallLeft = false; 
        }
        else if (dir.x < 0) // b is to the LEFT of a
        { 
            _mazeData[a.x, a.y, a.z].WallLeft = false; 
            _mazeData[b.x, b.y, b.z].WallRight = false; 
        }
        else if (dir.y > 0) // b is ABOVE a
        { 
            _mazeData[a.x, a.y, a.z].WallUp = false; 
            _mazeData[b.x, b.y, b.z].WallDown = false; 
        }
        else if (dir.y < 0) // b is BELOW a
        { 
            _mazeData[a.x, a.y, a.z].WallDown = false; 
            _mazeData[b.x, b.y, b.z].WallUp = false; 
        }
        else if (dir.z > 0) // b is IN FRONT of a
        { 
            _mazeData[a.x, a.y, a.z].WallFront = false; 
            _mazeData[b.x, b.y, b.z].WallBack = false; 
        }
        else if (dir.z < 0) // b is BEHIND a
        { 
            _mazeData[a.x, a.y, a.z].WallBack = false; 
            _mazeData[b.x, b.y, b.z].WallFront = false; 
        }
    }

    private bool IsWithinBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < _mazeWidth &&
               pos.y >= 0 && pos.y < _mazeHeight &&
               pos.z >= 0 && pos.z < _mazeDepth;
    }
}
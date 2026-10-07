using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeChunkManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MazeGenerator _mazeGenerator;
    [SerializeField] private Transform _player;

    [Header("Chunk Settings")]
    [Tooltip("Size of a chunk in cells (e.g. 5 = 5x5x5 cells)")]
    [SerializeField] private int _chunkSize = 5;
    
    [Tooltip("Radius of chunks to render around player (1 = 3x3x3 chunks, 2 = 5x5x5 chunks)")]
    [SerializeField] private int _renderDistance = 1;

    // Track instantiated chunks: Key = Chunk Coordinate (not world pos), Value = Container Object
    private Dictionary<Vector3Int, GameObject> _activeChunks = new Dictionary<Vector3Int, GameObject>();
    
    private Vector3Int _lastPlayerChunkCoord = new Vector3Int(-999, -999, -999);

    void Awake()
    {
        if (MazeGameManager.Instance != null)
        {
            _chunkSize = MazeGameManager.Instance.ChunkSize;
            _renderDistance = MazeGameManager.Instance.RenderDistance;
        }
    }

    void Start()
    {
        // 2. Locate Generator if missing
        if (_mazeGenerator == null) _mazeGenerator = Object.FindAnyObjectByType<MazeGenerator>();
        
        // 3. Generate the math/data for the whole maze
        // The Generator will use the settings we injected in its Awake()
        _mazeGenerator.InitializeAndGenerate();
        
        // 4. Initial Chunk Update
        // Ensure player is assigned, or find it by tag
        if (_player == null) 
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }

        if (_player != null)
        {
             UpdateChunks();
        }
    }

    void Update()
    {
        if (!_mazeGenerator.IsGenerationComplete || _player == null) return;

        Vector3Int currentChunk = GetChunkCoordinate(_player.position);

        // Only update if player crossed into a new chunk
        if (currentChunk != _lastPlayerChunkCoord)
        {
            _lastPlayerChunkCoord = currentChunk;
            UpdateChunks();
        }
    }

    private void UpdateChunks()
    {
        List<Vector3Int> chunksToKeep = new List<Vector3Int>();

        // 1. Determine which chunks should be visible
        for (int x = -_renderDistance; x <= _renderDistance; x++)
        {
            for (int y = -_renderDistance; y <= _renderDistance; y++)
            {
                for (int z = -_renderDistance; z <= _renderDistance; z++)
                {
                    Vector3Int chunkCoord = _lastPlayerChunkCoord + new Vector3Int(x, y, z);
                    
                    // Check if this chunk is actually inside the maze boundaries
                    if (IsChunkValid(chunkCoord))
                    {
                        chunksToKeep.Add(chunkCoord);
                        
                        if (!_activeChunks.ContainsKey(chunkCoord))
                        {
                            CreateChunk(chunkCoord);
                        }
                    }
                }
            }
        }

        // 2. Remove chunks that are far away
        // We use a separate list to avoid modifying the dictionary while iterating
        List<Vector3Int> chunksToRemove = new List<Vector3Int>();
        foreach (var kvp in _activeChunks)
        {
            if (!chunksToKeep.Contains(kvp.Key))
            {
                chunksToRemove.Add(kvp.Key);
            }
        }

        foreach (var coord in chunksToRemove)
        {
            Destroy(_activeChunks[coord]);
            _activeChunks.Remove(coord);
        }
        
        Debug.Log($"Active Chunks: {_activeChunks.Count}");
    }

    private void CreateChunk(Vector3Int chunkCoord)
    {
        GameObject chunkContainer = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}");
        chunkContainer.transform.parent = this.transform;
        
        // Calculate start and end indices for cells in this chunk
        int startX = chunkCoord.x * _chunkSize;
        int startY = chunkCoord.y * _chunkSize;
        int startZ = chunkCoord.z * _chunkSize;

        // Loop through the 5x5x5 area (or whatever chunkSize is)
        for (int x = 0; x < _chunkSize; x++)
        {
            for (int y = 0; y < _chunkSize; y++)
            {
                for (int z = 0; z < _chunkSize; z++)
                {
                    int gridX = startX + x;
                    int gridY = startY + y;
                    int gridZ = startZ + z;

                    // Ask Generator to spawn the cell
                    _mazeGenerator.InstantiateCellAt(gridX, gridY, gridZ, chunkContainer.transform);
                }
            }
        }

        _activeChunks.Add(chunkCoord, chunkContainer);
    }

    private Vector3Int GetChunkCoordinate(Vector3 position)
    {
        float realChunkSize = _chunkSize * _mazeGenerator.cellSize;
        
        return new Vector3Int(
            Mathf.FloorToInt(position.x / realChunkSize),
            Mathf.FloorToInt(position.y / realChunkSize),
            Mathf.FloorToInt(position.z / realChunkSize)
        );
    }

    private bool IsChunkValid(Vector3Int chunkCoord)
    {
        // Calculate the maximum possible chunk coordinate
        int maxChunkX = Mathf.CeilToInt((float)_mazeGenerator._mazeWidth / _chunkSize);
        int maxChunkY = Mathf.CeilToInt((float)_mazeGenerator._mazeHeight / _chunkSize);
        int maxChunkZ = Mathf.CeilToInt((float)_mazeGenerator._mazeDepth / _chunkSize);

        return chunkCoord.x >= 0 && chunkCoord.x < maxChunkX &&
               chunkCoord.y >= 0 && chunkCoord.y < maxChunkY &&
               chunkCoord.z >= 0 && chunkCoord.z < maxChunkZ;
    }
}
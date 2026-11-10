using System.Collections.Generic;
using UnityEngine;

public class MazeChunkManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MazeGenerator _mazeGenerator;
    [SerializeField] private Transform _player;

    private int _chunkSize;
    private int _renderDistance;

    private readonly Dictionary<Vector3Int, GameObject> _activeChunks = new Dictionary<Vector3Int, GameObject>();
    private readonly HashSet<Vector3Int> _keep = new HashSet<Vector3Int>();
    private readonly List<Vector3Int> _remove = new List<Vector3Int>();
    private Vector3Int _lastPlayerChunkCoord;

    private void Start()
    {
        _chunkSize = LocalSettings.ChunkSize;
        _renderDistance = LocalSettings.RenderDistance;

        if (_mazeGenerator == null) _mazeGenerator = Object.FindAnyObjectByType<MazeGenerator>();
        if (_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }

        if (GameSession.HasConfig) _mazeGenerator.Configure(GameSession.Config);
        _mazeGenerator.InitializeAndGenerate();

        if (_player != null)
        {
            _lastPlayerChunkCoord = GetChunkCoordinate(_player.position);
            UpdateChunks();
        }
    }

    private void Update()
    {
        if (_player == null || !_mazeGenerator.IsGenerationComplete) return;

        Vector3Int current = GetChunkCoordinate(_player.position);
        if (current == _lastPlayerChunkCoord) return;

        _lastPlayerChunkCoord = current;
        UpdateChunks();
    }

    private void UpdateChunks()
    {
        _keep.Clear();

        for (int x = -_renderDistance; x <= _renderDistance; x++)
        for (int y = -_renderDistance; y <= _renderDistance; y++)
        for (int z = -_renderDistance; z <= _renderDistance; z++)
        {
            Vector3Int coord = _lastPlayerChunkCoord + new Vector3Int(x, y, z);
            if (!IsChunkValid(coord)) continue;

            _keep.Add(coord);
            if (!_activeChunks.ContainsKey(coord)) CreateChunk(coord);
        }

        _remove.Clear();
        foreach (var kvp in _activeChunks)
            if (!_keep.Contains(kvp.Key)) _remove.Add(kvp.Key);

        foreach (var coord in _remove)
        {
            Destroy(_activeChunks[coord]);
            _activeChunks.Remove(coord);
        }
    }

    private void CreateChunk(Vector3Int chunkCoord)
    {
        var container = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}_{chunkCoord.z}");
        container.transform.parent = transform;

        int startX = chunkCoord.x * _chunkSize;
        int startY = chunkCoord.y * _chunkSize;
        int startZ = chunkCoord.z * _chunkSize;

        for (int x = 0; x < _chunkSize; x++)
        for (int y = 0; y < _chunkSize; y++)
        for (int z = 0; z < _chunkSize; z++)
            _mazeGenerator.InstantiateCellAt(startX + x, startY + y, startZ + z, container.transform);

        _activeChunks.Add(chunkCoord, container);
    }

    private Vector3Int GetChunkCoordinate(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / (_chunkSize * _mazeGenerator.CellSizeX)),
            Mathf.FloorToInt(position.y / (_chunkSize * _mazeGenerator.CellSizeY)),
            Mathf.FloorToInt(position.z / (_chunkSize * _mazeGenerator.CellSizeZ))
        );
    }

    private bool IsChunkValid(Vector3Int c)
    {
        int maxX = Mathf.CeilToInt((float)_mazeGenerator._mazeWidth / _chunkSize);
        int maxY = Mathf.CeilToInt((float)_mazeGenerator._mazeHeight / _chunkSize);
        int maxZ = Mathf.CeilToInt((float)_mazeGenerator._mazeDepth / _chunkSize);

        return c.x >= 0 && c.x < maxX &&
               c.y >= 0 && c.y < maxY &&
               c.z >= 0 && c.z < maxZ;
    }
}

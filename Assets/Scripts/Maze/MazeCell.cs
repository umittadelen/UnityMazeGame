using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField]
    private GameObject _leftWall;

    [SerializeField]
    private GameObject _rightWall;

    [SerializeField]
    private GameObject _frontWall;

    [SerializeField]
    private GameObject _backWall;

    [SerializeField]
    private GameObject _topWall;

    [SerializeField]
    private GameObject _topWallLight;

    [SerializeField]
    private GameObject _downWall;

    [SerializeField]
    private GameObject _unvisitedBlock;

    [SerializeField]
    private GameObject _endBlock;

    public void Visit()
    {
        _unvisitedBlock.SetActive(false);
        _endBlock.SetActive(false);
    }

    public void ClearLeftWall()
    {
        _leftWall.SetActive(false);
    }

    public void ClearRightWall()
    {
        _rightWall.SetActive(false);
    }

    public void ClearFrontWall()
    {
        _frontWall.SetActive(false);
    }

    public void ClearBackWall()
    {
        _backWall.SetActive(false);
    }

    // roll in [0,1): pass a deterministic value so lights match on every client and when chunks reload
    public void InitializeTopWall(float roll)
    {
        if (roll < 0.05f) // 5% chance
        {
            _topWall.SetActive(false);
            _topWallLight.SetActive(true);
        }
        else
        {
            _topWall.SetActive(true);
            _topWallLight.SetActive(false);
        }
    }

    public void ClearTopWall()
    {
        _topWall.SetActive(false);
        _topWallLight.SetActive(false);
    }

    public void ClearDownWall()
    {
        _downWall.SetActive(false);
    }

    public void ActivateEndBlock()
    {
        _endBlock.SetActive(true);
    }
}

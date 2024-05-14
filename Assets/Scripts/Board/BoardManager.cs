using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum MouseMoveDirection
{
    MOUSEMOVEUP,
    MOUSEMOVEDOWN,
    MOUSEMOVERIGHT,
    MOUSEMOVELEFT
};

public enum GamePlayState
{
    INPUTOK, 
    AFTERINPUTMOVECHECK,
    MATCHECK,
    AFTERMATCHECK_MOVECHECK,
    DROPBLOCK,
    AFTERDROPBLOCKBLOCK_MOVECHECK,
    INPUTCANCEL,
    UNDOBLOCK,
    DROPBLOCKMATCHCHECK,
    AFTERMATCHEFFECTPLAY
};

public class BoardManager : MonoBehaviour
{
    [SerializeField] private Sprite[] _Sprites;
    [SerializeField] private GameObject _BlockPrefab;
    private GameObject[,] _GameBoard;

    private Vector2 _screenPos;
    private float _ScreenWidth;
    private float _BlockWidth;

    public float _XMargin = 0.5f;
    public float _YMargin = 2;

    private float _Scale = 0.0f;

    [HideInInspector]
    public int _Column = 0;

    [HideInInspector]
    public int _Row = 0;

    private Vector3 _StartPos = Vector3.zero;
    private Vector3 _EndPos = Vector3.zero; 

    private GameObject _ClickObject; 
    private MouseMoveDirection _CurrentMoveDirection;
    private bool _MouseClick = false;

    [SerializeField] private float _MoveDistance = 0.01f;


    private bool _InputOk = true;

    private int TYPECOUNT = 6;

    private List<GameObject> _RemovingBlocks = new List<GameObject>();
    private List<GameObject> _RemovedBlocks = new List<GameObject>();

    [SerializeField] private GamePlayState _playState;
    private GamePlayState PlayState
    {
        set => _playState = value;
        get => _playState;

    }

    [SerializeField] private float _YPOS = 3.0f;

    private const int MATCHCOUNT = 3;

    [SerializeField] private ShuffleInformPopUp _ShuffleInformPopUp;
    [SerializeField] private GameUI _GameUI;

    private int _scoreValue = 0;
    public int ScoreValue
    {
        set
        {
            _scoreValue = value;
            _GameUI.SetScore(_scoreValue);
        }

        get => _scoreValue;
    }

    private const int BLOCKSCORE = 100;

    private void Awake()
    {
        _screenPos = Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 10.0f));

        Debug.Log($"_screenPos(0,0)=> WorldPos: {_screenPos}");

        _screenPos.y = -_screenPos.y;
        _ScreenWidth = Mathf.Abs(_screenPos.x + _screenPos.x);

        _BlockWidth = _BlockPrefab.GetComponent<Block>()._BlockImage.sprite.rect.size.x / 100;

        _playState = GamePlayState.INPUTCANCEL;
    }
    void Start()
    {
          AudioManager.Instance.PlayBgMusic();
    }

    private void Play()
    {
        PlayState = GamePlayState.AFTERINPUTMOVECHECK;
    }
    public void GameStart()
    {
        ScoreValue = 0;
        this.gameObject.SetActive(true);
        MakeBoard(5, 5);
        Invoke("Play", 1.0f);
    }

    void MakeBoard(int column, int row)
    {
        float width = _ScreenWidth - (_XMargin * 2);
        float blockWidth = _BlockWidth * row;

        _Scale = width / blockWidth;

        _MouseClick = false;
        if(_GameBoard != null)
        {
            foreach(var obj in _GameBoard)
            {
                if(obj != null)
                {
                    Destroy(obj);
                }
            }
        }
        _GameBoard = new GameObject[column,row];

        _Column = column;
        _Row = row;

        _GameBoard = new GameObject[column, row];

        for (int col = 0; col < column; col++)
        {
            for (int ro = 0; ro < row; ro++)
            {
                _GameBoard[col, ro] = Instantiate(_BlockPrefab) as GameObject;

                _GameBoard[col, ro].transform.localScale = new Vector3(_Scale, _Scale, 0.0f);

                _GameBoard[col, ro].transform.position =
                    new Vector3(_XMargin + _screenPos.x + ro * (_BlockWidth * _Scale) + (_BlockWidth * _Scale) / 2, _screenPos.y + -col * (_BlockWidth * _Scale) - (_BlockWidth * _Scale) / 2 - _YMargin, 0.0f);

                int type = UnityEngine.Random.Range(4, TYPECOUNT + 4);

                _GameBoard[col, ro].GetComponent<Block>().Type = type;
                _GameBoard[col, ro].GetComponent<Block>()._BlockImage.sprite = _Sprites[type];


                var block = _GameBoard[col, ro].GetComponent<Block>();
                block.Width = (_BlockWidth * _Scale);
                block.MovePos = _GameBoard[col, ro].transform.position;
                block.Column = col;
                block.Row = ro;

                _GameBoard[col, ro].name = $"Block[{col}, {ro}]";
            }
        }
    }

    private float CalculateAngle(Vector3 from, Vector3 to)
    {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
    }

    private MouseMoveDirection CalculateDirection()
    {
        float angle = CalculateAngle(_StartPos, _EndPos);

        if (angle >= 315.0f && angle <= 360.0f || angle >= 0 && angle < 45.0f)
        {
            return MouseMoveDirection.MOUSEMOVEUP;
        }
        else if (angle >= 45.0f && angle < 135.0f)
        {
            return MouseMoveDirection.MOUSEMOVELEFT;
        }
        else if (angle >= 135.0f && angle < 225.0f)
        {
            return MouseMoveDirection.MOUSEMOVEDOWN;
        }
        else if (angle >= 225.0f && angle < 315.0f)
        {
            return MouseMoveDirection.MOUSEMOVERIGHT;
        }
        else
        {
            return MouseMoveDirection.MOUSEMOVEDOWN;
        }
    }

    private bool CheckBlockMove()
    {
        foreach (var obj in _GameBoard)
        {
            if (obj != null)
            {
                if (obj.GetComponent<Block>().State == BLOCKSTATE.MOVE)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CheckBlockEffect()
    {
        foreach (var obj in _GameBoard)
        {
            if (obj != null)
            {
                if (obj.GetComponent<Block>().State == BLOCKSTATE.EFFECT)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void UNdoBlock()
    {

        _ClickObject.GetComponent<Block>()._BlockImage.sortingOrder = 1;

        switch (_CurrentMoveDirection)
        {
            case MouseMoveDirection.MOUSEMOVELEFT:
                {
                    int column = _ClickObject.GetComponent<Block>().Column;
                    int row = _ClickObject.GetComponent<Block>().Row;

                    var leftBlock = _GameBoard[column, row].GetComponent<Block>();
                    var rightBlock = _GameBoard[column, row + 1].GetComponent<Block>();

                    leftBlock.Row = row + 1;
                    rightBlock.Row = row;

                    _GameBoard[column, row] = _GameBoard[column, row + 1];
                    _GameBoard[column, row + 1] = _ClickObject;

                    _GameBoard[column, row].GetComponent<Block>().Move(Direction.LEFT);
                    _GameBoard[column, row + 1].GetComponent<Block>().Move(Direction.RIGHT);

                    PlayState = GamePlayState.AFTERINPUTMOVECHECK;

                }
                break;

            case MouseMoveDirection.MOUSEMOVERIGHT:
                {
                    int column = _ClickObject.GetComponent<Block>().Column;
                    int row = _ClickObject.GetComponent<Block>().Row;

                    _GameBoard[column, row].GetComponent<Block>().Row = row - 1;
                    _GameBoard[column, row - 1].GetComponent<Block>().Row = row;

                    _GameBoard[column, row] = _GameBoard[column, row - 1];
                    _GameBoard[column, row - 1] = _ClickObject;

                    _GameBoard[column, row].GetComponent<Block>().Move(Direction.RIGHT);
                    _GameBoard[column, row - 1].GetComponent<Block>().Move(Direction.LEFT);

                    PlayState = GamePlayState.AFTERINPUTMOVECHECK;
                }
                break;

            case MouseMoveDirection.MOUSEMOVEUP:
                {
                    int column = _ClickObject.GetComponent<Block>().Column;
                    int row = _ClickObject.GetComponent<Block>().Row;

                    _GameBoard[column, row].GetComponent<Block>().Column = column + 1;
                    _GameBoard[column + 1, row].GetComponent<Block>().Column = column;

                    _GameBoard[column, row] = _GameBoard[column + 1, row];
                    _GameBoard[column + 1, row] = _ClickObject;

                    _GameBoard[column, row].GetComponent<Block>().Move(Direction.UP);
                    _GameBoard[column + 1, row].GetComponent<Block>().Move(Direction.DOWN);

                    PlayState = GamePlayState.AFTERINPUTMOVECHECK;
                }
                break;

            case MouseMoveDirection.MOUSEMOVEDOWN:
                {
                    int column = _ClickObject.GetComponent<Block>().Column;
                    int row = _ClickObject.GetComponent<Block>().Row;

                    _GameBoard[column, row].GetComponent<Block>().Column = column - 1;
                    _GameBoard[column - 1, row].GetComponent<Block>().Column = column;

                    _GameBoard[column, row] = _GameBoard[column - 1, row];
                    _GameBoard[column - 1, row] = _ClickObject;

                    _GameBoard[column, row].GetComponent<Block>().Move(Direction.DOWN);
                    _GameBoard[column - 1, row].GetComponent<Block>().Move(Direction.UP);

                    PlayState = GamePlayState.AFTERINPUTMOVECHECK;
                }
                break;
        }




    }
    private void DownMoveBlocks()
    {
        int moveCount = 0;

        for (int row = 0; row < _Row; row++)
        {
            for (int col = _Column - 1; col >= 0; col--)
            {
                if (_GameBoard[col, row] == null)
                {
                    moveCount++;
                }
                else  
                {
                    if (moveCount > 0)
                    {
                        var block = _GameBoard[col, row].GetComponent<Block>();

                        block.MovePos = block.transform.position;

                        block.MovePos = new Vector3(block.MovePos.x, block.MovePos.y - block.Width * moveCount, block.MovePos.z);
                        
                        _GameBoard[col, row] = null;

                       
                        block.Column = block.Column + moveCount;
                        block.gameObject.name = string.Format("Block[{0}, {1}]", block.Column, block.Row);

                 
                        _GameBoard[block.Column, block.Row] = block.gameObject;

                        block.Move(Direction.DOWN, moveCount);
                    }

                }
            }

            moveCount = 0;
        }

    }


    private bool CheckAfterMoveMatchBlock()
    {
        int checkType = -1;

        for (int row = 0; row < _Row; row++)
        {
            for (int col = _Column - 1; col >= (MATCHCOUNT - 1); col--)
            {
                if (row >= 0 && row < (_Row - 1))
                {
                    checkType = _GameBoard[col, row + 1].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col - 1, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col - 2, row].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    checkType = _GameBoard[col - 1, row + 1].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col - 2, row].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    checkType = _GameBoard[col - 2, row + 1].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col - 1, row].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }

                if ((row > 0) && (row <= _Row - 1))
                {
                    checkType = _GameBoard[col, row - 1].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col - 1, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col - 2, row].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    checkType = _GameBoard[col - 1, row - 1].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col - 2, row].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    checkType = _GameBoard[col - 2, row - 1].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col - 1, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }

                // 0 0
                // 0 x
                // x 0
                // 0 0

                if (col >= MATCHCOUNT && col < (_Column))
                {
                    checkType = _GameBoard[col, row].GetComponent<Block>().Type;
                    // 0
                    // 0
                    // x
                    // 0
                    if ((checkType == _GameBoard[col - 2, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col - 3, row].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 0
                    // x
                    // 0
                    // 0
                    checkType = _GameBoard[col - 3, row].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col - 1, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }
            }
        }


        for (int col = 0; col < _Column; col++)
        {
            for (int row = 0; row < (_Row - MATCHCOUNT + 1); row++)
            {

                if (col >= 0 && col < (_Column - 1))
                {
                    // x 0 0
                    // 0 x x
                    checkType = _GameBoard[col + 1, row].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row + 1].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row + 2].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 0 x 0
                    // x 0 x
                    checkType = _GameBoard[col + 1, row + 1].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row + 2].GetComponent<Block>().Type))
                    {
                        return true;
                    }


                    // 0 0 x
                    // x x 0        
                    checkType = _GameBoard[col + 1, row + 2].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row + 1].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                }

                if ((col > 0) && (col <= (_Column - 1)))
                {
                    // 0 x x
                    // x 0 0
                    checkType = _GameBoard[col - 1, row].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row + 1].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row + 2].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // x 0 x
                    // 0 x 0
                    checkType = _GameBoard[col - 1, row + 1].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row + 2].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // x x 0
                    // 0 0 x
                    checkType = _GameBoard[col - 1, row + 2].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row + 1].GetComponent<Block>().Type))
                    {
                        return true;
                    }
                }

                if ((row >= 0) && (row < (_Row - MATCHCOUNT)))
                {
                    // 0 x 0 0
                    checkType = _GameBoard[col, row].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row + 2].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row + 3].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                    // 0 0 x 0
                    checkType = _GameBoard[col, row + 3].GetComponent<Block>().Type;

                    if ((checkType == _GameBoard[col, row].GetComponent<Block>().Type) &&
                        (checkType == _GameBoard[col, row + 1].GetComponent<Block>().Type))
                    {
                        return true;
                    }

                }
            }
        }


        return false;
    }

    private GameObject GetNewBlock(int column, int row, int type)
    {
        if (_RemovedBlocks.Count <= 0)
        {
            return null;
        }

        GameObject obj = _RemovedBlocks[0]; 

        obj.GetComponent<Block>().Init(column, row, type, _Sprites[type]);

        _RemovedBlocks.Remove(obj);

        return obj;
    }


    private void CreateMoveBlocks()
    {
        int moveCount = 0;

        for (int row = 0; row < _Row; row++)
        {
            for (int col = _Column - 1; col >= 0; col--)
            {
                if (_GameBoard[col, row] == null)
                {
                    int type = UnityEngine.Random.Range(4, TYPECOUNT + 4);
                    _GameBoard[col, row] = GetNewBlock(col, row, type);
                    _GameBoard[col, row].name = string.Format("Block[{0}, {1}]", col, row);
                    _GameBoard[col, row].gameObject.SetActive(true);

                    var block = _GameBoard[col, row].GetComponent<Block>();

                    _GameBoard[col, row].transform.position =
                        new Vector3(_screenPos.x + _XMargin + (_BlockWidth * _Scale) / 2 + row * (_BlockWidth * _Scale),
                                    _screenPos.y - _YMargin - col * (_BlockWidth * _Scale) - (_BlockWidth * _Scale) / 2, 0.0f);

                    block.MovePos = block.transform.position;  

                    float moveYpos = _GameBoard[col, row].GetComponent<Block>().MovePos.y +
                        (_BlockWidth * _Scale) * moveCount++ + _YPOS;

                    _GameBoard[col, row].transform.position = new Vector3(_GameBoard[col, row].GetComponent<Block>().MovePos.x,
                        moveYpos, _GameBoard[col, row].GetComponent<Block>().MovePos.z);


                    block.Move(Direction.DOWN, moveCount);

                }
            }

            moveCount = 0;

        }


    }

    private bool CheckAllBlockInGameBoard()
    {
        foreach (var obj in _GameBoard)
        {
            if (obj == null)
            {
                return false;
            }
        }

        return true;

    }
    private bool CheckMatchBlock()
    {
        List<GameObject> matchList = new List<GameObject>();  
        List<GameObject> tempMatchList = new List<GameObject>();   

        int checkType = 0;

        _RemovingBlocks.Clear();  

        for (int row = 0; row < _Row; row++)
        {
            if (_GameBoard[0, row] == null)
            {
                continue;
            }

            checkType = _GameBoard[0, row].GetComponent<Block>().Type;

            tempMatchList.Add(_GameBoard[0, row]); 

            for (int col = 1; col < _Column; col++)
            {
                if (_GameBoard[col, row] == null)
                {
                    continue;
                }

                if (checkType == _GameBoard[col, row].GetComponent<Block>().Type)
                {
                    tempMatchList.Add(_GameBoard[col, row]);
                }
                else 
                {
                    
                    if (tempMatchList.Count >= 3)
                    {
                        matchList.AddRange(tempMatchList);
                        tempMatchList.Clear();

                        
                        checkType = _GameBoard[col, row].GetComponent<Block>().Type;
                        tempMatchList.Add(_GameBoard[col, row]);
                    }
                    else 
                    {
                        tempMatchList.Clear(); 

                        checkType = _GameBoard[col, row].GetComponent<Block>().Type;
                        tempMatchList.Add(_GameBoard[col, row]);
                    }
                }
            }


            if (tempMatchList.Count >= 3)
            {
                matchList.AddRange(tempMatchList);
                tempMatchList.Clear();
            }
            else
            {
                tempMatchList.Clear();
            }
        }

        for (int col = 0; col < _Column; col++)
        {
            if (_GameBoard[col, 0] == null)
            {
                continue;
            }

            checkType = _GameBoard[col, 0].GetComponent<Block>().Type; 
            tempMatchList.Add(_GameBoard[col, 0]);

            for (int row = 1; row < _Row; row++)
            {
                if (_GameBoard[col, row] == null)
                {
                    continue;
                }

                if (checkType == _GameBoard[col, row].GetComponent<Block>().Type)
                {
                    tempMatchList.Add(_GameBoard[col, row]);
                }
                else  
                {
                    if (tempMatchList.Count >= 3)
                    {
                        matchList.AddRange(tempMatchList);
                        tempMatchList.Clear();

                        checkType = _GameBoard[col, row].GetComponent<Block>().Type;
                        tempMatchList.Add(_GameBoard[col, row]);
                    }
                    else  
                    {
                        tempMatchList.Clear();

                        checkType = _GameBoard[col, row].GetComponent<Block>().Type;
                        tempMatchList.Add(_GameBoard[col, row]);
                    }
                }
            }

            if (tempMatchList.Count >= 3)
            {
                matchList.AddRange(tempMatchList);
                tempMatchList.Clear();
            }
            else
            {
                tempMatchList.Clear();
            }
        }

        matchList = matchList.Distinct().ToList();

        if (matchList.Count > 0)
        {
            foreach (var obj in matchList)
            {
                
                _GameBoard[obj.GetComponent<Block>().Column, obj.GetComponent<Block>().Row] = null;

                obj.SetActive(false);
            }
            _RemovingBlocks.AddRange(matchList);

            ScoreValue += matchList.Count * BLOCKSCORE;

            
            _RemovedBlocks.AddRange(_RemovingBlocks);  

            _RemovedBlocks = _RemovedBlocks.Distinct().ToList();  

            DownMoveBlocks();

            return true;   
        }
        else
        {
            return false; 
        }



    }

    private void MouseMove()
    {
        float diff = Vector2.Distance(_StartPos, _EndPos);

        if (diff > _MoveDistance && _ClickObject != null && _MouseClick)
        {
            _MouseClick = false;


           
            MouseMoveDirection dir = CalculateDirection();

            _CurrentMoveDirection = dir;    


            int column = _ClickObject.GetComponent<Block>().Column;
            int row = _ClickObject.GetComponent<Block>().Row;

            switch (dir)
            {
                case MouseMoveDirection.MOUSEMOVELEFT:
                    {
                        if (row > 0)
                        {
                          
                            _GameBoard[column, row].GetComponent<Block>().Row = row - 1;
                            _GameBoard[column, row - 1].GetComponent<Block>().Row = row;

                            _GameBoard[column, row] = _GameBoard[column, row - 1];
                            _GameBoard[column, row - 1] = _ClickObject;

                            _GameBoard[column, row].GetComponent<Block>().Move(Direction.RIGHT);
                            _GameBoard[column, row - 1].GetComponent<Block>().Move(Direction.LEFT);

                            PlayState = GamePlayState.AFTERINPUTMOVECHECK;
                        }
                    }
                    break;

                case MouseMoveDirection.MOUSEMOVERIGHT:
                    {
                        if (row < (_Row - 1))
                        {
                            _GameBoard[column, row].GetComponent<Block>().Row = row + 1;
                            _GameBoard[column, row + 1].GetComponent<Block>().Row = row;

                            _GameBoard[column, row] = _GameBoard[column, row + 1];
                            _GameBoard[column, row + 1] = _ClickObject;

                           
                            _GameBoard[column, row].GetComponent<Block>().Move(Direction.LEFT);
                            _GameBoard[column, row + 1].GetComponent<Block>().Move(Direction.RIGHT);

                            PlayState = GamePlayState.AFTERINPUTMOVECHECK;

                        }
                    }

                    break;

                case MouseMoveDirection.MOUSEMOVEUP:
                    {
                        if (column > 0)
                        {
                            _GameBoard[column, row].GetComponent<Block>().Column = column - 1;
                            _GameBoard[column - 1, row].GetComponent<Block>().Column = column;

                            _GameBoard[column, row] = _GameBoard[column - 1, row];
                            _GameBoard[column - 1, row] = _ClickObject;

                            _GameBoard[column, row].GetComponent<Block>().Move(Direction.DOWN);
                            _GameBoard[column - 1, row].GetComponent<Block>().Move(Direction.UP);

                            PlayState = GamePlayState.AFTERINPUTMOVECHECK;

                        }
                    }
                    break;

                case MouseMoveDirection.MOUSEMOVEDOWN:
                    {
                        if (column < (_Column - 1))
                        {
                            _GameBoard[column, row].GetComponent<Block>().Column = column + 1;
                            _GameBoard[column + 1, row].GetComponent<Block>().Column = column;

                            _GameBoard[column, row] = _GameBoard[column + 1, row];
                            _GameBoard[column + 1, row] = _ClickObject;

                            _GameBoard[column, row].GetComponent<Block>().Move(Direction.UP);
                            _GameBoard[column + 1, row].GetComponent<Block>().Move(Direction.DOWN);

                            PlayState = GamePlayState.AFTERINPUTMOVECHECK;

                        }
                    }
                    break;
            }
        }

    }

    private void InputProcess()
    {    
        if (Input.GetMouseButtonDown(0))
        {
            _MouseClick = true;

            _EndPos = _StartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _EndPos.z = _StartPos.z = 0.0f;


            for (int i = 0; i < _Column; i++)
            {
                for (int j = 0; j < _Row; j++)
                {
                    bool isClick = _GameBoard[i, j].GetComponent<Block>()._BlockImage.GetComponent<SpriteRenderer>().bounds.Contains(_StartPos);

                    if (isClick)
                    {
                        _ClickObject = _GameBoard[i, j];
                        Debug.Log("_ClickObject = " + _ClickObject.name);

                        goto LoopExit;
                    }
                }
            }

        LoopExit:;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _MouseClick = false;
            _ClickObject = null;
            _InputOk = true;
        }
        if ((_MouseClick == true) && ((Input.GetAxis("Mouse X") < 0) || Input.GetAxis("Mouse X") > 0) ||
            (Input.GetAxis("Mouse Y") < 0) || (Input.GetAxis("Mouse Y") > 0))
        {
            _EndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _EndPos.z = 0.0f;

            MouseMove();

        }

    }

    private void ShuffleGameBoard()
    {
        List<Block> shuffleBlockList = new List<Block>();

        foreach (var obj in _GameBoard)
        {
            shuffleBlockList.Add(obj.GetComponent<Block>());
        }

        var rnd = new System.Random();

        var randomize = shuffleBlockList.OrderBy(item => rnd.Next());

        List<Block> shuffledListBlockList = new List<Block>();

        foreach (var obj in randomize)
        {
            shuffledListBlockList.Add(obj);
        }

        for (int i = 0; i < _Column; i++)
        {
            for (int j = 0; j < _Row; j++)
            {
                _GameBoard[i, j] = shuffledListBlockList[i * _Row + j].gameObject;
            }
        }

        for (int col = 0; col < _Column; col++)
        {
            for (int ro = 0; ro < _Row; ro++)
            {
                _GameBoard[col, ro].transform.position =
                    new Vector3(_screenPos.x + _XMargin + ro * (_BlockWidth * _Scale) + (_BlockWidth * _Scale) / 2.0f,
                    _screenPos.y - _YMargin - col * (_BlockWidth * _Scale) - (_BlockWidth * _Scale) / 2.0f, 0.0f);

                _GameBoard[col, ro].name = string.Format("Block[{0}, {1}", col, ro);

                _GameBoard[col, ro].GetComponent<Block>().Column = col;
                _GameBoard[col, ro].GetComponent<Block>().Row = ro;
            }
        }

    }

    void Update()
    {

        switch (PlayState)
        {
            case GamePlayState.INPUTOK:
                {
                    InputProcess();
                }

                break;

            case GamePlayState.AFTERINPUTMOVECHECK:
                {
                    if (!CheckBlockMove())
                    {
                        PlayState = GamePlayState.MATCHECK;
                    }
                }

                break;

            case GamePlayState.MATCHECK: 
                {
                    bool check = CheckMatchBlock(); 

                    if (check)
                    {
                        PlayState = GamePlayState.AFTERMATCHECK_MOVECHECK;
                    }
                    else
                    {

                        if (_ClickObject != null)
                        {
                            PlayState = GamePlayState.UNDOBLOCK;
                        }
                        else
                        {
                            PlayState = GamePlayState.INPUTOK;
                        }

                    }


                }
                break;

            case GamePlayState.DROPBLOCKMATCHCHECK:
                {
                    CheckMatchBlock();

                    PlayState = GamePlayState.AFTERMATCHECK_MOVECHECK;
                }
                break;

            case GamePlayState.AFTERMATCHEFFECTPLAY:
                    if (CheckBlockEffect())
                    {
                        PlayState = GamePlayState.AFTERMATCHECK_MOVECHECK;
                    }
                
                break;

            case GamePlayState.UNDOBLOCK:
                {
                    UNdoBlock();

                    PlayState = GamePlayState.AFTERMATCHECK_MOVECHECK;
                }
                break;


            case GamePlayState.DROPBLOCK:
                {
                    CreateMoveBlocks();

                    PlayState = GamePlayState.AFTERDROPBLOCKBLOCK_MOVECHECK;
                }
                break;

            case GamePlayState.AFTERMATCHECK_MOVECHECK:

                if (!CheckBlockMove())
                {
                    if (CheckAllBlockInGameBoard())
                    {
                        if (CheckAfterMoveMatchBlock())
                        {
                            PlayState = GamePlayState.INPUTOK;
                        }
                        else
                        {
                            _ShuffleInformPopUp.gameObject.SetActive(true);

                            ShuffleGameBoard();

                            PlayState = GamePlayState.DROPBLOCKMATCHCHECK;

                        }

                    }
                    else
                    {
                        PlayState = GamePlayState.DROPBLOCK;
                    }
                }


                break;

            case GamePlayState.AFTERDROPBLOCKBLOCK_MOVECHECK:
                {
                    if (!CheckBlockMove())
                    {
                        PlayState = GamePlayState.DROPBLOCKMATCHCHECK;
                    }
                }
                break;

        }



    }
}
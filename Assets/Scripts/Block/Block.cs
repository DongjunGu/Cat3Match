using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BLOCKSTATE
{
    STOP,
    MOVE,
    EFFECT
};

public enum Direction
{
    LEFT,
    RIGHT,
    UP,
    DOWN
};

public class Block : MonoBehaviour
{
    public int Type { set; get; } 
    public SpriteRenderer _BlockImage;

    public BLOCKSTATE State { set; get; } = BLOCKSTATE.STOP;
    private Direction _direct;

    public float Speed { set; get; } = 0.04f;

    private Vector3 _movePos;
    public Vector3 MovePos
    {
        get => _movePos;
        set => _movePos = value;
    }

    public float Width { set; get; } 
    public int Column { set; get; } 
    public int Row { set; get; }

    public void Init(int column, int row, int type, Sprite sprite)
    {
        Column = column;
        Row = row;
        Type = type;
        _BlockImage.sprite = sprite;
    }


    public void Move(Direction direct)
    {
        switch (direct)
        {
            case Direction.LEFT:
                {
                    _movePos = transform.position;
                    _movePos.x -= Width;
                    _direct = Direction.LEFT;
                    State = BLOCKSTATE.MOVE;
                }

                break;

            case Direction.RIGHT:
                {
                    _movePos = transform.position;
                    _movePos.x += Width;
                    _direct = Direction.RIGHT;
                    State = BLOCKSTATE.MOVE;
                }
                break;


            case Direction.UP:
                {
                    _movePos = transform.position;
                    _movePos.y += Width;
                    _direct = Direction.UP;
                    State = BLOCKSTATE.MOVE;
                }
                break;

            case Direction.DOWN:
                {
                    _movePos = transform.position;
                    _movePos.y -= Width;
                    _direct = Direction.DOWN;
                    State = BLOCKSTATE.MOVE;
                }
                break;
        }

    }

    public void Move(Direction direct, int moveCount)
    {
        AudioManager.Instance.PlayCatSound();
        switch (direct)
        {
            case Direction.LEFT:
                {
                    _direct = Direction.LEFT;
                    State = BLOCKSTATE.MOVE;
                }
                break;

            case Direction.RIGHT:
                {
                    _direct = Direction.RIGHT;
                    State = BLOCKSTATE.MOVE;
                }
                break;

            case Direction.UP:
                {
                    _direct = Direction.UP;
                    State = BLOCKSTATE.MOVE;
                }
                break;

            case Direction.DOWN:
                {
                    _direct = Direction.DOWN;
                    State = BLOCKSTATE.MOVE;
                }
                break;


        }

    }

    void Update()
    {
        if (State == BLOCKSTATE.MOVE)
        {
            switch (_direct)
            {
                case Direction.LEFT:
                    {
                        transform.Translate(Vector3.left * Speed);

                        if (transform.position.x <= _movePos.x)
                        {
                            transform.position = _movePos;
                            State = BLOCKSTATE.STOP;
                            
                        }
                    }
                    break;

                case Direction.RIGHT:
                    {
                        transform.Translate(-Vector3.left * Speed);

                        if (transform.position.x >= _movePos.x)
                        {
                            transform.position = _movePos;
                            State = BLOCKSTATE.STOP;
                        }
                    }
                    break;

                case Direction.UP:
                    {
                        transform.Translate(Vector3.up * Speed);

                        if (transform.position.y >= _movePos.y)
                        {
                            transform.position = _movePos;
                            State = BLOCKSTATE.STOP;
                        }
                    }
                    break;


                case Direction.DOWN:
                    {
                        transform.Translate(Vector3.down * Speed);

                        if (transform.position.y <= _movePos.y)
                        {
                            transform.position = _movePos;
                            State = BLOCKSTATE.STOP;
                        }
                    }
                    break;
            }
        }

    }
}
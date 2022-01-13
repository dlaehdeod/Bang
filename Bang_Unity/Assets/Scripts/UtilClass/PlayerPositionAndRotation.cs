using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionAndRotation
{
    private List<Vector2> position;
    private List<Quaternion> rotation;

    public void Initialize(int playerCount)
    {
        position = new List<Vector2>();
        rotation = new List<Quaternion>();

        switch (playerCount)
        {
            case 1:
                TestOnePlayer();
                break;
            case 2:
                SetTwoPlayer();
                break;
            case 3:
                SetThreePlayer();
                break;
            case 4:
                SetFourPlayer();
                break;
            case 5:
                SetFivePlayer();
                break;
            case 6:
                SetSixPlayer();
                break;
            case 7:
                SetSevenPlayer();
                break;
            default:
                break;
        }
    }

    public List<Vector2> GetPositionList()
    {
        return position;
    }

    public List<Quaternion> GetRotationList()
    {
        return rotation;
    }

    private void TestOnePlayer ()
    {
        position.Add(Vector2.zero);
        rotation.Add(Quaternion.identity);
    }

    private void SetTwoPlayer()
    {
        position.Add(Vector2.zero);
        position.Add(new Vector2(0.0f, 10.0f));

        rotation.Add(Quaternion.identity);
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
    }

    private void SetThreePlayer()
    {
        position.Add(Vector2.zero);
        position.Add(new Vector2(430.0f, 10.0f));
        position.Add(new Vector2(-430.0f, 10.0f));

        rotation.Add(Quaternion.identity);
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
    }

    private void SetFourPlayer()
    {
        position.Add(Vector2.zero);
        position.Add(new Vector2(430.0f, 50.0f));
        position.Add(new Vector2(0.0f, 10.0f));
        position.Add(new Vector2(-430.0f, 50.0f));

        rotation.Add(Quaternion.identity);
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 90.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 270.0f));
    }

    private void SetFivePlayer()
    {
        position.Add(Vector2.zero);
        position.Add(new Vector2(430.0f, 50.0f));
        position.Add(new Vector2(300.0f, 10.0f));
        position.Add(new Vector2(-300.0f, 10.0f));
        position.Add(new Vector2(-430.0f, 50.0f));

        rotation.Add(Quaternion.identity);
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 90.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 270.0f));
    }

    private void SetSixPlayer()
    {
        position.Add(Vector2.zero);
        position.Add(new Vector2(430.0f, 50.0f));
        position.Add(new Vector2(430.0f, 10.0f));
        position.Add(new Vector2(0.0f, 10.0f));
        position.Add(new Vector2(-430.0f, 10.0f));
        position.Add(new Vector2(-430.0f, 50.0f));

        rotation.Add(Quaternion.identity);
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 90.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 270.0f));
    }

    private void SetSevenPlayer()
    {
        position.Add(Vector2.zero);
        position.Add(new Vector2(430.0f, 50.0f));
        position.Add(new Vector2(430.0f, 10.0f));
        position.Add(new Vector2(0.0f, 10.0f));
        position.Add(new Vector2(-430.0f, 10.0f));
        position.Add(new Vector2(-430.0f, 100.0f));
        position.Add(new Vector2(-430.0f, -300.0f));

        rotation.Add(Quaternion.identity);
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 90.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 180.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 270.0f));
        rotation.Add(Quaternion.Euler(0.0f, 0.0f, 270.0f));
    }
}
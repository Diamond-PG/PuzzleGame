using UnityEngine;

public class Board : MonoBehaviour
{
    public GameObject squarePrefab;

    private void Start()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
        float startX = -1f;
        float startY = -1f;

        int index = 0;

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                GameObject s = Instantiate(squarePrefab);
                s.transform.position = new Vector2(startX + x, startY + y);

                index++;
            }
        }
    }
}
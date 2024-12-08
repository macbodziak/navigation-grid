using System.Threading.Tasks;
using Navigation;
using UnityEngine;

public class TestFaceTowards : MonoBehaviour
{
    [SerializeField] SquareGrid navGrid;
    [SerializeField] Vector2Int lookAt;
    [SerializeField] Actor actor_1;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var t = actor_1.FaceTowardsAsync(navGrid.WorldPositionAt(lookAt));
        }

    }
}

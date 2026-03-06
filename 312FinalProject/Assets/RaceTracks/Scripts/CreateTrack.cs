using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateTrack : MonoBehaviour
{
    /*
    public static CreateTrack Instance;

    [Range(10, 300)] public int trackLength = 10;
    [SerializeField] GameObject startPiece;
    [SerializeField] GameObject endPiece;
    [SerializeField] GameObject[] middlePieces;
    [SerializeField] GameObject player;

    List<GameObject> CreatedPieces = new List<GameObject>();
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    private void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // StartCoroutine(BuildTrack());
        // Instantiate(player, transform.position, Quaternion.identity);
    }

    // Build track coroutine; can't be normal method because of physics overlap reasons.
    public IEnumerator BuildTrack(int diffulty = 1)
    {
        trackLength = 10 * diffulty; // Set track length based on difficulty
        SpawnStart(); // Spawn the starting platform
        yield return null;

        // While the track length is shorter than the desired length
        while (CreatedPieces.Count < trackLength)
        {
            // Spawn a piece
            SpawnPiece();
            yield return null;
        }
        // Finally spawn the finish line
        SpawnEnd();
    }

    // Deletes all track pieces
    public void ClearTrack()
    {
        foreach (GameObject piece in CreatedPieces)
            Destroy(piece);

        CreatedPieces.Clear();
        occupiedCells.Clear();

        // Also wipe checkpoints from RaceManager
        RaceManager.Instance.checkpoints.Clear();
    }

    // Manually spawn pieces; used for testing
    // public void OnInteract()
    // {
    //     SpawnPiece();
    // }

    // Manually destroy pieces; used for testing
    // public void OnCrouch()
    // {
    //     if (CreatedPieces.Count > 1)
    //     {
    //         DestroyPiece();
    //     }
    // }

    // Instantiates the starting line
    private void SpawnStart()
    {
        GameObject startLine = Instantiate(startPiece, transform.position, Quaternion.identity);
        CreatedPieces.Add(startLine);
        var tp = startLine.GetComponent<TrackPiece>();
        occupiedCells.Add(QuantizePosition(tp.frontTransform.position));
    }

    // Spawns the end piece with the same logic as the middle pieces
    private void SpawnEnd()
    {
        GameObject lastPiece = CreatedPieces.Last();
        TrackPiece lastTrack = lastPiece.GetComponent<TrackPiece>();

        Transform connectPoint = lastTrack.frontTransform;

        // Instantiate new piece in a temporary pose
        GameObject newPiece = Instantiate(endPiece, Vector3.zero, Quaternion.identity);
        TrackPiece newTrack = newPiece.GetComponent<TrackPiece>();

        // Align rotation
        newPiece.transform.rotation =
            connectPoint.rotation * Quaternion.Inverse(newTrack.backTransform.localRotation);

        // Align position
        Vector3 offset = newTrack.backTransform.position - newPiece.transform.position;
        newPiece.transform.position = connectPoint.position - offset;
 
        // FOOTPRINT OVERLAP CHECK
        if (IsOverlapping(newTrack))
        {
            // If there's an overlap
            Destroy(newPiece); // Destroy the new piece
            Destroy(CreatedPieces.Last()); // destroy the last piece too
            CreatedPieces.Remove(CreatedPieces.Last()); // remove the last piece from the piece list
            // Remove the last checkpoint from the checkpoint list
            RaceManager.Instance.checkpoints.RemoveAt(RaceManager.Instance.checkpoints.Count -1); 
            Debug.Log("InvalidPiece");
            return; // INVALID — OVERLAP DETECTED
        }

        // If we passed the overlap check, accept the piece
        CreatedPieces.Add(newPiece);


        // Add the new pieces checkpoint to the checkpoint list.
        newPiece.GetComponent<TrackPiece>().Checkpoint.checkpointIndex = CreatedPieces.IndexOf(lastPiece);
        RaceManager.Instance.checkpoints.Add(newPiece.GetComponent<TrackPiece>().Checkpoint);
    }


    private void SpawnPiece()
    {
        int randInt = Random.Range(0, middlePieces.Length);

        GameObject lastPiece = CreatedPieces.Last();
        TrackPiece lastTrack = lastPiece.GetComponent<TrackPiece>();

        Transform connectPoint = lastTrack.frontTransform;

        // Instantiate new piece in a temporary pose
        GameObject newPiece = Instantiate(middlePieces[randInt], Vector3.zero, Quaternion.identity);
        TrackPiece newTrack = newPiece.GetComponent<TrackPiece>();

        // Align rotation
        newPiece.transform.rotation =
            connectPoint.rotation * Quaternion.Inverse(newTrack.backTransform.localRotation);

        // Align position
        Vector3 offset = newTrack.backTransform.position - newPiece.transform.position;
        newPiece.transform.position = connectPoint.position - offset;
 
        // FOOTPRINT OVERLAP CHECK
        if (IsOverlapping(newTrack))
        {
            // If there's an overlap
            Destroy(newPiece); // Destroy the new piece
            if (CreatedPieces.Count > 1)
            {
                Destroy(CreatedPieces.Last()); // destroy the last piece too
                CreatedPieces.Remove(CreatedPieces.Last()); // remove the last piece from the piece list
                // Remove the last checkpoint from the checkpoint list
                RaceManager.Instance.checkpoints.RemoveAt(RaceManager.Instance.checkpoints.Count -1);
            }
            Debug.Log("InvalidPiece");
            return; // INVALID — OVERLAP DETECTED
        }

        // Accept piece
        CreatedPieces.Add(newPiece);

        // Add the new pieces checkpoint to the checkpoint list.
        newPiece.GetComponent<TrackPiece>().Checkpoint.checkpointIndex = CreatedPieces.IndexOf(lastPiece);
        RaceManager.Instance.checkpoints.Add(newPiece.GetComponent<TrackPiece>().Checkpoint);
    }

    // Overlapping check
    private bool IsOverlapping(TrackPiece newTrack)
    {
        BoxCollider box = newTrack.pieceArea; // initialize the box collider of the new piece

        Vector3 center = box.transform.TransformPoint(box.center); // find the center of the collider
        Vector3 halfSize = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale); // find half-size vector
        Quaternion rotation = box.transform.rotation; // find collider's rotation

        // Calculate all overlapping colliders and store them in a list called hits
        Collider[] hits = Physics.OverlapBox(center, halfSize, rotation);

        // Test hits for "bad" overlaps
        foreach (var h in hits)
        {
            // Ignore itself
            if (h == box) continue;

            // Ignore the previous piece (close alignment causes false positives)
            if (h.transform.root == CreatedPieces.Last().transform) continue;

            // Ignore anything not tagged as TrackPiece
            if (!h.transform.root.CompareTag("TrackPiece")) continue;

            Debug.Log("OVERLAP WITH " + h.name);
            return true;
        }

        return false;
    }

    private void DestroyPiece()
    {
        Destroy(CreatedPieces.Last());
        CreatedPieces.RemoveAt(CreatedPieces.Count - 1);
    }

    // Quantize method to prevent small number errors
    private Vector3Int QuantizePosition(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.RoundToInt(pos.x),
            Mathf.RoundToInt(pos.y),
            Mathf.RoundToInt(pos.z)
        );
    }

    */
}
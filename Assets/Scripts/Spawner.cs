using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Tooltip("Color for spawn area, can be selected for each spawner independently")]
    [SerializeField] private Color _drawColor = Color.white;
    [Tooltip("Size of spawn area, only fitted items will be spawned")]
    [SerializeField] private Vector3 _spawnAreaSize = Vector3.one;
    [Tooltip("Delay before next spawn, after item is picked up")]
    [SerializeField] private float _spawnCooldown = 1f;

    private List<GameObject> _loots = new();
    private Bounds _spawnBounds;
    private GameObject _currentItem;

    // sorting and deactivating items
    private void Start()
    {
        _spawnBounds = new(transform.position + (Vector3.up * _spawnAreaSize.y / 2f), _spawnAreaSize);

        FindFittingLoots();

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        SpawnRandomLoot();
    }

    // this is for call from outside to pick up current item and spawn next
    public void PickupItem()
    {
        _currentItem.SetActive(false);
        StartCoroutine(SpawnCooldown());
    }

    private IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(_spawnCooldown);
        SpawnRandomLoot();
    }

    // making a list of loots that fits in spawn area
    private void FindFittingLoots()
    {
        foreach (Transform child in transform)
        {
            if (IsFitting(child.gameObject))
            {
                _loots.Add(child.gameObject);
            }
        }
    }

    private bool IsFitting(GameObject item)
    {
        if (item.TryGetComponent(out Collider itemCollider))
        {
            return _spawnBounds.Contains(itemCollider.bounds.min) && _spawnBounds.Contains(itemCollider.bounds.max);
        }
        return false;
    }

    // activating random object from list
    private void SpawnRandomLoot()
    {
        if (_loots.Count > 0)
        {
            int randomIndex = Random.Range(0, _loots.Count);
            _currentItem = _loots[randomIndex];
            _currentItem.SetActive(true);
        }
    }

    // for visualising only in editor, without using colliders
    private void OnDrawGizmos()
    {
        Gizmos.color = _drawColor;
        Gizmos.DrawCube(transform.position + (Vector3.up * _spawnAreaSize.y / 2f), _spawnAreaSize);
    }
}

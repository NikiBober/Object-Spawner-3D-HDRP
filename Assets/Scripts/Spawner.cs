using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Tooltip("Color for spawn area, can be selected for each spawner independently.")]
    [SerializeField] private Color _drawColor = Color.white;
    [Tooltip("Size of spawn area, only fitted items will be spawned.")]
    [SerializeField] private Vector3 _spawnAreaSize = Vector3.one;
    [Tooltip("Delay before next spawn, after item is picked up.")]
    [SerializeField] private float _spawnCooldown = 1f;

    [Tooltip("Layer with which to detect collisions.")]
    [SerializeField] private LayerMask _collisionLayer;

    private List<GameObject> _loots = new();
    private System.Random _random = new();
    private Bounds _spawnBounds;
    private GameObject _currentItem;
    private Vector3 _spawnAreaCenter;

    // sorting and deactivating items
    private void Start()
    {
        // offset so that the spawner's position is the bottom point of the spawn area (to match the spawn objects)
        _spawnAreaCenter = transform.position + (Vector3.up * _spawnAreaSize.y / 2f);
        _spawnBounds = new(_spawnAreaCenter, _spawnAreaSize);

        // this check can be avoid if on spawner will be only correct items
        FindFittingLoots();

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // this check can be avoid if spawner will be positioned correctly
        if (IsCollidersInSpawnArea())
        {
            Debug.Log($"Adjust {gameObject.name} position. There are colliders of the specified LayerMask in the spawn area.");
        }
        else
        {
            SpawnRandomLoot();
        }
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
            // System.Random used instead of Random.Range for even better performance
            int randomIndex = _random.Next(_loots.Count);
            _currentItem = _loots[randomIndex];
            _currentItem.SetActive(true);
        }
    }

    // for visualising only in editor, without using colliders
    private void OnDrawGizmos()
    {
        Gizmos.color = _drawColor;
        Gizmos.DrawCube(transform.position + (Vector3.up * _spawnAreaSize.y / 2f), _spawnAreaSize);
        // _spawnAreaCenter can`t be used here
    }

    // check, is there are colliders of the specified LayerMask in the spawn area
    private bool IsCollidersInSpawnArea()
    {
        Collider[] colliders = Physics.OverlapBox(_spawnAreaCenter, _spawnAreaSize / 2f, Quaternion.identity, _collisionLayer);
        return colliders.Length > 0;
    }
}

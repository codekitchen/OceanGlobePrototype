using UnityEngine;

public class PlayerControls : MonoBehaviour {
  public Map map;
  public Vector2 mapPos = Vector2.zero;
  public float moveSpeed = 4f;

  /// <summary>
  /// Update is called every frame, if the MonoBehaviour is enabled.
  /// </summary>
  void Update()
	{
    Vector2 newPos = mapPos;
    newPos.x += Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
		if (map.CanEnter(newPos)) {
      mapPos = newPos;
    }
    newPos = mapPos;
    newPos.y += Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime;
		if (map.CanEnter(newPos)) {
      mapPos = newPos;
    }
	}
}

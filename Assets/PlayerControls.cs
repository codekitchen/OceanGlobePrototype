using UnityEngine;

public class PlayerControls : MonoBehaviour {
  public Map map;
  public Vector2 mapPos = Vector2.zero;
  public float moveSpeed = 4f;

  public int x { get { return Mathf.RoundToInt(mapPos.x); } }
  public int y { get { return Mathf.RoundToInt(mapPos.y); } }
  public float xr { get { return mapPos.x - Mathf.Round(mapPos.x); } }
  public float yr { get { return mapPos.y - Mathf.Round(mapPos.y); } }

  /// <summary>
  /// Update is called every frame, if the MonoBehaviour is enabled.
  /// </summary>
  void Update() {
    Vector2 newPos = mapPos;
    float nxr = xr + Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
    if (!map.CanEnter(x + 1, y) && nxr >= .4f) {
      nxr = .4f;
    }
    else if (!map.CanEnter(x - 1, y) && nxr <= -.4f) {
      nxr = -.4f;
    }
    float nyr = yr + Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime;
    if (!map.CanEnter(x, y + 1) && nyr >= .4f) {
      nyr = .4f;
    }
    else if (!map.CanEnter(x, y - 1) && nyr <= -.4f) {
      nyr = -.4f;
    }

    while (nxr > .5f) {
      nxr--;
      newPos.x++;
    }
    while (nxr < -.5f) {
      nxr++;
      newPos.x--;
    }
    while (nyr > .5f) {
      nyr--;
      newPos.y++;
    }
    while (nyr < -.5f) {
      nyr++;
      newPos.y--;
    }
    mapPos.x = Mathf.Round(newPos.x) + nxr;
    mapPos.y = Mathf.Round(newPos.y) + nyr;
  }
}

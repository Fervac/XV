using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragObject : MonoBehaviour
{
	private bool _drag;
	private float speed = 7f;
	private void Update()
	{
		if (_drag)
		{
			Ray ray;
			RaycastHit hit;
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100.0f))
			{
				if (hit.collider.CompareTag("Floor"))
				{
					transform.position = Vector3.Lerp(transform.position, hit.point, speed * Time.deltaTime);
				}
			}

			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, Input.GetAxis("Mouse ScrollWheel") * 100 + transform.eulerAngles.y, 0), Time.time * speed);

			if (Input.GetMouseButtonUp(1))
			{
				// Update init_pos & init_rot of ModelManager ? And then all actions using this object ?
				_drag = false;
			}
		}
	}

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
			_drag = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragObject : MonoBehaviour
{
	private bool _drag;
	private float speed = 7f;

	ModelManager manager;

    private void Start()
    {
		manager = this.GetComponent<ModelManager>();
    }

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
				if (Manager.Instance.GetTimeCursor() == 0.0f)/* || Manager.Instance.GetTimeCursor() <= )*/
                {
					manager.init_pos = this.transform.position;
					manager.init_rot = this.transform.eulerAngles;

					// Then update first action of the actor
					// 1 - Get Actor
					// 2 - Actor.UpdateActions()
					ActionActor actor = Manager.Instance.timeline.GetActorFromName(this.gameObject.name);
					if (actor != null)
					{
						actor.position = manager.init_pos;
						actor.rotation = manager.init_rot;
						actor.UpdateActions();
					}
                }
			}
		}
	}

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1) && !(Manager.Instance.IsPlaying()))
        {
			_drag = true;
        }
    }
}

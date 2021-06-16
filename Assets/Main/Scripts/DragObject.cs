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
				_drag = false;
				if (Manager.Instance.GetTimeCursor() == 0.0f)/* || Manager.Instance.GetTimeCursor() <= )*/
                {
					manager.init_pos = this.transform.position;
					manager.init_rot = this.transform.eulerAngles;

					// Then update first action of the actor
					//	1 -	Get Actor																						- DONE
					//	2 -	Actor.UpdateActions() -> Will update actions of the actor										- DONE
					//	3 -	We also need to update position in actions from other actor is object_target is our actor.		- WIP
					//		The problem is how to update object_target position ?
					ActionActor actor = Manager.Instance.timeline.GetActorFromName(this.gameObject.name);
					if (actor != null)
					{
						actor.position = manager.init_pos;
						actor.rotation = manager.init_rot;
						actor.UpdateActions();
					}
					else
                    {
						List<ActionActor> tormentor = Manager.Instance.timeline.GetActorThatInteractWith(this.gameObject);
						foreach(ActionActor meanActor in tormentor)
							meanActor.UpdateActions();
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

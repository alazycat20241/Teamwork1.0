using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingRope : MonoBehaviour
{
    public Transform hangPoint;
    public Transform[] segments;
    public float swingForce = 10f;

    private PlayerController player;
    private Rigidbody2D playerRb;
    private Vector2 offset; 
    private bool isSwinging = false;
    private Magnet magnet;

    void Start()
    {
        magnet = GetComponent<Magnet>();
        ConnectRope();
    }

    void ConnectRope()
    {
        if (hangPoint.GetComponent<Rigidbody2D>() == null)
            hangPoint.gameObject.AddComponent<Rigidbody2D>().isKinematic = true;

        Transform prev = hangPoint;
        foreach (Transform seg in segments)
        {
            DistanceJoint2D joint = seg.gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = prev.GetComponent<Rigidbody2D>();
            joint.distance = Vector2.Distance(seg.position, prev.position);
            joint.autoConfigureDistance = false;
            prev = seg;
        }

        DistanceJoint2D magnetJoint = gameObject.AddComponent<DistanceJoint2D>();
        magnetJoint.connectedBody = prev.GetComponent<Rigidbody2D>();
        magnetJoint.distance = Vector2.Distance(transform.position, prev.position);
        magnetJoint.autoConfigureDistance = false;
    }

    void FixedUpdate()
    {
        if (!isSwinging || player == null) return;

        // 直接给磁铁加力，物理引擎自动处理摆动
        if (Input.GetKey(KeyCode.A))
            GetComponent<Rigidbody2D>().AddForce(Vector2.left * swingForce);
        if (Input.GetKey(KeyCode.D))
            GetComponent<Rigidbody2D>().AddForce(Vector2.right * swingForce);

    }

    bool IsAttract(PlayerController pc)
    {
        return (pc.currentPole == PlayerController.MagneticPole.North && magnet.pole == PlayerController.MagneticPole.South) ||
               (pc.currentPole == PlayerController.MagneticPole.South && magnet.pole == PlayerController.MagneticPole.North);
    }
}

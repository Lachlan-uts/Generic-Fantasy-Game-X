using UnityEngine;

namespace PowerGridInventory.Demo
{
    public class PlayerInput : MonoBehaviour
    {
        public float MaxSpeed = 5;
        public float Acceleration = 25f;
        public CharacterMotionAnimator.AxisOrientation MotionType;

        CharacterMotionAnimator CharacterAnim;
        Rigidbody2D Body;
        Vector3 LastMoveDirection;
        Vector3 RotVel;
        bool Frozen;


        public void Awake()
        {
            Body = GetComponentInChildren<Rigidbody2D>(true);
        }

        public void Update()
        {
            //gather and sanitize input
            float x = 0;
            float y = 0;
            if (!Frozen)
            {
                x = Input.GetAxisRaw("Horizontal");
                y = Input.GetAxisRaw("Vertical");
            }
            Vector3 moveDirection = (MotionType == CharacterMotionAnimator.AxisOrientation.XZ_2D) ? 
                Vector3.ClampMagnitude(new Vector3(x, 0, y), 1) :
                Vector3.ClampMagnitude(new Vector3(x, y, 0), 1);

            //smooth the velocity when movement angle changes
            moveDirection = Vector3.SmoothDamp(LastMoveDirection, moveDirection, ref RotVel, 1 / Acceleration);
            //moveDirection = Vector3.RotateTowards(LastMoveDirection, moveDirection, Mathf.PI/2/Acceleration, 1.0f);
            LastMoveDirection = moveDirection;

            //final composite velocity
            if (moveDirection.magnitude < 0.1f) moveDirection = Vector3.zero;
            Vector3 vel = MaxSpeed * moveDirection;
            vel = Vector3.ClampMagnitude(vel, MaxSpeed);

            //when in 3D, we need to preserve the vertical speed for gravity to work
            if (MotionType == CharacterMotionAnimator.AxisOrientation.XZ_2D) vel.y = Body.velocity.y;
            Body.velocity = vel;
        }

        /// <summary>
        /// Broadcast reciever. This lets our inventory freeze player input.
        /// </summary>
        /// <param name="state"></param>
        void MenuStateChanged(bool state)
        {
            Frozen = state;
        }
        
    }
}

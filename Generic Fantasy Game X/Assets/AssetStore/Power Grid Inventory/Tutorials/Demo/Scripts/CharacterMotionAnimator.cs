using UnityEngine;

namespace PowerGridInventory.Demo
{
    public class CharacterMotionAnimator : MonoBehaviour
    {
        [Tooltip("The directions to test motion for.")]
        public AxisOrientation MotionType;

        [Tooltip("Scaling factor for syncing motion with run animation.")]
        public float RunSpeedScale = 1;

        Animator Anim;
        Rigidbody2D Body;
        float MovingSpeed;
        CardinalOrientation LastOrientation;

        public enum CardinalOrientation
        {
            None = -1,
            South,
            North,
            West,
            East,
        }

        public enum AxisOrientation
        {
            XY_2D,
            XZ_2D,
        }


        public void Awake()
        {
            Anim = GetComponentInChildren<Animator>(true);
            Body = GetComponentInChildren<Rigidbody2D>(true);
            Anim.SetFloat("Moving", 0);
        }

        public void LateUpdate()
        {
            ApplyMotionToAnim(Body.velocity);
        }

        public void ApplyMotionToAnim(Vector2 velocity)
        {
            if (Anim == null || !Anim.isInitialized ||
                Anim.runtimeAnimatorController == null)
            {
                Debug.LogError("State Machine not inizialized on " + name + ".");
                return;
            }

            FaceDirection(DirectionToOrientation(velocity));

            float mag = velocity.magnitude;
            float finalSpeed = mag * RunSpeedScale;
            if (Mathf.Abs(MovingSpeed - finalSpeed) > 0.01f)
            {
                MovingSpeed = finalSpeed;
                Anim.SetFloat("Moving", finalSpeed);
            }

        }

        public void FaceDirection(CardinalOrientation dir)
        {
            if (LastOrientation != dir)
            {
                LastOrientation = dir;
                switch (LastOrientation)
                {
                    case CardinalOrientation.South:
                        {
                            Anim.SetFloat("Direction", 0.0f);
                            break;
                        }
                    case CardinalOrientation.East:
                        {
                            Anim.SetFloat("Direction", 0.4f);
                            break;
                        }
                    case CardinalOrientation.North:
                        {
                            Anim.SetFloat("Direction", 0.7f);
                            break;
                        }
                    case CardinalOrientation.West:
                        {
                            Anim.SetFloat("Direction", 1.0f);
                            break;
                        }
                }
            }
        }

        public Vector2 XZPosAsXYPos(Vector3 position)
        {
            return (this.MotionType == AxisOrientation.XY_2D) ? (Vector2)position : new Vector2(position.x, position.z);
        }

        public static CardinalOrientation DirectionToOrientation(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.05f) return CardinalOrientation.None;

            //find out which direction to face. We compare both ways to ensure 'fairness'
            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
            {
                if (direction.y > 0.0f) return CardinalOrientation.North;
                else if (direction.y < 0.0f) return CardinalOrientation.South;
            }
            else if (Mathf.Abs(direction.y) < Mathf.Abs(direction.x))
            {
                if (direction.x > 0.0f) return CardinalOrientation.East;
                else if (direction.x < 0.0f) return CardinalOrientation.West;
            }

            return CardinalOrientation.None;
        }
    }
}

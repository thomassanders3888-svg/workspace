using System.Numerics;

namespace TerraForgeServer.Game
{
    public class PlayerController
    {
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public float Stamina { get; private set; }
        public bool IsGrounded { get; private set; }

        private const float MoveSpeed = 5f;
        private const float Gravity = -9.81f;
        private const float GroundLevel = 0f;
        private const float StaminaMax = 100f;
        private const float StaminaDrain = 20f;
        private const float StaminaRegen = 10f;

        public PlayerController(Vector3 startPos)
        {
            Position = startPos;
            Velocity = Vector3.Zero;
            Stamina = StaminaMax;
            IsGrounded = false;
        }

        public void Update(float deltaTime, InputState input)
        {
            CheckGrounded();
            Move(deltaTime, input);
            ApplyGravity(deltaTime);
            UpdateStamina(deltaTime, input);
        }

        private void Move(float deltaTime, InputState input)
        {
            Vector3 moveDir = Vector3.Zero;
            if (input.Forward) moveDir.Z += 1;
            if (input.Backward) moveDir.Z -= 1;
            if (input.Left) moveDir.X -= 1;
            if (input.Right) moveDir.X += 1;

            if (moveDir != Vector3.Zero)
            {
                moveDir = Vector3.Normalize(moveDir);
                float speed = input.Sprint && Stamina > 0 ? MoveSpeed * 2f : MoveSpeed;
                Velocity = new Vector3(moveDir.X * speed, Velocity.Y, moveDir.Z * speed);
            }
            else
            {
                Velocity = new Vector3(0, Velocity.Y, 0);
            }

            Position += Velocity * deltaTime;
        }

        private void ApplyGravity(float deltaTime)
        {
            if (!IsGrounded)
            {
                Velocity = new Vector3(Velocity.X, Velocity.Y + Gravity * deltaTime, Velocity.Z);
            }
            else if (Velocity.Y < 0)
            {
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
            }

            if (Position.Y < GroundLevel)
            {
                Position = new Vector3(Position.X, GroundLevel, Position.Z);
                IsGrounded = true;
            }
        }

        private void CheckGrounded()
        {
            IsGrounded = Position.Y <= GroundLevel + 0.01f;
        }

        private void UpdateStamina(float deltaTime, InputState input)
        {
            if (input.Sprint && IsGrounded && Velocity.LengthSquared() > 0.1f)
            {
                Stamina = System.Math.Max(0, Stamina - StaminaDrain * deltaTime);
            }
            else
            {
                Stamina = System.Math.Min(StaminaMax, Stamina + StaminaRegen * deltaTime);
            }
        }
    }

    public struct InputState
    {
        public bool Forward;
        public bool Backward;
        public bool Left;
        public bool Right;
        public bool Sprint;
    }
}

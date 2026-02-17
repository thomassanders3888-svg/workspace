 

```csharp
using System;
using System.Collections.Generic;

public struct Vector3 {
    public float x;
    public float y;
    public float z;

    public Vector3(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b) {
        return new Vector3(a.x - b.x, a.y - b.b, a.z - b.z);
    }

    public static Vector3 operator *(Vector3 v, float s) {
        return new Vector3(v.x * s, v.y * s, v.z * s);
    }

    public static Vector3 operator /(Vector3 v, float s) {
        return new Vector3(v.x / s, v.y / s, v.z / s);
    }

    public float Length() {
        return (float)Math.Sqrt(x * x + y * y + z * z);
    }
}

public struct AABB {
    public Vector3 min;
    public Vector3 max;

    public AABB(Vector3 min, Vector3 max) {
        this.min = min;
        this.max = max;
    }

    public bool Intersects(AABB other) {
        return (min.x <= other.max.x && max.x >= other.min.x &&
                min.y <= other.max.y && max.y >= other.min.y &&
                min.z <= other.max.z && max.z >= other.min.z);
    }
}

public class PhysicsEngine {
    private const float Gravity = 9.81f;
    private Vector3 position;
    private Vector3 velocity;
    private float mass;

    public PhysicsEngine(Vector3 initialPosition, Vector3 initialVelocity, float mass) {
        this.position = initialPosition;
        this.velocity = initialVelocity;
        this.mass = mass;
    }

    public void Update(float deltaTime) {
        // Apply gravity
        velocity.y -= Gravity * deltaTime;

        // Move with collision detection and handling
        MoveWithCollision(deltaTime);
    }

    public bool GroundCheck(AABB groundAABB, List<AABB> environmentBBs) {
        AABB playerBounds = new AABB(new Vector3(position.x - 0.5f, position.y - 1f, position.z - 0.5f),
                                        new Vector3(position.x + 0.5f, position.y + 0.5f, position.z + 0.5f));

        // Check if the player's bottom is within the ground AABB
        return playerBounds.Intersects(groundAABB) && !environmentBBs.Exists(bb => playerBounds.Intersects(bb));
    }

    public void MoveWithCollision(float deltaTime) {
        // Project movement vector into velocity component
        Vector3 nextPosition = position + velocity * deltaTime;

        // Check for collision with environment boundaries and ground
        AABB playerBounds = new AABB(new Vector3(nextPosition.x - 0.5f, nextPosition.y - 1f, nextPosition.z - 0.5f),
                                        new Vector3(nextPosition.x + 0.5f, nextPosition.y + 0.5f, nextPosition.z

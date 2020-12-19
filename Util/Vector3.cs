using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public partial struct Vector3 : IEquatable<Vector3> 
    {              
        public float x;
        
        public float y;
        
        public float z;
             
            
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        
        public Vector3(float x, float y) { this.x = x; this.y = y; z = 0F; }
             
        public void Set(float newX, float newY, float newZ) { x = newX; y = newY; z = newZ; }
       
        public static Vector3 Scale(Vector3 a, Vector3 b) { return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z); }
        
        public void Scale(Vector3 scale) { x *= scale.x; y *= scale.y; z *= scale.z; }

        // Cross Product of two vectors.
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x);
        }

        // used to allow Vector3s to be used as keys in hash tables
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        // also required for being able to use Vector3s as keys in hash tables
        public override bool Equals(object other)
        {
            if (!(other is Vector3)) return false;

            return Equals((Vector3)other);
        }

        public bool Equals(Vector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
                

        // Returns the distance between /a/ and /b/.
        public static float Distance(Vector3 a, Vector3 b)
        {
            float diff_x = a.x - b.x;
            float diff_y = a.y - b.y;
            float diff_z = a.z - b.z;
            return (float)Math.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
        }
              

        static readonly Vector3 zeroVector = new Vector3(0F, 0F, 0F);
        static readonly Vector3 oneVector = new Vector3(1F, 1F, 1F);
        static readonly Vector3 upVector = new Vector3(0F, 1F, 0F);
        static readonly Vector3 downVector = new Vector3(0F, -1F, 0F);
        static readonly Vector3 leftVector = new Vector3(-1F, 0F, 0F);
        static readonly Vector3 rightVector = new Vector3(1F, 0F, 0F);
        static readonly Vector3 forwardVector = new Vector3(0F, 0F, 1F);
        static readonly Vector3 backVector = new Vector3(0F, 0F, -1F);
        static readonly Vector3 positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        static readonly Vector3 negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        // Shorthand for writing @@Vector3(0, 0, 0)@@
        public static Vector3 zero { get { return zeroVector; } }
        // Shorthand for writing @@Vector3(1, 1, 1)@@
        public static Vector3 one { get { return oneVector; } }
        // Shorthand for writing @@Vector3(0, 0, 1)@@
        public static Vector3 forward { get { return forwardVector; } }
        public static Vector3 back { get { return backVector; } }
        // Shorthand for writing @@Vector3(0, 1, 0)@@
        public static Vector3 up { get { return upVector; } }
        public static Vector3 down { get { return downVector; } }
        public static Vector3 left { get { return leftVector; } }
        // Shorthand for writing @@Vector3(1, 0, 0)@@
        public static Vector3 right { get { return rightVector; } }
        // Shorthand for writing @@Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)@@
        public static Vector3 positiveInfinity { get { return positiveInfinityVector; } }
        // Shorthand for writing @@Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity)@@
        public static Vector3 negativeInfinity { get { return negativeInfinityVector; } }

        
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        
        public static Vector3 operator -(Vector3 a) { return new Vector3(-a.x, -a.y, -a.z); }
        
        public static Vector3 operator *(Vector3 a, float d) { return new Vector3(a.x * d, a.y * d, a.z * d); }
        
        public static Vector3 operator *(float d, Vector3 a) { return new Vector3(a.x * d, a.y * d, a.z * d); }
        
        public static Vector3 operator /(Vector3 a, float d) { return new Vector3(a.x / d, a.y / d, a.z / d); }        
     
    }
}

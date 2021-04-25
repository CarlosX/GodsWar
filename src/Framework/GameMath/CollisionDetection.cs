using System;

namespace Framework.GameMath
{
    class CollisionDetection
    {
        public static float collisionTimeForMovingPointFixedAABox(Vector3 origin, Vector3 dir, AxisAlignedBox box, ref Vector3 location, out bool Inside)
        {
            Vector3 normal = Vector3.Zero;
            if (collisionLocationForMovingPointFixedAABox(origin, dir, box, ref location, out Inside, ref normal))
            {
                return (location - origin).magnitude();
            }
            else
            {
                return float.PositiveInfinity;
            }
        }
        public static bool collisionLocationForMovingPointFixedAABox(Vector3 origin, Vector3 dir, AxisAlignedBox box, ref Vector3 location, out bool Inside, ref Vector3 normal)
        {
            Inside = true;
            Vector3 MinB = box.Lo;
            Vector3 MaxB = box.Hi;
            Vector3 MaxT = new Vector3(-1.0f, -1.0f, -1.0f);

            // Find candidate planes.
            for (int i = 0; i < 3; ++i)
            {
                if (origin[i] < MinB[i])
                {
                    location[i] = MinB[i];
                    Inside = false;

                    // Calculate T distances to candidate planes
                    if ((uint)dir[i] != 0)
                    {
                        MaxT[i] = (MinB[i] - origin[i]) / dir[i];
                    }
                }
                else if (origin[i] > MaxB[i])
                {
                    location[i] = MaxB[i];
                    Inside = false;

                    // Calculate T distances to candidate planes
                    if ((uint)dir[i] != 0)
                    {
                        MaxT[i] = (MaxB[i] - origin[i]) / dir[i];
                    }
                }
            }

            if (Inside)
            {
                // Ray origin inside bounding box
                location = origin;
                return false;
            }

            // Get largest of the maxT's for final choice of intersection
            int WhichPlane = 0;
            if (MaxT[1] > MaxT[WhichPlane])
            {
                WhichPlane = 1;
            }

            if (MaxT[2] > MaxT[WhichPlane])
            {
                WhichPlane = 2;
            }

            // Check final candidate actually inside box
            if (Convert.ToBoolean((uint)MaxT[WhichPlane] & 0x80000000))
            {
                // Miss the box
                return false;
            }

            for (int i = 0; i < 3; ++i)
            {
                if (i != WhichPlane)
                {
                    location[i] = origin[i] + MaxT[WhichPlane] * dir[i];
                    if ((location[i] < MinB[i]) ||
                        (location[i] > MaxB[i]))
                    {
                        // On this plane we're outside the box extents, so
                        // we miss the box
                        return false;
                    }
                }
            }

            // Choose the normal to be the plane normal facing into the ray
            normal = Vector3.Zero;
            normal[WhichPlane] = (float)((dir[WhichPlane] > 0) ? -1.0 : 1.0);

            return true;
        }
    }
}

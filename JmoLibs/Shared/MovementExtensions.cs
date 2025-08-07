using Godot;
using System;

namespace Jmo.Shared
{
    public static partial class MovementExtensions
    {
        #region ORTHOG_EXTENSIONS
        public static AnimDirection GetAnimDirection(this AnimationPlayer animPlayer)
        {
            if (animPlayer.AssignedAnimation.ToString().Contains("Down"))
            {
                return AnimDirection.Down;
            }
            else
            {
                return AnimDirection.Up;
            }
        }
        public static AnimDirection GetAnimDir(this Dir4 orthogDir)
        {
            switch (orthogDir)
            {
                case Dir4.Left or Dir4.Up:
                    return AnimDirection.Up;
                case Dir4.Down or Dir4.Right:
                    return AnimDirection.Down;
                default:
                    throw new Exception($"ORTHOG DIR: {orthogDir} GAVE NO ANIM DIRECTION??");
            }
        }
        public static bool GetFlipH(this Dir4 orthogDir)
        {
            switch (orthogDir)
            {
                case Dir4.Right or Dir4.Up:
                    return false;
                case Dir4.Down or Dir4.Left:
                    return true;
                default:
                    throw new Exception($"ORTHOG DIR: {orthogDir} GAVE NO FLIP H??");
            }
        }
        public static Dir4 GetOppositeDir(this Dir4 direction)
        {
            switch (direction)
            {
                case Dir4.Left:
                    return Dir4.Right;
                case Dir4.Up:
                    return Dir4.Down;
                case Dir4.Right:
                    return Dir4.Left;
                case Dir4.Down:
                    return Dir4.Up;
                default:
                    GD.Print("not any face direction?? facedir = " + direction.ToString());
                    return Dir4.Down;
            }
        }
        public static Vector2 GetVector2(this Dir4 orthogDir)
        {
            switch (orthogDir)
            {
                case Dir4.Left:
                    return new Vector2(-1f, 0f).Normalized();
                //return new Vector2(-0.707f, -0.707f).Normalized().Normalized();
                case Dir4.Up:
                    return new Vector2(0f, -1f).Normalized();
                //return new Vector2(0.707f, -0.707f).Normalized();
                case Dir4.Down:
                    return new Vector2(0f, 1f).Normalized();
                //return new Vector2(-0.707f, 0.707f).Normalized();
                case Dir4.Right:
                    return new Vector2(1f, 0f).Normalized();
                //return new Vector2(0.707f, 0.707f).Normalized();
                default:
                    throw new Exception($"ORTHOG DIR: {orthogDir} GAVE NO VECTOR??");
            }
        }
        public static Vector3 GetVector3(this Dir4 orthogDir)
        {
            var vec2 = GetVector2(orthogDir);
            return new Vector3(vec2.X, 0f, vec2.Y);
        }
        public static string GetAnimationString(this AnimDirection dir)
        {
            switch (dir)
            {
                case AnimDirection.Down:
                    return "Down";
                case AnimDirection.Up:
                    return "Up";
                default:
                    GD.PrintErr("not any anim direction?? facedir = " + dir.ToString());
                    return "Null";
            }
        }
        //private static Dir4 GetDir4FromXY(float x, float y)
        //{
        //    if (x < 0 && y <= 0)
        //    {
        //        // IMPORTANT: Straight Left = UpLeft
        //        return Dir4.Left;
        //    }
        //    else if (x >= 0 && y < 0)
        //    {
        //        // IMPORTANT: Straight Up = UpRight
        //        return Dir4.Up;
        //    }
        //    else if (x <= 0 && y > 0)
        //    {
        //        // IMPORTANT: Straight Down = DownLeft
        //        return Dir4.Down;
        //    }
        //    else if (x > 0 && y >= 0)
        //    {
        //        // IMPORTANT: Straight Right = DownRight
        //        return Dir4.Right;
        //    }
        //    else { throw new Exception($"VALUES: ({x}, {y}) GAVE NO ORTHOG DIRECTION??"); }
        //}
        private static Dir4 GetDir4FromXY(float x, float y)
        {
            //Handle the zero vector case if necessary(optional, depends on requirements)
            if (x == 0 && y == 0)
            {
                // Decide what to return: a default, throw, or maybe a Dir4.None?
                throw new ArgumentException("Cannot determine direction for zero vector (0,0)");
                // Or return a default:
                // return Dir4.Right; // Or any other default
            }
            // Compare absolute values to find the dominant axis 
            if (Math.Abs(x) > Math.Abs(y))
            {
                // Vector is primarily horizontal
                if (x > 0)
                {
                    return Dir4.Right;
                }
                else // x < 0 (x cannot be 0 here if Abs(x) > Abs(y) unless y is also 0, handled above)
                {
                    return Dir4.Left;
                }
            }
            else // Abs(y) >= Abs(x)
            {
                // Vector is primarily vertical (or perfectly diagonal)
                if (y < 0) // Corresponds to negative Y in standard coords, but Up in original logic
                {
                    // If y is exactly 0 here, it means x must also be 0 (Abs(x) <= Abs(y)=0),
                    // which should be handled by the zero vector check above if uncommented.
                    // If the zero check isn't present, y==0 falls into this 'else',
                    // resulting in Dir4.Up if the previous 'if (y < 0)' is false.
                    // This might be okay as a default for (0,0) if not handled explicitly.
                    return Dir4.Up;
                }
                else // y >= 0 (Corresponds to positive Y or zero Y)
                {
                    return Dir4.Down; // Mapped from y > 0 in original logic
                }
            }
            // Note: The original 'throw new Exception' is unreachable with this logic
            // unless x or y are NaN, which might warrant explicit checks if needed.
        }
        public static Dir16 GetDir16FromXY(float x, float y)
        {
            // Convert vector to angle (in degrees)
            float angle = (float)(Math.Atan2(y, x) * (180.0 / Math.PI));
            if (angle < 0) angle += 360; // Convert negative angles to positive

            // Find the closest direction by rounding to nearest 22.5°
            int directionIndex = (int)Math.Round(angle / 22.5f) % 16;

            return (Dir16)directionIndex;
        }
        public static Dir16 GetDir16(this Vector2 vec)
        {
            return GetDir16FromXY(vec.X, vec.Y);
        }
        public static Dir16 GetDir16(this Vector3 vec)
        {
            return GetDir16FromXY(vec.X, vec.Z);
        }
        public static Dir4 GetOrthogDirection(this Vector2 dir)
        {
            return GetDir4FromXY(dir.X, dir.Y);
        }
        public static Dir4 GetOrthogDirection(this Vector3 dir)
        {
            return GetDir4FromXY(dir.X, dir.Z);
        }
        private static AnimDirection GetAnimDirection(float forwardDir)
        {
            if (forwardDir > 0)
            { return AnimDirection.Down; }
            else
            { return AnimDirection.Up; }
        }
        public static AnimDirection GetAnimDir(this Vector2 dir)
        {
            return GetAnimDirection(dir.Y);
        }
        public static AnimDirection GetAnimDir(this Vector3 dir)
        {
            return GetAnimDirection(dir.Z);
        }
        public static bool GetFlipH(float horizDir)
        {
            if (horizDir < 0)
            { return true; }
            else
            { return false; }
        }
        public static bool GetFlipH(this Vector2 dir)
        {
            return GetFlipH(dir.X);
        }
        public static bool GetFlipH(this Vector3 dir)
        {
            return GetFlipH(dir.X);
        }
        public static Vector3 ClampIsometric(this Vector3 direction)
        {
            return direction.GetOrthogDirection().GetVector3();
        }
        public static Vector2 ClampIsometric(this Vector2 direction)
        {
            return direction.GetOrthogDirection().GetVector2();
        }
        public static Vector3 ClampInputToIsometric(this Vector2 inputDir)
        {
            // INPUT DIRECTION MEANS UP Y IS NEGATIVE
            return ClampIsometric(new Vector3(inputDir.X, 0f, -inputDir.Y));
        }
        #endregion
        #region VELOCITY_EXTENSIONS

        public const float DEFAULT_WEIGHT_PERCENTAGE = 0.075f;
        public static Vector3 GetWeightedGravity(this CharacterBody3D body,
            float weightPercentage = DEFAULT_WEIGHT_PERCENTAGE)
        {
            Vector3 weightedGrav;
            if (body.Velocity.Y < 0)
            {
                weightedGrav = body.GetGravity() - (body.Velocity * body.GetGravity() * weightPercentage);
            }
            else
            {
                weightedGrav = body.GetGravity();
            }
            //GD.Print("weighted grav: ", weightedGrav);
            return weightedGrav;
        }
        public static Vector3 GetCustomWeightedGravity(this CharacterBody3D body,
            Vector3 customGravity, float weightPercentage)
        {
            Vector3 weightedGrav;
            if (body.Velocity.Y < 0)
            {
                weightedGrav = customGravity - (body.Velocity * customGravity * weightPercentage);
            }
            else
            {
                weightedGrav = customGravity;
            }
            //GD.Print("weighted grav: ", weightedGrav);
            return weightedGrav;
        }
        #endregion

        public static Vector2 GetVector2(this Dir8 dir)
        {
            switch (dir) //RETURN NORMALIZED VECTOR
            {
                case Dir8.Down:
                    return new Vector2(0, 1).Normalized();
                case Dir8.Up:
                    return new Vector2(0, -1).Normalized();
                case Dir8.Left:
                    return new Vector2(-1, 0).Normalized();
                case Dir8.Right:
                    return new Vector2(1, 0).Normalized();
                case Dir8.DownLeft:
                    return new Vector2(-0.7071f, 0.7071f).Normalized();
                case Dir8.DownRight:
                    return new Vector2(0.7071f, 0.7071f).Normalized();
                case Dir8.UpLeft:
                    return new Vector2(-0.7071f, -0.7071f).Normalized();
                case Dir8.UpRight:
                    return new Vector2(0.7071f, -0.7071f).Normalized();
                default:
                    GD.Print("not any face direction?? facedir = " + dir.ToString());
                    return Vector2.Zero;
            }
        }
        public static Vector3 GetVector3(this Dir8 direction)
        {
            var dirVec2 = direction.GetVector2();
            return new Vector3(dirVec2.X, 0, dirVec2.Y);
        }
        public static (Dir16, Dir16) GetNeighboring16Dirs(this Dir16 dir)
        {
            switch (dir) //RETURN NORMALIZED VECTOR
            {
                case Dir16.D:
                    return (Dir16.DDR, Dir16.DDL);
                case Dir16.U:
                    return (Dir16.UUL, Dir16.UUR);
                case Dir16.L:
                    return (Dir16.DLL, Dir16.ULL);
                case Dir16.R:
                    return (Dir16.URR, Dir16.DRR);
                case Dir16.DL:
                    return (Dir16.DDL, Dir16.DLL);
                case Dir16.DR:
                    return (Dir16.DRR, Dir16.DDR);
                case Dir16.UL:
                    return (Dir16.ULL, Dir16.UUL);
                case Dir16.UR:
                    return (Dir16.UUR, Dir16.URR);
                default:
                    throw new Exception("not any face direction?? facedir = " + dir.ToString());
            }
        }
        public static (Dir16, Dir16) GetNeighboring16Dirs(this Dir8 dir)
        {
            switch (dir) //RETURN NORMALIZED VECTOR
            {
                case Dir8.Down:
                    return (Dir16.DDR, Dir16.DDL);
                case Dir8.Up:
                    return (Dir16.UUL, Dir16.UUR);
                case Dir8.Left:
                    return (Dir16.DLL, Dir16.ULL);
                case Dir8.Right:
                    return (Dir16.URR, Dir16.DRR);
                case Dir8.DownLeft:
                    return (Dir16.DDL, Dir16.DLL);
                case Dir8.DownRight:
                    return (Dir16.DRR, Dir16.DDR);
                case Dir8.UpLeft:
                    return (Dir16.ULL, Dir16.UUL);
                case Dir8.UpRight:
                    return (Dir16.UUR, Dir16.URR);
                default:
                    throw new Exception("not any face direction?? facedir = " + dir.ToString());
            }
        }
        //public static T GetLeftDir<T>(this T dir, int dirCount)
        //{

        //}
        public static Dir8 GetLeftDir(this Dir8 dir)
        {
            if ((int)dir == 0)
            {
                return (Dir8)7;
            }
            return (Dir8)((int)dir - 1);
        }
        public static Dir8 GetRightDir(this Dir8 dir)
        {
            if ((int)dir == 7)
            {
                return (Dir8)0;
            }
            return (Dir8)((int)dir + 1);
        }
        public static (Dir8, Dir8) GetNeighboringDirs(this Dir8 dir)
        {
            return (dir.GetLeftDir(), dir.GetRightDir());
        }
        public static Dir16 GetLeftDir(this Dir16 dir)
        {
            if ((int)dir == 0)
            {
                return (Dir16)15;
            }
            return (Dir16)((int)dir - 1);
        }
        public static Dir16 GetRightDir(this Dir16 dir)
        {
            if ((int)dir == 15)
            {
                return (Dir16)0;
            }
            return (Dir16)((int)dir + 1);
        }
        public static (Dir16, Dir16) GetNeighboringDirs(this Dir16 dir)
        {
            return (dir.GetLeftDir(), dir.GetRightDir());
        }
        //public static AIFacing GetAIFacing(this Dir8 rayDir, Dir4 orthogDir)
        //{
        //    switch (rayDir) //RETURN NORMALIZED VECTOR
        //    {
        //        case Dir8.Down:
        //            switch (orthogDir)
        //            {
        //                case Dir4.Down or Dir4.Right:
        //                    return AIFacing.Peripheral;
        //                case Dir4.Left or Dir4.Up:
        //                    return AIFacing.Distant;
        //                default:
        //                    throw new Exception();
        //            }
        //        case Dir8.Up:
        //            switch (orthogDir)
        //            {
        //                case Dir4.Down or Dir4.Right:
        //                    return AIFacing.Distant;
        //                case Dir4.Left or Dir4.Up:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    throw new Exception();
        //            }
        //        case Dir8.Left:
        //            switch (orthogDir)
        //            {
        //                case Dir4.Down or Dir4.Left:
        //                    return AIFacing.Peripheral;
        //                case Dir4.Right or Dir4.Up:
        //                    return AIFacing.Distant;
        //                default:
        //                    throw new Exception();
        //            }
        //        case Dir8.Right:
        //            switch (orthogDir)
        //            {
        //                case Dir4.Down or Dir4.Left:
        //                    return AIFacing.Distant;
        //                case Dir4.Right or Dir4.Up:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    throw new Exception();
        //            }
        //        case Dir8.DownLeft:
        //            switch (orthogDir)
        //            {
        //                case Dir4.Down:
        //                    return AIFacing.Facing;
        //                case Dir4.Right or Dir4.Left:
        //                    return AIFacing.Perpindicular;
        //                case Dir4.Up:
        //                    return AIFacing.Opposite;
        //                default:
        //                    throw new Exception();
        //            }
        //        case Dir8.DownRight:
        //            switch (orthogDir)
        //            {
        //                case Dir4.Right:
        //                    return AIFacing.Facing;
        //                case Dir4.Up or Dir4.Down:
        //                    return AIFacing.Perpindicular;
        //                case Dir4.Left:
        //                    return AIFacing.Opposite;
        //                default:
        //                    throw new Exception();
        //            }
        //        case Dir8.UpLeft:
        //            switch (orthogDir)
        //            {
        //                case Dir4.Right:
        //                    return AIFacing.Opposite;
        //                case Dir4.Up or Dir4.Down:
        //                    return AIFacing.Perpindicular;
        //                case Dir4.Left:
        //                    return AIFacing.Facing;
        //                default:
        //                    throw new Exception();
        //            }
        //        case Dir8.UpRight:
        //            switch (orthogDir)
        //            {
        //                case Dir4.Down:
        //                    return AIFacing.Opposite;
        //                case Dir4.Right or Dir4.Left:
        //                    return AIFacing.Perpindicular;
        //                case Dir4.Up:
        //                    return AIFacing.Facing;
        //                default:
        //                    throw new Exception();
        //            }
        //        default:
        //            throw new Exception("not any face direction?? facedir = " + rayDir.ToString());
        //    }
        //}
        //// Compare Dir16 to Dir8 and return one of the 3 AIFacing enum options
        //public static AIFacing GetAIFacing(this Dir16 dir, Dir8 eightDir)
        //{
        //    switch (dir) // Dir16 is the "center" direction
        //    {
        //        case Dir16.U:
        //            switch (eightDir)
        //            {
        //                case Dir8.Up or Dir8.UpRight or Dir8.UpLeft:
        //                    return AIFacing.Facing;
        //                case Dir8.Right or Dir8.Left:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.UR:
        //            switch (eightDir)
        //            {
        //                case Dir8.UpRight or Dir8.Up or Dir8.Right:
        //                    return AIFacing.Facing;
        //                case Dir8.UpLeft or Dir8.DownRight:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.R:
        //            switch (eightDir)
        //            {
        //                case Dir8.Right or Dir8.UpRight or Dir8.DownRight:
        //                    return AIFacing.Facing;
        //                case Dir8.Up or Dir8.Down:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.DR:
        //            switch (eightDir)
        //            {
        //                case Dir8.DownRight or Dir8.Right or Dir8.Down:
        //                    return AIFacing.Facing;
        //                case Dir8.UpRight or Dir8.DownLeft:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.D:
        //            switch (eightDir)
        //            {
        //                case Dir8.Down or Dir8.DownLeft or Dir8.DownRight:
        //                    return AIFacing.Facing;
        //                case Dir8.Left or Dir8.Right:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.DL:
        //            switch (eightDir)
        //            {
        //                case Dir8.DownLeft or Dir8.Down or Dir8.Left:
        //                    return AIFacing.Facing;
        //                case Dir8.DownRight or Dir8.UpLeft:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.L:
        //            switch (eightDir)
        //            {
        //                case Dir8.Left or Dir8.DownLeft or Dir8.UpLeft:
        //                    return AIFacing.Facing;
        //                case Dir8.Down or Dir8.Right:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.UL:
        //            switch (eightDir)
        //            {
        //                case Dir8.UpLeft or Dir8.Up or Dir8.Left:
        //                    return AIFacing.Facing;
        //                case Dir8.DownLeft or Dir8.UpRight:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }
        //        case Dir16.UUR:
        //            switch (eightDir)
        //            {
        //                case Dir8.Up or Dir8.UpRight or Dir8.UpLeft:
        //                    return AIFacing.Facing;
        //                case Dir8.Right or Dir8.Left:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }
        //        case Dir16.UUL:
        //            switch (eightDir)
        //            {
        //                case Dir8.UpLeft or Dir8.Up:
        //                    return AIFacing.Facing;
        //                case Dir8.Left or Dir8.Right or Dir8.UpRight:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        default:
        //            return AIFacing.Opposite; // Default to opposite if no case matches
        //    }
        //}
        //// Compare Dir16 to Dir4 and return one of the 3 AIFacing enum options
        //public static AIFacing GetAIFacing(this Dir16 dir, Dir4 fourDir)
        //{
        //    switch (dir) // Dir16 is the "center" direction
        //    {
        //        case Dir16.U or Dir16.UUR or Dir16.UUL:
        //            switch (fourDir)
        //            {
        //                case Dir4.Up:
        //                    return AIFacing.Facing;
        //                case Dir4.Right or Dir4.Left:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.UR:
        //            switch (fourDir)
        //            {
        //                case Dir4.Up or Dir4.Right:
        //                    return AIFacing.Facing;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.R or Dir16.URR or Dir16.DRR:
        //            switch (fourDir)
        //            {
        //                case Dir4.Right:
        //                    return AIFacing.Facing;
        //                case Dir4.Up or Dir4.Down:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.DR:
        //            switch (fourDir)
        //            {
        //                case Dir4.Down or Dir4.Right:
        //                    return AIFacing.Facing;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.D or Dir16.DDR or Dir16.DDL:
        //            switch (fourDir)
        //            {
        //                case Dir4.Down:
        //                    return AIFacing.Facing;
        //                case Dir4.Left or Dir4.Right:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.DL:
        //            switch (fourDir)
        //            {
        //                case Dir4.Down or Dir4.Left:
        //                    return AIFacing.Facing;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.L or Dir16.DLL or Dir16.ULL:
        //            switch (fourDir)
        //            {
        //                case Dir4.Left:
        //                    return AIFacing.Facing;
        //                case Dir4.Down or Dir4.Up:
        //                    return AIFacing.Peripheral;
        //                default:
        //                    return AIFacing.Opposite;
        //            }

        //        case Dir16.UL:
        //            switch (fourDir)
        //            {
        //                case Dir4.Up or Dir4.Left:
        //                    return AIFacing.Facing;
        //                default:
        //                    return AIFacing.Opposite; // Treat UR, DL, and DR as peripheral
        //            }
        //        default:
        //            return AIFacing.Opposite; // Default to opposite if no case matches
        //    }
        //}
        public static Vector2 GetVector2(this Dir16 direction)
        {
            switch (direction)
            {
                case Dir16.U: return new Vector2(0, -1).Normalized();             // Up
                case Dir16.UUR: return new Vector2(0.3827f, -0.9239f).Normalized();  // Up-Right-Right (67.5 degrees)
                case Dir16.UR: return new Vector2(0.7071f, -0.7071f).Normalized();  // Up-Right (45 degrees)
                case Dir16.URR: return new Vector2(0.9239f, -0.3827f).Normalized();  // Up-Up-Right (22.5 degrees)
                case Dir16.R: return new Vector2(1, 0).Normalized();              // Right
                case Dir16.DRR: return new Vector2(0.9239f, 0.3827f).Normalized();   // Down-Down-Right (157.5 degrees)
                case Dir16.DR: return new Vector2(0.7071f, 0.7071f).Normalized();   // Down-Right (135 degrees)
                case Dir16.DDR: return new Vector2(0.3827f, 0.9239f).Normalized();   // Down-Right-Right (112.5 degrees)
                case Dir16.D: return new Vector2(0, 1).Normalized();               // Down
                case Dir16.DDL: return new Vector2(-0.3827f, 0.9239f).Normalized();  // Down-Left-Left (247.5 degrees)
                case Dir16.DL: return new Vector2(-0.7071f, 0.7071f).Normalized();  // Down-Left (225 degrees)
                case Dir16.DLL: return new Vector2(-0.9239f, 0.3827f).Normalized();  // Down-Down-Left (202.5 degrees)
                case Dir16.L: return new Vector2(-1, 0).Normalized();              // Left
                case Dir16.ULL: return new Vector2(-0.9239f, -0.3827f).Normalized(); // Up-Left-Left (292.5 degrees)
                case Dir16.UL: return new Vector2(-0.7071f, -0.7071f).Normalized(); // Up-Left (315 degrees)
                case Dir16.UUL: return new Vector2(-0.3827f, -0.9239f).Normalized(); // Up-Up-Left (337.5 degrees)
                default: return Vector2.Zero;
            }
        }
        public static Vector3 GetVector3(this Dir16 direction)
        {
            var dirVec2 = direction.GetVector2();
            return new Vector3(dirVec2.X, 0, dirVec2.Y);
        }
        // Function to compute the angle between two vectors in degrees
        public static float GetAngleToVector(this Vector2 v1, Vector2 v2)
        {
            // Ensure vectors are normalized (they should be normalized beforehand)
            if (!v1.IsNormalized() || !v2.IsNormalized())
                throw new ArgumentException("Both vectors must be normalized (unit vectors).");

            // Calculate the dot product
            float dotProduct = v1.Dot(v2);

            // Clamp the dot product to avoid potential issues with floating-point precision errors
            dotProduct = Math.Clamp(dotProduct, -1f, 1f);

            // Calculate the angle in radians
            float angleInRadians = MathF.Acos(dotProduct);

            // Convert radians to degrees
            float angleInDegrees = angleInRadians * (180f / MathF.PI);

            return angleInDegrees;
        }

        // Compare two normalized vectors and return their AIFacing classification based on the angle
        public static AIFacing GetAIFacing(this Vector2 v1, Vector2 v2)
        {
            // Get the angle between the two vectors
            float angle = v1.GetAngleToVector(v2);

            // Classify based on the angle
            if (angle <= 60f) // Angle <= 60° (Facing)
            {
                return AIFacing.Facing;
            }
            else if (angle <= 120f) // 60° < Angle <= 120° (Peripheral)
            {
                return AIFacing.Peripheral;
            }
            else // Angle > 120° (Opposite)
            {
                return AIFacing.Opposite;
            }
        }
        public static AIFacing GetAIFacing(this Dir16 dir, Dir8 dir8)
        {
            return dir.GetVector2().GetAIFacing(dir8.GetVector2());
        }
        public static AIFacing GetAIFacing(this Dir16 dir, Dir4 dir4)
        {
            return dir.GetVector2().GetAIFacing(dir4.GetVector2());
        }

        public static readonly Dictionary<Dir16, Dir8> Dir16To8Map = new Dictionary<Dir16, Dir8>()
    {
        {Dir16.U, Dir8.Up },
        {Dir16.UR, Dir8.UpRight },
        {Dir16.R, Dir8.Right },
        {Dir16.DR, Dir8.DownRight },
        {Dir16.D, Dir8.Down },
        {Dir16.DL, Dir8.DownLeft },
        {Dir16.L, Dir8.Left},
        {Dir16.UL, Dir8.UpLeft },
    };
        public static readonly Dictionary<Dir8, Dir16> Dir8To16Map = Dir16To8Map.ToDictionary((i) => i.Value, (i) => i.Key);
        public static readonly Dictionary<Dir8, Dir4> Dir8To4Map = new Dictionary<Dir8, Dir4>
    {
        {Dir8.Up, Dir4.Up },
        {Dir8.Right, Dir4.Right },
        {Dir8.Down, Dir4.Down },
        {Dir8.Left, Dir4.Left }
    };
        public static readonly Dictionary<Dir4, Dir8> Dir4To8Map = Dir8To4Map.ToDictionary((i) => i.Value, (i) => i.Key);

        public static readonly Dictionary<Dir16, Dir4> Dir16To4Map = new Dictionary<Dir16, Dir4>()
    {
        {Dir16.U, Dir4.Up},
        {Dir16.R, Dir4.Right},
        {Dir16.D, Dir4.Down},
        {Dir16.L, Dir4.Left},
    };
        public static readonly Dictionary<Dir4, Dir16> Dir4To16Map = Dir16To4Map.ToDictionary((i) => i.Value, (i) => i.Key);

        public static Dir8? GetDir8(this Dir16 dir)
        {
            if (Dir16To8Map.TryGetValue(dir, out Dir8 dir8))
            {
                return dir8;
            }
            else
            {
                return null;
            }
        }
        public static Dir4? GetDir4(this Dir16 dir)
        {
            if (Dir16To4Map.TryGetValue(dir, out Dir4 dir4))
            {
                return dir4;
            }
            else
            {
                return null;
            }
        }
        public static Dir16 GetDir16(this Dir8 dir)
        {
            return Dir8To16Map[dir];
        }
        public static Dir4? GetDir4(this Dir8 dir)
        {
            if (Dir8To4Map.TryGetValue(dir, out Dir4 dir4))
            {
                return dir4;
            }
            else
            {
                return null;
            }
        }
        public static Dir16 GetDir16(this Dir4 dir)
        {
            return Dir4To16Map[dir];
        }
        public static Dir8 GetDir8(this Dir4 dir)
        {
            return Dir4To8Map[dir];
        }
        public static Vector3 GetVector3(this Vector2 vec2)
        {
            return new Vector3(vec2.X, 0f, vec2.Y);
        }
        public static Vector2 GetVector2(this Vector3 vec3)
        {
            return new Vector2(vec3.X, vec3.Z);
        }
    }
}

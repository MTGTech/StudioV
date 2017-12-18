

public class BoneLimit
{

    public struct Head
    {

        public static float xMin = -55f;
        public static float xMax = 80f;
        public static float yMin = -70f;
        public static float yMax = 70f;
        public static float zMin = -35f;
        public static float zMax = 35f;
    }

    public struct LShoulder
    {

        public static float xMin = -180f;
        public static float xMax = 90f;
        public static float yMin = -45f;
        public static float yMax = 130f;
        public static float zMin = -85f;
        public static float zMax = 130f;
    }

    public struct RShoulder
    {

        public static float xMin = -180f;
        public static float xMax = 90f;
        public static float yMin = -130f;
        public static float yMax = 45f;
        public static float zMin = -130f;
        public static float zMax = 85f;
    }

    public struct Ab
    {
        public static float xMin = -30f;
        public static float xMax = 75f;
        public static float yMin = -360f;
        public static float yMax = 360f;
        public static float zMin = -35f;
        public static float zMax = 35f;
    }

    public struct RFArm
    {
        public static float xMin = -180f;
        public static float xMax = 90f;
        public static float yMin = -150f;
        public static float yMax = 5f;
        public static float zMin = -150f;
        public static float zMax = 5f;
    }

    public struct LFArm
    {
        public static float xMin = -180f;
        public static float xMax = 90f;
        public static float yMin = -5f;
        public static float yMax = 150f;
        public static float zMin = -5f;
        public static float zMax = 150f;
    }

    // TODO: clamp thumb
    #region Thumb
    public struct LThumb1
    {
        public static float xMin = -10f;
        public static float xMax = 40f;
        public static float yMin = -70f;
        public static float yMax = 30f;
        public static float zMin = -30f;
        public static float zMax = 30f;
    }

    public struct RThumb1
    {
        public static float xMin = -10f;
        public static float xMax = 40f;
        public static float yMin = -30f;
        public static float yMax = 70f;
        public static float zMin = -30f;
        public static float zMax = 30f;
    }

    public struct LThumb2
    {
        public static float xMin = -15f;
        public static float xMax = 40f;
        public static float yMin = -100f;
        public static float yMax = 25f;
        public static float zMin = -130f;
        public static float zMax = 130f;
    }

    public struct RThumb2
    {
        public static float xMin = -15f;
        public static float xMax = 40f;
        public static float yMin = -25f;
        public static float yMax = 100f;
        public static float zMin = -130f;
        public static float zMax = 130f;
    }

    #endregion

    #region IndexFinger, RingFinger, MiddleFinger and Pinky

    public struct RRing1
    {
        public static float zMin = -95f;
        public static float zMax = 5f;
        public static float yMin = 0f;
        public static float yMax = 20f;
        public static float xMin = -5f;
        public static float xMax = 5f;
    }
    public struct RRing2
    {
        public static float zMin = -100f;
        public static float zMax = 5f;
        public static float yMin = 0f;
        public static float yMax = 20f;
        public static float xMin = -5f;
        public static float xMax = 5f;
    }

    public struct LRing1
    {
        public static float zMin = -5f;
        public static float zMax = 95f;
        public static float yMin = 0f;
        public static float yMax = 20f;
        public static float xMin = -5f;
        public static float xMax = 5f;
    }
    public struct LRing2
    {
        public static float zMin = -5f;
        public static float zMax = 100f;
        public static float yMin = 0f;
        public static float yMax = 20f;
        public static float xMin = -5f;
        public static float xMax = 5f;
    }

    #endregion

    public struct LHand
    {
        public static float xMin = -5f;
        public static float xMax = 5f;
        public static float yMin = -40f;
        public static float yMax = 20f;       
        public static float zMin = -70f;
        public static float zMax = 85f;
    }

    public struct RHand
    {
        public static float xMin = -5f;
        public static float xMax = 5f;
        public static float yMin = -20f;
        public static float yMax = 40f;
        public static float zMin = -85f;
        public static float zMax = 70f;
    }

    public struct LThigh
    {
        public static float xMin = -120f;
        public static float xMax = 30f;
        public static float yMin = -45f;
        public static float yMax = 40f;
        public static float zMin = -120f;
        public static float zMax = 25f;
    }

    public struct RThigh
    {
        public static float xMin = -120f;
        public static float xMax = 30f;
        public static float yMin = -40f;
        public static float yMax = 45f;
        public static float zMin = -25f;
        public static float zMax = 120f;
    }

    public struct LShin
    {
        public static float xMin = -15f;
        public static float xMax = 130f;
        public static float yMin = -10f;
        public static float yMax = 10f;
        public static float zMin = -5f;
        public static float zMax = 5f;
    }

    public struct RShin
    {
        public static float xMin = -130f;
        public static float xMax = 15f;
        public static float yMin = -10f;
        public static float yMax = 10f;
        public static float zMin = -5f;
        public static float zMax = 5f;
    }

    public struct LFoot
    {
        public static float xMin = -45f;
        public static float xMax = 20f;
        public static float yMin = -85f;
        public static float yMax = 50f;
        public static float zMin = -20f;
        public static float zMax = 30f;
    }

    public struct RFoot
    {
        public static float xMin = -45f;
        public static float xMax = 20f;
        public static float yMin = -50f;
        public static float yMax = 85f;
        public static float zMin = -30f;
        public static float zMax = 20f;
    }
}
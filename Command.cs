using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    public class Command
    {
        public const string POSITION = "SEND_POSITION";
        public const string FRAME = "CAPTURE_FRAME";
        public const string FRAME_ROT = "CAPTURE_ROT";
        public const string FRAME_LOC = "CAPTURE_LOC";
        public const string REC = "RECORD_POSITION";
        public const string STOP = "STOP_RECORD";
        public const string FORWARD = "NEXT_FRAME";
        public const string FAST_FORWARD = "NEXT_MORE_FRAME";
        public const string BACKWARD = "PREW_FRAME";
        public const string FAST_BACKWARD = "PREW_MORE_FRAME";
        public const string DELETE = "DELETE_ALL";
        public const string DELETE_CURRENT = "DELETE_CURRENT";
        public const string RESET = "RESET_SENSOR_VALUE";
        public const string PLAY_ANIMATION = "PLAY_ANIMATION";
        public const string HIDE_CAPTURE = "HIDE_CAPTURE";
        public const string VIRTUAL_ARMATURE = "VIRTUAL_ARMATURE";
        public const string BONES_ASSOCIATION = "BONES_ASSOCIATION";
        public const string CHANGE_PRESET = "CHANGE_PRESET";
        public const string TRANSLATE_COORDINATE_SYSTEM = "TRANSLATE_SYS";
        public const string RESET_ORIGIN = "RESET_ORIGIN";
        public const string FIRST_FRAME = "FIRST_FRAME";
        public const string LAST_FRAME = "LAST_FRAME";
        public const string ACTIVE_GHOST_FRAME = "ACTIVE_GHOST_FRAME";
        public const string DISABLE_GHOST_FRAME = "DISABLE_GHOST_FRAME";
        public const string START_TEST = "START_TEST";

        public const string MORE_ACCURACY = "MORE_ACCURACY";
        public const string LESS_ACCURACY = "LESS_ACCURACY";
        public const string START_POSE = "START_POSE";
        public const string END_POSE = "END_POSE";
        public const string COMPUTE_FACTOR = "COMPUTE_FACTOR";

        public const string LOCK_POSE = "LOCK_POSE";
        public const string LOAD_POSE = "LOAD_POSE";
        public const string UNLOCK_POSE = "UNLOCK_POSE";

        public static string AUTO_CONFIG = "AUTO_CONFIG";
    }
    public class FrameType
    {
        public const string ROTATION = "ROT";
        public const string LOCATION = "LOC";
        public const string LOCATION_ROTATION = "LOC_ROT";
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class KinectSkeleton
    {
        // DoFs of Kinect Skeleton
        public const int KINECT_SKELETON_DOF = 27;        

        public static List<List<Bone>> GetKinectPartition()
        {

            List<char> axis = new List<char>() { 'x', 'y', 'z' };
            List<List<Bone>> skeleton = new List<List<Bone>>();

            /*            
            // Kinect skeleton full DoFs
            Bone shoulder_right = new Bone("Shoulder.R_NUI");
            shoulder_right.rot_DoF = axis;
            shoulder_right.loc_DoF = axis;
            shoulder_right.level = 0;
            shoulder_right.parent = "Shoulder_NUI";
            shoulder_right.children = new List<string>() { "Elbow.R_NUI" };

            Bone elbow_right = new Bone("Elbow.R_NUI");
            elbow_right.rot_DoF = axis;
            elbow_right.loc_DoF = axis;
            elbow_right.level = 1;
            elbow_right.parent = "Shoulder.R_NUI";
            elbow_right.children = new List<string>() { "Wrist.R_NUI" };

            Bone wrist_right = new Bone("Wrist.R_NUI");
            wrist_right.rot_DoF = axis;
            wrist_right.loc_DoF = axis;
            wrist_right.level = 2;
            wrist_right.parent = "Elbow.R_NUI";
            wrist_right.children = new List<string>() { "Hand.R_NUI" };

            Bone hand_right = new Bone("Hand.R_NUI");
            hand_right.rot_DoF = axis;
            hand_right.loc_DoF = axis;
            hand_right.level = 3;
            hand_right.parent = "Wrist.R_NUI";
            hand_right.children = new List<string>() { };

            List<Bone> upper_right = new List<Bone>();
            upper_right.Add(shoulder_right);
            upper_right.Add(elbow_right);
            upper_right.Add(wrist_right);
            upper_right.Add(hand_right);
            skeleton.Add(upper_right);


            Bone shoulder_left = new Bone("Shoulder.L_NUI");
            shoulder_left.rot_DoF = axis;
            shoulder_left.loc_DoF = axis;
            shoulder_left.level = 0;
            shoulder_left.parent = "Shoulder_NUI";
            shoulder_left.children = new List<string>() { "Elbow.L_NUI" };

            Bone elbow_left = new Bone("Elbow.L_NUI");
            elbow_left.rot_DoF = axis;
            elbow_left.loc_DoF = axis;
            elbow_left.level = 1;
            elbow_left.parent = "Shoulder.L_NUI";
            elbow_left.children = new List<string>() { "Wrist.L_NUI" };

            Bone wrist_left = new Bone("Wrist.L_NUI");
            wrist_left.rot_DoF = axis;
            wrist_left.loc_DoF = axis;
            wrist_left.level = 2;
            wrist_left.parent = "Elbow.L_NUI";
            wrist_left.children = new List<string>() { "Hand.L_NUI" };

            Bone hand_left = new Bone("Hand.L_NUI");
            hand_left.rot_DoF = axis;
            hand_left.loc_DoF = axis;
            hand_left.level = 3;
            hand_left.parent = "Wrist.L_NUI";
            hand_left.children = new List<string>() { };

            List<Bone> upper_left = new List<Bone>();
            upper_left.Add(shoulder_left);
            upper_left.Add(elbow_left);
            upper_left.Add(wrist_left);
            upper_left.Add(hand_left);
            skeleton.Add(upper_left);


            Bone head = new Bone("Head_NUI");
            head.rot_DoF = axis;
            head.loc_DoF = axis;
            head.level = 3;
            head.parent = "Shoulder_NUI";
            head.children = new List<string>() { };

            Bone shoulder_center = new Bone("Shoulder_NUI");
            shoulder_center.rot_DoF = axis;
            shoulder_center.loc_DoF = axis;
            shoulder_center.level = 2;
            shoulder_center.parent = "Spine_NUI";
            shoulder_center.children = new List<string>() { "Shoulder.R_NUI", "Shoulder.L_NUI", "Head_NUI" };

            Bone spine = new Bone("Spine_NUI");
            spine.rot_DoF = axis;
            spine.loc_DoF = axis;
            spine.level = 1;
            spine.parent = "Hip_NUI";
            spine.children = new List<string>() { "Shoulder_NUI" };

            Bone hip_center = new Bone("Hip_NUI");
            hip_center.rot_DoF = axis;
            hip_center.loc_DoF = axis;
            hip_center.level = 0;
            hip_center.parent = "";
            hip_center.children = new List<string>() { "Spine_NUI", "Hip.R_NUI", "Hip.L_NUI" };

            List<Bone> center = new List<Bone>();
            center.Add(head);
            center.Add(shoulder_center);
            center.Add(spine);
            center.Add(hip_center);
            skeleton.Add(center);


            Bone hip_right = new Bone("Hip.R_NUI");
            hip_right.rot_DoF = axis;
            hip_right.loc_DoF = axis;
            hip_right.level = 0;
            hip_right.parent = "Hip";
            hip_right.children = new List<string>() { "Knee.R_NUI" };

            Bone knee_right = new Bone("Knee.R_NUI");
            knee_right.rot_DoF = axis;
            knee_right.loc_DoF = axis;
            knee_right.level = 1;
            knee_right.parent = "Hip.R_NUI";
            knee_right.children = new List<string>() { "Ankle.R_NUI" };

            Bone ankle_right = new Bone("Ankle.R_NUI");
            ankle_right.rot_DoF = axis;
            ankle_right.loc_DoF = axis;
            ankle_right.level = 2;
            ankle_right.parent = "Knee.R_NUI";
            ankle_right.children = new List<string>() { "Foot.R_NUI" };

            Bone foot_right = new Bone("Foot.R_NUI");
            foot_right.rot_DoF = axis;
            foot_right.loc_DoF = axis;
            foot_right.level = 3;
            foot_right.parent = "Ankle.R_NUI";
            foot_right.children = new List<string>() { };

            List<Bone> lower_right = new List<Bone>();
            lower_right.Add(hip_right);
            lower_right.Add(knee_right);
            lower_right.Add(ankle_right);
            lower_right.Add(foot_right);
            skeleton.Add(lower_right);


            Bone hip_left = new Bone("Hip.L_NUI");
            hip_left.rot_DoF = axis;
            hip_left.loc_DoF = axis;
            hip_left.level = 0;
            hip_left.parent = "Hip_NUI";
            hip_left.children = new List<string>() { "Knee.L_NUI" };

            Bone knee_left = new Bone("Knee.L_NUI");
            knee_left.rot_DoF = axis;
            knee_left.loc_DoF = axis;
            knee_left.level = 1;
            knee_left.parent = "Hip.L_NUI";
            knee_left.children = new List<string>() { "Ankle.L_NUI" };

            Bone ankle_left = new Bone("Ankle.L_NUI");
            ankle_left.rot_DoF = axis;
            ankle_left.loc_DoF = axis;
            ankle_left.level = 2;
            ankle_left.parent = "Knee.L_NUI";
            ankle_left.children = new List<string>() { "Foot.L_NUI" };

            Bone foot_left = new Bone("Foot.L_NUI");
            foot_left.rot_DoF = axis;
            foot_left.loc_DoF = axis;
            foot_left.level = 3;
            foot_left.parent = "Ankle.L_NUI";
            foot_left.children = new List<string>() { };

            List<Bone> lower_left = new List<Bone>();
            lower_left.Add(hip_left);
            lower_left.Add(knee_left);
            lower_left.Add(ankle_left);
            lower_left.Add(foot_left);
            skeleton.Add(lower_left);

            return skeleton;
            */            
            

            Bone shoulder_right = new Bone("Shoulder.R_NUI");
            shoulder_right.rot_DoF = new List<char>() { 'x','z' };
            shoulder_right.loc_DoF = axis;
            shoulder_right.level = 0;
            shoulder_right.parent = "Shoulder_NUI";
            shoulder_right.children = new List<string>() { "Elbow.R_NUI" };

            Bone elbow_right = new Bone("Elbow.R_NUI");
            elbow_right.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            elbow_right.loc_DoF = axis;
            elbow_right.level = 1;
            elbow_right.parent = "Shoulder.R_NUI";
            elbow_right.children = new List<string>() { "Wrist.R_NUI" };

            Bone wrist_right = new Bone("Wrist.R_NUI");
            wrist_right.rot_DoF = new List<char>() { 'x' };
            wrist_right.loc_DoF = axis;
            wrist_right.level = 2;
            wrist_right.parent = "Elbow.R_NUI";
            wrist_right.children = new List<string>() { "Hand.R_NUI" };

            Bone hand_right = new Bone("Hand.R_NUI");
            hand_right.rot_DoF = new List<char>() { 'x', 'z' };
            hand_right.loc_DoF = axis;
            hand_right.level = 3;
            hand_right.parent = "Wrist.R_NUI";
            hand_right.children = new List<string>() { };

            List<Bone> upper_right = new List<Bone>();
            upper_right.Add(shoulder_right);
            upper_right.Add(elbow_right);
            upper_right.Add(wrist_right);
            upper_right.Add(hand_right);
            skeleton.Add(upper_right);



            Bone shoulder_left = new Bone("Shoulder.L_NUI");
            shoulder_left.rot_DoF = new List<char>() { 'x','z' };
            shoulder_left.loc_DoF = axis;
            shoulder_left.level = 0;
            shoulder_left.parent = "Shoulder_NUI";
            shoulder_left.children = new List<string>() { "Elbow.L_NUI" };

            Bone elbow_left = new Bone("Elbow.L_NUI");
            elbow_left.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            elbow_left.loc_DoF = axis;
            elbow_left.level = 1;
            elbow_left.parent = "Shoulder.L_NUI";
            elbow_left.children = new List<string>() { "Wrist.L_NUI" };

            Bone wrist_left = new Bone("Wrist.L_NUI");
            wrist_left.rot_DoF = new List<char>() { 'x' };
            wrist_left.loc_DoF = axis;
            wrist_left.level = 2;
            wrist_left.parent = "Elbow.L_NUI";
            wrist_left.children = new List<string>() { "Hand.L_NUI" };

            Bone hand_left = new Bone("Hand.L_NUI");
            hand_left.rot_DoF = new List<char>() { 'x', 'z' };
            hand_left.loc_DoF = axis;
            hand_left.level = 3;
            hand_left.parent = "Wrist.L_NUI";
            hand_left.children = new List<string>() { };

            List<Bone> upper_left = new List<Bone>();
            upper_left.Add(shoulder_left);
            upper_left.Add(elbow_left);
            upper_left.Add(wrist_left);
            upper_left.Add(hand_left);
            skeleton.Add(upper_left);


            Bone head = new Bone("Head_NUI");
            head.rot_DoF = new List<char>() { 'x', 'z' };
            head.loc_DoF = axis;
            head.level = 3;
            head.parent = "Shoulder_NUI";
            head.children = new List<string>() { };

            Bone shoulder_center = new Bone("Shoulder_NUI");
            shoulder_center.rot_DoF = new List<char>() { '0' };
            shoulder_center.loc_DoF = axis;
            shoulder_center.level = 2;
            shoulder_center.parent = "Spine_NUI";
            shoulder_center.children = new List<string>() { "Shoulder.R_NUI", "Shoulder.L_NUI", "Head_NUI" };

            Bone spine = new Bone("Spine_NUI");
            spine.rot_DoF = new List<char>() { '0' };
            spine.loc_DoF = axis;
            spine.level = 1;
            spine.parent = "Hip_NUI";
            spine.children = new List<string>() { "Shoulder_NUI" };

            Bone hip_center = new Bone("Hip_NUI");
            hip_center.rot_DoF = new List<char>() { 'x', 'y', 'z' };
            hip_center.loc_DoF = axis;
            hip_center.level = 0;
            hip_center.parent = "";
            hip_center.children = new List<string>() { "Spine_NUI", "Hip.R_NUI", "Hip.L_NUI" };

            List<Bone> center = new List<Bone>();
            center.Add(head);
            center.Add(shoulder_center);
            center.Add(spine);
            center.Add(hip_center);
            skeleton.Add(center);


            Bone hip_right = new Bone("Hip.R_NUI");
            hip_right.rot_DoF = new List<char>() { '0' };
            hip_right.loc_DoF = axis;
            hip_right.level = 0;
            hip_right.parent = "Hip";
            hip_right.children = new List<string>() { "Knee.R_NUI" };

            Bone knee_right = new Bone("Knee.R_NUI");
            knee_right.rot_DoF = new List<char>() { 'x', 'z' };
            knee_right.loc_DoF = axis;
            knee_right.level = 1;
            knee_right.parent = "Hip.R_NUI";
            knee_right.children = new List<string>() { "Ankle.R_NUI" };

            Bone ankle_right = new Bone("Ankle.R_NUI");
            ankle_right.rot_DoF = new List<char>() { 'x' };
            ankle_right.loc_DoF = axis;
            ankle_right.level = 2;
            ankle_right.parent = "Knee.R_NUI";
            ankle_right.children = new List<string>() { "Foot.R_NUI" };

            Bone foot_right = new Bone("Foot.R_NUI");
            foot_right.rot_DoF = new List<char>() { '0' };
            foot_right.loc_DoF = axis;
            foot_right.level = 3;
            foot_right.parent = "Ankle.R_NUI";
            foot_right.children = new List<string>() { };

            List<Bone> lower_right = new List<Bone>();
            lower_right.Add(hip_right);
            lower_right.Add(knee_right);
            lower_right.Add(ankle_right);
            lower_right.Add(foot_right);
            skeleton.Add(lower_right);


            Bone hip_left = new Bone("Hip.L_NUI");
            hip_left.rot_DoF = new List<char>() { '0' };
            hip_left.loc_DoF = axis;
            hip_left.level = 0;
            hip_left.parent = "Hip_NUI";
            hip_left.children = new List<string>() { "Knee.L_NUI" };

            Bone knee_left = new Bone("Knee.L_NUI");
            knee_left.rot_DoF = new List<char>() { 'x', 'z' };
            knee_left.loc_DoF = axis;
            knee_left.level = 1;
            knee_left.parent = "Hip.L_NUI";
            knee_left.children = new List<string>() { "Ankle.L_NUI" };

            Bone ankle_left = new Bone("Ankle.L_NUI");
            ankle_left.rot_DoF = new List<char>() { 'x' };
            ankle_left.loc_DoF = axis;
            ankle_left.level = 2;
            ankle_left.parent = "Knee.L_NUI";
            ankle_left.children = new List<string>() { "Foot.L_NUI" };

            Bone foot_left = new Bone("Foot.L_NUI");
            foot_left.rot_DoF = new List<char>() { '0' };
            foot_left.loc_DoF = axis;
            foot_left.level = 3;
            foot_left.parent = "Ankle.L_NUI";
            foot_left.children = new List<string>() { };

            List<Bone> lower_left = new List<Bone>();
            lower_left.Add(hip_left);
            lower_left.Add(knee_left);
            lower_left.Add(ankle_left);
            lower_left.Add(foot_left);
            skeleton.Add(lower_left);

            return skeleton;

        }

        public static List<Bone> GetKinectSkeleton()
        {
            List<Bone> kinectSkeleton = new List<Bone>();
            foreach (List<Bone> listOfBones in KinectSkeleton.GetKinectPartition())
                foreach (Bone b in listOfBones)
                    kinectSkeleton.Add(b);
            return kinectSkeleton;
        }

        public static bool KinectAssignmentConsistency(PartitionAssignment result)
        {
            for (int i = 1; i < result.Assignment.Length; i++)
            {
                string componentName1 = result.Handler[result.Assignment[i - 1]].name;
                if (componentName1.Contains("PORT"))
                    continue;
                else
                    componentName1 = componentName1.Split('_')[0];

                string componentName2 = result.Handler[result.Assignment[i]].name;
                if (componentName2.Contains("PORT"))
                    continue;
                else
                    componentName2 = componentName2.Split('_')[0];

                if (componentName1.Equals(componentName2))
                {
                    string boneName1 = result.Partition[i - 1].name.Split('_')[0];
                    string boneName2 = result.Partition[i].name.Split('_')[0];

                    if (!boneName1.Equals(boneName2))
                        return false;
                }

            }
            return true;
        }      
    }

    public class KinectPieces
    {
        public float Score { get; set; }
        public Bone Bones { get; set; }

        public KinectPieces(float score, Bone bones)
        {
            this.Bones = bones;
            this.Score = score;
        }
    }
}

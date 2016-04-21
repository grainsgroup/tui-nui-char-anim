using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    
    public class Setting
    {
        public string SettingName { get; set; }
        public List<SettingItem> CheckBoxStatus { get; set; }
        public List<SettingItem> ComboBoxStatus { get; set; }
        public List<SettingItem> SliderStatus { get; set; }        

        public Setting(string settingName)
        {
            this.SettingName = settingName;
            this.CheckBoxStatus = new List<SettingItem>();
            this.ComboBoxStatus = new List<SettingItem>();
            this.SliderStatus = new List<SettingItem>();
            
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Setting s = (Setting)obj;
            return (SettingName == s.SettingName);
        }

    }

    public class SettingItem
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public SettingItem(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class VoiceCommand
    {
        public string SpeechRecognized { get; set; }
        public string Rule { get; set; }

        public VoiceCommand(string speechRecognized, string rule) 
        {
            this.SpeechRecognized = speechRecognized;
            this.Rule = rule;
        }
        
        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            VoiceCommand p = (VoiceCommand)obj;
            return (Rule == p.Rule) && (SpeechRecognized == p.SpeechRecognized);
        }
    }

    public class Config
    {
        public List<Setting> Settings { get; set; }
        //public List<VoiceCommand> VoiceCommands { get; set; }

        public Config() 
        {
            this.Settings = new List<Setting>();
            //this.VoiceCommands = new List<VoiceCommand>();
        }
    }

    public class Partition
    {
        public List<List<Bone>> partition;
        public List<Bone> bones;
        int motorAvailable, legoMotors;

        public Partition(int motor) 
        {
            partition = new List<List<Bone>>();
            bones = new List<Bone>();
            motorAvailable = motor;
            legoMotors = motor;            
        }
        
        public void dfs_DiscoverVertex_MaxRotDoF(Bone vertex)
        {
            if (vertex.rot_DoF.Count > 0)
            {
                // first element for parent checking
                if (bones.Count < 1)
                {
                    bones.Add(vertex);
                    motorAvailable -= vertex.rot_DoF.Count;
                }
                
                //else if(bones[bones.Count - 1].name.Equals(vertex.parent) && (motorAvailable - vertex.rot_DoF.Count >= 0))           
                //else if ((bones[bones.Count - 1].name.Equals(vertex.parent) || vertex.children.Contains(bones[bones.Count - 1].name)) && (motorAvailable - vertex.rot_DoF.Count >= 0))

                //else if (motorAvailable - vertex.rot_DoF.Count >= 0)
                else if (IsConnectedBone(bones, vertex) && (motorAvailable - vertex.rot_DoF.Count >= 0))
                {
                    bones.Add(vertex);
                    motorAvailable -= vertex.rot_DoF.Count;
                }
                else
                {
                    partition.Add(bones);
                    bones = new List<Bone>();
                    motorAvailable = legoMotors - vertex.rot_DoF.Count;
                    bones.Add(vertex);
                } 
            }
        }

        
        
        public void dfs_DiscoverVertex_MaxLocRotDoF(Bone vertex)
        {                    
            if (vertex.rot_DoF.Count > 0 || vertex.loc_DoF.Count > 0)
            {
                // first element for parent checking
                if (bones.Count < 1)
                {
                    bones.Add(vertex);
                    motorAvailable = motorAvailable - (vertex.rot_DoF.Count + vertex.loc_DoF.Count);
                }
                //else if(bones[bones.Count - 1].name.Equals(vertex.parent) && (motorAvailable - vertex.rot_DoF.Count >= 0))  
                //else if ((bones[bones.Count - 1].name.Equals(vertex.parent) || vertex.children.Contains(bones[bones.Count - 1].name)) && (motorAvailable - (vertex.rot_DoF.Count + vertex.loc_DoF.Count)>= 0))
                //else if (motorAvailable - (vertex.rot_DoF.Count + vertex.loc_DoF.Count) >= 0)
                else if (IsConnectedBone(bones, vertex) && (motorAvailable - (vertex.rot_DoF.Count + vertex.loc_DoF.Count) >= 0))
                {
                    bones.Add(vertex);
                    motorAvailable = motorAvailable - (vertex.rot_DoF.Count + vertex.loc_DoF.Count);
                } 
                else
                {
                    partition.Add(bones);
                    bones = new List<Bone>();
                    motorAvailable = legoMotors - (vertex.rot_DoF.Count + vertex.loc_DoF.Count);
                    bones.Add(vertex);
                } 
            }

        }

        private bool IsConnectedBone(List<Bone> bones, Bone vertex)
        {
            bool connectedElement = false;
            foreach (Bone b in bones)
            {
                if (b.parent.Equals(vertex.name))
                {
                    connectedElement = true;
                    break;
                }
                if (b.children.Contains(vertex.name))
                {
                    connectedElement = true;
                    break;
                }
            }
            return connectedElement;
        }

        public void dfs_DiscoverVertex_CostDof(Bone vertex)
        {
            // first element for parent checking
            if (bones.Count < 1)
            {
                bones.Add(vertex);
                motorAvailable -= vertex.rot_DoF.Count;
            } 
            else if (bones[bones.Count - 1].name.Equals(vertex.parent) || vertex.children.Contains(bones[bones.Count - 1].name))
            {
                // Adds the entire bone in the partition
                if ((motorAvailable - vertex.rot_DoF.Count >= 0))
                {
                    bones.Add(vertex);
                    motorAvailable -= vertex.rot_DoF.Count;
                }
                else 
                {
                    // Splits the joint
                    Bone PartialBone = new Bone(vertex.name + "_PART");
                    PartialBone.level = vertex.level;
                    for (int i = 0; i < motorAvailable; i++) 
                    {
                        PartialBone.rot_DoF.Add(vertex.rot_DoF[i]);
                    }
                    if (PartialBone.rot_DoF.Count > 0)
                    {
                        bones.Add(PartialBone);
                    }
                    partition.Add(bones);
                    bones = new List<Bone>();

                    PartialBone = new Bone(vertex.name + "_PART");
                    PartialBone.level = vertex.level;
                    for (int i = motorAvailable; i < vertex.rot_DoF.Count; i++)
                    {
                        PartialBone.rot_DoF.Add(vertex.rot_DoF[i]);
                    }
                    if (PartialBone.rot_DoF.Count > 0)
                    {
                        bones.Add(PartialBone);
                    }
                    motorAvailable = legoMotors - PartialBone.rot_DoF.Count;    
                }                
            }
            else
            {
                partition.Add(bones);
                bones = new List<Bone>();
                motorAvailable = legoMotors - vertex.rot_DoF.Count;
                bones.Add(vertex);
            }
        }

    }

  
    public class AxisArrangement : IComparable<AxisArrangement>
    {
        public string Name { get; set; }
        public char[] AxisCombination { get; set; }
        public float Score { get; set; }

        public AxisArrangement(string name, char[] comb, float score)
        {
            this.Name = name;
            this.AxisCombination = comb;
            this.Score = score;            
        }

        public int CompareTo(AxisArrangement compareAxisArrangement)
        {
            // A null value means that this object is greater.
            if (compareAxisArrangement == null)
                return 1;
            else
            { 
                return this.Score.CompareTo(compareAxisArrangement.Score);
            }        
        }
    }

    public class PartitionAssignment : IComparable<PartitionAssignment>
    {
        public string Name { get; set; }

        public int[] Assignment { get; set;}
        public List<Bone> Partition { get; set;}
        public List<Bone> Handler { get; set; }        
        public float Score { get; set; }                

        public PartitionAssignment(string name, int[] assignment, List<Bone> partition, List<Bone>motorDeomposition, float score) 
        {
            this.Name = name;
            this.Assignment = assignment;
            this.Partition = partition;
            this.Handler = motorDeomposition;
            this.Score = score;            
        }
        
        public int CompareTo(PartitionAssignment compareAxisArrangement)
        {
            // A null value means that this object is greater.
            if (compareAxisArrangement == null)
                return 1;
            else{ 
                if(this.Score!=compareAxisArrangement.Score)
                    return this.Score.CompareTo(compareAxisArrangement.Score);
                else
                    return this.Partition.Count.CompareTo(compareAxisArrangement.Partition.Count);
                }
        }
        
    }

    public class DecompositionAssignment : IComparable<DecompositionAssignment>
    {
        
        public List<PartitionAssignment> PartitionAss { get; set; }
        public float TotalScore { get; set; }

        public DecompositionAssignment(List<PartitionAssignment> partAss, float score)
        {
            this.PartitionAss = partAss;
            this.TotalScore = score;
        }

        public int CompareTo(DecompositionAssignment compareDecompositionAssignment)
        {
            // A null value means that this object is greater.
            if (compareDecompositionAssignment == null)
                return 1;
            else
            {
                if (this.TotalScore != compareDecompositionAssignment.TotalScore)
                    return this.TotalScore.CompareTo(compareDecompositionAssignment.TotalScore);
                else
                    return this.PartitionAss.Count.CompareTo(compareDecompositionAssignment.PartitionAss.Count);
            }
        }
    }
    
}

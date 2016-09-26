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

    public class GraphTraversal
    {
        public List<Bone> BonesToVisit;
        public List<Bone> Partition;
        public List<List<Bone>> Decomposition;
        public int MotorAvailable;
        
        
        public GraphTraversal(int motors) 
        {
            BonesToVisit = new List<Bone>();
            Partition = new List<Bone>();
            Decomposition = new List<List<Bone>>();
            MotorAvailable = motors;
        }
        
        public void dfs_DiscoverVertex_MaxRotDoF(Bone vertex)
        {
            if(vertex.rot_DoF.Count > 0)
                BonesToVisit.Add(vertex);
        }

        public void dfs_DiscoverVertex_MaxLocRotDoF(Bone vertex)
        {
            if (vertex.rot_DoF.Count + vertex.loc_DoF.Count > 0)
                BonesToVisit.Add(vertex);
        }
    }


  
    public class AxisArrangement : IComparable<AxisArrangement>
    {
        public char[] AxisCombination { get; set; }
        public float Score { get; set; }
        
        
        public AxisArrangement(char[] comb, float score)
        {
            this.AxisCombination = comb;
            this.Score = score;            
        }

        public AxisArrangement()
        {
            // TODO: Complete member initialization
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

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            AxisArrangement s = (AxisArrangement)obj;
            return (Metrics.GetDofString(AxisCombination.ToList()).Equals(Metrics.GetDofString(s.AxisCombination.ToList())));
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

        public PartitionAssignment()
        {
            
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
        public const char SEQUENTIAL_TYPE = 'S';
        public const char SPLITTED_TYPE = 'Y';
        public const char SINGLE_CONF_TYPE = 'O';
        public const char KINECT_TYPE = 'K';

        public List<PartitionAssignment> PartitionAss { get; set; }
        public float TotalScore { get; set; }
        public char Type { get; set; }

        public List<Bone> SplittedArmature { get; set; }

        public DecompositionAssignment(List<PartitionAssignment> partAss, float score, char type)
        {
            this.PartitionAss = partAss;
            this.TotalScore = score;
            this.Type = type;
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

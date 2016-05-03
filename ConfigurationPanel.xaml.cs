using Lego.Ev3.Core;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QuickGraph;



namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Logica di interazione per ConfigurationPanel.xaml
    /// </summary>
    public partial class ConfigurationPanel : Window
    {

        MainWindow mw;
        Brick brick;
        List<string> objects;

        ArrayList currentSensorLegoInfo { get; set; }
        ArrayList currentSensorKinectInfo { get; set; }

        Config configuration { get; set; }

        string TouchComboBoxName;
        
        string[] axes = new string[] { "", "X", "Y", "Z", "-X", "-Y", "-Z" };
        string[] rotOrder = new string[] { "", "0", "1", "2" };

        public string[] VocalCommand = new string[] { Command.POSITION, Command.FRAME, Command.FRAME_LOC, Command.FRAME_ROT, Command.REC, Command.STOP, Command.FORWARD, Command.BACKWARD, Command.DELETE, Command.DELETE_CURRENT, Command.PLAY_ANIMATION, Command.RESET, Command.FAST_FORWARD, Command.FAST_BACKWARD, Command.HIDE_CAPTURE, Command.BONES_ASSOCIATION, Command.TRANSLATE_COORDINATE_SYSTEM, Command.RESET_ORIGIN, Command.FIRST_FRAME, Command.ACTIVE_GHOST_FRAME, Command.DISABLE_GHOST_FRAME, Command.LAST_FRAME, Command.START_POSE, Command.END_POSE, Command.COMPUTE_FACTOR, Command.LESS_ACCURACY, Command.MORE_ACCURACY, Command.LOCK_POSE, Command.LOAD_POSE, Command.UNLOCK_POSE};        


        public ConfigurationPanel(MainWindow mainWin, Brick brick, List<string> objects)
        {
            this.mw = mainWin;
            this.brick = brick;
            brick.BrickChanged += _brick_BrickChanged;

            this.currentSensorKinectInfo = new ArrayList();
            this.currentSensorLegoInfo = new ArrayList();
            this.configuration = new Config();

            this.objects = objects;
            this.objects.Insert(0,"");
            this.TouchComboBoxName = string.Empty;
            mw.virtualArmature = false;
            
            InitializeComponent();
            InitGui();
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {            
            mw.captureFlag = true;
        }

        private void _brick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
            this.AValueLabel.Content = e.Ports[InputPort.A].SIValue.ToString();
            this.BValueLabel.Content = e.Ports[InputPort.B].SIValue.ToString();
            this.CValueLabel.Content = e.Ports[InputPort.C].SIValue.ToString();
            this.DValueLabel.Content = e.Ports[InputPort.D].SIValue.ToString();
            this.OneValueLabel.Content = e.Ports[InputPort.One].SIValue.ToString();
            this.TwoValueLabel.Content = e.Ports[InputPort.Two].SIValue.ToString();
            this.ThreeValueLabel.Content = e.Ports[InputPort.Three].SIValue.ToString();
            this.FourValueLabel.Content = e.Ports[InputPort.Four].SIValue.ToString();           
        }

        private void InitGui()
        {
            
            foreach (var ctrl in this.Canvas.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox))
                {
                    ((ComboBox)ctrl).ItemsSource = objects;
                }
            }

            foreach (var ctrl in this.KinectAxisCanvas.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox))
                {
                    ((ComboBox)ctrl).ItemsSource = axes;
                }
            }
           
            this.ALabel.Content = "A - " + brick.Ports[InputPort.A].Type + " :";
            this.BLabel.Content = "B - " + brick.Ports[InputPort.B].Type + " :";
            this.CLabel.Content = "C - " + brick.Ports[InputPort.C].Type + " :";
            this.DLabel.Content = "D - " + brick.Ports[InputPort.D].Type + " :";
            this.OneLabel.Content = "One - " + brick.Ports[InputPort.One].Type + " :";
            this.TwoLabel.Content = "Two - " + brick.Ports[InputPort.Two].Type + " :";
            this.ThreeLabel.Content = "Three - " + brick.Ports[InputPort.Three].Type + " :";
            this.FourLabel.Content = "Four - " + brick.Ports[InputPort.Four].Type + " :";

            InitLegoItemInterface(brick.Ports[InputPort.A].Type.ToString(), this.ALabel, this.AValueLabel, this.SlotAImage, this.AAxis, this.ABoneName, this.ALoc, this.ARot, InputPort.A, this.ARotOrder);

            InitLegoItemInterface(brick.Ports[InputPort.B].Type.ToString(), this.BLabel, this.BValueLabel, this.SlotBImage, this.BAxis, this.BBoneName, this.BLoc, this.BRot, InputPort.B, this.BRotOrder);

            InitLegoItemInterface(brick.Ports[InputPort.C].Type.ToString(), this.CLabel, this.CValueLabel, this.SlotCImage, this.CAxis, this.CBoneName, this.CLoc, this.CRot, InputPort.C, this.CRotOrder);

            InitLegoItemInterface(brick.Ports[InputPort.D].Type.ToString(), this.DLabel, this.DValueLabel, this.SlotDImage, this.DAxis, this.DBoneName, this.DLoc, this.DRot, InputPort.D, this.DRotOrder);

            InitLegoItemInterface(brick.Ports[InputPort.One].Type.ToString(), this.OneLabel, this.OneValueLabel, this.SlotOneImage, this.OneAxis, this.OneBoneName, this.OneLoc, this.OneRot, InputPort.One, this.OneRotOrder);

            InitLegoItemInterface(brick.Ports[InputPort.Two].Type.ToString(), this.TwoLabel, this.TwoValueLabel, this.SlotTwoImage, this.TwoAxis, this.TwoBoneName, this.TwoLoc, this.TwoRot, InputPort.Two, this.TwoRotOrder);

            InitLegoItemInterface(brick.Ports[InputPort.Three].Type.ToString(), this.ThreeLabel, this.ThreeValueLabel, this.SlotThreeImage, this.ThreeAxis, this.ThreeBoneName, this.ThreeLoc, this.ThreeRot, InputPort.Three, this.ThreeRotOrder);

            InitLegoItemInterface(brick.Ports[InputPort.Four].Type.ToString(), this.FourLabel, this.FourValueLabel, this.SlotFourImage, this.FourAxis, this.FourBoneName, this.FourLoc, this.FourRot, InputPort.Four, this.FourRotOrder);


            string[] preset = { "Humanoid", "Pixar_Lamp", "Crocodile", "Guss T_1", "Guss T_2" };
            this.PresetComboBox.ItemsSource = preset;
            UpdatePresetList(mw.presetConfig);
            this.VocalCommandComboBox.ItemsSource = VocalCommand;

        }

        // sets the components (image, combo box values) in the GUI depending on the sensor type 
        private void InitLegoItemInterface(string sensorType, Label labelSensorName, Label labelValue, Image image, ComboBox comboBoxAxis, ComboBox comboBoxBones, CheckBox checkBoxLoc, CheckBox checkBoxRot, InputPort inputPort, ComboBox comboBoxRotOrder)
        {
            
            switch (sensorType)
            {
                case "LMotor":
                    image.Source = new BitmapImage(new Uri(@"/Images/L-Motor.png", UriKind.Relative));
                    comboBoxAxis.ItemsSource = axes;
                    comboBoxBones.ItemsSource = objects;
                    comboBoxRotOrder.ItemsSource = rotOrder;
                    comboBoxRotOrder.SelectedIndex = 0;
                    break;

                case "MMotor":
                    image.Source = new BitmapImage(new Uri(@"/Images/M-Motor.png", UriKind.Relative));
                    comboBoxAxis.ItemsSource = axes;
                    comboBoxBones.ItemsSource = objects;
                    comboBoxRotOrder.ItemsSource = rotOrder;
                    comboBoxRotOrder.SelectedIndex = 0;
                    break;

                case "Gyroscope":
                    image.Source = new BitmapImage(new Uri(@"/Images/Gyroscope.png", UriKind.Relative));
                    comboBoxAxis.ItemsSource = axes;
                    comboBoxBones.ItemsSource = objects;
                    comboBoxRotOrder.ItemsSource = rotOrder;
                    comboBoxRotOrder.SelectedIndex = 0;
                    break;

                case "Ultrasonic":
                    image.Source = new BitmapImage(new Uri(@"/Images/Ultrasonic.png", UriKind.Relative));
                    comboBoxAxis.ItemsSource = axes;
                    comboBoxBones.ItemsSource = objects;
                    comboBoxRotOrder.ItemsSource = rotOrder;
                    comboBoxRotOrder.SelectedIndex = 0;
                    break;

                case "Touch":
                    image.Source = new BitmapImage(new Uri(@"/Images/Touch.png", UriKind.Relative));
                    labelValue.Visibility = System.Windows.Visibility.Collapsed;
                    comboBoxAxis.Visibility = System.Windows.Visibility.Collapsed;
                    checkBoxLoc.Visibility = System.Windows.Visibility.Collapsed;
                    checkBoxRot.Visibility = System.Windows.Visibility.Collapsed;

                    comboBoxBones.ItemsSource = VocalCommand;
                    TouchComboBoxName = comboBoxBones.Name;
                    mw.touchPort = inputPort;

                    break;

                case "Empty":
                    image.Source = new BitmapImage(new Uri(@"/Images/slotEmpty.png", UriKind.Relative));
                    labelValue.Visibility = System.Windows.Visibility.Collapsed;
                    comboBoxAxis.Visibility = System.Windows.Visibility.Collapsed;
                    comboBoxBones.Visibility = System.Windows.Visibility.Collapsed;
                    checkBoxLoc.Visibility = System.Windows.Visibility.Collapsed;
                    checkBoxRot.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }
       
        // Creates the back up file from the current configuration
        private void SaveConfig() 
        {
            if (configuration.Settings.Count > 0)
            {
                string jsonSetting = JsonManager.SaveSetting(configuration, "PresetConfig");    
            }            
        }
        
        private void SaveSetting(string fileName, Setting setting)
        {
            string jsonSetting = JsonManager.SaveSetting(setting, fileName);
        }
        
        private void SetSetting(Setting settingToload)
        {
            foreach (SettingItem c in settingToload.CheckBoxStatus)
            {
                CheckBox check = (CheckBox)this.FindName(c.Name);
                check.IsChecked = Convert.ToBoolean(c.Value);
            }

            foreach (SettingItem c in settingToload.ComboBoxStatus)
            {
                ComboBox combo = (ComboBox)this.FindName(c.Name);
                combo.SelectedValue = c.Value;
            }

            foreach (SettingItem c in settingToload.SliderStatus)
            {
                Slider slider = (Slider)this.FindName(c.Name);
                slider.Value = Convert.ToDouble(c.Value);
            }            

            UpdateVocalCommandList();
        }        

        private void LoadConfig(string filename) 
        {
            // Deletes old configurations
            foreach (Setting s in configuration.Settings)
            {
                System.IO.File.Delete("config\\" + s.SettingName + ".json");
                mw.grammarRules.Remove(new VoiceCommand(s.SettingName, Command.CHANGE_PRESET));               
            }
            mw.presetConfig.Clear();
            UpdatePresetList(mw.presetConfig);
            

            configuration = JsonManager.LoadConfig(filename);
            ConvertSettingToPreset(configuration.Settings);
            /*
            foreach (Setting s in configuration.Settings)
            {
                ResetControl();
                SetSetting(s);
                
                Preset preset = CreatePreset();
                preset.Name = s.SettingName;
                mw.presetConfig.Add(preset);
                mw.grammarRules.Add(new VoiceCommand(s.SettingName, Command.CHANGE_PRESET));
                
                Setting currentSetting = GetSetting();
                SaveSetting(preset.Name, currentSetting);

                currentSensorKinectInfo.RemoveRange(0, currentSensorKinectInfo.Count);
                currentSensorLegoInfo.RemoveRange(0, currentSensorLegoInfo.Count);
            }

            UpdatePresetList(mw.presetConfig);
            SetSetting((Setting)configuration.Settings[0]);
            */
                      
        }
        
        private Setting LoadSetting(string filename)
        {
            Setting settingLoaded = JsonManager.LoadSetting(filename);
            return settingLoaded;
        }
        
        private Setting GetSetting()
        {
            Setting setting = new Setting(this.PresetName.Text);
            
            foreach (var ctrl in this.Canvas.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox) && ((ComboBox)ctrl).SelectedItem != null)
                {
                    setting.ComboBoxStatus.Add(new SettingItem(((ComboBox)ctrl).Name.ToString(), ((ComboBox)ctrl).SelectedItem.ToString()));
                }

                if (ctrl.GetType() == typeof(CheckBox))
                {
                    setting.CheckBoxStatus.Add(new SettingItem(((CheckBox)ctrl).Name.ToString(), ((CheckBox)ctrl).IsChecked.Value.ToString()));
                }

                if (ctrl.GetType() == typeof(Slider))
                {
                    setting.SliderStatus.Add(new SettingItem(((Slider)ctrl).Name.ToString(), ((Slider)ctrl).Value.ToString()));
                }
            }

            foreach (var ctrl in this.LocRotCanvas.Children)
            {
                if (ctrl.GetType() == typeof(CheckBox))
                {
                    setting.CheckBoxStatus.Add(new SettingItem(((CheckBox)ctrl).Name.ToString(), ((CheckBox)ctrl).IsChecked.Value.ToString()));
                }

            }

            foreach (var ctrl in this.BoneNameGrid.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox) && ((ComboBox)ctrl).SelectedItem != null)
                {
                    setting.ComboBoxStatus.Add(new SettingItem(((ComboBox)ctrl).Name.ToString(), ((ComboBox)ctrl).SelectedItem.ToString()));
                }
            }

            foreach (var ctrl in this.AxisGrid.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox) && ((ComboBox)ctrl).SelectedItem != null)
                {
                    setting.ComboBoxStatus.Add(new SettingItem(((ComboBox)ctrl).Name.ToString(), ((ComboBox)ctrl).SelectedItem.ToString()));
                }
            }

            foreach (var ctrl in this.RotationOrderGrid.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox) && ((ComboBox)ctrl).SelectedItem != null)
                {
                    setting.ComboBoxStatus.Add(new SettingItem(((ComboBox)ctrl).Name.ToString(), ((ComboBox)ctrl).SelectedItem.ToString()));
                }
            }

            foreach (var ctrl in this.KinectAxisCanvas.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox) && ((ComboBox)ctrl).SelectedItem != null)
                {
                    setting.ComboBoxStatus.Add(new SettingItem(((ComboBox)ctrl).Name.ToString(), ((ComboBox)ctrl).SelectedItem.ToString()));
                }
            }

            
            return setting;

        }
        
        private void ConfigValidation()
        {            
            // LEGO SENSOR //
            if ((this.ALoc.IsChecked.Value || this.ARot.IsChecked.Value) && (this.ABoneName.SelectedIndex >= 0) && (this.AAxis.SelectedIndex >= 0))
            {
                if (!ARot.IsChecked.Value)
                    ARotOrder.SelectedIndex = 1;
                // Add the data read from the panel to the currentSensorLegoInfo 
                SetSensorLegoInfo(InputPort.A, this.ABoneName.SelectedItem.ToString(), this.ALoc.IsChecked.Value, this.ARot.IsChecked.Value, this.AAxis.SelectedItem.ToString(), Convert.ToInt32(ARotOrder.SelectedItem.ToString()));

            }

            if ((this.BLoc.IsChecked.Value || this.BRot.IsChecked.Value) && (this.BBoneName.SelectedIndex >= 0) && (this.BAxis.SelectedIndex >= 0))
            {
                if (!BRot.IsChecked.Value)
                    BRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo(InputPort.B, this.BBoneName.SelectedItem.ToString(), this.BLoc.IsChecked.Value, this.BRot.IsChecked.Value, this.BAxis.SelectedItem.ToString(), Convert.ToInt32(BRotOrder.SelectedItem.ToString()));
            }

            if ((this.CLoc.IsChecked.Value || this.CRot.IsChecked.Value) && (this.CBoneName.SelectedIndex >= 0) && (this.CAxis.SelectedIndex >= 0))
            {
                if (!CRot.IsChecked.Value)
                    CRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo(InputPort.C, this.CBoneName.SelectedItem.ToString(), this.CLoc.IsChecked.Value, this.CRot.IsChecked.Value, this.CAxis.SelectedItem.ToString(), Convert.ToInt32(CRotOrder.SelectedItem.ToString()));
            }

            if ((this.DLoc.IsChecked.Value || this.DRot.IsChecked.Value) && (this.DBoneName.SelectedIndex >= 0) && (this.DAxis.SelectedIndex >= 0))
            {
                if (!DRot.IsChecked.Value)
                    DRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo(InputPort.D, this.DBoneName.SelectedItem.ToString(), this.DLoc.IsChecked.Value, this.DRot.IsChecked.Value, this.DAxis.SelectedItem.ToString(), Convert.ToInt32(DRotOrder.SelectedItem.ToString()));
            }

            if ((this.OneLoc.IsChecked.Value || this.OneRot.IsChecked.Value) && (this.OneBoneName.SelectedIndex >= 0) && (this.OneAxis.SelectedIndex >= 0))
            {
                if (!OneRot.IsChecked.Value)
                    OneRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo(InputPort.One, this.OneBoneName.SelectedItem.ToString(), this.OneLoc.IsChecked.Value, this.OneRot.IsChecked.Value, this.OneAxis.SelectedItem.ToString(), Convert.ToInt32(OneRotOrder.SelectedItem.ToString()));
            }

            if ((this.TwoLoc.IsChecked.Value || this.TwoRot.IsChecked.Value) && (this.TwoBoneName.SelectedIndex >= 0) && (this.TwoAxis.SelectedIndex >= 0))
            {
                if (!TwoRot.IsChecked.Value)
                    TwoRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo(InputPort.Two, this.TwoBoneName.SelectedItem.ToString(), this.TwoLoc.IsChecked.Value, this.TwoRot.IsChecked.Value, this.TwoAxis.SelectedItem.ToString(), Convert.ToInt32(TwoRotOrder.SelectedItem.ToString()));
            }

            if ((this.ThreeLoc.IsChecked.Value || this.ThreeRot.IsChecked.Value) && (this.ThreeBoneName.SelectedIndex >= 0) && (this.ThreeAxis.SelectedIndex >= 0))
            {
                if (!ThreeRot.IsChecked.Value)
                    ThreeRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo(InputPort.Three, this.ThreeBoneName.SelectedItem.ToString(), this.ThreeLoc.IsChecked.Value, this.ThreeRot.IsChecked.Value, this.ThreeAxis.SelectedItem.ToString(), Convert.ToInt32(ThreeRotOrder.SelectedItem.ToString()));
            }

            if ((this.FourLoc.IsChecked.Value || this.FourRot.IsChecked.Value) && (this.FourBoneName.SelectedIndex >= 0) && (this.FourAxis.SelectedIndex >= 0))
            {
                if (!FourRot.IsChecked.Value)
                    FourRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo(InputPort.Four, this.FourBoneName.SelectedItem.ToString(), this.FourLoc.IsChecked.Value, this.FourRot.IsChecked.Value, this.FourAxis.SelectedItem.ToString(), /*Convert.ToInt32(FourRotOrder.SelectedItem.ToString())*/-1);
            }


            // KINECT //

            if ((this.HeadLocX.IsChecked.Value || this.HeadLocY.IsChecked.Value || this.HeadLocZ.IsChecked.Value || this.HeadRot.IsChecked.Value) && (this.HeadBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.Head, this.HeadBoneName.SelectedItem.ToString(), this.HeadLocX.IsChecked.Value, this.HeadLocY.IsChecked.Value, this.HeadLocZ.IsChecked.Value, this.HeadRot.IsChecked.Value, (float)HeadSlider.Value, this.HeadXfrom.SelectedItem.ToString(), this.HeadYfrom.SelectedItem.ToString(), this.HeadZfrom.SelectedItem.ToString());
            }

            if ((this.ShoulderLocX.IsChecked.Value || this.ShoulderLocY.IsChecked.Value || this.ShoulderLocZ.IsChecked.Value || this.ShoulderRot.IsChecked.Value) && (this.ShoulderBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.ShoulderCenter, this.ShoulderBoneName.SelectedItem.ToString(), this.ShoulderLocX.IsChecked.Value, this.ShoulderLocY.IsChecked.Value, this.ShoulderLocZ.IsChecked.Value, this.ShoulderRot.IsChecked.Value, (float)ShoulderSlider.Value, this.ShoulderXfrom.SelectedItem.ToString(), this.ShoulderYfrom.SelectedItem.ToString(), this.ShoulderZfrom.SelectedItem.ToString());
            }

            if ((this.ShoulderLLocX.IsChecked.Value || this.ShoulderLLocY.IsChecked.Value || this.ShoulderLLocZ.IsChecked.Value || this.ShoulderLRot.IsChecked.Value) && (this.ShoulderLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.ShoulderLeft, this.ShoulderLBoneName.SelectedItem.ToString(), this.ShoulderLLocX.IsChecked.Value, this.ShoulderLLocY.IsChecked.Value, this.ShoulderLLocZ.IsChecked.Value, this.ShoulderLRot.IsChecked.Value, (float)ShoulderLeftSlider.Value, this.ShoulderLXfrom.SelectedItem.ToString(), this.ShoulderLYfrom.SelectedItem.ToString(), this.ShoulderLZfrom.SelectedItem.ToString());
            }

            if ((this.ElbowLLocX.IsChecked.Value || this.ElbowLLocY.IsChecked.Value || this.ElbowLLocZ.IsChecked.Value || this.ElbowLRot.IsChecked.Value) && (this.ElbowLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.ElbowLeft, this.ElbowLBoneName.SelectedItem.ToString(),
                    this.ElbowLLocX.IsChecked.Value, this.ElbowLLocY.IsChecked.Value, 
                    this.ElbowLLocZ.IsChecked.Value, this.ElbowLRot.IsChecked.Value, 
                    (float)ElbowLeftSlider.Value, this.ElbowLXfrom.SelectedItem.ToString(),
                    this.ElbowLYfrom.SelectedItem.ToString(),this.ElbowLZfrom.SelectedItem.ToString());
            }

            if ((this.WristLLocX.IsChecked.Value || this.WristLLocY.IsChecked.Value || this.WristLLocZ.IsChecked.Value || this.WristLRot.IsChecked.Value) && (this.WristLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.WristLeft, this.WristLBoneName.SelectedItem.ToString(), this.WristLLocX.IsChecked.Value, this.WristLLocY.IsChecked.Value, this.WristLLocZ.IsChecked.Value, this.WristLRot.IsChecked.Value, (float)WristLeftSlider.Value, this.WristLXfrom.SelectedItem.ToString(),this.WristLYfrom.SelectedItem.ToString(),this.WristLZfrom.SelectedItem.ToString());
            }

            if ((this.HandLLocX.IsChecked.Value || this.HandLLocY.IsChecked.Value || this.HandLLocZ.IsChecked.Value || this.HandLRot.IsChecked.Value) && (this.HandLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HandLeft, this.HandLBoneName.SelectedItem.ToString(),
                    this.HandLLocX.IsChecked.Value, this.HandLLocY.IsChecked.Value, this.HandLLocZ.IsChecked.Value,
                    this.HandLRot.IsChecked.Value, (float)HandLeftSlider.Value, this.HandLXfrom.SelectedItem.ToString(),
                    this.HandLYfrom.SelectedItem.ToString(),this.HandLZfrom.SelectedItem.ToString());
            }

            if ((this.ShoulderRLocX.IsChecked.Value || this.ShoulderRLocY.IsChecked.Value || this.ShoulderRLocZ.IsChecked.Value || this.ShoulderRRot.IsChecked.Value) && (this.ShoulderRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.ShoulderRight, this.ShoulderRBoneName.SelectedItem.ToString(), this.ShoulderRLocX.IsChecked.Value, this.ShoulderRLocY.IsChecked.Value, this.ShoulderRLocZ.IsChecked.Value, this.ShoulderRRot.IsChecked.Value, (float)ShoulderRightSlider.Value, this.ShoulderRXfrom.SelectedItem.ToString(),this.ShoulderRYfrom.SelectedItem.ToString(),this.ShoulderRZfrom.SelectedItem.ToString());
            }

            if ((this.ElbowRLocX.IsChecked.Value || this.ElbowRLocY.IsChecked.Value || this.ElbowRLocZ.IsChecked.Value || this.ElbowRRot.IsChecked.Value) && (this.ElbowRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.ElbowRight, this.ElbowRBoneName.SelectedItem.ToString(), this.ElbowRLocX.IsChecked.Value, this.ElbowRLocY.IsChecked.Value, this.ElbowRLocZ.IsChecked.Value, this.ElbowRRot.IsChecked.Value, (float)ElbowRightSlider.Value, this.ElbowRXfrom.SelectedItem.ToString(),this.ElbowRYfrom.SelectedItem.ToString(),this.ElbowRZfrom.SelectedItem.ToString());
            }

            if ((this.WristRLocX.IsChecked.Value || this.WristRLocY.IsChecked.Value || this.WristRLocZ.IsChecked.Value || this.WristRRot.IsChecked.Value) && (this.WristRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.WristRight, this.WristRBoneName.SelectedItem.ToString(), this.WristRLocX.IsChecked.Value, this.WristRLocY.IsChecked.Value, this.WristRLocZ.IsChecked.Value, this.WristRRot.IsChecked.Value, (float)WristRightSlider.Value, this.WristRXfrom.SelectedItem.ToString(),this.WristRYfrom.SelectedItem.ToString(),this.WristRZfrom.SelectedItem.ToString());
            }

            if ((this.HandRLocX.IsChecked.Value || this.HandRLocY.IsChecked.Value || this.HandRLocZ.IsChecked.Value || this.HandRRot.IsChecked.Value) && (this.HandRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HandRight, this.HandRBoneName.SelectedItem.ToString(), this.HandRLocX.IsChecked.Value, this.HandRLocY.IsChecked.Value, this.HandRLocZ.IsChecked.Value, this.HandRRot.IsChecked.Value, (float)HandRightSlider.Value, this.HandRXfrom.SelectedItem.ToString(),this.HandRYfrom.SelectedItem.ToString(),this.HandRZfrom.SelectedItem.ToString());
            }

            if ((this.SpineLocX.IsChecked.Value || this.SpineLocY.IsChecked.Value || this.SpineLocZ.IsChecked.Value || this.SpineRot.IsChecked.Value) && (this.SpineBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.Spine, this.SpineBoneName.SelectedItem.ToString(), this.SpineLocX.IsChecked.Value, this.SpineLocY.IsChecked.Value, this.SpineLocZ.IsChecked.Value, this.SpineRot.IsChecked.Value, (float)SpineSlider.Value,this.SpineXfrom.SelectedItem.ToString(),this.SpineYfrom.SelectedItem.ToString(),this.SpineZfrom.SelectedItem.ToString());
            }

            if ((this.HipLocX.IsChecked.Value || this.HipLocY.IsChecked.Value || this.HipLocZ.IsChecked.Value || this.HipRot.IsChecked.Value) && (this.HipBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HipCenter, this.HipBoneName.SelectedItem.ToString(), this.HipLocX.IsChecked.Value, this.HipLocY.IsChecked.Value, this.HipLocZ.IsChecked.Value, this.HipRot.IsChecked.Value, (float)HipSlider.Value, this.HipXfrom.SelectedItem.ToString(),this.HipYfrom.SelectedItem.ToString(),this.HipZfrom.SelectedItem.ToString());
            }

            if ((this.HipLLocX.IsChecked.Value || this.HipLLocY.IsChecked.Value || this.HipLLocZ.IsChecked.Value || this.HipLRot.IsChecked.Value) && (this.HipLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HipLeft, this.HipLBoneName.SelectedItem.ToString(), this.HipLLocX.IsChecked.Value, this.HipLLocY.IsChecked.Value, this.HipLLocZ.IsChecked.Value, this.HipLRot.IsChecked.Value, (float)HipLeftSlider.Value,this.HipLXfrom.SelectedItem.ToString(),this.HipLYfrom.SelectedItem.ToString(),this.HipLZfrom.SelectedItem.ToString());
            }

            if ((this.KneeLLocX.IsChecked.Value || this.KneeLLocY.IsChecked.Value || this.KneeLLocY.IsChecked.Value || this.KneeLRot.IsChecked.Value) && (this.KneeLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.KneeLeft, this.KneeLBoneName.SelectedItem.ToString(), this.KneeLLocX.IsChecked.Value, this.KneeLLocY.IsChecked.Value, this.KneeLLocZ.IsChecked.Value, this.KneeLRot.IsChecked.Value, (float)KneeLeftSlider.Value, this.KneeLXfrom.SelectedItem.ToString(),this.KneeLYfrom.SelectedItem.ToString(),this.KneeLZfrom.SelectedItem.ToString());
            }
            if ((this.AnkleLLocX.IsChecked.Value || this.AnkleLLocY.IsChecked.Value || this.AnkleLLocZ.IsChecked.Value || this.AnkleLRot.IsChecked.Value) && (this.AnkleLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.AnkleLeft, this.AnkleLBoneName.SelectedItem.ToString(), this.AnkleLLocX.IsChecked.Value, this.AnkleLLocY.IsChecked.Value, this.AnkleLLocZ.IsChecked.Value, this.AnkleLRot.IsChecked.Value, (float)AnkleLeftSlider.Value, this.AnkleLXfrom.SelectedItem.ToString(), this.AnkleLYfrom.SelectedItem.ToString(), this.AnkleLZfrom.SelectedItem.ToString());
            }
            if ((this.FootLLocX.IsChecked.Value || this.FootLLocY.IsChecked.Value || this.FootLLocZ.IsChecked.Value || this.FootLRot.IsChecked.Value) && (this.FootLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.FootLeft, this.FootLBoneName.SelectedItem.ToString(), this.FootLLocX.IsChecked.Value, this.FootLLocY.IsChecked.Value, this.FootLLocZ.IsChecked.Value, this.FootLRot.IsChecked.Value, (float)FootLeftSlider.Value, this.FootLXfrom.SelectedItem.ToString(), this.FootLYfrom.SelectedItem.ToString(), this.FootLZfrom.SelectedItem.ToString());
            }
            if ((this.HipRLocX.IsChecked.Value || this.HipRLocY.IsChecked.Value || this.HipRLocZ.IsChecked.Value || this.HipRRot.IsChecked.Value) && (this.HipRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HipRight, this.HipRBoneName.SelectedItem.ToString(), this.HipRLocX.IsChecked.Value, this.HipRLocY.IsChecked.Value, this.HipRLocZ.IsChecked.Value, this.HipRRot.IsChecked.Value, (float)HipRightSlider.Value, this.HipRXfrom.SelectedItem.ToString(), this.HipRYfrom.SelectedItem.ToString(), this.HipRZfrom.SelectedItem.ToString());
            }
            if ((this.KneeRLocX.IsChecked.Value || this.KneeRLocY.IsChecked.Value || this.KneeRLocZ.IsChecked.Value || this.KneeRRot.IsChecked.Value) && (this.KneeRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.KneeRight, this.KneeRBoneName.SelectedItem.ToString(), this.KneeRLocX.IsChecked.Value, this.KneeRLocY.IsChecked.Value, this.KneeRLocZ.IsChecked.Value, this.KneeRRot.IsChecked.Value, (float)KneeRightSlider.Value, this.KneeRXfrom.SelectedItem.ToString(),this.KneeRYfrom.SelectedItem.ToString(),this.KneeRZfrom.SelectedItem.ToString());
            }
            if ((this.AnkleRLocX.IsChecked.Value || this.AnkleRLocY.IsChecked.Value || this.AnkleRLocZ.IsChecked.Value || this.AnkleRRot.IsChecked.Value) && (this.AnkleRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.AnkleRight, this.AnkleRBoneName.SelectedItem.ToString(), this.AnkleRLocX.IsChecked.Value, this.AnkleRLocY.IsChecked.Value, this.AnkleRLocZ.IsChecked.Value, this.AnkleRRot.IsChecked.Value, (float)AnkleRightSlider.Value, this.AnkleRXfrom.SelectedItem.ToString(),  this.AnkleRYfrom.SelectedItem.ToString(), this.AnkleRZfrom.SelectedItem.ToString());
            }
            if ((this.FootRLocX.IsChecked.Value || this.FootRLocY.IsChecked.Value || this.FootRLocZ.IsChecked.Value || this.FootRRot.IsChecked.Value) && (this.FootLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.FootRight, this.FootRBoneName.SelectedItem.ToString(), this.FootRLocX.IsChecked.Value, this.FootRLocY.IsChecked.Value, this.FootRLocZ.IsChecked.Value, this.FootRRot.IsChecked.Value, (float)FootRightSlider.Value, this.FootRXfrom.SelectedItem.ToString(), this.FootRYfrom.SelectedItem.ToString(), this.FootRZfrom.SelectedItem.ToString());
            }
        }

        private void SetSensorKinectInfo(JointType jointType, string objectName, bool locX, bool locY, bool locZ, bool rot, float sliderFactor,string XFrom, string YFrom, string ZFrom)
        {
            if (objectName.Contains(":"))
            {
                string[] splittedString = objectName.Split(':');
                currentSensorKinectInfo.Add(new SensorKinectInfo(jointType, splittedString[0], splittedString[1], locX, locY, locZ, rot, sliderFactor, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, XFrom, YFrom, ZFrom));

            }
            else
            {
                currentSensorKinectInfo.Add(new SensorKinectInfo(jointType, "", objectName, locX, locY, locZ, rot, sliderFactor, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, XFrom, YFrom, ZFrom));
            }
        }

        private void SetSensorLegoInfo(InputPort inputPort, string objectName, bool loc, bool rot, string axis, int rotOrder)
        {
            if (objectName.Contains(":"))
            {
                string[] splittedString = objectName.Split(':');
                currentSensorLegoInfo.Add(new SensorLegoInfo(inputPort, splittedString[0], splittedString[1], loc, rot, axis, 0, 0, rotOrder));
            }
            else
            {
                currentSensorLegoInfo.Add(new SensorLegoInfo(inputPort, "", objectName, loc, rot, axis, 0, 0, rotOrder));
            }
        }

        private void ResetControl()
        {
            foreach (var ctrl in this.Grid.Children)
            {
                if (ctrl.GetType() == typeof(CheckBox))
                {
                    ((CheckBox)ctrl).IsChecked = false;
                }
            }

            foreach (var ctrl in this.Canvas.Children)
            {
                if (ctrl.GetType() == typeof(CheckBox))
                {
                    ((CheckBox)ctrl).IsChecked = false;
                }

                if (ctrl.GetType() == typeof(ComboBox))
                {
                    ((ComboBox)ctrl).SelectedIndex = -1;
                }

                if (ctrl.GetType() == typeof(Slider))
                {
                    ((Slider)ctrl).Value = 1;
                }
            }

            foreach (var ctrl in this.AxisGrid.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox))
                {
                    ((ComboBox)ctrl).SelectedIndex = -1;
                }
            }

            foreach (var ctrl in this.BoneNameGrid.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox))
                {
                    ((ComboBox)ctrl).SelectedIndex = -1;
                }
            }

            foreach (var ctrl in this.RotationOrderGrid.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox))
                {
                    ((ComboBox)ctrl).SelectedIndex = -1;
                }
            }

            foreach (var ctrl in this.LocRotCanvas.Children)
            {
                if (ctrl.GetType() == typeof(CheckBox))
                {
                    ((CheckBox)ctrl).IsChecked = false;
                }
            }
        }

        private void UpdateVocalCommandList()
        {
            ArrayList commandToShow = new ArrayList();

            foreach (VoiceCommand vcom in mw.grammarRules)
            {
                if (VocalCommandComboBox.SelectedItem != null)
                {
                    if (vcom.Rule.Equals(this.VocalCommandComboBox.SelectedItem.ToString()))
                    {
                        commandToShow.Add(vcom.SpeechRecognized.ToString());
                    }
                }
            }

            ListCommand.ItemsSource = commandToShow;
        }

        private void UpdatePresetList(List<Preset> SourceArrayList)
        {
            ArrayList itemToShow = new ArrayList();

            foreach (Preset p in SourceArrayList)
            {
                itemToShow.Add(p.Name);
            }

            this.PresetListBox.ItemsSource = itemToShow;
        }

        private void ButtonRemoveSetting_Click(object sender, RoutedEventArgs e)
        {
            foreach (string presetToRemove in this.PresetListBox.SelectedItems)
            {
                configuration.Settings.Remove(new Setting(presetToRemove));
                mw.presetConfig.Remove(new Preset(presetToRemove));
                mw.grammarRules.Remove(new VoiceCommand(presetToRemove, Command.CHANGE_PRESET));
                System.IO.File.Delete("config\\" + presetToRemove + ".json");
            }

            UpdatePresetList(mw.presetConfig);
        }

        private void PresetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.PresetListBox.SelectedItem != null)
            {
                ResetControl();
                Setting settingToLoad = LoadSetting(this.PresetListBox.SelectedItem.ToString());
                SetSetting(settingToLoad);
            }
        }

        private void ButtonAddSetting_Click(object sender, RoutedEventArgs e)
        {
            if (this.PresetName.Text.Equals("") || mw.presetConfig.Contains(new Preset(this.PresetName.Text)))
            {
                MessageBoxResult result = MessageBox.Show(this, "Add the name to the configuration or configuration already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else
            {                
                Preset preset = CreatePreset();             
                // Adds the new preset to the list of presets managed by the MainWindows
                mw.presetConfig.Add(preset);
                
                // Updates the GUI to show the new preset
                UpdatePresetList(mw.presetConfig);

                // Creates a new Setting to memorize the new preset data 
                Setting currentSetting = GetSetting();
                SaveSetting(preset.Name, currentSetting);
                configuration.Settings.Add(currentSetting);

                mw.grammarRules.Add(new VoiceCommand(preset.Name, Command.CHANGE_PRESET));

                this.PresetName.Text = "";

            }

            // Clears the currentSensorInfo to receive the next user specification
            currentSensorKinectInfo.RemoveRange(0, currentSensorKinectInfo.Count);
            currentSensorLegoInfo.RemoveRange(0, currentSensorLegoInfo.Count);

        }

        private Preset CreatePreset()
        {
            // Creates a new default preset
            Preset preset = new Preset(this.PresetName.Text);

            // Updates the currentSensorInfo using data extract from the panel 
            ConfigValidation();

            // Sets the currentSensorLegoInfo for this new preset
            preset.sensorKinectInfoSet = (ArrayList)currentSensorKinectInfo.Clone();
            
            // Sets the currentSensorKinectInfo for this new preset
            preset.sensorLegoInfoSet = (ArrayList)currentSensorLegoInfo.Clone();

            if (!TouchComboBoxName.Equals(string.Empty))
            {
                ComboBox combo = (ComboBox)this.FindName(TouchComboBoxName);
                if (combo.SelectedItem != null)
                {
                    preset.TouchCommand = combo.SelectedItem.ToString();
                }
                else
                {
                    preset.TouchCommand = Command.FRAME;
                }
            }

            return preset;
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetControl();

            switch (PresetComboBox.SelectedItem.ToString())
            {
                case "Humanoid":
                    LoadConfig("Preset_Humanoid");
                    break;

                case "Pixar_Lamp":
                    // FOR TESTING
                    this.factorSlider.Value = 70;
                    LoadConfig("Preset_Pixar_Lamp");                    
                    break;

                case "Crocodile":
                    LoadConfig("Preset_Crocodile_Test");                    
                    break;

                case "Guss T_1":
                    // FOR TESTING
                    this.factorSlider.Value = 15;
                    LoadConfig("Preset_Guss_Training_1");                    
                    break;

                case "Guss T_2":
                    // FOR TESTING
                    this.factorSlider.Value = 15;
                    LoadConfig("Preset_Guss_Training_2");                    
                    break;
            }

            UpdatePresetList(mw.presetConfig);
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (VocalCommandComboBox.SelectedItem != null && !NewVocalCommandTextBox.Text.Equals(""))
            {
                mw.grammarRules.Add(new VoiceCommand(this.NewVocalCommandTextBox.Text.ToString(), this.VocalCommandComboBox.SelectedItem.ToString()));

                this.NewVocalCommandTextBox.Text = "";

                UpdateVocalCommandList();
            }
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            foreach (string commandToRemove in this.ListCommand.SelectedItems)
            {
                mw.grammarRules.Remove(new VoiceCommand(commandToRemove, this.VocalCommandComboBox.SelectedItem.ToString()));
            }
            UpdateVocalCommandList();
        }

        private void VocalCommandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateVocalCommandList();
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            ResetControl();

            LoadConfig("PresetConfig");

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mw.factor = (int)e.NewValue;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();                                  
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            int selectedConfig = 0;
            if (PresetListBox.SelectedIndex != -1)
            {
                selectedConfig = PresetListBox.SelectedIndex;
            }

            if (mw.presetConfig.Count > 0)
            {
                mw.sensorKinectInfo = (ArrayList)mw.presetConfig[selectedConfig].sensorKinectInfoSet.Clone();
                mw.sensorLegoInfo = (ArrayList)mw.presetConfig[selectedConfig].sensorLegoInfoSet.Clone();
                mw.touchCommand = mw.presetConfig[selectedConfig].TouchCommand;

                mw.rotationMatrix.Clear();
                foreach (SensorLegoInfo slf in mw.sensorLegoInfo) 
                {
                    ObjectRotationMatrix newObjcetRotMatrix = new ObjectRotationMatrix(slf.ObjectName+":"+slf.BoneName);
                    if (!mw.rotationMatrix.Contains(newObjcetRotMatrix))
                        mw.rotationMatrix.Add(newObjcetRotMatrix);
                }

                mw.StartSpeechRecognition();
                mw.captureFlag = true;
                mw.virtualArmature = this.VirtualArmatureCheckBox.IsChecked.Value;
                mw.ConfidenceThreshold = this.ConfidenceThresholdSlider.Value;
                mw.ghostFrameRange = (int)Decimal.Parse(this.RangeTextBox.Text, System.Globalization.CultureInfo.InvariantCulture);
                mw.ghostFrameStep = (int)Decimal.Parse(this.StepTextBox.Text, System.Globalization.CultureInfo.InvariantCulture);
                mw.currentPreset = mw.presetConfig[selectedConfig].Name;
                
                this.Close();
            }
            else 
            {
                MessageBoxResult result = MessageBox.Show(this, "Add at least one configuration", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonAutoConfig_Click(object sender, RoutedEventArgs e)
        {

            // Alternative representation of dof sequence
            Dictionary<string, List<List<char>>> dictionary = AutomaticMapping.InitDoFDictionary();
            int componentAvailable = 0;

            // Requires the topologies information to Blender 
            string armatureName = RequestArmatureSelected();
            List<Bone> armature = RequestArmatureInfo(armatureName);
            int maxLevelBone = AutomaticMapping.GetMaxLengthChain(armature);

            // Create graph from armature
            List<Bone> uniquePartition = new List<Bone>();
            var graph = AutomaticMapping.CreateDirectedGraph(armature);
            // Obtaints the graph connected component 
            List<List<Bone>> graphComponents = AutomaticMapping.GetConnectedComponentList(graph);

            GraphVisualizatio.ShowGraph(graph, "Blender Armature");


            // User chooses the Tui Interface
            if (this.UserPreferenceSlider.Value > 0)
            {
                componentAvailable = AutomaticMapping.CountComponentAvailable
                    (new List<string>() { "LMotor", "MMotor" }, brick);

                if (this.UseSensorCheckBox.IsChecked.Value)
                {
                    componentAvailable += AutomaticMapping.CountComponentAvailable
                        (new List<string>() { "Gyroscope", "Ultrasonic" }, brick);
                }


                // componentAvailable is increased in order to consider the hip joint
                if ((AutomaticMapping.CountArmatureDofs(armature) <= componentAvailable + 3) &&
                    (graphComponents.Count < 2))
                {
                    uniquePartition = armature;
                }
            }
            // User chooses the Nui Interface
            else
            {
                componentAvailable = 20;
                if (armature.Count < 20 && graphComponents.Count < 2)
                {
                    uniquePartition = armature;
                }
            }

            // Tries to create a configuration for the armature which contains only one partition
            bool configurationCreated = false;
            if (uniquePartition.Count > 0)
            {
                List<PartitionAssignment> locRotArrangements = new List<PartitionAssignment>();
                if (this.UserPreferenceSlider.Value > 0)
                {
                    // Computes Tui + Hip score assignement                
                    List<Bone> virtualArmature = AutomaticMapping.GetTuiArmature
                        (uniquePartition, this.UseSensorCheckBox.IsChecked.Value, brick);

                    // Computes assignement
                    locRotArrangements.Add(Metrics.ComputeAssignement
                        (uniquePartition, virtualArmature, maxLevelBone, dictionary, "TUI+HIP_CONFIG"));

                }
                if (this.UserPreferenceSlider.Value < 0)
                {
                    // Computes kinect score assignment
                    List<Bone> virtualArmature = KinectSkeleton.GetKinectSkeleton();
                    locRotArrangements.Add(Metrics.ComputeAssignement
                        (uniquePartition, virtualArmature, maxLevelBone, dictionary, "KINECT_CONFIG"));

                }

                // Creates configuration
                if (locRotArrangements[0] != null)
                {
                    locRotArrangements.Sort();
                    CreateConfiguration(locRotArrangements[0], armatureName, maxLevelBone, dictionary);
                    //SetArrangmentLabel(locRotArrangements[0]);
                    configurationCreated = true;
                }
            }

            if (!configurationCreated)
            {
                if (this.UserPreferenceSlider.Value >= 0)
                {
                    // CREATES PARTITION
                    List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();
                    try
                    {
                        graphPartitions = AutomaticMapping.GraphPartitioning
                            (componentAvailable, graph, graphComponents, graphPartitions,
                            this.SplitDofCheckBox.IsChecked.Value, true);
                    }
                    catch (ApplicationException)
                    {
                        MessageBoxResult result = MessageBox.Show(this, "Required components not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    // ROTATION ASSIGNMENT
                    // Computes ROT -> TUI Assignment
                    List<DecompositionAssignment> decAssign = new List<DecompositionAssignment>();

                    // Creates combination with repetition of n element (x,y,z) choose k (number of motor available) 
                    List<char[]> combination = new List<char[]>();
                    foreach (var c in Combinatorics.CombinationsWithRepetition(new string[] { "x", "y", "z" }, componentAvailable))
                    {
                        char[] array = c.ToCharArray();
                        combination.Add(array);
                    }


                    ////////////////////////////////////////////////////////////////////////////
                    int Calculations = graphPartitions.Count;
                    // One event is used for each Fibonacci object.
                    ManualResetEvent[] doneEvents = new ManualResetEvent[Calculations];
                    Calculation[] calcArray = new Calculation[Calculations];
                    Random r = new Random();

                     // Search the best sequence
                    foreach (List<List<Bone>> decomposition in graphPartitions)
                    {                                                                                                                 
                        // Configure and start threads using ThreadPool.
                        Console.WriteLine("launching {0} tasks...", Calculations);
                        for (int i = 0; i < Calculations; i++)
                        {
                            doneEvents[i] = new ManualResetEvent(false);
                            Calculation f = new Calculation(doneEvents[i], combination, componentAvailable, dictionary, decomposition, UseSensorCheckBox.IsChecked.Value, brick, maxLevelBone);
                            calcArray[i] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, i);
                        }
                    }
                    
                    // Wait for all threads in pool to calculate.
                    foreach (var end in doneEvents) 
                    {
                        end.WaitOne();
                        Console.WriteLine("bip");
                    }
                    
                    Console.WriteLine("All calculations are complete.");
                    
                    // Display the results.
                    for (int i = 0; i < Calculations; i++)
                    {
                        Calculation f = calcArray[i];
                        foreach (DecompositionAssignment decItem in f.DecAssign) 
                        {
                            decAssign.Add(decItem);
                        }
                        
                    }
                    ////////////////////////////////////////////////////////////////////////////

                    decAssign.Sort();
                    
                    foreach (PartitionAssignment partAssi in decAssign[0].PartitionAss)
                    {
                        CreateConfiguration(partAssi, armatureName, maxLevelBone, dictionary);
                    }

                    // LOCATION ASSIGNMENT
                    // Computes LOC -> TUI Assignment
                    //List<Bone> locHandler = AutomaticMapping.GetLocHandler(brick);
                    List<Bone> locBones = GetLocBones(armature);
                    List<Bone> virtualArmature =
                        AutomaticMapping.GetTuiArmature(locBones, this.UseSensorCheckBox.IsChecked.Value, brick);

                    foreach (Bone bone in locBones)
                    {
                        PartitionAssignment locArrangements =
                            Metrics.ComputeAssignement(new List<Bone>() { bone }, virtualArmature,
                             maxLevelBone, dictionary, bone.name + "_LOC");

                        CreateConfiguration(locArrangements, armatureName, maxLevelBone, dictionary);
                    }
                }

                if (this.UserPreferenceSlider.Value <= 0)
                {
                    List<DecompositionAssignment> decAssign = new List<DecompositionAssignment>();

                    // CREATES PARTITION
                    List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();
                    graphPartitions = AutomaticMapping.GraphPartitioning
                        (KinectSkeleton.KINECT_SKELETON_DOF, graph, graphComponents, graphPartitions,
                         this.SplitDofCheckBox.IsChecked.Value, false);

                    List<Bone> virtualArmature = KinectSkeleton.GetKinectSkeleton();
                    foreach (List<List<Bone>> decomposition in graphPartitions)
                    {
                        List<PartitionAssignment> partAssign = new List<PartitionAssignment>();
                        foreach (List<Bone> currentPartition in decomposition)
                        {
                            partAssign.Add(Metrics.ComputeAssignement
                                (currentPartition, virtualArmature, maxLevelBone, dictionary, currentPartition[0].name));
                        }

                        decAssign.Add(AutomaticMapping.GetDecompositionAssignment(partAssign, DecompositionAssignment.KINECT_TYPE));

                    }

                    decAssign.Sort();
                    foreach (PartitionAssignment partAssi in decAssign[0].PartitionAss)
                    {
                        CreateConfiguration(partAssi, armatureName, maxLevelBone, dictionary);
                    }
                }

                //SetArrangmentLabel(rotArrangements[0]);               
            }                        
        }        
                

        public static List<Bone> GetLocBones(List<Bone> armature)
        {
            List<Bone> result = new List<Bone>();
            foreach (Bone b in armature)
            {
                if (b.loc_DoF.Count > 0)
                {
                    Bone boneToAdd = new Bone(b.name);
                    boneToAdd.loc_DoF = b.loc_DoF.ToList();
                    boneToAdd.level = b.level;
                    boneToAdd.parent = b.parent;
                    boneToAdd.children = b.children.ToList();
                    result.Add(boneToAdd);

                }
            }
            return result;            
        }             
       
        private void SetArrangmentLabel(PartitionAssignment axisArrangement)
        {
            this.ArrangementsOrderLabel.Content= "Arrangments: ";
            switch (axisArrangement.Name)
            { 
                
                case "TuiHip_Configuration":
                    foreach (int handlerIndex in axisArrangement.Assignment)
                        this.ArrangementsOrderLabel.Content += axisArrangement.Handler[handlerIndex].name + "_";
                    break;

                case "Kinect_Configuration":
                    this.ArrangementsOrderLabel.Content += axisArrangement.Name;
                    break;

                default:
                    this.ArrangementsOrderLabel.Content += axisArrangement.Handler[axisArrangement.Handler.Count-1].name;
                    break;
            }
                       
                
            
        }

        private void CreateConfiguration(PartitionAssignment arrangement, string armatureName, int maxLenghtChain, Dictionary<string, List<List<char>>> dictionary)
        {
            Setting settingToAdd = new Setting(arrangement.Name);
            List<Setting> settings = new List<Setting>();
            for (int i = 0; i < arrangement.Assignment.Length; i++)
            {
                Bone currentBone = arrangement.Partition[i];
                string boneName = currentBone.name;
                
                List<Bone> components = new List<Bone>();
                Bone handler = arrangement.Handler[arrangement.Assignment[i]];
                
                if (handler.name.Contains(" | "))
                    components = Metrics.DecomposeHandler(handler);
                else            
                    components.Add(handler);            

                List<Bone> oneDofBones = AutomaticMapping.GetOneDofBones(currentBone);

                PartitionAssignment BoneDofsAssignment =
                    Metrics.ComputeAssignement(oneDofBones, components, maxLenghtChain, dictionary, currentBone.name);
                int rotOrder = 0;

                for (int boneDof = 0; boneDof < BoneDofsAssignment.Assignment.Length; boneDof++)
                {                    
                    Bone component = components[BoneDofsAssignment.Assignment[boneDof]];
                    if (component.name.Contains("_TUI"))
                    {
                        string componentName = component.name;
                        string port = componentName.Substring(componentName.IndexOf("PORT-") + 5,
                             componentName.IndexOf(")_TUI") - (componentName.IndexOf("PORT-") + 5));
                        string axis = "";

                        if (componentName.Contains("LOC"))
                        {
                            axis = componentName.Substring(componentName.IndexOf(":LOC(") + 5, 1).ToUpper();
                            settingToAdd.CheckBoxStatus.Add(new SettingItem(port + "Loc", "true"));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "BoneName", boneName + ":" + armatureName));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "Axis", axis));
                        }

                        if (componentName.Contains("ROT"))
                        {
                            axis = componentName.Substring(componentName.IndexOf(":ROT(") + 5, 1).ToUpper();
                            settingToAdd.CheckBoxStatus.Add(new SettingItem(port + "Rot", "true"));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "BoneName", boneName + ":" + armatureName));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "Axis", axis));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "RotOrder", rotOrder.ToString()));
                            rotOrder++;
                        }
                    }
                    else
                    {
                        // "Hip_NUI:LOC(z)"
                        // is a Kinect component
                        //string[] splittedStringComponent = component.name.Split('_');
                        string componentName = component.name;
                        if (componentName.Contains('.'))
                            componentName = componentName.Remove(componentName.IndexOf('.'), 1);
                        string jointName = componentName.Substring(0, componentName.IndexOf("_NUI"));

                        if (componentName.Contains("LOC"))
                        {
                            string axis = componentName.Substring(componentName.IndexOf(":LOC(") + 5, 1).ToUpper();
                            settingToAdd.CheckBoxStatus.Add(new SettingItem(jointName + "Loc" + axis, "true"));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(jointName + "BoneName", boneName + ":" + armatureName));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(jointName + axis + "from", axis));
                        }

                        if (componentName.Contains("ROT"))
                        {
                            settingToAdd.CheckBoxStatus.Add(new SettingItem(jointName + "Rot", "true"));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(jointName + "BoneName", boneName + ":" + armatureName));
                        }

                        if (!componentName.Contains("ROT") && (!componentName.Contains("LOC")))
                        {
                            List<char> locDofs = currentBone.loc_DoF;
                            //LOC                            
                            settingToAdd.ComboBoxStatus.Add(
                                new SettingItem(jointName + "BoneName", boneName + ":" + armatureName));

                            foreach (char Dof in locDofs)
                            {
                                settingToAdd.ComboBoxStatus.Add(
                                    new SettingItem(jointName + Dof.ToString().ToUpper() + "from", Dof.ToString().ToUpper()));

                                settingToAdd.CheckBoxStatus.Add(
                                    new SettingItem(jointName + "Loc" + Dof.ToString().ToUpper(), "true"));
                            }
                            //ROT
                            settingToAdd.CheckBoxStatus.Add(new SettingItem(jointName + "Rot", "true"));
                            settingToAdd.ComboBoxStatus.Add(
                                new SettingItem(jointName + "BoneName", boneName + ":" + armatureName));
                        }
                    }

                }
                
                
                for(int componentIndex = 0 ; componentIndex < components.Count;componentIndex++)
                {
                    ;
                }
            }
            settings.Add(settingToAdd);
            ConvertSettingToPreset(settings);
        }
        
       

        
        private void ConvertSettingToPreset(List<Setting> settings)
        {
            foreach (Setting s in settings)
            {
                ResetControl();
                SetSetting(s);

                Preset preset = CreatePreset();
                preset.Name = s.SettingName;
                mw.presetConfig.Add(preset);
                mw.grammarRules.Add(new VoiceCommand(s.SettingName, Command.CHANGE_PRESET));

                Setting currentSetting = GetSetting();
                SaveSetting(preset.Name, currentSetting);

                currentSensorKinectInfo.RemoveRange(0, currentSensorKinectInfo.Count);
                currentSensorLegoInfo.RemoveRange(0, currentSensorLegoInfo.Count);
            }

            UpdatePresetList(mw.presetConfig);
            SetSetting(settings[0]);
        }
      
        private List<string> GetComponentsName(Bone components)
        {
            List<string> componentName = new List<string>();
            int oldIndex = 0;            
            foreach (int newIndex in AllIndexOf(components.name, "_"))
            {
                componentName.Add(components.name.Substring(oldIndex, newIndex - oldIndex));
                oldIndex = newIndex + 1;
            }
            
            // there is only one component
            if (componentName.Count == 0)
                componentName.Add(components.name);
            
            return componentName;
        }
       
        private List<Bone> RequestArmatureInfo(string armatureName)
        {
            List<Bone> armature = new List<Bone>();            
            Packet packet = new Packet();
            packet.header = Command.AUTO_CONFIG;
            packet.payload.Add(armatureName);
            mw.SendPacket(packet);

            // Receives armature info from Blender
            string stringReceived = AsynSocket.SyncReceiver();
            return JsonManager.GetBoneList(stringReceived);                        
        }

        private string RequestArmatureSelected() 
        {
            Packet packet = new Packet();
            packet.header = Command.ARMATURE_SELECTED;
            mw.SendPacket(packet);

            string stringReceived = AsynSocket.SyncReceiver();
            return JsonManager.GetString(stringReceived);
        }        

        public List<int> AllIndexOf(string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
            
        }
    }

    public class Calculation
    {        
        private ManualResetEvent _doneEvent;
        
        bool _useSensor;
        int _componentAvailable;
        int _maxLevelBone;
        List<char[]> _combination;
        List<List<Bone>> _decomposition;        
        Dictionary<string, List<List<char>>> _dictionary;
        Brick _brick;

        public List<DecompositionAssignment> _decAssign;
        public List<DecompositionAssignment> DecAssign { get { return _decAssign; } }

        // Constructor.
        public Calculation(ManualResetEvent doneEvent, List<char[]> combination, int componentAvailable,
            Dictionary<string, List<List<char>>> dictionary, List<List<Bone>> decomposition, bool useSensor, Brick brick, int maxLevelBone)
        {
            _doneEvent = doneEvent;

            _combination = combination;
            _componentAvailable = componentAvailable;
            _dictionary = dictionary;
            _decomposition = decomposition;
            _useSensor = useSensor;
            _brick = brick;
            _maxLevelBone = maxLevelBone;

            _decAssign = new List<DecompositionAssignment>();            
        }

        // Wrapper method for use with thread pool.
        public void ThreadPoolCallback(Object threadContext)
        {
            int threadIndex = (int)threadContext;
            Console.WriteLine("thread {0} started...", threadIndex);
            _decAssign = ComputingTask();
            Console.WriteLine("thread {0} result calculated...", threadIndex);
            _doneEvent.Set();
        }

        // Recursive method that calculates the Nth Fibonacci number.
        public List<DecompositionAssignment> ComputingTask()
        {                        
            // Finds the most frequent axis arrangement in the partition
            List<AxisArrangement> arrangements = new List<AxisArrangement>();
            foreach (char[] comb in _combination)
            {
                arrangements.Add(Metrics.GetBestAxisArrangement
                    (_componentAvailable, _dictionary, _decomposition, comb, _useSensor, _brick));
            }

            arrangements.Sort();

            float bestScore = arrangements[0].Score;

            //////////////////////////////////////////////////////////////
            foreach (AxisArrangement axisArr in arrangements)
            {
                if (axisArr.Score <= bestScore)
                {

                    // Creates all possible armature generated by the current axis arrangements
                    List<List<Bone>> virtualArmatures = AutomaticMapping.CreateArmaturesFromComb
                        (axisArr.AxisCombination, _brick, new string[] { "_ROT" });

                    /*List<List<Bone>> virtualArmatures = AutomaticMapping.CreateArmaturesFromComb
                        (arrangements[0].AxisCombination, brick, new string[] { "_ROT" });*/

                    /*List<List<Bone>> virtualArmatures = AutomaticMapping.CreateArmaturesFromComb
                        (new char[]{'x','y','z'}, brick, new string[] { "_ROT" });*/


                    List<List<Bone>> sequntialArmature = new List<List<Bone>>();
                    List<List<Bone>> splittedArmature = new List<List<Bone>>();
                    // Subdivides armatures into two groups: sequential and  splitted
                    foreach (List<Bone> arm in virtualArmatures)
                    {
                        if (AutomaticMapping.IsSplittedArmature(arm))
                        {
                            splittedArmature.Add(arm);
                        }
                        else
                        {
                            sequntialArmature.Add(arm);
                        }
                    }

                    DecompositionAssignment partialDecAssign = new DecompositionAssignment
                        (new List<PartitionAssignment>(), 0, DecompositionAssignment.SEQUENTIAL_TYPE);

                    // Computes assignment with sequential armature
                    foreach (List<Bone> currentPartition in _decomposition)
                    {
                        List<PartitionAssignment> partAssign = new List<PartitionAssignment>();

                        foreach (List<Bone> currentVirtualArmature in sequntialArmature)
                        {
                            // For all the sequential armatures can be used the same component arrangement                                                
                            // Virtual armature is able to control the current partition
                            List<Bone> rotCurrentPartition = AutomaticMapping.GetRotBones(currentPartition);
                            if (rotCurrentPartition.Count <= currentVirtualArmature.Count)
                            {
                                partAssign.Add(Metrics.ComputeAssignement
                                    (rotCurrentPartition, currentVirtualArmature, _maxLevelBone,
                                     _dictionary, rotCurrentPartition[0].name + "_ROT"));
                            }
                        }
                        partAssign.Sort();
                        partialDecAssign.PartitionAss.Add(partAssign[0]);
                    }

                    // Cost updates
                    _decAssign.Add(AutomaticMapping.GetDecompositionAssignment
                        (partialDecAssign.PartitionAss, DecompositionAssignment.SEQUENTIAL_TYPE));


                    // Computes assignment with splitted armature
                    foreach (List<Bone> currentVirtualArmature in splittedArmature)
                    {
                        List<PartitionAssignment> partAssign = new List<PartitionAssignment>();
                        bool validVirtualArm = true;

                        foreach (List<Bone> currentPartition in _decomposition)
                        {
                            List<Bone> rotCurrentPartition = AutomaticMapping.GetRotBones(currentPartition);
                            if (rotCurrentPartition.Count <= currentVirtualArmature.Count)
                            {
                                partAssign.Add(Metrics.ComputeAssignement
                                    (rotCurrentPartition, currentVirtualArmature, _maxLevelBone,
                                     _dictionary, currentPartition[0].name + "_ROT"));
                            }
                            else
                            {
                                validVirtualArm = false;
                                break;
                            }
                        }

                        if (validVirtualArm)
                        {
                            _decAssign.Add(AutomaticMapping.GetDecompositionAssignment(partAssign, DecompositionAssignment.SPLITTED_TYPE));
                        }
                    }
                }

                else
                {
                    break;
                }
            }

            return _decAssign;
        }
    }

}


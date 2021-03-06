﻿using Lego.Ev3.Core;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public string[] VocalCommand = new string[] { Command.POSITION, Command.FRAME, Command.FRAME_LOC, Command.FRAME_ROT, Command.REC, Command.STOP, Command.FORWARD, Command.BACKWARD, Command.DELETE, Command.DELETE_CURRENT, Command.PLAY_ANIMATION, Command.RESET, Command.FAST_FORWARD, Command.FAST_BACKWARD, Command.HIDE_CAPTURE, Command.BONES_ASSOCIATION, Command.TRANSLATE_COORDINATE_SYSTEM, Command.RESET_ORIGIN, Command.FIRST_FRAME, Command.ACTIVE_GHOST_FRAME, Command.DISABLE_GHOST_FRAME, Command.LAST_FRAME, Command.START_POSE, Command.END_POSE, Command.COMPUTE_FACTOR, Command.LESS_ACCURACY, Command.MORE_ACCURACY, Command.LOCK_POSE, Command.LOAD_POSE, Command.UNLOCK_POSE, Command.CHANGE_CAMERA, Command.COPY_FRAME, Command.PASTE_FRAME };


        public ConfigurationPanel(MainWindow mainWin, Brick brick, List<string> objects)
        {
            this.mw = mainWin;
            this.brick = brick;
            brick.BrickChanged += _brick_BrickChanged;

            this.currentSensorKinectInfo = new ArrayList();
            this.currentSensorLegoInfo = new ArrayList();
            this.configuration = new Config();

            this.objects = objects;
            this.objects.Insert(0, "");
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
            // mw.captureFlag = true;
            // mw.CaptureCheckBox.IsChecked = true;
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


            string[] preset = { "Humanoid", "Pixar_Lamp", "Crocodile", "Guss T_1", "Guss T_2", "Horse", "Dyno", "Dragon", "Elephant" };
            this.PresetComboBox.ItemsSource = preset;
            UpdatePresetList(mw.presetConfig);
            this.VocalCommandComboBox.ItemsSource = VocalCommand;

            this.XAxis.ItemsSource = axes;
            this.XBoneName.ItemsSource = objects;
            this.XRotOrder.ItemsSource = rotOrder;
            this.XRotOrder.SelectedIndex = 0;

            this.YAxis.ItemsSource = axes;
            this.YBoneName.ItemsSource = objects;
            this.YRotOrder.ItemsSource = rotOrder;
            this.YRotOrder.SelectedIndex = 0;

            this.ZAxis.ItemsSource = axes;
            this.ZBoneName.ItemsSource = objects;
            this.ZRotOrder.ItemsSource = rotOrder;
            this.ZRotOrder.SelectedIndex = 0;

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
        private void SaveConfig(string filename)
        {
            if (configuration.Settings.Count > 0)
            {
                string jsonSetting = JsonManager.SaveSetting(configuration, filename);
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
                string _filePath = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
                _filePath = System.IO.Directory.GetParent(System.IO.Directory.GetParent(_filePath).FullName).FullName + "\\Config\\";
                System.IO.File.Delete(_filePath + s.SettingName + ".json");
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

            foreach (var ctrl in this.HipDofCanvas.Children)
            {
                if (ctrl.GetType() == typeof(ComboBox) && ((ComboBox)ctrl).SelectedItem != null)
                {
                    setting.ComboBoxStatus.Add(new SettingItem(((ComboBox)ctrl).Name.ToString(), ((ComboBox)ctrl).SelectedItem.ToString()));
                }

                if (ctrl.GetType() == typeof(CheckBox))
                {
                    setting.CheckBoxStatus.Add(new SettingItem(((CheckBox)ctrl).Name.ToString(), ((CheckBox)ctrl).IsChecked.Value.ToString()));
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
                SetSensorLegoInfo((int)InputPort.A, this.ABoneName.SelectedItem.ToString(), this.ALoc.IsChecked.Value, this.ARot.IsChecked.Value, this.AAxis.SelectedItem.ToString(), Convert.ToInt32(ARotOrder.SelectedItem.ToString()));

            }

            if ((this.BLoc.IsChecked.Value || this.BRot.IsChecked.Value) && (this.BBoneName.SelectedIndex >= 0) && (this.BAxis.SelectedIndex >= 0))
            {
                if (!BRot.IsChecked.Value)
                    BRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo((int)InputPort.B, this.BBoneName.SelectedItem.ToString(), this.BLoc.IsChecked.Value, this.BRot.IsChecked.Value, this.BAxis.SelectedItem.ToString(), Convert.ToInt32(BRotOrder.SelectedItem.ToString()));
            }

            if ((this.CLoc.IsChecked.Value || this.CRot.IsChecked.Value) && (this.CBoneName.SelectedIndex >= 0) && (this.CAxis.SelectedIndex >= 0))
            {
                if (!CRot.IsChecked.Value)
                    CRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo((int)InputPort.C, this.CBoneName.SelectedItem.ToString(), this.CLoc.IsChecked.Value, this.CRot.IsChecked.Value, this.CAxis.SelectedItem.ToString(), Convert.ToInt32(CRotOrder.SelectedItem.ToString()));
            }

            if ((this.DLoc.IsChecked.Value || this.DRot.IsChecked.Value) && (this.DBoneName.SelectedIndex >= 0) && (this.DAxis.SelectedIndex >= 0))
            {
                if (!DRot.IsChecked.Value)
                    DRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo((int)InputPort.D, this.DBoneName.SelectedItem.ToString(), this.DLoc.IsChecked.Value, this.DRot.IsChecked.Value, this.DAxis.SelectedItem.ToString(), Convert.ToInt32(DRotOrder.SelectedItem.ToString()));
            }

            if ((this.OneLoc.IsChecked.Value || this.OneRot.IsChecked.Value) && (this.OneBoneName.SelectedIndex >= 0) && (this.OneAxis.SelectedIndex >= 0))
            {
                if (!OneRot.IsChecked.Value)
                    OneRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo((int)InputPort.One, this.OneBoneName.SelectedItem.ToString(), this.OneLoc.IsChecked.Value, this.OneRot.IsChecked.Value, this.OneAxis.SelectedItem.ToString(), Convert.ToInt32(OneRotOrder.SelectedItem.ToString()));
            }

            if ((this.TwoLoc.IsChecked.Value || this.TwoRot.IsChecked.Value) && (this.TwoBoneName.SelectedIndex >= 0) && (this.TwoAxis.SelectedIndex >= 0))
            {
                if (!TwoRot.IsChecked.Value)
                    TwoRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo((int)InputPort.Two, this.TwoBoneName.SelectedItem.ToString(), this.TwoLoc.IsChecked.Value, this.TwoRot.IsChecked.Value, this.TwoAxis.SelectedItem.ToString(), Convert.ToInt32(TwoRotOrder.SelectedItem.ToString()));
            }

            if ((this.ThreeLoc.IsChecked.Value || this.ThreeRot.IsChecked.Value) && (this.ThreeBoneName.SelectedIndex >= 0) && (this.ThreeAxis.SelectedIndex >= 0))
            {
                if (!ThreeRot.IsChecked.Value)
                    ThreeRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo((int)InputPort.Three, this.ThreeBoneName.SelectedItem.ToString(), this.ThreeLoc.IsChecked.Value, this.ThreeRot.IsChecked.Value, this.ThreeAxis.SelectedItem.ToString(), Convert.ToInt32(ThreeRotOrder.SelectedItem.ToString()));
            }

            if ((this.FourLoc.IsChecked.Value || this.FourRot.IsChecked.Value) && (this.FourBoneName.SelectedIndex >= 0) && (this.FourAxis.SelectedIndex >= 0))
            {
                if (!FourRot.IsChecked.Value)
                    FourRotOrder.SelectedIndex = 1;
                SetSensorLegoInfo((int)InputPort.Four, this.FourBoneName.SelectedItem.ToString(), this.FourLoc.IsChecked.Value, this.FourRot.IsChecked.Value, this.FourAxis.SelectedItem.ToString(), Convert.ToInt32(FourRotOrder.SelectedItem.ToString()));
            }

            // HIP for LOC_ROT_Partition
            if ((this.XLoc.IsChecked.Value || this.XRot.IsChecked.Value) && (this.XBoneName.SelectedIndex >= 0) && (this.XAxis.SelectedIndex >= 0))
            {
                if (!XRot.IsChecked.Value)
                    XRotOrder.SelectedIndex = 1;

                SetSensorLegoInfo(20, this.XBoneName.SelectedItem.ToString(),
                    this.XLoc.IsChecked.Value, this.XRot.IsChecked.Value, this.XAxis.SelectedItem.ToString(),
                    Convert.ToInt32(XRotOrder.SelectedItem.ToString()));
            }
            if ((this.YLoc.IsChecked.Value || this.YRot.IsChecked.Value) && (this.YBoneName.SelectedIndex >= 0) && (this.YAxis.SelectedIndex >= 0))
            {
                if (!YRot.IsChecked.Value)
                    YRotOrder.SelectedIndex = 1;

                SetSensorLegoInfo(21, this.YBoneName.SelectedItem.ToString(),
                    this.YLoc.IsChecked.Value, this.YRot.IsChecked.Value, this.YAxis.SelectedItem.ToString(),
                    Convert.ToInt32(YRotOrder.SelectedItem.ToString()));
            }
            if ((this.ZLoc.IsChecked.Value || this.ZRot.IsChecked.Value) && (this.ZBoneName.SelectedIndex >= 0) && (this.ZAxis.SelectedIndex >= 0))
            {
                if (!ZRot.IsChecked.Value)
                    ZRotOrder.SelectedIndex = 1;

                SetSensorLegoInfo(22, this.ZBoneName.SelectedItem.ToString(),
                    this.ZLoc.IsChecked.Value, this.ZRot.IsChecked.Value, this.ZAxis.SelectedItem.ToString(),
                    Convert.ToInt32(ZRotOrder.SelectedItem.ToString()));
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
                    this.ElbowLYfrom.SelectedItem.ToString(), this.ElbowLZfrom.SelectedItem.ToString());
            }

            if ((this.WristLLocX.IsChecked.Value || this.WristLLocY.IsChecked.Value || this.WristLLocZ.IsChecked.Value || this.WristLRot.IsChecked.Value) && (this.WristLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.WristLeft, this.WristLBoneName.SelectedItem.ToString(), this.WristLLocX.IsChecked.Value, this.WristLLocY.IsChecked.Value, this.WristLLocZ.IsChecked.Value, this.WristLRot.IsChecked.Value, (float)WristLeftSlider.Value, this.WristLXfrom.SelectedItem.ToString(), this.WristLYfrom.SelectedItem.ToString(), this.WristLZfrom.SelectedItem.ToString());
            }

            if ((this.HandLLocX.IsChecked.Value || this.HandLLocY.IsChecked.Value || this.HandLLocZ.IsChecked.Value || this.HandLRot.IsChecked.Value) && (this.HandLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HandLeft, this.HandLBoneName.SelectedItem.ToString(),
                    this.HandLLocX.IsChecked.Value, this.HandLLocY.IsChecked.Value, this.HandLLocZ.IsChecked.Value,
                    this.HandLRot.IsChecked.Value, (float)HandLeftSlider.Value, this.HandLXfrom.SelectedItem.ToString(),
                    this.HandLYfrom.SelectedItem.ToString(), this.HandLZfrom.SelectedItem.ToString());
            }

            if ((this.ShoulderRLocX.IsChecked.Value || this.ShoulderRLocY.IsChecked.Value || this.ShoulderRLocZ.IsChecked.Value || this.ShoulderRRot.IsChecked.Value) && (this.ShoulderRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.ShoulderRight, this.ShoulderRBoneName.SelectedItem.ToString(), this.ShoulderRLocX.IsChecked.Value, this.ShoulderRLocY.IsChecked.Value, this.ShoulderRLocZ.IsChecked.Value, this.ShoulderRRot.IsChecked.Value, (float)ShoulderRightSlider.Value, this.ShoulderRXfrom.SelectedItem.ToString(), this.ShoulderRYfrom.SelectedItem.ToString(), this.ShoulderRZfrom.SelectedItem.ToString());
            }

            if ((this.ElbowRLocX.IsChecked.Value || this.ElbowRLocY.IsChecked.Value || this.ElbowRLocZ.IsChecked.Value || this.ElbowRRot.IsChecked.Value) && (this.ElbowRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.ElbowRight, this.ElbowRBoneName.SelectedItem.ToString(), this.ElbowRLocX.IsChecked.Value, this.ElbowRLocY.IsChecked.Value, this.ElbowRLocZ.IsChecked.Value, this.ElbowRRot.IsChecked.Value, (float)ElbowRightSlider.Value, this.ElbowRXfrom.SelectedItem.ToString(), this.ElbowRYfrom.SelectedItem.ToString(), this.ElbowRZfrom.SelectedItem.ToString());
            }

            if ((this.WristRLocX.IsChecked.Value || this.WristRLocY.IsChecked.Value || this.WristRLocZ.IsChecked.Value || this.WristRRot.IsChecked.Value) && (this.WristRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.WristRight, this.WristRBoneName.SelectedItem.ToString(), this.WristRLocX.IsChecked.Value, this.WristRLocY.IsChecked.Value, this.WristRLocZ.IsChecked.Value, this.WristRRot.IsChecked.Value, (float)WristRightSlider.Value, this.WristRXfrom.SelectedItem.ToString(), this.WristRYfrom.SelectedItem.ToString(), this.WristRZfrom.SelectedItem.ToString());
            }

            if ((this.HandRLocX.IsChecked.Value || this.HandRLocY.IsChecked.Value || this.HandRLocZ.IsChecked.Value || this.HandRRot.IsChecked.Value) && (this.HandRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HandRight, this.HandRBoneName.SelectedItem.ToString(), this.HandRLocX.IsChecked.Value, this.HandRLocY.IsChecked.Value, this.HandRLocZ.IsChecked.Value, this.HandRRot.IsChecked.Value, (float)HandRightSlider.Value, this.HandRXfrom.SelectedItem.ToString(), this.HandRYfrom.SelectedItem.ToString(), this.HandRZfrom.SelectedItem.ToString());
            }

            if ((this.SpineLocX.IsChecked.Value || this.SpineLocY.IsChecked.Value || this.SpineLocZ.IsChecked.Value || this.SpineRot.IsChecked.Value) && (this.SpineBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.Spine, this.SpineBoneName.SelectedItem.ToString(), this.SpineLocX.IsChecked.Value, this.SpineLocY.IsChecked.Value, this.SpineLocZ.IsChecked.Value, this.SpineRot.IsChecked.Value, (float)SpineSlider.Value, this.SpineXfrom.SelectedItem.ToString(), this.SpineYfrom.SelectedItem.ToString(), this.SpineZfrom.SelectedItem.ToString());
            }

            if ((this.HipLocX.IsChecked.Value || this.HipLocY.IsChecked.Value || this.HipLocZ.IsChecked.Value || this.HipRot.IsChecked.Value) && (this.HipBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HipCenter, this.HipBoneName.SelectedItem.ToString(), this.HipLocX.IsChecked.Value, this.HipLocY.IsChecked.Value, this.HipLocZ.IsChecked.Value, this.HipRot.IsChecked.Value, (float)HipSlider.Value, this.HipXfrom.SelectedItem.ToString(), this.HipYfrom.SelectedItem.ToString(), this.HipZfrom.SelectedItem.ToString());
            }

            if ((this.HipLLocX.IsChecked.Value || this.HipLLocY.IsChecked.Value || this.HipLLocZ.IsChecked.Value || this.HipLRot.IsChecked.Value) && (this.HipLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.HipLeft, this.HipLBoneName.SelectedItem.ToString(), this.HipLLocX.IsChecked.Value, this.HipLLocY.IsChecked.Value, this.HipLLocZ.IsChecked.Value, this.HipLRot.IsChecked.Value, (float)HipLeftSlider.Value, this.HipLXfrom.SelectedItem.ToString(), this.HipLYfrom.SelectedItem.ToString(), this.HipLZfrom.SelectedItem.ToString());
            }

            if ((this.KneeLLocX.IsChecked.Value || this.KneeLLocY.IsChecked.Value || this.KneeLLocY.IsChecked.Value || this.KneeLRot.IsChecked.Value) && (this.KneeLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.KneeLeft, this.KneeLBoneName.SelectedItem.ToString(), this.KneeLLocX.IsChecked.Value, this.KneeLLocY.IsChecked.Value, this.KneeLLocZ.IsChecked.Value, this.KneeLRot.IsChecked.Value, (float)KneeLeftSlider.Value, this.KneeLXfrom.SelectedItem.ToString(), this.KneeLYfrom.SelectedItem.ToString(), this.KneeLZfrom.SelectedItem.ToString());
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
                SetSensorKinectInfo(JointType.KneeRight, this.KneeRBoneName.SelectedItem.ToString(), this.KneeRLocX.IsChecked.Value, this.KneeRLocY.IsChecked.Value, this.KneeRLocZ.IsChecked.Value, this.KneeRRot.IsChecked.Value, (float)KneeRightSlider.Value, this.KneeRXfrom.SelectedItem.ToString(), this.KneeRYfrom.SelectedItem.ToString(), this.KneeRZfrom.SelectedItem.ToString());
            }
            if ((this.AnkleRLocX.IsChecked.Value || this.AnkleRLocY.IsChecked.Value || this.AnkleRLocZ.IsChecked.Value || this.AnkleRRot.IsChecked.Value) && (this.AnkleRBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.AnkleRight, this.AnkleRBoneName.SelectedItem.ToString(), this.AnkleRLocX.IsChecked.Value, this.AnkleRLocY.IsChecked.Value, this.AnkleRLocZ.IsChecked.Value, this.AnkleRRot.IsChecked.Value, (float)AnkleRightSlider.Value, this.AnkleRXfrom.SelectedItem.ToString(), this.AnkleRYfrom.SelectedItem.ToString(), this.AnkleRZfrom.SelectedItem.ToString());
            }
            if ((this.FootRLocX.IsChecked.Value || this.FootRLocY.IsChecked.Value || this.FootRLocZ.IsChecked.Value || this.FootRRot.IsChecked.Value) && (this.FootLBoneName.SelectedIndex >= 0))
            {
                SetSensorKinectInfo(JointType.FootRight, this.FootRBoneName.SelectedItem.ToString(), this.FootRLocX.IsChecked.Value, this.FootRLocY.IsChecked.Value, this.FootRLocZ.IsChecked.Value, this.FootRRot.IsChecked.Value, (float)FootRightSlider.Value, this.FootRXfrom.SelectedItem.ToString(), this.FootRYfrom.SelectedItem.ToString(), this.FootRZfrom.SelectedItem.ToString());
            }
        }



        private void SetSensorKinectInfo(JointType jointType, string objectName, bool locX, bool locY, bool locZ, bool rot, float sliderFactor, string XFrom, string YFrom, string ZFrom)
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

        private void SetSensorLegoInfo(int inputPort, string objectName, bool loc, bool rot, string axis, int rotOrder)
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

            foreach (var ctrl in this.HipDofCanvas.Children)
            {
                if (ctrl.GetType() == typeof(CheckBox))
                {
                    ((CheckBox)ctrl).IsChecked = false;
                }

                if (ctrl.GetType() == typeof(ComboBox))
                {
                    ((ComboBox)ctrl).SelectedIndex = -1;
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
                string _filePath = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
                _filePath = System.IO.Directory.GetParent(System.IO.Directory.GetParent(_filePath).FullName).FullName + "\\Config\\";
                System.IO.File.Delete(_filePath + presetToRemove + ".json");
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
                    LoadConfig("Preset_Pixar_Lamp_TestVIDEO");
                    break;

                case "Crocodile":
                    this.factorSlider.Value = 10;
                    LoadConfig("Preset_Crocodile_TestVIDEO");
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

                case "Horse":
                    this.factorSlider.Value = 15;
                    LoadConfig("Preset_Horse");
                    break;

                case "Dyno":
                    this.factorSlider.Value = 15;
                    LoadConfig("Preset_Dyno");
                    break;

                case "Dragon":
                    this.factorSlider.Value = 15;
                    LoadConfig("Preset_Dragon");
                    break;

                case "Elephant":
                    this.factorSlider.Value = 15;
                    LoadConfig("Preset_Elephant");
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

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.Filter = "JSON Files (*.json)|*.json";
            string _filePath = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            _filePath = System.IO.Directory.GetParent(System.IO.Directory.GetParent(_filePath).FullName).FullName + "\\Config";
            dlg.InitialDirectory = _filePath;
            
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                LoadConfig(filename);
            }
            
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mw.factor = (int)e.NewValue;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig(this.ConfigName.Text);
            this.ConfigName.Text = "";
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
                    ObjectRotationMatrix newObjcetRotMatrix = new ObjectRotationMatrix(slf.ObjectName + ":" + slf.BoneName);
                    if (!mw.rotationMatrix.Contains(newObjcetRotMatrix))
                        mw.rotationMatrix.Add(newObjcetRotMatrix);
                }

                mw.StartSpeechRecognition();
                mw.CaptureCheckBox.IsChecked = false;
                mw.captureFlag = false;
                //mw.virtualArmature = this.VirtualArmatureCheckBox.IsChecked.Value;
                mw.ConfidenceThreshold = this.ConfidenceThresholdSlider.Value;
                mw.ghostFrameRange = (int)Decimal.Parse(this.RangeTextBox.Text, System.Globalization.CultureInfo.InvariantCulture);
                mw.ghostFrameStep = (int)Decimal.Parse(this.StepTextBox.Text, System.Globalization.CultureInfo.InvariantCulture);
                mw.currentPreset = mw.presetConfig[selectedConfig].Name;
                mw.UpdateChangePresetList();
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
            List<string> symmetricBonesName = RemoveUnusedBones(armature, this.ConsiderSplitArmatureCheckBox.IsChecked.Value);


            int maxLevelBone = AutomaticMapping.GetMaxLengthChain(armature);
            int maxPartitionCount = 0;

            // Create graph from armature
            List<Bone> uniquePartition = new List<Bone>();
            var graph = AutomaticMapping.CreateDirectedGraph(armature);
            // Obtaints the graph connected component 
            List<List<Bone>> graphComponents = AutomaticMapping.GetConnectedComponentList(graph);


            UserPreference pref = new UserPreference(
                this.LocRotCheckBox.IsChecked.Value, this.UseSensorCheckBox.IsChecked.Value,
                this.ConsiderSplitArmatureCheckBox.IsChecked.Value,
                (float)Convert.ToDouble(this.NodSim.Text), (float)Convert.ToDouble(this.DofCov.Text),
                (float)Convert.ToDouble(this.ComRan.Text), (float)Convert.ToDouble(this.ComAnn.Text),
                (float)Convert.ToDouble(this.PosInC.Text), (float)Convert.ToDouble(this.Sym.Text),
                (float)Convert.ToDouble(this.ParCou.Text));

            bool useHipJoint = false;
            if (this.UserPreferenceSlider.Value <= 0)
                useHipJoint = true;

            // Counts the number of components available depending on user preferences
            if (this.UserPreferenceSlider.Value >= 0)
            {
                // User chooses the Tui Interface
                componentAvailable = AutomaticMapping.CountComponentAvailable
                    (new List<string>() { "LMotor", "MMotor" }, brick);

                if (this.UseSensorCheckBox.IsChecked.Value)
                {
                    componentAvailable += AutomaticMapping.CountComponentAvailable
                        (new List<string>() { "Gyroscope", "Ultrasonic" }, brick);

                    if(useHipJoint)
                    // componentAvailable is increased to consider the Kinect_Hip DoFs
                        componentAvailable += 3;
                }

                // VIRTUAL_MOTOR 
                //componentAvailable += 2;

                // Checks that the available components are able to control the bones of the partition
                if (DofCountTest(armature, this.UseSensorCheckBox.IsChecked.Value, componentAvailable) && (graphComponents.Count < 2))
                {
                    uniquePartition = armature;
                }
            }
            else
            {
                // User chooses the Nui Interface
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
                if (this.UserPreferenceSlider.Value >= 0)
                {
                    // Computes Tui + Hip score assignement                
                    List<Bone> virtualArmature = AutomaticMapping.GetTuiArmature
                        (uniquePartition, this.UseSensorCheckBox.IsChecked.Value, brick, dictionary);

                    // Computes assignement
                    locRotArrangements.Add(ComputeAssignement
                        (uniquePartition, virtualArmature, 1, 1, dictionary, "TUI+HIP_CONFIG", maxLevelBone, pref));

                    CreateLegoInstruction(AutomaticMapping.GetDecompositionAssignment
                        (locRotArrangements, DecompositionAssignment.SINGLE_CONF_TYPE));
                }
                if (this.UserPreferenceSlider.Value < 0)
                {
                    List<Bone> targetArmature = new List<Bone>();
                    for (int i = 0; i < uniquePartition.Count; i++)
                    {
                        if (uniquePartition[i].rot_DoF.Count + uniquePartition[i].loc_DoF.Count > 0)
                            targetArmature.Add(uniquePartition[i]);
                    }
                    // Computes kinect score assignment
                    List<Bone> virtualArmature = KinectSkeleton.GetKinectSkeleton();
                    locRotArrangements.Add(ComputeAssignement
                        (targetArmature, virtualArmature, 1, 1, dictionary, "KINECT_CONFIG", maxLevelBone, pref));
                }

                // Creates configuration
                if (locRotArrangements[0] != null)
                {
                    locRotArrangements.Sort();
                    CreateConfiguration(locRotArrangements[0], armatureName, maxLevelBone, 1, 1, dictionary, pref);
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
                        if (!this.ReadDecAssignFromFile.IsChecked.Value)
                        {
                            graphPartitions = GraphPartitioning(componentAvailable, graph, graphComponents, graphPartitions,
                                this.LocRotCheckBox.IsChecked.Value, 1, AutomaticMapping.GetMaxBoneDofCount(graphComponents));
                        }
                    }
                    catch (ApplicationException)
                    {
                        MessageBoxResult result = MessageBox.Show
                            (this, "Required components not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    maxPartitionCount = AutomaticMapping.GetMaxPartitionCount(graphPartitions);

                    if (!this.LocRotCheckBox.IsChecked.Value)
                    {
                        // ROTATION ASSIGNMENT
                        // Computes ROT -> TUI Assignment
                        List<DecompositionAssignment> decAssign = new List<DecompositionAssignment>();
                        float decAssignBestScore = int.MaxValue;

                        List<char[]> combination = AutomaticMapping.ComputeDofCombination(componentAvailable);

                        // Progress bar
                        float CompletePercentage = 0;
                        float indexCurrentGraphPartition = 0;

                        // Data structures used during computing
                        List<AxisArrangement> arrangements = new List<AxisArrangement>();
                        List<List<List<Bone>>> sourceArmatureTemplateList = new List<List<List<Bone>>>();
                        List<List<Bone>> splitArmAlternatives = new List<List<Bone>>();

                        // Search the best sequence                    
                        foreach (List<List<Bone>> decomposition in graphPartitions)
                        {
                            // Reset data
                            arrangements.Clear();
                            sourceArmatureTemplateList.Clear();

                            // Updates progress bar                                   
                            CompletePercentage = indexCurrentGraphPartition / (graphPartitions.Count - 1) * 100;
                            indexCurrentGraphPartition++;
                            System.Diagnostics.Debug.WriteLine(CompletePercentage.ToString() + " %");
                            //Console.WriteLine(CompletePercentage.ToString() + " %");

                            // Finds the most frequent axis arrangement in the partition                        
                            AxisArrangement arr = new AxisArrangement();
                            foreach (char[] comb in combination)
                            {
                                arr = Metrics.GetBestAxisArrangement(componentAvailable, dictionary, decomposition,
                                    comb, this.UseSensorCheckBox.IsChecked.Value, brick);

                                if (!arrangements.Contains(arr))
                                    arrangements.Add(arr);
                            }
                            arrangements.Sort();

                            //////////////////////////////////////////////////////////////
                            float bestScore = arrangements[0].Score;

                            int arrangementsIndex = 0;
                            foreach (AxisArrangement axisArr in arrangements)
                            {
                                arrangementsIndex++;
                                if (axisArr.Score <= bestScore)
                                {
                                    // Creates all possible armature generated by the current axis arrangements
                                    sourceArmatureTemplateList = AutomaticMapping.CreateArmaturesFromComb
                                        (axisArr.AxisCombination, brick, componentAvailable,
                                        this.LocRotCheckBox.IsChecked.Value, this.UseSensorCheckBox.IsChecked.Value, true, useHipJoint);

                                    // Each of its PartAss contains the best 
                                    // association between partition and sequential armature     
                                    DecompositionAssignment partialDecAssign = new DecompositionAssignment
                                        (new List<PartitionAssignment>(), 0, DecompositionAssignment.SEQUENTIAL_TYPE);

                                    // Computes assignment with sequential armature
                                    foreach (List<Bone> currentPartition in decomposition)
                                    {
                                        List<PartitionAssignment> partAssign = new List<PartitionAssignment>();
                                        float partAssigBestScore = float.MaxValue;

                                        foreach (List<Bone> currentVirtualArmature in sourceArmatureTemplateList[0])
                                        {
                                            // For all the sequential armatures can be used the same component arrangement, 
                                            // so the algorithm chooses the best;                                              

                                            List<Bone> rotCurrentPartition = GetRotBones(currentPartition);
                                            // Virtual armature is able to control the current partition
                                            if (rotCurrentPartition.Count <= currentVirtualArmature.Count)
                                            {
                                                partAssign.Add(ComputeAssignement
                                                    (rotCurrentPartition, currentVirtualArmature,
                                                    decomposition.Count, maxPartitionCount, dictionary,
                                                     rotCurrentPartition[0].name + "_ROT", maxLevelBone, pref));

                                                if (partAssign[partAssign.Count - 1].Score > partAssigBestScore)
                                                {
                                                    partAssign.RemoveAt(partAssign.Count - 1);
                                                }
                                                else
                                                {
                                                    partAssigBestScore = partAssign[partAssign.Count - 1].Score;
                                                }
                                            }
                                        }

                                        partAssign.Sort();
                                        partAssign.RemoveRange(1, partAssign.Count - 1);
                                        partialDecAssign.PartitionAss.Add(partAssign[0]);
                                    }

                                    // Cost updates                                
                                    decAssign.Add(GetDecompositionAssignment
                                        (partialDecAssign.PartitionAss, DecompositionAssignment.SEQUENTIAL_TYPE));

                                    if (decAssign[decAssign.Count - 1].TotalScore > decAssignBestScore ||
                                        !IsFeasibleSolution(decAssign[decAssign.Count - 1], dictionary))
                                    {
                                        decAssign.RemoveAt(decAssign.Count - 1);
                                    }
                                    else
                                    {
                                        decAssignBestScore = decAssign[decAssign.Count - 1].TotalScore;
                                    }


                                    // DEBUG
                                    int splitIndex = 0;
                                    // Computes assignment with splitted armature
                                    foreach (List<Bone> splittedVirtualArmature in sourceArmatureTemplateList[1])
                                    {
                                        splitIndex++;

                                        splitArmAlternatives =
                                            ComputeAlternatives(splittedVirtualArmature, componentAvailable);

                                        partialDecAssign = new DecompositionAssignment
                                        (new List<PartitionAssignment>(), 0, DecompositionAssignment.SPLITTED_TYPE);

                                        // Computes assignment with all the alteratives splitted armature
                                        foreach (List<Bone> currentPartition in decomposition)
                                        {
                                            List<PartitionAssignment> partAssign = new List<PartitionAssignment>();
                                            float partAssigBestScore = float.MaxValue;

                                            foreach (List<Bone> currentVirtualArmature in splitArmAlternatives)
                                            {
                                                List<Bone> rotCurrentPartition = GetRotBones(currentPartition);
                                                // Virtual armature is able to control the current partition
                                                if (rotCurrentPartition.Count <= currentVirtualArmature.Count)
                                                {
                                                    partAssign.Add(ComputeAssignement
                                                        (rotCurrentPartition, currentVirtualArmature,
                                                         decomposition.Count, maxPartitionCount, dictionary,
                                                         rotCurrentPartition[0].name + "_ROT", maxLevelBone, pref));

                                                    if (partAssign[partAssign.Count - 1].Score > partAssigBestScore)
                                                    {
                                                        partAssign.RemoveAt(partAssign.Count - 1);
                                                    }
                                                    else
                                                    {
                                                        partAssigBestScore = partAssign[partAssign.Count - 1].Score;
                                                    }
                                                }
                                            }

                                            // 
                                            //if(partAssign.Count>0)
                                            //{
                                            partAssign.Sort();
                                            partAssign.RemoveRange(1, partAssign.Count - 1);
                                            partialDecAssign.PartitionAss.Add(partAssign[0]);
                                            //}
                                        }

                                        // Cost updates                                
                                        decAssign.Add(GetDecompositionAssignment
                                            (partialDecAssign.PartitionAss, DecompositionAssignment.SPLITTED_TYPE));
                                        decAssign[decAssign.Count - 1].SplittedArmature = splittedVirtualArmature;



                                        if (decAssign[decAssign.Count - 1].TotalScore > decAssignBestScore ||
                                        !IsFeasibleSolution(decAssign[decAssign.Count - 1], dictionary))
                                        {
                                            decAssign.RemoveAt(decAssign.Count - 1);
                                        }
                                        else
                                        {
                                            decAssignBestScore = decAssign[decAssign.Count - 1].TotalScore;
                                        }
                                    }
                                }

                                else
                                {
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////////////////                                               
                        }

                        decAssign.Sort();
                        List<List<ComputationData>> data = ViewScores(decAssign, dictionary, maxLevelBone, maxPartitionCount, pref);

                        CreateLegoInstruction(decAssign[0]);

                        foreach (PartitionAssignment partAssi in decAssign[0].PartitionAss)
                        {
                            CreateConfiguration(partAssi, armatureName, maxLevelBone,
                                decAssign[0].PartitionAss.Count, maxPartitionCount, dictionary, pref);
                        }

                        // LOCATION ASSIGNMENT
                        // Computes LOC -> TUI Assignment
                        //List<Bone> locHandler = AutomaticMapping.GetLocHandler(brick);
                        List<Bone> locBones = GetLocBones(armature);

                        foreach (Bone bone in locBones)
                        {
                            List<Bone> virtualArmature = AutomaticMapping.GetTuiArmature
                                (new List<Bone> { bone }, this.UseSensorCheckBox.IsChecked.Value, brick, dictionary);

                            PartitionAssignment locArrangements =
                                ComputeAssignement(new List<Bone>() { bone },
                                virtualArmature, maxLevelBone, 1, dictionary, bone.name + "_LOC", 1, pref);

                            CreateConfiguration(locArrangements, armatureName, maxLevelBone, 1, 1, dictionary, pref);

                        }
                    }
                    else
                    {
                        List<DecompositionAssignment> decAssign = new List<DecompositionAssignment>();
                        List<List<ComputationData>> data = new List<List<ComputationData>>();

                        if (!this.ReadDecAssignFromFile.IsChecked.Value)
                        {
                            DateTime startTime = DateTime.Now;
                            System.Diagnostics.Debug.WriteLine("START AT" + " " + startTime);

                            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { this.StartTime.Content = DateTime.Now; }));

                            // LOCATION + ROTATION ASSIGNMENT
                            // object lockObj = new object();                                                
                            List<char[]> combination = AutomaticMapping.ComputeDofCombination(componentAvailable);

                            bool LocRotCheckBox = this.LocRotCheckBox.IsChecked.Value;
                            bool UseSensorCheckBox = this.UseSensorCheckBox.IsChecked.Value;

                            // Progress bar
                            float CompletePercentage = 0;
                            float indexCurrentGraphPartition = 0;

                            // Data structures used during computing 
                            List<AxisArrangement> arrangements = new List<AxisArrangement>();
                            HashSet<AxisArrangement> arrangementsHash = new HashSet<AxisArrangement>();
                            List<List<Bone>> splitArmAlternatives = new List<List<Bone>>();

                            float decAssignBestScore = int.MaxValue;
                            Dictionary<string, PartitionAssignmentTmp> partitionsHash =
                                new Dictionary<string, PartitionAssignmentTmp>();

                            foreach (List<List<Bone>> decomposition in graphPartitions)
                            {

                                bool IsSplittedPartition = false;

                                //Reset data
                                arrangements.Clear();
                                arrangementsHash.Clear();

                                // Updates progress bar  
                                if (graphPartitions.Count == 1)
                                    CompletePercentage = 100;
                                else
                                    CompletePercentage = indexCurrentGraphPartition / (graphPartitions.Count - 1) * 100;
                                indexCurrentGraphPartition++;
                                System.Diagnostics.Debug.WriteLine(CompletePercentage.ToString() + " %");
                                // DEBUG                            
                                if (CompletePercentage != 0)
                                {
                                    System.Diagnostics.Debug.WriteLine(decomposition.Count + " " + DateTime.Now + " estimated remaining time: " +
                                        startTime.AddMinutes(((DateTime.Now - startTime).TotalMinutes * 100 / CompletePercentage)));
                                    AsynSocket.Send(decomposition.Count + " " + DateTime.Now + " estimated remaining time: " +
                                        startTime.AddMinutes(((DateTime.Now - startTime).TotalMinutes * 100 / CompletePercentage)));
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine(decomposition.Count + " " + DateTime.Now + " calculating remaining time ... ");
                                    AsynSocket.Send(decomposition.Count + " " + DateTime.Now + " calculating remaining time ... ");
                                }
                                Application.Current.Dispatcher.Invoke
                                    (System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                                    new System.Threading.ThreadStart(delegate
                                    {
                                        this.ProgressBar.Value = Math.Round(CompletePercentage, 2);
                                        if (CompletePercentage != 0)
                                        {
                                            this.Progress.Content = Math.Round(CompletePercentage, 2).ToString() + " % AT " + DateTime.Now
                                                + " estimated remaining time: " +
                                                startTime.AddMinutes(((DateTime.Now - startTime).TotalMinutes * 100 / CompletePercentage));
                                        }
                                        else
                                        {
                                            this.Progress.Content = Math.Round(CompletePercentage, 2).ToString() + " % AT " + DateTime.Now
                                                + " calculating remaining time ...";
                                        }
                                    }
                                ));

                                //Reset the bone level and check if the current partition has split
                                List<List<Bone>> currDecomposition = new List<List<Bone>>();
                                foreach (List<Bone> partition in decomposition)
                                {
                                    List<Bone> bones = new List<Bone>();
                                    int minLevel = AutomaticMapping.GetMinLengthChain(partition);
                                    foreach (Bone b in partition)
                                    {
                                        Bone bone = new Bone(b.name);
                                        bone.level = b.level - minLevel;
                                        bone.parent = b.parent;
                                        bone.children = b.children;
                                        bone.rot_DoF = b.rot_DoF;
                                        bone.loc_DoF = b.loc_DoF;

                                        bones.Add(bone);

                                        int childrenInPartition = 0;
                                        if (bone.children.Count >= 2)
                                        {
                                            foreach (string child in bone.children)
                                            {
                                                if (!partition.FindIndex(x => x.name.Equals(child)).Equals(-1))
                                                    childrenInPartition++;
                                            }
                                        }
                                        if (childrenInPartition >= 2)
                                            IsSplittedPartition = true;
                                    }
                                    currDecomposition.Add(bones);
                                }

                                // Finds the most frequent axis arrangement in the partition                        
                                int combLenght = AutomaticMapping.ReduceCombList(currDecomposition);
                                if (combLenght == 0)
                                {
                                    foreach (char[] comb in combination)
                                    {
                                        // Insertion into arrangementsHash avoids duplicate
                                        arrangementsHash.Add(Metrics.GetBestAxisArrangement
                                            (componentAvailable, dictionary, currDecomposition,
                                            comb, UseSensorCheckBox, brick));
                                    }
                                }
                                else
                                {
                                    List<char> comb = new List<char>();
                                    for (int i = 0; i < combLenght; i++)
                                    {
                                        comb.Add('x');
                                        comb.Add('y');
                                        comb.Add('z');
                                    }
                                    arrangementsHash.Add(new AxisArrangement(comb.ToArray(), 0));
                                }
                                arrangements = arrangementsHash.ToList();
                                arrangements.Sort();
                                float bestScore = arrangements[0].Score;

                                var lck = new object();

                                List<Task> tasks = new List<Task>();
                                foreach (AxisArrangement axisArr in arrangements)
                                {
                                    if (axisArr.Score <= bestScore)
                                    {
                                        tasks.Add(Task.Factory.StartNew(() =>
                                        {
                                            System.Diagnostics.Debug.WriteLine
                                                (Metrics.GetDofString(axisArr.AxisCombination.ToList())
                                                + " Start at: " + DateTime.Now);
                                            // Creates all possible armatures generated by the current axis arrangements
                                            // and subdivides armatures into two groups: sequential (sourceArmatureTemplateList[0])
                                            // and splitted (sourceArmatureTemplateList[1])
                                            List<List<List<Bone>>> sourceArmatureTemplateList =
                                                AutomaticMapping.CreateArmaturesFromComb (axisArr.AxisCombination, brick, componentAvailable,
                                                LocRotCheckBox, UseSensorCheckBox,/*IsSplittedPartition && */pref.ConsiderSplitSourceCheckBox, useHipJoint);

                                            // Each of its PartAss contains the best association 
                                            // between a partition and sequential armatures
                                            DecompositionAssignment partialDecAssign = new DecompositionAssignment
                                                (new List<PartitionAssignment>(), 0, DecompositionAssignment.SEQUENTIAL_TYPE);

                                            // Computes assignment with sequential armature
                                            for (int partitionIndex = 0; partitionIndex < currDecomposition.Count; partitionIndex++)
                                            {
                                                List<Bone> currentPartition = currDecomposition[partitionIndex];
                                                List<PartitionAssignment> partAssign = new List<PartitionAssignment>();

                                                /*string partHashCode = AutomaticMapping.ComputeArmatureHash (currentPartition) +
                                                    Metrics.GetDofString(axisArr.AxisCombination.ToList()) +
                                                    "_" + DecompositionAssignment.SEQUENTIAL_TYPE;*/

                                                bool partPreComputed = false;
                                                //lock (partitionsHash)
                                                //{                                                
                                                //    partPreComputed = partitionsHash.ContainsKey(partHashCode);
                                                //}

                                                if (!partPreComputed)
                                                {
                                                    float partAssigBestScore = float.MaxValue;
                                                    int sequentialArmIndex = 0;

                                                    //for each sequential armature altenative compute assignment
                                                    foreach (List<Bone> sequentialArm in sourceArmatureTemplateList[0])
                                                    {

                                                        sequentialArmIndex++;

                                                        // For all the sequential armatures can be used the same component arrangement, 
                                                        // so the algorithm chooses the best;                                              
                                                        // The current virtual armature contains at least the same number of bone
                                                        // otherwise the assignment is not possible
                                                        if (currentPartition.Count <= sequentialArm.Count)
                                                        {
                                                            PartitionAssignment p = ComputeAssignement(currentPartition, sequentialArm,
                                                                currDecomposition.Count, maxPartitionCount, dictionary,
                                                                currentPartition[0].name + "_LOCROT",
                                                                maxLevelBone, pref);

                                                            //DEBUG
                                                            //partAssignDebug.Add(p);
                                                            if (p.Score < partAssigBestScore && IsFeasibleSolution(p, dictionary))
                                                            {
                                                                partAssign.Add(p);
                                                                partAssigBestScore = p.Score;
                                                            }
                                                        }
                                                    }


                                                    // Searches for the best partitionAssignment feasible
                                                    partAssign.Sort();
                                                    if (partAssign.Count > 0)
                                                    {
                                                        partialDecAssign.PartitionAss.Add(partAssign[0]);
                                                        //lock (partitionsHash)
                                                        //{
                                                        //    partitionsHash.Add(partHashCode,
                                                        //        new PartitionAssignmentTmp (partAssign[0].Score,
                                                        //        (float)currDecomposition.Count / (float)maxPartitionCount,
                                                        //        partAssign[0].Handler));
                                                        //}
                                                    }
                                                    else
                                                    {
                                                        //DEBUG
                                                        Console.WriteLine("Sequential Aramture for " +
                                                            Metrics.GetDofString(axisArr.AxisCombination) +
                                                            " not feasible");
                                                        // PartitionAssignment not found
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    PartitionAssignment preComputedPartAss = new PartitionAssignment();
                                                    preComputedPartAss.Name = currentPartition[0].name + "_LOCROT_TMP";
                                                    preComputedPartAss.Partition = currentPartition;
                                                    //lock (partitionsHash)
                                                    //{
                                                    //    preComputedPartAss.Score = UpdateTmpPartitionScore(currDecomposition.Count, 
                                                    //        maxPartitionCount, partitionsHash[partHashCode], currentPartition.Count);
                                                    //    preComputedPartAss.Handler = partitionsHash[partHashCode].Handler;
                                                    //}
                                                    partialDecAssign.PartitionAss.Add(preComputedPartAss);
                                                }
                                            }

                                            // Updates decAssign
                                            if (partialDecAssign.PartitionAss.Count == currDecomposition.Count)
                                            {
                                                DecompositionAssignment decSeq = GetDecompositionAssignment
                                                    (partialDecAssign.PartitionAss, DecompositionAssignment.SEQUENTIAL_TYPE);

                                                bool decAssignToAdd = false;
                                                lock (lck)
                                                {
                                                    if (decSeq.TotalScore <= decAssignBestScore)
                                                    {
                                                        decAssignToAdd = true;
                                                        decAssignBestScore = decSeq.TotalScore;
                                                        decSeq.combination = Metrics.GetDofString(axisArr.AxisCombination);
                                                    }
                                                }
                                                if (decAssignToAdd)
                                                {
                                                    lock (decAssign)
                                                    {
                                                        decAssign.Add(decSeq);
                                                    }
                                                }
                                            }

                                            if (IsSplittedPartition && pref.ConsiderSplitSourceCheckBox)
                                            {
                                                // Computes assignment with splitted armature
                                                for (int splitArmIndex = 0;
                                                    splitArmIndex < sourceArmatureTemplateList[1].Count;
                                                    splitArmIndex++)
                                                {
                                                    splitArmAlternatives = ComputeAlternatives
                                                        (sourceArmatureTemplateList[1][splitArmIndex], componentAvailable);

                                                    partialDecAssign = new DecompositionAssignment
                                                    (new List<PartitionAssignment>(), 0, DecompositionAssignment.SPLITTED_TYPE);

                                                    // Computes assignment with all the alteratives splitted armature
                                                    for (int partitionIndex = 0; partitionIndex < currDecomposition.Count; partitionIndex++)
                                                    {
                                                        List<Bone> currentPartition = currDecomposition[partitionIndex];
                                                        List<PartitionAssignment> partAssign = new List<PartitionAssignment>();
                                                        //string partHashCode = AutomaticMapping.ComputeArmatureHash(currentPartition) +
                                                        //    Metrics.GetDofString(axisArr.AxisCombination.ToList()) +
                                                        //    "_" + DecompositionAssignment.SPLITTED_TYPE + splitArmIndex;

                                                        bool partPreComputed = false;
                                                        //lock (partitionsHash)
                                                        //{
                                                        //    partPreComputed = partitionsHash.ContainsKey(partHashCode);
                                                        //}

                                                        if (!partPreComputed)
                                                        {
                                                            float partAssigBestScore = float.MaxValue;
                                                            foreach (List<Bone> currentVirtualArmature in splitArmAlternatives)
                                                            {

                                                                // Virtual armature is able to control the current partition
                                                                if (currentPartition.Count <= currentVirtualArmature.Count)
                                                                {

                                                                    PartitionAssignment p = ComputeAssignement(
                                                                        currentPartition, currentVirtualArmature,
                                                                        currDecomposition.Count, maxPartitionCount, dictionary,
                                                                        currentPartition[0].name + "_LOCROT",
                                                                        maxLevelBone, pref);

                                                                    if (p.Score < partAssigBestScore && IsFeasibleSolution(p, dictionary))
                                                                    {
                                                                        partAssign.Add(p);
                                                                        partAssigBestScore = p.Score;
                                                                    }
                                                                }
                                                            }
                                                            partAssign.Sort();
                                                            if (partAssign.Count > 0)
                                                            {
                                                                partialDecAssign.PartitionAss.Add(partAssign[0]);
                                                                //lock (partitionsHash)
                                                                //{
                                                                //    partitionsHash.Add(partHashCode, new PartitionAssignmentTmp(
                                                                //        partAssign[0].Score,
                                                                //        (float)currDecomposition.Count / (float)maxPartitionCount,
                                                                //        partAssign[0].Handler));
                                                                //}
                                                            }
                                                            else
                                                            {
                                                                // best partitionAssignment not found
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            PartitionAssignment preComputedPartAss = new PartitionAssignment();
                                                            preComputedPartAss.Name = currentPartition[0].name +
                                                                "_LOCROT_TMP[" + splitArmIndex + "]";
                                                            preComputedPartAss.Partition = currentPartition;
                                                            //lock (partitionsHash)
                                                            //{
                                                            //    preComputedPartAss.Score = UpdateTmpPartitionScore(currDecomposition.Count,
                                                            //        maxPartitionCount, partitionsHash[partHashCode], currentPartition.Count);
                                                            //    preComputedPartAss.Handler =
                                                            //        partitionsHash[partHashCode].Handler;
                                                            //}
                                                            partialDecAssign.PartitionAss.Add(preComputedPartAss);
                                                        }
                                                    }

                                                    // Update decAssign
                                                    if (partialDecAssign.PartitionAss.Count == currDecomposition.Count)
                                                    {
                                                        DecompositionAssignment decSplit = GetDecompositionAssignment
                                                            (partialDecAssign.PartitionAss, DecompositionAssignment.SPLITTED_TYPE);
                                                        decSplit.SplittedArmature = sourceArmatureTemplateList[1][splitArmIndex];

                                                        bool decAssignToAdd = false;
                                                        lock (lck)
                                                        {
                                                            if (decSplit.TotalScore <= decAssignBestScore)
                                                            {
                                                                decAssignToAdd = true;
                                                                decAssignBestScore = decSplit.TotalScore;
                                                                decSplit.combination = Metrics.GetDofString(axisArr.AxisCombination);
                                                            }
                                                        }
                                                        if (decAssignToAdd)
                                                        {
                                                            lock (decAssign)
                                                            {
                                                                decAssign.Add(decSplit);
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            System.Diagnostics.Debug.WriteLine(Metrics.GetDofString(axisArr.AxisCombination.ToList())
                                                + " Finish at: " + DateTime.Now);
                                        }));
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                try
                                {
                                    Task.WaitAll(tasks.ToArray());
                                }
                                catch (AggregateException ae)
                                {

                                }

                            }

                            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { this.EndTime.Content = DateTime.Now; }));
                            System.Diagnostics.Debug.WriteLine("FINISH AT" + " " + DateTime.Now);

                            decAssign.Sort();
                            //for (int i = 0; i < decAssign[0].PartitionAss.Count; i++)
                            //{
                            //    PartitionAssignment p = decAssign[0].PartitionAss[i];
                            //    if (p.Name.Contains("_TMP"))
                            //    {
                            //        int maxLenghtChain = AutomaticMapping.GetMaxLengthChain(p.Partition);
                            //        int currDecCount = decAssign[0].PartitionAss.Count;
                            //        decAssign[0].PartitionAss[i] = ComputeAssignement
                            //            (p.Partition, p.Handler, currDecCount, maxPartitionCount,
                            //            dictionary, p.Name, maxLenghtChain, pref);
                            //    }
                            //}
                            data = ViewScores(decAssign, dictionary, maxLevelBone, maxPartitionCount, pref);

                            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(decAssign);
                            System.IO.File.WriteAllText(path + "\\CONFIG.json", jsonString);
                            jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                            System.IO.File.WriteAllText(path + "\\DATA.json", jsonString);
                        }

                        else
                        {
                            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                            string jsonText = System.IO.File.ReadAllText(path + "\\CONFIG.json");
                            decAssign = JsonManager.GetDecompositionAssignment(jsonText);
                            jsonText = System.IO.File.ReadAllText(path + "\\DATA.json");
                            data = JsonManager.GetComputationData(jsonText);
                        }

                        CreateLegoInstruction(decAssign[0]);

                        foreach (PartitionAssignment partAss in decAssign[0].PartitionAss)
                        {
                            CreateConfiguration(partAss, armatureName, maxLevelBone,
                                decAssign[0].PartitionAss.Count, decAssign[0].PartitionAss.Count, dictionary, pref);

                            // Creates symmetric configuration
                            if (!this.ConsiderSplitArmatureCheckBox.IsChecked.Value)
                            {
                                List<Bone> symmetricPartition = GetSymmetricPartition(partAss, symmetricBonesName);

                                if (symmetricPartition.Count > 0)
                                {
                                    PartitionAssignment symmetricPartAss = new PartitionAssignment(symmetricPartition[0].name + "_LOCROT", partAss.Assignment, symmetricPartition, partAss.Handler, partAss.Score);

                                    CreateConfiguration(symmetricPartAss, armatureName, maxLevelBone,
                                        decAssign[0].PartitionAss.Count, decAssign[0].PartitionAss.Count, dictionary, pref);
                                }

                            }

                        }

                    }
                }

                if (this.UserPreferenceSlider.Value < 0)
                {
                    List<DecompositionAssignment> decAssign = new List<DecompositionAssignment>();

                    // CREATES PARTITIONS
                    List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();
                    // TODO: Correct Parameters 
                    graphPartitions = GraphPartitioning
                        (KinectSkeleton.KINECT_SKELETON_DOF, graph, graphComponents, graphPartitions, false, -1, new int[3]);
                    maxPartitionCount = AutomaticMapping.GetMaxPartitionCount(graphPartitions);

                    List<Bone> virtualArmature = KinectSkeleton.GetKinectSkeleton();
                    foreach (List<List<Bone>> decomposition in graphPartitions)
                    {
                        List<PartitionAssignment> partAssign = new List<PartitionAssignment>();
                        foreach (List<Bone> currentPartition in decomposition)
                        {
                            partAssign.Add(ComputeAssignement
                                (currentPartition, virtualArmature, decomposition.Count, maxPartitionCount,
                                 dictionary, currentPartition[0].name + "_KINECT_CONFIG",
                                 maxLevelBone, pref));
                        }

                        decAssign.Add(GetDecompositionAssignment(partAssign, DecompositionAssignment.KINECT_TYPE));

                    }

                    decAssign.Sort();
                    foreach (PartitionAssignment partAssi in decAssign[0].PartitionAss)
                    {
                        CreateConfiguration(partAssi, armatureName, maxLevelBone,
                            decAssign[0].PartitionAss.Count, maxPartitionCount, dictionary, pref);
                    }
                }


                //SetArrangmentLabel(rotArrangements[0]);               
            }
        }

        private List<Bone> GetSymmetricPartition(PartitionAssignment partAss, List<string> symmetricBonesName)
        {
            bool partitonContatinsSplit = false;
            List<Bone> result = new List<Bone>();
            foreach (Bone b in partAss.Partition)
            {
                Bone symBone = new Bone("");
                symBone.level = b.level;
                symBone.rot_DoF = b.rot_DoF;
                symBone.loc_DoF = b.loc_DoF;
                symBone.name = b.name;
                if (symBone.name.Contains(".R"))
                {
                    symBone.name = symBone.name.Replace(".R", ".L");
                    if (symmetricBonesName.Contains(symBone.name))
                        partitonContatinsSplit = true;
                }
                if (b.parent.Contains(".R"))
                    symBone.parent = b.parent.Replace(".R", ".L");
                else
                    symBone.parent = b.parent;

                for (int i = 0; i < b.children.Count; i++)
                {
                    if (b.children[i].Contains(".R"))
                        symBone.children.Add(b.children[i].Replace(".R", ".L"));
                    else
                        symBone.children.Add(b.children[i]);
                }
                result.Add(symBone);
            }


            if (partitonContatinsSplit)
                return result;
            else
                return new List<Bone>();

        }

        private static List<string> RemoveUnusedBones(List<Bone> armature, bool considerSplitArmature)
        {
            List<string> result = new List<string>();
            for (int pos = 0; pos < armature.Count; pos++)
            {
                Bone b = armature[pos];
                if (!considerSplitArmature)
                {
                    if (b.name.Contains(".L"))
                    {
                        // Removes other tag
                        if (b.name.Contains("#NOT_CONNECTED") || b.name.Contains("#TO_REMOVE"))
                        {
                            b.name = b.name.Substring(0, b.name.IndexOf("#"));
                        }
                        result.Add(b.name);

                        // Adds a new tag to remove symmetric bone
                        b.name += "#TO_REMOVE";
                    }
                }
                if (b.name.Contains("#NOT_CONNECTED"))
                {
                    b.name = b.name.Substring(0, b.name.IndexOf("#NOT_CONNECTED"));
                    Bone parent = AutomaticMapping.GetBoneFromName(b.parent, armature);
                    if (b.loc_DoF.Count + parent.loc_DoF.Count == 0)
                    {
                        Bone bridge = new Bone("BridgeTo" + b.name);
                        bridge.loc_DoF = new List<char>();
                        bridge.rot_DoF = new List<char>();
                        bridge.parent = parent.name;
                        bridge.children = new List<string>() { b.name };
                        armature.Insert(armature.FindIndex(x => x.name.Equals(b.name)), bridge);
                        b.parent = bridge.name;
                        parent.children.Add(bridge.name);
                        parent.children.Remove(b.name);
                    }
                }
                if (b.name.Contains("#TO_REMOVE"))
                {
                    b.name = b.name.Substring(0, b.name.IndexOf("#TO_REMOVE"));

                    if (!b.parent.Equals("") && AutomaticMapping.GetBoneFromName(b.parent, armature) != null)
                    {
                        AutomaticMapping.GetBoneFromName(b.parent, armature).children.Remove(b.name);
                    }
                    foreach (string child in b.children)
                    {
                        if (AutomaticMapping.GetBoneFromName(child, armature) != null)
                            AutomaticMapping.GetBoneFromName(child, armature).parent = "";
                    }
                    armature.RemoveAt(pos);
                    pos--;
                }
            }
            return result;
            //// Identify and split aramtures components
            //List<int> bonesToDelete = new List<int>();
            //for(int pos = 0; pos < armature.Count; pos++)                
            //{
            //    Bone b = armature[pos];
            //    if (b.rot_DoF.Count + b.loc_DoF.Count == 0)
            //    {                    
            //        // Updates
            //        if (!b.parent.Equals(""))
            //        {
            //            AutomaticMapping.GetBoneFromName(b.parent, armature).children.Remove(b.name);
            //        }
            //        foreach (string child in b.children)
            //        {
            //            AutomaticMapping.GetBoneFromName(child, armature).parent = "";
            //        }
            //        armature.RemoveAt(pos);
            //        pos--;
            //    }                
            //}
        }

        private static float UpdateTmpPartitionScore(int decompositionCount, int maxPartitionCount, PartitionAssignmentTmp suorcePartition, int currentPartitionCount)
        {
            float tmpScore = suorcePartition.Score;
            float oldPartFact = suorcePartition.PartCountFact;
            float newPartFact = (float)decompositionCount / (float)maxPartitionCount;

            return tmpScore + (newPartFact - oldPartFact) * Metrics.MAX_COST * currentPartitionCount;
        }

        private bool IsFeasibleSolution(DecompositionAssignment decAssign, Dictionary<string, List<List<char>>> dictionary)
        {
            foreach (PartitionAssignment p in decAssign.PartitionAss)
            {
                for (int index = 0; index < p.Assignment.Length; index++)
                {
                    Bone bone = p.Partition[index];
                    Bone handler = p.Handler[p.Assignment[index]];

                    if (bone.rot_DoF.Count + bone.loc_DoF.Count > handler.rot_DoF.Count + handler.loc_DoF.Count)
                        return false;
                    //if (bone.rot_DoF.Count == 3)
                    //{
                    //    if (dictionary[Metrics.GetDofString(bone.rot_DoF)].FindIndex(x => x.SequenceEqual(handler.rot_DoF)) == -1)
                    //        //if (!dictionary[Metrics.GetDofString(bone.rot_DoF)].Contains(handler.rot_DoF))
                    //        return false;
                    //}
                }
            }
            return true;
        }

        private bool IsFeasibleSolution(PartitionAssignment p, Dictionary<string, List<List<char>>> dictionary)
        {
            for (int index = 0; index < p.Assignment.Length; index++)
            {
                Bone bone = p.Partition[index];
                Bone handler = p.Handler[p.Assignment[index]];

                if (bone.rot_DoF.Count + bone.loc_DoF.Count > handler.rot_DoF.Count + handler.loc_DoF.Count)
                    return false;
                //if (bone.rot_DoF.Count == 3)
                //{
                //    if (dictionary[Metrics.GetDofString(bone.rot_DoF)].FindIndex(x => x.SequenceEqual(handler.rot_DoF)) == -1)
                //        //if (!dictionary[Metrics.GetDofString(bone.rot_DoF)].Contains(handler.rot_DoF))
                //        return false;
                //}
            }
            return true;
        }

        public List<List<List<Bone>>> GraphPartitioning(int motors, BidirectionalGraph<Bone, Edge<Bone>> graph, List<List<Bone>> components, List<List<List<Bone>>> graphPartitions, bool isLocRotPartition, int minBoneLeght, int[] maxBoneDofCounts)
        {

            foreach (List<Bone> armatureComponent in components)
            {
                // This list is called partial because contains 
                // only the partition for a specific connected component    

                List<List<List<Bone>>> partialGraphPartitions = new List<List<List<Bone>>>();
                try
                {
                    if (isLocRotPartition)
                    {
                        partialGraphPartitions = PartitionConnectedComp_LOCROT
                            (armatureComponent, graph, motors, isLocRotPartition, minBoneLeght, maxBoneDofCounts[0]);
                    }
                    else
                    {
                        partialGraphPartitions = PartitionConnectedComp_ROT
                            (armatureComponent, graph, motors, isLocRotPartition);
                    }
                }
                catch (ApplicationException ex)
                {
                    throw ex;
                }

                // Item is not the first element
                if (graphPartitions.Count > 0)
                {
                    // Combines partitions
                    int lastItemIndex = graphPartitions.Count;

                    for (int i = 0; i < lastItemIndex; i++)
                    {
                        foreach (List<List<Bone>> partialPartition in partialGraphPartitions)
                        {
                            graphPartitions.Add(graphPartitions[i].Concat(partialPartition).ToList());
                        }
                    }
                    graphPartitions.RemoveRange(0, lastItemIndex);
                }
                else
                {
                    // Inserts the first item
                    graphPartitions = partialGraphPartitions;
                }
            }

            // <-- Adds the reference partition index 

            return graphPartitions;
        }

        private static List<List<List<Bone>>> PartitionConnectedComp_ROT(List<Bone> armature, BidirectionalGraph<Bone, Edge<Bone>> graph, int motors, bool isRotOnly)
        {

            List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();
            if (AutomaticMapping.PartitionCapacityOverflow(armature, motors))
            {
                throw new ApplicationException();
            }

            int limit = 3;
            for (; motors >= limit; motors--)
            {
                foreach (Bone startBone in armature)
                {
                    List<GraphTraversal> graphTraversalList = new List<GraphTraversal>();

                    GraphTraversal graphTraversal = new GraphTraversal(motors);
                    var dfs = new QuickGraph.Algorithms.Search.DepthFirstSearchAlgorithm<Bone, Edge<Bone>>(graph);
                    dfs.DiscoverVertex += new VertexAction<Bone>(graphTraversal.dfs_DiscoverVertex_MaxRotDoF);
                    dfs.Compute(startBone);
                    graphTraversalList.Add(graphTraversal);

                    while (graphTraversalList.Count > 0)
                    {
                        GraphTraversal currGraphTrav = graphTraversalList[0];

                        while (currGraphTrav.BonesToVisit.Count > 0)
                        {
                            Bone currentBone = currGraphTrav.BonesToVisit[0];

                            if (currentBone.children.Count > 1)
                            {
                                // The bone is a split

                                // Checks if the current partition can contain this bone:
                                //1. There are not enough available motors
                                if ((currGraphTrav.MotorAvailable - currentBone.rot_DoF.Count < 0) ||
                                    // 2. The new bone is not connected
                                    !AutomaticMapping.IsConnectedBone(currGraphTrav.Partition, currentBone) ||
                                    // symmetric split check
                                    (currGraphTrav.Partition.Count > 0 &&
                                        currGraphTrav.Partition[currGraphTrav.Partition.Count - 1].name.Contains(".R")) ||
                                    (currGraphTrav.Partition.Count > 0 &&
                                        currGraphTrav.Partition[currGraphTrav.Partition.Count - 1].name.Contains(".L")))
                                {
                                    // Terminates the inclusion into the current partition
                                    currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
                                    currGraphTrav.Partition = new List<Bone>();
                                    currGraphTrav.MotorAvailable = motors;
                                }
                                else
                                {
                                    currGraphTrav.Partition.Add(currentBone);
                                    currGraphTrav.BonesToVisit.RemoveAt(0);
                                    currGraphTrav.MotorAvailable -= currentBone.rot_DoF.Count;
                                    bool currGraphTravEdited = false;

                                    // Explore neighborhood:

                                    // 1. Depth-First 
                                    List<List<Bone>> alternativePaths = AutomaticMapping.ChildrenWithDepthSearch
                                        (currentBone, currGraphTrav.BonesToVisit, currGraphTrav.MotorAvailable, graph);
                                    if (alternativePaths.Count > 0)
                                    {
                                        currGraphTravEdited = true;
                                        foreach (List<Bone> path in alternativePaths)
                                        {
                                            // Copies the old GraphTraversal values 
                                            GraphTraversal newGraphTr = new GraphTraversal(currGraphTrav.MotorAvailable);
                                            newGraphTr.BonesToVisit = currGraphTrav.BonesToVisit.ToList();
                                            newGraphTr.Decomposition = currGraphTrav.Decomposition.ToList();
                                            newGraphTr.Partition = currGraphTrav.Partition.ToList();

                                            // Updates new GraphTraversal object, adding new bone visited
                                            foreach (Bone childToAdd in path)
                                            {
                                                newGraphTr.Partition.Add(childToAdd);
                                                newGraphTr.BonesToVisit.Remove(childToAdd);
                                                newGraphTr.MotorAvailable -= childToAdd.rot_DoF.Count;
                                            }

                                            newGraphTr.Decomposition.Add(newGraphTr.Partition);
                                            newGraphTr.Partition = new List<Bone>();
                                            newGraphTr.MotorAvailable = motors;

                                            graphTraversalList.Add(newGraphTr);
                                        }
                                    }

                                    // 2. Breadth-first
                                    List<Bone> neighborsToAdd = AutomaticMapping.ChildrenWithBreadthFirst
                                        (currentBone, currGraphTrav.BonesToVisit, currGraphTrav.MotorAvailable, graph);

                                    if (neighborsToAdd.Count > 1)
                                    {
                                        currGraphTravEdited = true;

                                        // Copies the old GraphTraversal values 
                                        GraphTraversal newGraphTr = new GraphTraversal(currGraphTrav.MotorAvailable);
                                        newGraphTr.BonesToVisit = currGraphTrav.BonesToVisit.ToList();
                                        newGraphTr.Decomposition = currGraphTrav.Decomposition.ToList();
                                        newGraphTr.Partition = currGraphTrav.Partition.ToList();

                                        foreach (Bone childToAdd in neighborsToAdd)
                                        {
                                            newGraphTr.Partition.Add(childToAdd);
                                            newGraphTr.BonesToVisit.Remove(childToAdd);
                                            newGraphTr.MotorAvailable -= childToAdd.rot_DoF.Count;
                                        }

                                        newGraphTr.Decomposition.Add(newGraphTr.Partition);
                                        newGraphTr.Partition = new List<Bone>();
                                        newGraphTr.MotorAvailable = motors;

                                        graphTraversalList.Add(newGraphTr);
                                    }


                                    if (currGraphTravEdited)
                                    {
                                        graphTraversalList.Remove(currGraphTrav);
                                        currGraphTrav = graphTraversalList[0];
                                    }
                                    else
                                    {
                                        currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
                                        currGraphTrav.Partition = new List<Bone>();
                                        currGraphTrav.MotorAvailable = motors;
                                    }
                                }
                            }
                            else
                            {
                                // Current bone is a sequential bone
                                AutomaticMapping.UpdatePartition(motors, currGraphTrav.Decomposition,
                                    ref currGraphTrav.MotorAvailable,
                                    ref currGraphTrav.Partition, currentBone);
                                currGraphTrav.BonesToVisit.RemoveAt(0);
                            }
                        }

                        if (currGraphTrav.Partition.Count > 0)
                        {
                            // Adds last partition
                            currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
                        }

                        graphPartitions.Add(currGraphTrav.Decomposition.ToList());
                        currGraphTrav.Decomposition = new List<List<Bone>>();
                        graphTraversalList.RemoveAt(0);
                    }

                }
            }

            // Remove decompositions from graphPartitions that propose the same partitioning
            for (int i = 0; i < graphPartitions.Count; i++)
            {
                for (int j = i + 1; j < graphPartitions.Count; j++)
                {
                    if (AutomaticMapping.IsEqualDecomposition(graphPartitions[i], graphPartitions[j]))
                    {
                        graphPartitions.RemoveAt(j);
                        j--;
                    }
                }
            }

            return graphPartitions;

        }

        //private static List<List<List<Bone>>> PartitionConnectedComp_LOCROT(List<Bone> armature, BidirectionalGraph<Bone, Edge<Bone>> graph, int motors, bool isRotOnly)
        //{

        //    List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();


        //    if (AutomaticMapping.PartitionCapacityOverflow_LOCROT(armature, motors))
        //    {
        //        throw new ApplicationException();
        //    }

        //    foreach (Bone startBone in armature)
        //    {
        //        List<GraphTraversal> graphTraversalList = new List<GraphTraversal>();

        //        GraphTraversal graphTraversal = new GraphTraversal(motors);
        //        var dfs = new QuickGraph.Algorithms.Search.DepthFirstSearchAlgorithm<Bone, Edge<Bone>>(graph);
        //        dfs.DiscoverVertex += new VertexAction<Bone>(graphTraversal.dfs_DiscoverVertex_MaxLocRotDoF);
        //        dfs.Compute(startBone);
        //        graphTraversalList.Add(graphTraversal);

        //        while (graphTraversalList.Count > 0)
        //        {
        //            GraphTraversal currGraphTrav = graphTraversalList[0];

        //            while (currGraphTrav.BonesToVisit.Count > 0)
        //            {
        //                Bone currentBone = currGraphTrav.BonesToVisit[0];

        //                if (currentBone.children.Count > 1)
        //                {
        //                    // The bone is a split

        //                    // Checks if the current partition can contain this bone:
        //                    // 1. there are not enough available motors
        //                    if ((currGraphTrav.MotorAvailable - currentBone.rot_DoF.Count - currentBone.loc_DoF.Count < 0) ||
        //                        // 2. the new bone is not connected
        //                        !AutomaticMapping.IsConnectedBone(currGraphTrav.Partition, currentBone) || 
        //                        // 3. symmetric split check
        //                        (currGraphTrav.Partition.Count > 0 &&
        //                            currGraphTrav.Partition[currGraphTrav.Partition.Count - 1].name.Contains(".R")) ||
        //                        (currGraphTrav.Partition.Count > 0 &&
        //                            currGraphTrav.Partition[currGraphTrav.Partition.Count - 1].name.Contains(".L")))
        //                    {
        //                        // Terminates the inclusion into the current partition
        //                        currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
        //                        currGraphTrav.Partition = new List<Bone>();
        //                        currGraphTrav.MotorAvailable = motors;
        //                    }
        //                    else
        //                    {
        //                        currGraphTrav.Partition.Add(currentBone);
        //                        currGraphTrav.BonesToVisit.RemoveAt(0);
        //                        currGraphTrav.MotorAvailable -= currentBone.rot_DoF.Count + currentBone.loc_DoF.Count;
        //                        bool currGraphTravEdited = false;

        //                        // Explore neighborhood:

        //                        // 1. Depth-First 
        //                        List<List<Bone>> alternativePaths = AutomaticMapping.ChildrenWithDepthSearch
        //                            (currentBone, currGraphTrav.BonesToVisit, currGraphTrav.MotorAvailable, graph);
        //                        if (alternativePaths.Count > 0)
        //                        {
        //                            currGraphTravEdited = true;
        //                            foreach (List<Bone> path in alternativePaths)
        //                            {
        //                                // Copies the old GraphTraversal values 
        //                                GraphTraversal newGraphTr = new GraphTraversal(currGraphTrav.MotorAvailable);
        //                                newGraphTr.BonesToVisit = currGraphTrav.BonesToVisit.ToList();
        //                                newGraphTr.Decomposition = currGraphTrav.Decomposition.ToList();
        //                                newGraphTr.Partition = currGraphTrav.Partition.ToList();

        //                                // Updates new GraphTraversal object, adding new bone visited
        //                                foreach (Bone childToAdd in path)
        //                                {
        //                                    newGraphTr.Partition.Add(childToAdd);
        //                                    newGraphTr.BonesToVisit.Remove(childToAdd);
        //                                    newGraphTr.MotorAvailable -= childToAdd.rot_DoF.Count;
        //                                }

        //                                newGraphTr.Decomposition.Add(newGraphTr.Partition);
        //                                newGraphTr.Partition = new List<Bone>();
        //                                newGraphTr.MotorAvailable = motors;

        //                                graphTraversalList.Add(newGraphTr);
        //                            }
        //                        }

        //                        // 2. Breadth-first
        //                        List<Bone> neighborsToAdd = AutomaticMapping.ChildrenWithBreadthFirst
        //                            (currentBone, currGraphTrav.BonesToVisit, currGraphTrav.MotorAvailable, graph);

        //                        if (neighborsToAdd.Count > 1)
        //                        {
        //                            currGraphTravEdited = true;

        //                            // Copies the old GraphTraversal values 
        //                            GraphTraversal newGraphTr = new GraphTraversal(currGraphTrav.MotorAvailable);
        //                            newGraphTr.BonesToVisit = currGraphTrav.BonesToVisit.ToList();
        //                            newGraphTr.Decomposition = currGraphTrav.Decomposition.ToList();
        //                            newGraphTr.Partition = currGraphTrav.Partition.ToList();

        //                            foreach (Bone childToAdd in neighborsToAdd)
        //                            {
        //                                newGraphTr.Partition.Add(childToAdd);
        //                                newGraphTr.BonesToVisit.Remove(childToAdd);
        //                                newGraphTr.MotorAvailable -= childToAdd.rot_DoF.Count;
        //                            }

        //                            newGraphTr.Decomposition.Add(newGraphTr.Partition);
        //                            newGraphTr.Partition = new List<Bone>();
        //                            newGraphTr.MotorAvailable = motors;

        //                            graphTraversalList.Add(newGraphTr);
        //                        }


        //                        if (currGraphTravEdited)
        //                        {
        //                            graphTraversalList.Remove(currGraphTrav);
        //                            currGraphTrav = graphTraversalList[0];
        //                        }
        //                        else
        //                        {
        //                            currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
        //                            currGraphTrav.Partition = new List<Bone>();
        //                            currGraphTrav.MotorAvailable = motors;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    // Current bone is a sequential bone
        //                    AutomaticMapping.UpdatePartition_LOCROT(motors, currGraphTrav.Decomposition,
        //                        ref currGraphTrav.MotorAvailable,
        //                        ref currGraphTrav.Partition, currentBone);
        //                    currGraphTrav.BonesToVisit.RemoveAt(0);
        //                }
        //            }

        //            if (currGraphTrav.Partition.Count > 0)
        //            {
        //                // Adds last partition
        //                currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
        //            }

        //            graphPartitions.Add(currGraphTrav.Decomposition.ToList());
        //            currGraphTrav.Decomposition = new List<List<Bone>>();
        //            graphTraversalList.RemoveAt(0);
        //        }

        //    }

        //    // Remove decompositions from graphPartitions that propose the same partitioning
        //    for (int i = 0; i < graphPartitions.Count; i++)
        //    {
        //        for (int j = i + 1; j < graphPartitions.Count; j++)
        //        {
        //            if (AutomaticMapping.IsEqualDecomposition(graphPartitions[i], graphPartitions[j]))
        //            {
        //                graphPartitions.RemoveAt(j);
        //                j--;
        //            }
        //        }
        //    }

        //    return graphPartitions;

        //}

        private static List<List<List<Bone>>> PartitionConnectedComp_LOCROT(List<Bone> armature, BidirectionalGraph<Bone, Edge<Bone>> graph, int motors, bool isRotOnly, int minBoneLeght, int maxDoFCount)
        {
            List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();
            if (AutomaticMapping.PartitionCapacityOverflow_LOCROT(armature, motors))
            {
                throw new ApplicationException();
            }

            if (maxDoFCount == -1)
            {
                maxDoFCount = motors;
            }
            if (minBoneLeght == -1)
            {
                minBoneLeght = armature.Count;
            }

            foreach (Bone startBone in armature)
            {
                for (int limitDofCount = motors; limitDofCount >= maxDoFCount; limitDofCount--)
                {
                    // max legnht chain
                    for (int limitBoneCount = armature.Count; limitBoneCount >= minBoneLeght; limitBoneCount--)
                    {
                        List<GraphTraversal> graphTraversalList = new List<GraphTraversal>();

                        GraphTraversal graphTraversal = new GraphTraversal(limitDofCount);
                        var dfs = new QuickGraph.Algorithms.Search.DepthFirstSearchAlgorithm<Bone, Edge<Bone>>(graph);
                        dfs.DiscoverVertex += new VertexAction<Bone>(graphTraversal.dfs_DiscoverVertex_MaxLocRotDoF);
                        dfs.Compute(startBone);
                        graphTraversalList.Add(graphTraversal);

                        while (graphTraversalList.Count > 0)
                        {
                            GraphTraversal currGraphTrav = graphTraversalList[0];

                            while (currGraphTrav.BonesToVisit.Count > 0)
                            {
                                Bone currentBone = currGraphTrav.BonesToVisit[0];

                                // Current bone is a split
                                if (currentBone.children.Count > 1)
                                {
                                    // Checks if the current partition can contain this bone:
                                    // 1. There are not enough available motors
                                    if ((currGraphTrav.MotorAvailable - (currentBone.rot_DoF.Count + currentBone.loc_DoF.Count) < 0) ||
                                        // 2. Limit of bone in the chain is reached
                                        currGraphTrav.Partition.Count >= limitBoneCount ||
                                        // 3. The new bone is not connected
                                        !AutomaticMapping.IsConnectedBone(currGraphTrav.Partition, currentBone) ||
                                        // 4. Symmetric split check
                                        (currGraphTrav.Partition.Count > 0 &&
                                            currGraphTrav.Partition[currGraphTrav.Partition.Count - 1].name.Contains(".R")) ||
                                        (currGraphTrav.Partition.Count > 0 &&
                                            currGraphTrav.Partition[currGraphTrav.Partition.Count - 1].name.Contains(".L")))
                                    {
                                        // Terminates the inclusion into the current partition keeping the currentBone not visited
                                        currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
                                        currGraphTrav.Partition = new List<Bone>();
                                        currGraphTrav.MotorAvailable = limitDofCount;
                                    }
                                    else
                                    {
                                        currGraphTrav.Partition.Add(currentBone);
                                        currGraphTrav.BonesToVisit.RemoveAt(0);
                                        currGraphTrav.MotorAvailable -= currentBone.rot_DoF.Count + currentBone.loc_DoF.Count;
                                        bool currGraphTravEdited = false;

                                        // Explore neighborhood:

                                        // 1. Depth-First 
                                        List<List<Bone>> alternativePaths = AutomaticMapping.ChildrenWithDepthSearch_LOCROT
                                            (currentBone, currGraphTrav.BonesToVisit, currGraphTrav.MotorAvailable,
                                            graph, currGraphTrav.Partition.Count, limitBoneCount);

                                        if (alternativePaths.Count > 0)
                                        {
                                            currGraphTravEdited = true;
                                            foreach (List<Bone> path in alternativePaths)
                                            {
                                                AddNewGraphTraversal(limitDofCount, graphTraversalList, currGraphTrav, path);
                                            }
                                        }

                                        // 2. Breadth-first
                                        List<Bone> neighborsToAdd = AutomaticMapping.ChildrenWithBreadthFirst_LOCROT
                                            (currentBone, currGraphTrav.BonesToVisit, currGraphTrav.MotorAvailable, graph,
                                             currGraphTrav.Partition.Count, limitBoneCount);

                                        if (neighborsToAdd.Count > 1)
                                        {
                                            currGraphTravEdited = true;
                                            AddNewGraphTraversal
                                                (limitDofCount, graphTraversalList, currGraphTrav, neighborsToAdd);
                                        }


                                        if (currGraphTravEdited)
                                        {
                                            // Adds the version with only the parent of the split without children
                                            AddNewGraphTraversal
                                                (limitDofCount, graphTraversalList, currGraphTrav, new List<Bone>());
                                            // Removes the graphTrav not updated
                                            graphTraversalList.Remove(currGraphTrav);
                                            currGraphTrav = graphTraversalList[0];
                                        }
                                        else
                                        {
                                            // continues the analysis with the current grapTrav, 
                                            // because alternatives were not found
                                            currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
                                            currGraphTrav.Partition = new List<Bone>();
                                            currGraphTrav.MotorAvailable = limitDofCount;
                                        }
                                    }
                                }
                                else
                                {
                                    // Current bone is a sequential bone
                                    AutomaticMapping.UpdatePartition_LOCROT(limitDofCount, currGraphTrav.Decomposition,
                                        ref currGraphTrav.MotorAvailable,
                                        ref currGraphTrav.Partition, currentBone, limitBoneCount);
                                    currGraphTrav.BonesToVisit.RemoveAt(0);
                                }
                            }

                            if (currGraphTrav.Partition.Count > 0)
                            {
                                // Adds last partition
                                currGraphTrav.Decomposition.Add(currGraphTrav.Partition);
                            }

                            graphPartitions.Add(currGraphTrav.Decomposition.ToList());
                            currGraphTrav.Decomposition = new List<List<Bone>>();
                            graphTraversalList.RemoveAt(0);
                        }
                        // Create partition with a different limit lenght chain                        
                    }
                    // Create partition with a different limit of dof                    
                }
                // Change start bone
                RemoveDuplicatedPartitioning(graphPartitions);
            }

            RemoveDuplicatedPartitioning(graphPartitions);

            return graphPartitions;

        }

        private static void AddNewGraphTraversal(int limitDofCount, List<GraphTraversal> graphTraversalList, GraphTraversal currGraphTrav, List<Bone> BoneVisited)
        {
            // Copies the old GraphTraversal values 
            GraphTraversal newGraphTr = new GraphTraversal(currGraphTrav.MotorAvailable);
            newGraphTr.BonesToVisit = currGraphTrav.BonesToVisit.ToList();
            newGraphTr.Decomposition = currGraphTrav.Decomposition.ToList();
            newGraphTr.Partition = currGraphTrav.Partition.ToList();
            // Updates new GraphTraversal object, adding new bone visited
            foreach (Bone childToAdd in BoneVisited)
            {
                newGraphTr.Partition.Add(childToAdd);
                newGraphTr.BonesToVisit.Remove(childToAdd);
                newGraphTr.MotorAvailable -=
                    childToAdd.rot_DoF.Count + childToAdd.loc_DoF.Count;
            }
            newGraphTr.Decomposition.Add(newGraphTr.Partition);
            newGraphTr.Partition = new List<Bone>();
            newGraphTr.MotorAvailable = limitDofCount;
            graphTraversalList.Add(newGraphTr);
        }

        private static void RemoveDuplicatedPartitioning(List<List<List<Bone>>> graphPartitions)
        {
            // Remove decompositions from graphPartitions that propose the same partitioning
            for (int i = 0; i < graphPartitions.Count; i++)
            {
                for (int j = i + 1; j < graphPartitions.Count; j++)
                {
                    if (AutomaticMapping.IsEqualDecomposition(graphPartitions[i], graphPartitions[j]))
                    {
                        graphPartitions.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        private List<List<Bone>> ComputeAlternatives(List<Bone> currentVirtualArmature, int componentAvailable)
        {
            List<List<Bone>> result = new List<List<Bone>>();
            List<string> hashedResult = new List<string>();

            foreach (List<Bone> arm in ArmatureAssignSymmetry(currentVirtualArmature))
                result.Add(arm);

            //  Partitioning of the currentVirtualArmature
            var graph = AutomaticMapping.CreateDirectedGraph(currentVirtualArmature);
            // Obtaints the graph connected component 
            List<List<Bone>> graphComponents = AutomaticMapping.GetConnectedComponentList(graph);
            List<List<List<Bone>>> graphPartitions = new List<List<List<Bone>>>();
            graphPartitions = GraphPartitioning
                (componentAvailable, graph, graphComponents, graphPartitions, true, -1, new int[] { -1, -1, -1 });

            List<List<List<Bone>>> partitionsToCombine = new List<List<List<Bone>>>();
            List<List<Bone>> currentPartAlternatives = new List<List<Bone>>();
            List<List<Bone>> partitionAlternative = new List<List<Bone>>();

            bool decContainsSplitted;
            foreach (List<List<Bone>> decomposition in graphPartitions)
            {
                decContainsSplitted = false;
                currentPartAlternatives.Clear();

                for (int i = 0; i < decomposition.Count; i++)
                {
                    List<Bone> partition = UpdateChildrenCount(decomposition[i]);

                    decContainsSplitted = IsSplittedArmature(partition);
                    if (!decContainsSplitted)
                    {
                        List<string> partitionSequence_Dof = new List<string>();
                        List<Bone> partitionSigle_Component = new List<Bone>();
                        foreach (Bone b in partition)
                        {
                            if (b.name.Contains(" | "))
                            {
                                foreach (Bone component in Metrics.DecomposeHandler(b))
                                    partitionSigle_Component.Add(component);
                            }
                            else
                            {
                                partitionSigle_Component.Add(b);
                            }
                        }
                        foreach (Bone comp in partitionSigle_Component)
                        {
                            partitionSequence_Dof.Add(comp.name);
                        }

                        List<char> comb = AutomaticMapping.GetDofSequenceFromPartition(partitionSigle_Component);
                        partitionAlternative = AutomaticMapping.CreateArmature
                            (new List<List<char>> { comb }, new List<List<string>>() { partitionSequence_Dof })[0];
                        partitionsToCombine.Add(partitionAlternative.ToList());
                    }

                    else
                    {
                        break;
                    }
                }

                if (!decContainsSplitted)
                {
                    List<List<Bone>> partialArmature = new List<List<Bone>>();

                    foreach (List<List<Bone>> ptc in partitionsToCombine)
                    {
                        if (partialArmature.Count == 0)
                        {
                            partialArmature = ptc.ToList();
                        }
                        else
                        {
                            int iteration = partialArmature.Count;
                            for (int k = 0; k < iteration; k++)
                            {
                                foreach (List<Bone> lb in ptc)
                                {
                                    List<Bone> armatureToAdd = partialArmature[0].ToList();
                                    // Combines the two lists
                                    foreach (Bone b in lb)
                                    {
                                        armatureToAdd.Add(b);
                                    }
                                    partialArmature.Add(armatureToAdd);
                                }
                                partialArmature.RemoveAt(0);
                            }
                        }
                    }

                    //Adds new armature computed to the result
                    foreach (List<Bone> arm in partialArmature)
                    {
                        result.Add(arm.ToList());
                    }
                    partialArmature.Clear();
                }
                partitionsToCombine.Clear();

            }

            return result;
        }

        public List<List<Bone>> ArmatureAssignSymmetry(List<Bone> currentVirtualArmature)
        {
            List<char> symType = new List<char>() { 'L', 'R', '0' };
            List<List<Bone>> splittedArm = new List<List<Bone>>();


            // Finds split at minimum level
            int lowestLevel = int.MaxValue;
            Bone split = new Bone("");
            foreach (Bone b in currentVirtualArmature)
            {
                if (b.children.Count > 1)
                {
                    if (b.level == lowestLevel) { return splittedArm; }
                    if (b.level < lowestLevel) { split = b; lowestLevel = b.level; }
                }
            }

            List<List<char>> symAssignedToChild = new List<List<char>>();
            for (int i = 0; i < split.children.Count; i++)
            {
                if (symAssignedToChild.Count == 0)
                {
                    foreach (char c in symType)
                    {
                        symAssignedToChild.Add(new List<char>() { c });
                    }
                }
                else
                {
                    int iteration = symAssignedToChild.Count;
                    for (int j = 0; j < iteration; j++)
                    {
                        foreach (char c in symType)
                        {
                            List<char> p = symAssignedToChild[0].ToList();
                            p.Add(c);
                            symAssignedToChild.Add(p);
                        }

                        symAssignedToChild.RemoveAt(0);
                    }
                }
            }

            for (int i = 0; i < symAssignedToChild.Count; i++)
            {
                if (IsSymmetricSplit(symAssignedToChild[i]))
                {
                    List<Bone> symmetricArm = new List<Bone>();

                    foreach (Bone b in currentVirtualArmature)
                    {
                        Bone newBone = new Bone(b.name);
                        newBone.level = b.level;
                        newBone.children = b.children.ToList();
                        newBone.parent = b.parent;
                        newBone.loc_DoF = b.loc_DoF.ToList();
                        newBone.rot_DoF = b.rot_DoF.ToList();
                        symmetricArm.Add(newBone);
                    }

                    for (int childIndex = 0; childIndex < split.children.Count; childIndex++)
                    {
                        Bone child = symmetricArm.Find(x => x.name.Equals(split.children[childIndex]));
                        BoneAssignSymmetry(symmetricArm, child, symAssignedToChild[i][childIndex]);
                    }

                    for (int k = 0; k < symmetricArm.Count; k++)
                    {
                        Bone currBone = symmetricArm[k];

                        if (!currBone.parent.Equals(""))
                        {
                            Bone parent = symmetricArm.Find(x => x.name.Contains(currBone.parent));
                            currBone.parent = parent.name;

                        }
                        for (int j = 0; j < currBone.children.Count; j++)
                        {
                            // Finds the new name in the symmetric aramture
                            Bone child = symmetricArm.Find(x => x.name.Contains(currBone.children[j]));
                            currBone.children[j] = child.name;
                        }
                    }

                    splittedArm.Add(symmetricArm);
                }

            }

            return splittedArm;

        }

        private void BoneAssignSymmetry(List<Bone> symmetricArm, Bone boneToUpdate, char symmetry)
        {
            if (symmetry.Equals('0'))
                return;

            boneToUpdate.name += "." + symmetry;
            foreach (string child in boneToUpdate.children)
            {
                BoneAssignSymmetry(symmetricArm, symmetricArm.Find(x => x.name.Equals(child)), symmetry);
            }

        }

        private bool IsSymmetricSplit(List<char> list)
        {
            int lBones = 0;
            int rBones = 0;
            foreach (char c in list)
            {
                if (c.Equals('L')) { lBones++; continue; }
                if (c.Equals('R')) { rBones++; continue; }
            }
            return lBones == rBones;
        }

        private List<Bone> UpdateChildrenCount(List<Bone> partition)
        {
            List<Bone> part = new List<Bone>();

            foreach (Bone b in partition)
            {
                Bone boneToUpdate = new Bone(b.name);
                boneToUpdate.level = b.level;
                boneToUpdate.loc_DoF = b.loc_DoF.ToList();
                boneToUpdate.rot_DoF = b.rot_DoF.ToList();
                int index;
                for (int i = 0; i < b.children.Count; i++)
                {
                    string child = b.children[i];
                    index = partition.FindIndex(x => x.name.Equals(child));
                    if (index >= 0)
                    {
                        boneToUpdate.children.Add(child);
                    }
                }

                index = partition.FindIndex(x => x.name.Equals(b.parent));
                if (index >= 0)
                    boneToUpdate.parent = b.parent;

                part.Add(boneToUpdate);
            }
            return part;
        }

        public bool DofCountTest(List<Bone> armature, bool useSensor, int componentAvailable)
        {

            if (!this.LocRotCheckBox.IsChecked.Value)
            {
                List<Bone> rotBones = GetRotBones(armature);
                List<Bone> locBones = GetLocBones(armature);

                int rotComponent = AutomaticMapping.CountComponentAvailable(new List<string>() { "LMotor", "MMotor" }, brick);
                if (useSensor)
                {
                    rotComponent += AutomaticMapping.CountComponentAvailable(new List<string> { "Gyroscope" }, brick);
                }

                // increase numbers of componentes in order to consider Hip and the Ultrasonic sensors
                int locComponent = rotComponent - AutomaticMapping.CountArmatureDofs(rotBones) +
                    AutomaticMapping.CountComponentAvailable(new List<string>() { "Ultrasonic" }, brick) + 3;

                if (AutomaticMapping.CountArmatureDofs(rotBones) > rotComponent)
                    return false;

                if (AutomaticMapping.CountArmatureDofs(locBones) > locComponent)
                    return false;

                return true;
            }
            else
            {
                if (useSensor)
                    componentAvailable -= 2;
                if (AutomaticMapping.CountArmatureDofs(armature) > componentAvailable)
                    return false;

                return true;
            }

        }

        private bool IsSplittedArmature(List<Bone> armature)
        {
            bool splitterFound = false;
            foreach (Bone b in armature)
            {
                if (b.children.Count > 1)
                {
                    splitterFound = true;
                    break;
                }

            }

            return splitterFound;
        }

        public List<Bone> GetRotBones(List<Bone> armature)
        {
            List<Bone> result = new List<Bone>();
            int minLevelBone = AutomaticMapping.GetMinLengthChain(armature);
            foreach (Bone b in armature)
            {
                if (b.rot_DoF.Count > 0)
                {
                    Bone boneToAdd = new Bone(b.name);
                    boneToAdd.rot_DoF = b.rot_DoF.ToList();
                    boneToAdd.level = b.level - minLevelBone;
                    if (!b.parent.Equals("") && armature.FindIndex(x => x.name.Equals(b.parent)) != -1)
                        boneToAdd.parent = b.parent;
                    else
                        boneToAdd.parent = "";
                    foreach (string child in b.children)
                    {
                        if (armature.Contains(AutomaticMapping.GetBoneFromName(child, armature)))
                        {
                            boneToAdd.children.Add(child);
                        }
                    }
                    result.Add(boneToAdd);
                }
            }

            return result;
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

        private DecompositionAssignment GetDecompositionAssignment(List<PartitionAssignment> partAssign, char decType)
        {
            float totalCost = 0;

            foreach (PartitionAssignment partition in partAssign)
                totalCost += partition.Score;

            return new DecompositionAssignment(partAssign, totalCost, decType);
        }

        private PartitionAssignment ComputeAssignement(List<Bone> partition, List<Bone> virtualArmature, int currDecCount, int maxPartitionCount, Dictionary<string, List<List<char>>> dictionary, string configurationName, int maxLenghtChain, UserPreference pref)
        {
            try
            {
                // solves assignment problem with Hungarian Algorithm                                        
                //float[,] costsMatrix = new float[partition.Count, virtualArmature.Count];
                // Computes node similarity
                float[,] costsMatrix = Metrics.NodeSimilarityScore(partition, virtualArmature);
                // Defines the costs
                for (int row = 0; row < partition.Count; row++)
                {
                    for (int col = 0; col < virtualArmature.Count; col++)
                    {
                        costsMatrix[row, col] = costsMatrix[row, col] * pref.NodSim +
                            Metrics.DofCoverageScore(partition[row], virtualArmature[col], dictionary) * pref.DofCov +
                            Metrics.ChainLengthScore(partition[row], virtualArmature[col], maxLenghtChain) * pref.PosInC +
                            Metrics.SymmetryScore(partition[row], virtualArmature[col]) * pref.Sym +
                            Metrics.PartitionsCountScore(currDecCount, maxPartitionCount) * pref.ParCou +
                            Metrics.ComponentRangeAnnoyanceScore2(partition[row], virtualArmature[col], pref.ComRan, pref.ComAnn, dictionary);
                    }
                }

                int[] assignment = HungarianAlgorithm.FindAssignments(costsMatrix);

                float score = Metrics.ComputeCostAssignment(costsMatrix, assignment);

                PartitionAssignment result =
                    new PartitionAssignment(configurationName, assignment, partition, virtualArmature, score);

                /*
                if (AutomaticMapping.KinectAssignmentConsistency(result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
                */
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                throw;
            }
        }

        private void CreateConfiguration(PartitionAssignment arrangement, string armatureName, int maxLenghtChain, int currPartitionCount, int maxPartitionCount, Dictionary<string, List<List<char>>> dictionary, UserPreference pref)
        {
            Setting settingToAdd = new Setting(arrangement.Name);
            List<Setting> settings = new List<Setting>();
            for (int i = 0; i < arrangement.Assignment.Length; i++)
            {
                Bone currentBone = arrangement.Partition[i];

                List<Bone> components = new List<Bone>();
                Bone handler = arrangement.Handler[arrangement.Assignment[i]];

                if (handler.name.Contains(" | "))
                    components = Metrics.DecomposeHandler(handler);
                else
                    components.Add(handler);

                PartitionAssignment BoneDofsAssignment = new PartitionAssignment();
                if (arrangement.Name.Contains("KINECT_CONFIG"))
                {
                    BoneDofsAssignment = arrangement;
                    components = KinectSkeleton.GetKinectSkeleton();
                }
                else
                {
                    List<Bone> oneDofBones = AutomaticMapping.GetOneDofBones(currentBone, components, dictionary);

                    BoneDofsAssignment =
                        ComputeAssignement(oneDofBones, components,
                         currPartitionCount, maxPartitionCount, dictionary, currentBone.name, maxLenghtChain, pref);
                }

                int rotOrder = 0;
                for (int boneDof = 0; boneDof < BoneDofsAssignment.Assignment.Length; boneDof++)
                {
                    Bone component = new Bone("");
                    if (arrangement.Name.Contains("KINECT_CONFIG"))
                        component = components[BoneDofsAssignment.Assignment[i]];
                    else
                        component = components[BoneDofsAssignment.Assignment[boneDof]];
                    if (component.name.Contains("_TUI"))
                    {
                        string componentName = component.name;
                        string port = componentName.Substring(componentName.IndexOf("PORT-") + 5,
                             componentName.IndexOf(")_TUI") - (componentName.IndexOf("PORT-") + 5));
                        string axis = "";

                        if (BoneDofsAssignment.Partition[boneDof].loc_DoF.Count > 0)
                        {
                            //axis = componentName.Substring(componentName.IndexOf(":LOC(") + 5, 1).ToUpper();
                            axis = BoneDofsAssignment.Partition[boneDof].loc_DoF[0].ToString().ToUpper();
                            settingToAdd.CheckBoxStatus.Add(new SettingItem(port + "Loc", "true"));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "BoneName", currentBone.name + ":" + armatureName));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "Axis", axis));
                        }

                        if (BoneDofsAssignment.Partition[boneDof].rot_DoF.Count > 0)
                        {
                            axis = BoneDofsAssignment.Partition[boneDof].rot_DoF[0].ToString().ToUpper();
                            settingToAdd.CheckBoxStatus.Add(new SettingItem(port + "Rot", "true"));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "BoneName", currentBone.name + ":" + armatureName));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "Axis", axis));
                            /*settingToAdd.ComboBoxStatus.Add (new SettingItem(port + "RotOrder", components.FindIndex(x => x.name.Equals(component.name)).ToString()));*/
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(port + "RotOrder", rotOrder.ToString()));
                            rotOrder++;
                        }
                    }
                    else
                    {
                        if (/*!pref.LocRotCheckBox*/UserPreferenceSlider.Value < 0)
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
                                settingToAdd.ComboBoxStatus.Add(new SettingItem(jointName + "BoneName", currentBone.name + ":" + armatureName));
                                settingToAdd.ComboBoxStatus.Add(new SettingItem(jointName + axis + "from", axis));
                            }

                            if (componentName.Contains("ROT"))
                            {
                                settingToAdd.CheckBoxStatus.Add(new SettingItem(jointName + "Rot", "true"));
                                settingToAdd.ComboBoxStatus.Add(new SettingItem(jointName + "BoneName", currentBone.name + ":" + armatureName));
                            }

                            if (!componentName.Contains("ROT") && (!componentName.Contains("LOC")))
                            {
                                settingToAdd.ComboBoxStatus.Add(
                                    new SettingItem(jointName + "BoneName", currentBone.name + ":" + armatureName));

                                //LOC                            
                                List<char> locDofs = currentBone.loc_DoF;
                                foreach (char Dof in locDofs)
                                {
                                    settingToAdd.ComboBoxStatus.Add(
                                        new SettingItem(jointName + Dof.ToString().ToUpper() + "from", Dof.ToString().ToUpper()));

                                    settingToAdd.CheckBoxStatus.Add(
                                        new SettingItem(jointName + "Loc" + Dof.ToString().ToUpper(), "true"));
                                }
                                //ROT
                                settingToAdd.CheckBoxStatus.Add(new SettingItem(jointName + "Rot", "true"));

                                break;
                            }
                        }
                        else
                        {
                            string componentName = component.name; // "Hip_NUI_DoF(x):LOC(L)"
                            string jointDof = componentName.Substring(componentName.IndexOf("_DoF(") + 5, 1).ToUpper(); // X
                            string axis = "";

                            // ROT
                            if (BoneDofsAssignment.Partition[boneDof].rot_DoF.Count > 0)
                            {
                                axis = BoneDofsAssignment.Partition[boneDof].rot_DoF[0].ToString().ToUpper();
                                settingToAdd.CheckBoxStatus.Add(new SettingItem(jointDof + "Rot", "true"));
                                settingToAdd.ComboBoxStatus.Add(new SettingItem
                                    (jointDof + "RotOrder", components.FindIndex(x => x.name.Equals(component.name)).ToString()));

                            }
                            // LOC
                            if (BoneDofsAssignment.Partition[boneDof].loc_DoF.Count > 0)
                            {
                                axis = BoneDofsAssignment.Partition[boneDof].loc_DoF[0].ToString().ToUpper();
                                settingToAdd.CheckBoxStatus.Add(new SettingItem(jointDof + "Loc", "true"));
                            }
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(jointDof + "BoneName", currentBone.name + ":" + armatureName));
                            settingToAdd.ComboBoxStatus.Add(new SettingItem(jointDof + "Axis", axis));

                        }
                    }
                }
            }

            settings.Add(settingToAdd);
            ConvertSettingToPreset(settings);

        }

        private void CreateLegoInstruction(DecompositionAssignment decAssign)
        {
            LegoJoint legoJoint = new LegoJoint();
            int brickRefID = 0;
            int partRefID = 0;
            float[] delta = new float[3];
            List<BrickNode> joints = new List<BrickNode>();
            List<Bone> assemblyArm = new List<Bone>();


            if (decAssign.Type.Equals(DecompositionAssignment.SEQUENTIAL_TYPE))
            {
                // create the source sequential armature                
                for (int i = 0; i < decAssign.PartitionAss[0].Handler.Count; i++)
                {
                    Bone boneToAdd = decAssign.PartitionAss[0].Handler[i];
                    boneToAdd.level = i;
                    boneToAdd.children.RemoveRange(0, boneToAdd.children.Count);
                    if (i > 0)
                        boneToAdd.parent = decAssign.PartitionAss[0].Handler[i - 1].name;
                    if (i < decAssign.PartitionAss[0].Handler.Count - 1)
                        boneToAdd.children.Add(decAssign.PartitionAss[0].Handler[i + 1].name);
                    assemblyArm.Add(boneToAdd);
                }
                assemblyArm = Instruction.SplitHandlers(assemblyArm);
            }

            if (decAssign.Type.Equals(DecompositionAssignment.SPLITTED_TYPE))
            {
                //ArmatureAssignSymmetry(decAssign.SplittedArmature);
                assemblyArm = Instruction.SplitHandlers(decAssign.SplittedArmature);
            }

            if (decAssign.Type.Equals(DecompositionAssignment.SINGLE_CONF_TYPE))
            {
                assemblyArm = Instruction.SplitHandlers(decAssign.PartitionAss[0].Handler);
            }

            List<LegoJoint> jointsToBuild = Instruction.GetLegoAssembly(assemblyArm);

            for (int j = 0; j < jointsToBuild.Count; j++)
            {
                legoJoint = jointsToBuild[j];
                LXFML jointToAdd = LXFML.ReadLXFML(legoJoint);

                foreach (BrickNode brickToAdd in jointToAdd.Bricks.Brick)
                {
                    brickToAdd.refID = brickRefID.ToString();
                    foreach (PartNode part in brickToAdd.Part)
                    {
                        part.refID = partRefID.ToString();
                        part.Bone.refID = partRefID.ToString();

                        part.Bone = BoneNode.TranslateBoneNode
                            (jointsToBuild[j].position[0], jointsToBuild[j].position[1], jointsToBuild[j].position[2], part.Bone);
                        partRefID++;
                    }

                    joints.Add(brickToAdd);
                    brickRefID++;
                }

            }

            LXFML.WriteLXFML(joints);
        }

        private List<List<ComputationData>> ViewScores(List<DecompositionAssignment> decAssign, Dictionary<string, List<List<char>>> dictionary, int maxLevelBone, int maxPartitionCount, UserPreference pref)
        {
            List<List<ComputationData>> result = new List<List<ComputationData>>();
            foreach (DecompositionAssignment dec in decAssign)
            {
                List<ComputationData> currentDecResult = new List<ComputationData>();

                for (int i = 0; i < dec.PartitionAss.Count; i++)
                {
                    ComputationData data = new ComputationData(dec.PartitionAss[i].Partition,
                        dec.PartitionAss[i].Handler, dec.PartitionAss[i].Assignment, dec.PartitionAss[i].Score);

                    data.NodeSimilarityScores = Metrics.NodeSimilarityScore(data.Partition, data.Handler);

                    for (int row = 0; row < data.Partition.Count; row++)
                    {
                        for (int col = 0; col < data.Handler.Count; col++)
                        {
                            data.NodeSimilarityScores[row, col] = data.NodeSimilarityScores[row, col] * pref.NodSim;

                            data.DofCoverageScores[row, col] =
                                Metrics.DofCoverageScore(data.Partition[row], data.Handler[col], dictionary) * pref.DofCov;

                            /*data.ComponentRangeScores[row, col] =
                                Metrics.ComponentRangeScore(data.Partition[row], data.Handler[col]);
                            data.ComponentAnnoyanceScores[row, col] =
                                Metrics.ComponentAnnoyanceScore(data.Partition[row], data.Handler[col]);*/
                            
                            data.ChainLengthScores[row, col] =
                                Metrics.ChainLengthScore(data.Partition[row], data.Handler[col], maxLevelBone) * pref.PosInC;

                            data.SymmetryScores[row, col] =
                                Metrics.SymmetryScore(data.Partition[row], data.Handler[col]) * pref.Sym;

                            data.PartitionCountScores[row, col] =
                                Metrics.PartitionsCountScore(dec.PartitionAss.Count, maxPartitionCount) * pref.ParCou;
                            
                            data.ComponentRangeAnnoyanceScore[row, col] =
                                Metrics.ComponentRangeAnnoyanceScore2(data.Partition[row], data.Handler[col], pref.ComRan, pref.ComAnn, dictionary);

                            data.CostMatrix[row, col] = data.NodeSimilarityScores[row, col] +
                                data.DofCoverageScores[row, col] +
                                data.ChainLengthScores[row, col] +
                                data.SymmetryScores[row, col] +
                                data.PartitionCountScores[row, col] +
                                data.ComponentRangeAnnoyanceScore[row, col];

                            int[] assignment = HungarianAlgorithm.FindAssignments(data.CostMatrix);

                            data.Assignment = assignment;

                            data.PartitionScore = Metrics.ComputeCostAssignment(data.CostMatrix, data.Assignment);
                        }
                    }


                    currentDecResult.Add(data);
                }

                result.Add(currentDecResult);
            }
            return result;
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

        private List<Bone> RequestArmatureInfo(string armatureName)
        {
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


    class ComputationData
    {
        public List<Bone> Handler { get; set; }
        public List<Bone> Partition { get; set; }
        public int[] Assignment { get; set; }
        public float PartitionScore { get; set; }

        public float[,] NodeSimilarityScores { get; set; }
        public float[,] DofCoverageScores { get; set; }
        public float[,] ComponentRangeScores { get; set; }
        public float[,] ComponentAnnoyanceScores { get; set; }
        public float[,] ChainLengthScores { get; set; }
        public float[,] SymmetryScores { get; set; }
        public float[,] PartitionCountScores { get; set; }
        public float[,] CostMatrix { get; set; }
        public float[,] ComponentRangeAnnoyanceScore { get; set; }

        public ComputationData(List<Bone> partition, List<Bone> handler, int[] assignment, float partitionScore)
        {

            this.Partition = partition;
            this.Handler = handler;
            this.Assignment = assignment;
            this.PartitionScore = partitionScore;

            this.NodeSimilarityScores = new float[partition.Count, handler.Count];
            this.DofCoverageScores = new float[partition.Count, handler.Count];
            this.ComponentRangeScores = new float[partition.Count, handler.Count];
            this.ComponentAnnoyanceScores = new float[partition.Count, handler.Count];
            this.ChainLengthScores = new float[partition.Count, handler.Count];
            this.SymmetryScores = new float[partition.Count, handler.Count];
            this.PartitionCountScores = new float[partition.Count, handler.Count];
            this.CostMatrix = new float[partition.Count, handler.Count];
            this.ComponentRangeAnnoyanceScore = new float[partition.Count, handler.Count];
        }

    }

}
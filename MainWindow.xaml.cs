//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
// 	 
//	 Copyright 2013 Microsoft Corporation 
// 	 
//	Licensed under the Apache License, Version 2.0 (the "License"); 
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
// 	 
//		 http://www.apache.org/licenses/LICENSE-2.0 
// 	 
//	Unless required by applicable law or agreed to in writing, software 
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//	See the License for the specific language governing permissions and 
//	limitations under the License. 
// 	 
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;

    using System;
    using System.Collections;

    using System.Collections.Generic;
    using System.Windows.Documents;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;

    using Lego.Ev3.Core;
    using Lego.Ev3.Desktop;
    using System.Windows.Input;
    using System.Numerics;

    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Preset> presetConfig { get; set; }
        public ArrayList sensorLegoInfo{ get; set; }
        public ArrayList sensorKinectInfo { get; set; }
        public ArrayList rotationMatrix { get; set; }
        public string currentPreset {get;set;}

        public List<VoiceCommand> grammarRules { get; set; }
        RecognizerInfo recInfo;

        public bool captureFlag { get; set; }

        public InputPort touchPort { get; set; }
        public string touchCommand { get; set; }

        Skeleton skeletonTracked;

        public int currentFrame { get; set; }        
        public float factor { get; set;}
        private Position startPosition { get; set; }
        private Position endPosition { get; set; }
        public float accuracy { get; set; }
        public int ghostFrameRange { get; set; }
        public int ghostFrameStep { get; set; }

        public bool virtualArmature { get; set; }
              
        Boolean recFlag = false;

        public Brick legoBrick { get; set; }

        public const float alpha = 0.30f;
        public bool filterOn = true;

        /// <summary>
        /// Likelihood that a RecognizedaaPhrase matches a given input
        /// </summary>
        
        public double ConfidenceThreshold = 0.5;           

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;
        
        /// <summary>
        /// Resource key for medium-gray-colored brush.
        /// </summary>
        private const string MediumGreyBrushKey = "MediumGreyBrush";

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine;

        
       


        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeList();
            InitVariable();                        
            InitializeComponent();
            TryConnectionStart();

        }

        private void TryConnectionStart()
        {
            try
            {
                AsynSocket.StartClient();
                //StartLegoConnection();
                GetCurrentFrame();
                this.statusBarText.Text = "Connection Start";
            }
            catch (Exception e)
            {
                this.statusBarText.Text = "Connection Problem";
            }
            
        }

        private void InitVariable()
        {
            currentFrame = -1;            
            ghostFrameRange = 0;
            ghostFrameStep = 0;
            accuracy = 1.0f;
            captureFlag = false;
            virtualArmature = false;
            currentPreset = string.Empty;
            startPosition = new Position();
            endPosition = new Position();            
            
        }

        private void InitializeList()
        {
            sensorLegoInfo = new ArrayList();
            sensorKinectInfo = new ArrayList();
            rotationMatrix = new ArrayList();
            presetConfig = new List<Preset>();
            

            // ITA
            // default VoiceCommand
            grammarRules = new List<VoiceCommand>();
            //grammarRules.Add(new VoiceCommand("Activate sensor", Command.POSITION));
            //grammarRules.Add(new VoiceCommand("posizione", Command.POSITION));
            //grammarRules.Add(new VoiceCommand("frame", Command.FRAME));
            grammarRules.Add(new VoiceCommand("Insert frame", Command.FRAME));
            //grammarRules.Add(new VoiceCommand("cattura", Command.FRAME));
            //grammarRules.Add(new VoiceCommand("Record", Command.REC));
            //grammarRules.Add(new VoiceCommand("Stop recording", Command.STOP));
            //grammarRules.Add(new VoiceCommand("pausa", Command.STOP));
            grammarRules.Add(new VoiceCommand("Next", Command.FAST_FORWARD));
            //grammarRules.Add(new VoiceCommand("avanza", Command.FAST_FORWARD));
            //grammarRules.Add(new VoiceCommand("avanti veloce", Command.FAST_FORWARD));
            //grammarRules.Add(new VoiceCommand("Previous frame", Command.BACKWARD));
            //grammarRules.Add(new VoiceCommand("cancella", Command.DELETE));
            //grammarRules.Add(new VoiceCommand("elimina", Command.DELETE));
            //grammarRules.Add(new VoiceCommand("Delete frame", Command.DELETE_CURRENT));
            grammarRules.Add(new VoiceCommand("Reset", Command.RESET));
            grammarRules.Add(new VoiceCommand("Play animation", Command.PLAY_ANIMATION));
            //grammarRules.Add(new VoiceCommand("Disable sensor", Command.HIDE_CAPTURE));
            //grammarRules.Add(new VoiceCommand("indietro", Command.FAST_BACKWARD));
            //grammarRules.Add(new VoiceCommand("associa", Command.BONES_ASSOCIATION));
            //grammarRules.Add(new VoiceCommand("blocca posizione", Command.POSITION_LOCK));
            //grammarRules.Add(new VoiceCommand("azzera origine", Command.RESET_ORIGIN));
            //grammarRules.Add(new VoiceCommand("Frame ghost", Command.ACTIVE_GHOST_FRAME));
            //grammarRules.Add(new VoiceCommand("disabilita frame ghost", Command.DISABLE_GHOST_FRAME));
            //grammarRules.Add(new VoiceCommand("inizio", Command.FIRST_FRAME));
            //grammarRules.Add(new VoiceCommand("fine", Command.LAST_FRAME));
            //grammarRules.Add(new VoiceCommand("inzio", Command.START_POSE));
            //grammarRules.Add(new VoiceCommand("fine", Command.END_POSE));
            //grammarRules.Add(new VoiceCommand("calcola fattore", Command.COMPUTE_FACTOR));

            //EN
            //...

        }        


        /// <summary>
        /// Gets the metadata for the speech recognizer (acoustic model) most suitable to
        /// process audio from Kinect device.
        /// </summary>
        /// <returns>
        /// RecognizerInfo if found, <code>null</code> otherwise.
        /// </returns>
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);

                //"en-US"
                //"it-IT"
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "it-IT".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }
            return null;
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }

            recInfo = GetKinectRecognizer();

            if (null != recInfo)
            {
                StartSpeechRecognition();               
            }
            else
            {
                this.statusBarText.Text = Properties.Resources.NoSpeechRecognizer;
            }


        }

        public void StartSpeechRecognition()
        {
            this.speechEngine = new SpeechRecognitionEngine(recInfo.Id);
            var newCommand = new Choices();
            foreach (VoiceCommand s in grammarRules)
            {
                newCommand.Add(new SemanticResultValue(s.SpeechRecognized,s.Rule));
            }
            var gb = new GrammarBuilder { Culture = recInfo.Culture };
            gb.Append(newCommand);
            var g = new Grammar(gb);
            speechEngine.LoadGrammar(g);

            speechEngine.SpeechRecognized -= SpeechRecognized;
            speechEngine.SpeechRecognized += SpeechRecognized;
            speechEngine.SpeechRecognitionRejected += SpeechRejected;

            // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
            // This will prevent recognition accuracy from degrading over time.
            ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

            speechEngine.SetInputToAudioStream(
                sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
        }        

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();
                this.sensor.Stop();

            }
            
            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }

            foreach (Preset p in presetConfig) 
            {
                File.Delete("config\\" + p.Name + ".json");
            }            


        }

        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {            
            if (e.Result.Confidence >= ConfidenceThreshold)
            {                
                SendCommand(e.Result.Semantics.Value.ToString(), e.Result.Text);                
            }
        }

        // Sends commands to the server
        private void SendCommand(string command, string capturedText)
        {

            Packet packet = new Packet();
            List<string> activeObject = new List<string>();
            this.statusBarText.Text = "Command " + command + " recognized";

            switch (command)
            {
                case Command.POSITION:
                    captureFlag = true;
                    SendKinectData();
                    System.Media.SystemSounds.Asterisk.Play();
                    break;

                case Command.FRAME:
                    GetCurrentFrame();
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();                    
                    packet.header = Command.FRAME;
                    packet.payload.Add(0);
                    packet.payload.Add(FrameType.LOCATION_ROTATION);
                    activeObject = getActiveObject();
                    foreach (string obj in activeObject)
                    {
                        packet.payload.Add(obj);
                    }
                    break;

                case Command.FRAME_LOC:
                    GetCurrentFrame();
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();
                    packet.header = Command.FRAME;
                    packet.payload.Add(0);
                    packet.payload.Add(FrameType.LOCATION);
                    activeObject = getActiveObject();
                    foreach (string obj in activeObject)
                    {
                        packet.payload.Add(obj);
                    }
                    break;

                case Command.FRAME_ROT:
                    GetCurrentFrame();
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();
                    packet.header = Command.FRAME;
                    packet.payload.Add(0);
                    packet.payload.Add(FrameType.ROTATION);
                    activeObject = getActiveObject();
                    foreach (string obj in activeObject)
                    {
                        packet.payload.Add(obj);
                    }
                    break;

                case Command.REC:
                    currentFrame = 1;
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();                    
                    recFlag = true;
                    System.Media.SystemSounds.Asterisk.Play();
                    break;

                case Command.STOP:
                    recFlag = false;
                    System.Media.SystemSounds.Asterisk.Play();
                    break;

                case Command.FORWARD:
                    packet.header = Command.FORWARD;
                    packet.payload.Add(10);
                    currentFrame += 10;
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();                    
                    break;

                case Command.BACKWARD:
                    packet.header = Command.BACKWARD;
                    packet.payload.Add(10);
                    currentFrame -= 10;
                    if (currentFrame < 0) 
                    { 
                        currentFrame = 0;
                    }
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();                                       
                    break;

                case Command.DELETE:
                    packet.header = Command.DELETE;
                    packet.payload.Add(0);
                    activeObject = getActiveObject();
                    foreach (string obj in activeObject)
                    {
                        packet.payload.Add(obj);
                    }
                    break;

                case Command.DELETE_CURRENT:
                    packet.header = Command.DELETE_CURRENT;
                    packet.payload.Add(0);
                    activeObject = getActiveObject();
                    foreach (string obj in activeObject)
                    {
                        packet.payload.Add(obj);
                    }                    
                    break;

                case Command.RESET:
                    ResetSensorInfo();
                    System.Media.SystemSounds.Asterisk.Play();

                    SendLegoData();

                    packet.header = Command.RESET;
                    packet.payload.Add(0);

                    if (TestCheckBox.IsChecked.Value)
                    {
                        captureFlag = true;
                    }

                    //GetCurrentFrame();
                    
                    break;

                case Command.TRANSLATE_COORDINATE_SYSTEM:
                    ChageCoordinateSystem();
                    System.Media.SystemSounds.Asterisk.Play();
                    break;

                case Command.RESET_ORIGIN:
                    ResetOrigin();
                    System.Media.SystemSounds.Asterisk.Play();
                    break;

                case Command.PLAY_ANIMATION:
                    packet.header = Command.PLAY_ANIMATION;
                    packet.payload.Add(0);
                    break;

                case Command.FAST_FORWARD:
                    packet.header = Command.FORWARD;
                    packet.payload.Add(24);
                    currentFrame += 24;
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();
                    if (this.TestCheckBox.IsChecked.Value)
                    {
                        //SendCommand(Command.FRAME, "");
                        //captureFlag = false;
                        SendCommand(Command.START_TEST, "");
                    }
                    break;

                case Command.FAST_BACKWARD:
                    packet.header = Command.BACKWARD;
                    packet.payload.Add(24);
                    currentFrame -= 24;
                    if (currentFrame < 0) 
                    { 
                        currentFrame = 0;
                    }
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();                                      
                    break;

                case Command.HIDE_CAPTURE:
                    captureFlag = false;
                    System.Media.SystemSounds.Asterisk.Play();
                    GetCurrentFrame();
                    break;

                case Command.BONES_ASSOCIATION:
                    System.Media.SystemSounds.Asterisk.Play();
                    BonesAssociation();
                    break;

                case Command.CHANGE_PRESET:
                    System.Media.SystemSounds.Asterisk.Play();                   
                    foreach (Preset preset in this.presetConfig)
                    {
                        if (capturedText.Equals(preset.Name))
                        {
                            this.statusBarText.Text = capturedText;
                            sensorKinectInfo = preset.sensorKinectInfoSet;
                            sensorLegoInfo = preset.sensorLegoInfoSet;


                            rotationMatrix.Clear();
                            foreach (SensorLegoInfo slf in sensorLegoInfo)
                            {
                                ObjectRotationMatrix newObjcetRotMatrix = new ObjectRotationMatrix(slf.ObjectName + ":" + slf.BoneName);
                                if (!rotationMatrix.Contains(newObjcetRotMatrix))
                                    rotationMatrix.Add(newObjcetRotMatrix);
                            }

                            touchCommand = preset.TouchCommand;
                            currentPreset = preset.Name;
                            break;
                        }
                    }

                    if (TestCheckBox.IsChecked.Value)
                    {
                        captureFlag = false;
                        packet.header = Command.CHANGE_PRESET;
                        packet.payload.Add(currentPreset);
                    }
                    
                    break;

                case Command.FIRST_FRAME:
                    currentFrame = 0;
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();
                    packet.header = Command.FIRST_FRAME;
                    packet.payload.Add(0);
                    break;

                case Command.LAST_FRAME:
                    packet.header = Command.LAST_FRAME;
                    packet.payload.Add(-1);
                    SendPacket(packet);

                    string stringJson = AsynSocket.SyncReceiver();
                    currentFrame = JsonManager.GetInt(stringJson);
                    this.currentFrameText.Text = "frame: " + currentFrame.ToString();
                    packet.header = null;
                    break;

                case Command.ACTIVE_GHOST_FRAME:
                    packet.header = Command.ACTIVE_GHOST_FRAME;
                    packet.payload.Add(this.ghostFrameRange);
                    packet.payload.Add(this.ghostFrameStep);
                    break;

                case Command.DISABLE_GHOST_FRAME:
                    packet.header = Command.ACTIVE_GHOST_FRAME;
                    packet.payload.Add(0);
                    packet.payload.Add(0);
                    break;
                
                case Command.START_TEST:
                    packet.header = Command.START_TEST;
                    packet.payload.Add(this.TestName.Text);
                    packet.payload.Add(currentFrame);
                    break;

                case Command.MORE_ACCURACY:
                    accuracy = accuracy * 2;
                    break;
                
                case Command.LESS_ACCURACY:
                    accuracy = accuracy / 2;
                    break;

                case Command.START_POSE:
                    Joint startJoint = skeletonTracked.Joints[JointType.HandRight];
                    startPosition.locX = -startJoint.Position.X;
                    startPosition.locY = startJoint.Position.Y;
                    startPosition.locZ = -startJoint.Position.Z;
                    break;

                case Command.END_POSE:
                    Joint endJoint = skeletonTracked.Joints[JointType.HandRight];
                    endPosition.locX = -endJoint.Position.X;
                    endPosition.locY = endJoint.Position.Y;
                    endPosition.locZ = -endJoint.Position.Z;
                    break;

                case Command.COMPUTE_FACTOR:                    

                    float distance = (float)Math.Sqrt(Math.Pow(endPosition.locX - startPosition.locX, 2) + Math.Pow(endPosition.locY - startPosition.locY, 2) + Math.Pow(endPosition.locZ - startPosition.locZ, 2));

                    AsynSocket.Send(Command.COMPUTE_FACTOR);
                    string stringJsonD = AsynSocket.SyncReceiver();
                    float ReferenceDist = JsonManager.GetFloat(stringJsonD);
                    factor = ReferenceDist / distance;
                    break;

                case Command.LOCK_POSE:
                    LockPos();
                    break;
                case Command.UNLOCK_POSE:
                    UnLockPos();
                    break;
            }

            if (packet.header != null)
            {
                try
                {
                    SendPacket(packet);             
                }
                catch (Exception ex)
                {
                    this.statusBarText.Text = "Connection Problem";
                }
            }   
        }

        public void SendLegoData()
        {
            if (captureFlag)
            {
                Packet packet = new Packet();                

                if (!virtualArmature)
                {
                    foreach (SensorLegoInfo sf in sensorLegoInfo)
                    {
                        packet.header = Command.POSITION;
                        Motions newMotion = new Motions();

                        newMotion = createMotion(sf.ObjectName, sf.BoneName, 0, 0, 0, 0, 0, 0, 0, false, false, false, false);

                        //check if the bone is mapped with another sensor
                        foreach (Motions m in packet.payload)
                        {
                            if (m.objectName.Equals(sf.ObjectName) && m.boneName.Equals(sf.BoneName))
                            {
                                newMotion = m;
                                break;
                            }
                        }

                        float newValue = 0;
                        
                        if (filterOn)
                        {
                            newValue = sf.Value + alpha * (legoBrick.Ports[sf.InputPort].SIValue - sf.Value);
                            //newMotion.vectorPos.locX = newValue - sf.Offset + sf.LocPos;
                            //newMotion.vectorPos.locY = newValue - sf.Offset + sf.LocPos;
                            //newMotion.vectorPos.locZ = newValue - sf.Offset + sf.LocPos;
                            sf.Value = newValue;
                        }
                        else
                        {
                            // No-fiter
                            newValue = legoBrick.Ports[sf.InputPort].SIValue;
                            //newMotion.vectorPos.locX = legoBrick.Ports[sf.InputPort].SIValue - sf.Offset + sf.LocPos;
                            //newMotion.vectorPos.locY = legoBrick.Ports[sf.InputPort].SIValue - sf.Offset + sf.LocPos;
                            //newMotion.vectorPos.locZ = legoBrick.Ports[sf.InputPort].SIValue - sf.Offset + sf.LocPos;
                        }

                        switch (sf.Axis)
                        {
                            case "X":
                                if (sf.LocationTrack)
                                {
                                    newMotion.locXTrack = true;
                                    newMotion.vectorPos.locX = newValue - sf.Offset + sf.LocPos;
                                }
                                if (sf.OrientationTrack)
                                {
                                    newMotion.rotTrack = true;
                                    
                                    // UUpdates the rotation matrix depending on the order of rotations
                                    Matrix4x4 rotToApply = UpdateObjectRotMatrix(sf.Axis, sf.ObjectName + ":" + sf.BoneName,
                                        (legoBrick.Ports[sf.InputPort].SIValue - sf.Offset) / accuracy, sf.RotationOrder);
                                    
                                    // Computes quaternion representation (w,x,y,z) and updates the new motion
                                    newMotion.vectorOr.W = 
                                        (float)Math.Sqrt(Convert.ToDouble(1.0f + rotToApply.M11 + rotToApply.M22 + rotToApply.M33))/2;
                                    newMotion.vectorOr.X = 
                                        (rotToApply.M32 - rotToApply.M23) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Y = 
                                        (rotToApply.M13 - rotToApply.M31) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Z = 
                                        (rotToApply.M21 - rotToApply.M12) / (4 * newMotion.vectorOr.W);
                                    
                                    // Old version
                                    //newMotion.vectorOr.X = (legoBrick.Ports[sf.InputPort].SIValue - sf.Offset) / accuracy;
                                }
                                break;

                            case "-X":
                                if (sf.LocationTrack)
                                {
                                    newMotion.locXTrack = true;
                                    newMotion.vectorPos.locX = -newValue + sf.Offset;
                                     
                                }
                                if (sf.OrientationTrack)
                                {
                                    newMotion.rotTrack = true;

                                    // Updates rotation matrix
                                    Matrix4x4 rotToApply = UpdateObjectRotMatrix(sf.Axis, sf.ObjectName + ":" + sf.BoneName,
                                        (-legoBrick.Ports[sf.InputPort].SIValue + sf.Offset) / accuracy, sf.RotationOrder);

                                    // Compute w,x,y,z coordinates the new rotation matrix
                                    newMotion.vectorOr.W = 
                                        (float)Math.Sqrt(Convert.ToDouble(1.0f + rotToApply.M11 + rotToApply.M22 + rotToApply.M33))/2;
                                    newMotion.vectorOr.X = 
                                        (rotToApply.M32 - rotToApply.M23) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Y = 
                                        (rotToApply.M13 - rotToApply.M31) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Z = 
                                        (rotToApply.M21 - rotToApply.M12) / (4 * newMotion.vectorOr.W);
                                }
                                break;

                            case "Y":
                                if (sf.LocationTrack)
                                {
                                    // if is an armature using Blender local space
                                    if (!sf.BoneName.Equals(""))
                                    {
                                        newMotion.locYTrack = true;
                                        newMotion.vectorPos.locY = newValue - sf.Offset + sf.LocPos;
                                    }
                                    // using wordspace
                                    else
                                    {
                                        // translates into Kinect coordinates
                                        newMotion.locZTrack = true;
                                        newMotion.vectorPos.locY = newValue - sf.Offset + sf.LocPos;
                                    }
                                }
                                if (sf.OrientationTrack)
                                {
                                    newMotion.rotTrack = true;
                                    //newMotion.vectorOr.Y = (legoBrick.Ports[sf.InputPort].SIValue - sf.Offset) / accuracy;
                                    
                                    Matrix4x4 rotToApply = UpdateObjectRotMatrix(sf.Axis, sf.ObjectName + ":" + sf.BoneName,
                                        (legoBrick.Ports[sf.InputPort].SIValue - sf.Offset) / accuracy, sf.RotationOrder);
                                    
                                    newMotion.vectorOr.W = 
                                        (float)Math.Sqrt(Convert.ToDouble(1.0f + rotToApply.M11 + rotToApply.M22 + rotToApply.M33))/2;
                                    newMotion.vectorOr.X = 
                                        (rotToApply.M32 - rotToApply.M23) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Y = 
                                        (rotToApply.M13 - rotToApply.M31) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Z = 
                                        (rotToApply.M21 - rotToApply.M12) / (4 * newMotion.vectorOr.W);                                
                                }
                                break;

                            case "-Y":
                                if (sf.LocationTrack)
                                {
                                    // if is an armature using Blender local space
                                    if (!sf.BoneName.Equals(""))
                                    {
                                        newMotion.locYTrack = true;
                                        newMotion.vectorPos.locY = -newValue + sf.Offset;
                                    }
                                    // using wordspace
                                    else
                                    {
                                        // translates into Kinect coordinates
                                        newMotion.locZTrack = true;
                                        newMotion.vectorPos.locY = -newValue + sf.Offset;
                                    }
                                                                        
                                }
                                if (sf.OrientationTrack)
                                {
                                    newMotion.rotTrack = true;
                                    //newMotion.vectorOr.Y = (-legoBrick.Ports[sf.InputPort].SIValue + sf.Offset) / accuracy;
                                    
                                    Matrix4x4 rotToApply = UpdateObjectRotMatrix(sf.Axis, sf.ObjectName + ":" + sf.BoneName,
                                        (-legoBrick.Ports[sf.InputPort].SIValue + sf.Offset) / accuracy, sf.RotationOrder);
                                    
                                    newMotion.vectorOr.W = 
                                        (float)Math.Sqrt(Convert.ToDouble(1.0f + rotToApply.M11 + rotToApply.M22 + rotToApply.M33))/2;
                                    newMotion.vectorOr.X = 
                                        (rotToApply.M32 - rotToApply.M23) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Y = 
                                        (rotToApply.M13 - rotToApply.M31) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Z = 
                                        (rotToApply.M21 - rotToApply.M12) / (4 * newMotion.vectorOr.W);
                                }
                                break;

                            case "Z":
                                if (sf.LocationTrack)
                                {
                                    if (!sf.BoneName.Equals(""))
                                    {
                                        newMotion.locZTrack = true;
                                        newMotion.vectorPos.locZ = newValue - sf.Offset + sf.LocPos;
                                    }
                                    else
                                    {
                                        newMotion.locYTrack = true;
                                        newMotion.vectorPos.locZ = newValue - sf.Offset + sf.LocPos;
                                    }
                                }
                                if (sf.OrientationTrack)
                                {
                                    newMotion.rotTrack = true;
                                    //newMotion.vectorOr.Z = (legoBrick.Ports[sf.InputPort].SIValue - sf.Offset) / accuracy;

                                    // Aggiornare la matrice di rotazione dell'oggetto in funzione dell'ordine
                                    Matrix4x4 rotToApply = UpdateObjectRotMatrix(sf.Axis, sf.ObjectName + ":" + sf.BoneName,
                                        (legoBrick.Ports[sf.InputPort].SIValue - sf.Offset) / accuracy, sf.RotationOrder);

                                    // calcolare le coordinate w,x,y,z -> scriverle in motion
                                    newMotion.vectorOr.W = 
                                        (float)Math.Sqrt(Convert.ToDouble(1.0f + rotToApply.M11 + rotToApply.M22 + rotToApply.M33))/2;
                                    newMotion.vectorOr.X = 
                                        (rotToApply.M32 - rotToApply.M23) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Y = 
                                        (rotToApply.M13 - rotToApply.M31) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Z = 
                                        (rotToApply.M21 - rotToApply.M12) / (4 * newMotion.vectorOr.W);
                                }
                                break;

                            case "-Z":
                                if (sf.LocationTrack)
                                {
                                    if (!sf.BoneName.Equals(""))
                                    {
                                        newMotion.locZTrack = true;
                                        newMotion.vectorPos.locZ = -newValue + sf.Offset;
                                    }
                                    else
                                    {
                                        newMotion.locYTrack = true;
                                        newMotion.vectorPos.locZ = -newValue + sf.Offset;
                                    }
                                    
                                    /* No filter value
                                     * newMotion.vectorPos.locZ = -legoBrick.Ports[sf.InputPort].SIValue + sf.Offset;
                                     */
                                }
                                if (sf.OrientationTrack)
                                {
                                    newMotion.rotTrack = true;
                                    //newMotion.vectorOr.Z = (-legoBrick.Ports[sf.InputPort].SIValue + sf.Offset) / accuracy;

                                    // Aggiornare la matrice di rotazione dell'oggetto in funzione dell'ordine
                                    Matrix4x4 rotToApply = UpdateObjectRotMatrix(sf.Axis, sf.ObjectName + ":" + sf.BoneName,
                                        (-legoBrick.Ports[sf.InputPort].SIValue + sf.Offset) / accuracy, sf.RotationOrder);

                                    // calcolare le coordinate w,x,y,z -> scriverle in motion
                                    newMotion.vectorOr.W = 
                                        (float)Math.Sqrt(Convert.ToDouble(1.0f + rotToApply.M11 + rotToApply.M22 + rotToApply.M33))/2;
                                    newMotion.vectorOr.X = 
                                        (rotToApply.M32 - rotToApply.M23) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Y = 
                                        (rotToApply.M13 - rotToApply.M31) / (4 * newMotion.vectorOr.W);
                                    newMotion.vectorOr.Z = 
                                        (rotToApply.M21 - rotToApply.M12) / (4 * newMotion.vectorOr.W);
                                }
                                break;
                        }
                        packet.payload.Add(newMotion);
                    }


                    string jsontToSend = JsonManager.CreateJson((Object)packet);
                    //System.Console.WriteLine(jsontToSend);
                    try
                    {
                        AsynSocket.Send(jsontToSend);
                    }
                    catch (Exception ex)
                    {
                        this.statusBarText.Text = "Connection Problem";
                    }
                }

                else 
                {
                    Console.WriteLine("Virtual Armature mode not implemented yet");
                }
            }   
            

        }

        private Matrix4x4 UpdateObjectRotMatrix(string axes, string name, float angle, int index)
        {
            Matrix4x4 rot = new Matrix4x4();
            Matrix4x4 rotToApply = new Matrix4x4();
            switch (axes) 
            {
                case "X":
                    rot = Matrix4x4.CreateRotationX((float)(Math.PI / 180) * angle);
                    break;
                case "-X":
                    rot = Matrix4x4.CreateRotationX((float)(Math.PI / 180) * angle);
                    break;
                case "Y":
                    rot = Matrix4x4.CreateRotationY((float)(Math.PI / 180) * angle);
                    break;
                case "-Y":
                    rot = Matrix4x4.CreateRotationY((float)(Math.PI / 180) * angle);
                    break;                
                case "Z":
                    rot = Matrix4x4.CreateRotationZ((float)(Math.PI / 180) * angle);
                    break;
                case "-Z":
                    rot = Matrix4x4.CreateRotationZ((float)(Math.PI / 180) * angle);
                    break;
            }

            foreach (ObjectRotationMatrix m in rotationMatrix)
            {
                if (m.ObjectName == name) 
                {
                    m.RotationMatrix[index] = rot;
                    rotToApply = Matrix4x4.Multiply(m.RotationMatrix[0], m.RotationMatrix[1]);
                    rotToApply = Matrix4x4.Multiply(rotToApply, m.RotationMatrix[2]);
                    break;
                }
            }
            return rotToApply;

        }

        public void SendPacket(Packet packet)
        {
            string jsontToSend = JsonManager.CreateJson((Object)packet);
            AsynSocket.Send(jsontToSend);
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void BonesAssociation()
        {
            Packet packet = new Packet();
            packet.header = Command.BONES_ASSOCIATION;

            foreach (SensorKinectInfo sf in sensorKinectInfo)
            {
                packet.payload.Add(new SensorInfo(sf.BoneName, sf.ObjectName, false));
            }

            foreach (SensorLegoInfo sf in sensorLegoInfo)
            {
                packet.payload.Add(new SensorInfo(sf.BoneName, sf.ObjectName, false));
            }

            string jsontToSend = JsonManager.CreateJson((Object)packet);
            try
            {
                AsynSocket.Send(jsontToSend);
                System.Media.SystemSounds.Asterisk.Play();

            }
            catch (Exception ex)
            {
                this.statusBarText.Text = "Connection Problem";
            }


            // Receive the response from Blender
            string stringJson = AsynSocket.SyncReceiver();
            List<string> association = JsonManager.GetList(stringJson);
            
            for (int i = 0; i < association.Count; i = i + 2)
            {
                string CurrentBoneAssociation = association[i];

                foreach (SensorKinectInfo sf in sensorKinectInfo)
                {
                    if (sf.BoneName.Equals(association[i+1]))
                    {
                        sensorKinectInfo.Add(new SensorKinectInfo(sf.Joint, CurrentBoneAssociation, sf.ObjectName, sf.LocationXTrack, sf.LocationYTrack, sf.LocationZTrack, sf.OrientationTrack, sf.Factor, sf.OffsetW, sf.OffsetX, sf.OffsetY, sf.OffsetZ, sf.OffsetLocX, sf.OffsetLocY, sf.OffsetLocZ,sf.LocPosX,sf.LocPosY,sf.LocPosZ, sf.takeXFrom, sf.takeYFrom, sf.takeZFrom));
                        
                        break;
                    }
                }

                foreach (SensorLegoInfo sf in sensorLegoInfo)
                {
                    if (sf.BoneName.Equals(association[i+1]))
                    {
                        sensorLegoInfo.Add(new SensorLegoInfo(sf.InputPort, CurrentBoneAssociation, sf.ObjectName, sf.LocationTrack,sf.OrientationTrack,sf.Axis,sf.Offset, sf.LocPos, sf.RotationOrder));
                        
                        break;
                    }
                }
            }
            
            virtualArmature = false;
        }
       
        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            // access to skeletal tracking information

            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // open the skeleton frame
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons); // get the skeletal information in this frame                    

                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                // access to tracked skeleton data 

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        // render red rectangle when edges are outside the screen
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }

        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            skeletonTracked = skeleton;

            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }

            if (sensorKinectInfo.Count > 0 && captureFlag)
            {
                SendKinectData();
            }

            if (recFlag) 
            {
                
                Packet packet = new Packet();
                packet.header = Command.FRAME;
                packet.payload.Add(currentFrame);
                packet.payload.Add(FrameType.LOCATION_ROTATION);
                List<string> activeObject = getActiveObject();
                foreach (string obj in activeObject)
                {
                    packet.payload.Add(obj);
                }
                currentFrame++;
                this.currentFrameText.Text = "frame: " + currentFrame.ToString();
                string jsontToSend = JsonManager.CreateJson((Object)packet);
                try
                {
                    AsynSocket.Send(jsontToSend);
                }
                catch (Exception ex)
                {
                    this.statusBarText.Text = "Connection Problem";
                }
            }             
            
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));

        }
     
        private void _brick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
            if (legoBrick.Ports[touchPort].SIValue == 1)
            {
                SendCommand(touchCommand, "");
            }
            else
            {
                SendLegoData();
            }
        }

        private Motions createMotion(string objectName, string jointName, float pos_X, float pos_Y, float pos_Z, float or_W, float or_X, float or_Y, float or_Z, bool locXTrack, bool locYTrack, bool locZTrack, bool rotTrack)
        {
            Motions mot = new Motions();
            mot.objectName = objectName;
            mot.boneName = jointName;
            mot.locXTrack = locXTrack;
            mot.locYTrack = locYTrack;
            mot.locZTrack = locZTrack;
            mot.rotTrack = rotTrack;

            Position pos = new Position();
            pos.locX = pos_X;
            pos.locY = pos_Y;
            pos.locZ = pos_Z;
            mot.vectorPos = pos;

            Orientation or = new Orientation();
            or.W = or_W;
            or.X = or_X;
            or.Y = or_Y;
            or.Z = or_Z;
            mot.vectorOr = or;

            return mot;
        }

        private void SendKinectData()
        {
            Packet packet = new Packet();
            if (virtualArmature) 
            {
                packet.header = Command.VIRTUAL_ARMATURE;
            }
            else
            {
                packet.header = Command.POSITION;
            }

            foreach (SensorKinectInfo sf in sensorKinectInfo)
            {
                Joint joint = skeletonTracked.Joints[sf.Joint];
                BoneOrientation orientation = skeletonTracked.BoneOrientations[joint.JointType];
                Motions newMotion = createMotion(sf.ObjectName, sf.BoneName, 0, 0, 0, 1, 0, 0, 0, false, false, false, false);

                newMotion.locXTrack = sf.LocationXTrack;
                newMotion.locYTrack = sf.LocationYTrack;
                newMotion.locZTrack = sf.LocationZTrack;

                if (sf.LocationXTrack)
                {
                    switch (sf.takeXFrom)
                    {
                        case "X":
                            newMotion.vectorPos.locX = -joint.Position.X * factor * sf.Factor - sf.OffsetLocX + sf.LocPosX;
                            break;
                        case "Y":
                            newMotion.vectorPos.locX = joint.Position.Y * factor * sf.Factor - sf.OffsetLocY + sf.LocPosY;
                            break;
                        case "Z":
                            newMotion.vectorPos.locX = -joint.Position.Z * factor * sf.Factor - sf.OffsetLocZ + sf.LocPosZ;
                            break;
                    }
                }

                if (sf.LocationYTrack)
                {
                    switch (sf.takeYFrom)
                    {
                        case "X":
                            newMotion.vectorPos.locY = -joint.Position.X * factor * sf.Factor - sf.OffsetLocX + sf.LocPosX;
                            break;
                        case "Y":
                            newMotion.vectorPos.locY = joint.Position.Y * factor * sf.Factor - sf.OffsetLocY + sf.LocPosY;
                            break;
                        case "Z":
                            newMotion.vectorPos.locY = -joint.Position.Z * factor * sf.Factor - sf.OffsetLocZ + sf.LocPosZ;
                            break;
                    }
                }

                if (sf.LocationZTrack)
                {
                    switch (sf.takeZFrom)
                    {
                        case "X":
                            newMotion.vectorPos.locZ = -joint.Position.X * factor * sf.Factor - sf.OffsetLocX + sf.LocPosX;
                            break;
                        case "Y":
                            newMotion.vectorPos.locZ = joint.Position.Y * factor * sf.Factor - sf.OffsetLocY + sf.LocPosY;
                            break;
                        case "Z":
                            newMotion.vectorPos.locZ = -joint.Position.Z * factor * sf.Factor - sf.OffsetLocZ + sf.LocPosZ;
                            break;
                    }
                }

                //newMotion.vectorPos.locX = -joint.Position.X * factor * sf.Factor - sf.OffsetLocX + sf.LocPosX;
                //newMotion.vectorPos.locY = joint.Position.Y * factor * sf.Factor - sf.OffsetLocY + sf.LocPosY;
                //newMotion.vectorPos.locZ = -joint.Position.Z * factor * sf.Factor - sf.OffsetLocZ + sf.LocPosZ;
                //newMotion.locXTrack = sf.LocationXTrack;
                //newMotion.locYTrack = sf.LocationYTrack;
                //newMotion.locZTrack = sf.LocationZTrack;

                if (sf.OrientationTrack)
                {
                    newMotion.rotTrack = true;
                    newMotion.vectorOr.W = orientation.HierarchicalRotation.Quaternion.W * sf.Factor;
                    newMotion.vectorOr.X = orientation.HierarchicalRotation.Quaternion.X * sf.Factor - sf.OffsetX;
                    newMotion.vectorOr.Y = orientation.HierarchicalRotation.Quaternion.Y * sf.Factor - sf.OffsetY;
                    newMotion.vectorOr.Z = orientation.HierarchicalRotation.Quaternion.Z * sf.Factor - sf.OffsetZ;
                }

                packet.payload.Add(newMotion);
            }
                
            string jsontToSend = JsonManager.CreateJson((Object)packet);
            //System.Console.WriteLine(jsontToSend);
            try
            {
                AsynSocket.Send(jsontToSend);
            }
            catch (Exception ex)
            {
                this.statusBarText.Text = "Connection Problem";
            } 
        }

        public async void StartLegoConnection(string connType, string address)        
        {
            Console.WriteLine("LEGO");

            try
            {
                switch (connType)
                {
                    case LegoConnectionSetup.USB_CONNECTION:
                        var usbType = new UsbCommunication();
                        legoBrick = new Brick(usbType);
                        break;
                    case LegoConnectionSetup.WIFI_CONNECTION:
                        var wifiType = new NetworkCommunication(address);
                        legoBrick = new Brick(wifiType);
                        break;

                }

                legoBrick.BrickChanged += _brick_BrickChanged;
                await legoBrick.ConnectAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
            
            //await legoBrick.DirectCommand.PlayToneAsync(1, 1000, 150);                                

        }

        // Retrives the list of objcet 
        private List<string> getActiveObject() 
        {
            List<string> ActiveObject = new List<string>();
            foreach (SensorLegoInfo sf in sensorLegoInfo)
            {
                if (!ActiveObject.Contains(sf.ObjectName.ToString()))
                {
                    ActiveObject.Add(sf.ObjectName);
                }

            }
            foreach (SensorKinectInfo sf in sensorKinectInfo)
            {
                if (!ActiveObject.Contains(sf.ObjectName.ToString()))
                {
                    ActiveObject.Add(sf.ObjectName);
                }
            }
            return ActiveObject;
        }

        public void ResetSensorInfo() 
        {
            
            foreach (SensorLegoInfo sf in sensorLegoInfo)
            {
                sf.Offset = legoBrick.Ports[sf.InputPort].SIValue;
            }
            

            if(skeletonTracked != null)
            {
                foreach (SensorKinectInfo sf in sensorKinectInfo)
                {
                    Joint joint = skeletonTracked.Joints[sf.Joint];
                    sf.OffsetLocX = -joint.Position.X * factor * sf.Factor;
                    sf.OffsetLocY = joint.Position.Y * factor * sf.Factor;
                    sf.OffsetLocZ = -joint.Position.Z * factor * sf.Factor;
                
                    BoneOrientation orientation = skeletonTracked.BoneOrientations[joint.JointType];
                    sf.OffsetW = orientation.HierarchicalRotation.Quaternion.W * sf.Factor;
                    sf.OffsetX = orientation.HierarchicalRotation.Quaternion.X * sf.Factor;
                    sf.OffsetY = orientation.HierarchicalRotation.Quaternion.Y * sf.Factor;
                    sf.OffsetZ = orientation.HierarchicalRotation.Quaternion.Z * sf.Factor;                    
                }
            }

            //legoBrick.DirectCommand.PlayToneAsync(50, 1000, 50);                                

        }

        public void ChageCoordinateSystem() 
        {
            foreach (SensorLegoInfo sf in sensorLegoInfo)
            {
                sf.LocPos =  sf.LocPos + legoBrick.Ports[sf.InputPort].SIValue - sf.Offset;

            }

            foreach (SensorKinectInfo sf in sensorKinectInfo)
            {
                 Joint joint = skeletonTracked.Joints[sf.Joint];
                 sf.LocPosX = -joint.Position.X * factor * sf.Factor - sf.OffsetLocX + sf.LocPosX;
                 sf.LocPosY = joint.Position.Y * factor * sf.Factor - sf.OffsetLocY + sf.LocPosY;
                 sf.LocPosZ = -joint.Position.Z * factor * sf.Factor - sf.OffsetLocZ + sf.LocPosZ;
            }
        }

        public void GetCurrentFrame() 
        {
            // Request current frame 
            AsynSocket.Send("CURRENT_FRAME_REQUEST");
            // Receive the response from Blender
            string stringJson = AsynSocket.SyncReceiver();
            currentFrame = JsonManager.GetInt(stringJson);
            this.currentFrameText.Text = "frame: " + currentFrame.ToString();
        }

        public void ResetOrigin()
        {
            foreach (SensorLegoInfo sf in sensorLegoInfo)
            {
                sf.LocPos = 0;
            }

            foreach (SensorKinectInfo sf in sensorKinectInfo)
            {
                sf.LocPosX = 0;
                sf.LocPosY = 0;
                sf.LocPosZ = 0;
            }
        }

        // GUI
        
        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            if (legoBrick == null)
            {
                LegoConnectionSetup win = new LegoConnectionSetup(this);
                win.Show();
            }
            else
            {
                try
                {
                   OpenConfigPanel();
               }

                catch
                {
                    MessageBoxResult result = MessageBox.Show(this, "Connection problem", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
     }

        public void OpenConfigPanel()
        {
            // Request armature information
            AsynSocket.Send("ARMATURE_REQUEST");
            // Receive the response from Blender
            string armatureListJson = AsynSocket.SyncReceiver();
            List<string> objects = JsonManager.GetList(armatureListJson);

            GetCurrentFrame();

            // stop bones tracking 
            captureFlag = false;

            ConfigurationPanel win2 = new ConfigurationPanel(this, legoBrick, objects);
            win2.Show();
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }      
        
        // Starts the test
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {            
            SendCommand(Command.START_TEST, "");
            SendCommand(Command.CHANGE_PRESET, currentPreset);
            captureFlag = true;
        }

        // Handles the shortcut maping to launch animation features
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
                        
            if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                SendCommand(Command.FRAME, "");
            }
            
            if (e.Key == Key.R && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                SendCommand(Command.RESET, "");
            }

            if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                GetCurrentFrame();
            }            
            
            if (e.Key == Key.A && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                SendCommand(Command.FAST_FORWARD, "");
            }

            if (e.Key == Key.B && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                filterOn = !filterOn;
                if (filterOn)
                    this.statusBarText.Text = "Low Pass Filter On ";
                else
                    this.statusBarText.Text = "Low Pass Filter Off ";
            }



            if (e.Key == Key.D && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                
            }
            
            /* 
            if (e.Key == Key.L && Keyboard.IsKeyDown(Key.RightCtrl))
            {
                await legoBrick.DirectCommand.StopMotorAsync(OutputPort.All, true);
            }

            if (e.Key == Key.U && Keyboard.IsKeyDown(Key.RightCtrl))
            {
                await legoBrick.DirectCommand.StopMotorAsync(OutputPort.All, false);
            }

            if (e.Key == Key.M && Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.C, 20, 1000, false);
                //await legoBrick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B, 100);
                //await legoBrick.DirectCommand.StopMotorAsync(OutputPort.B, false);
                //await legoBrick.DirectCommand.StepMotorAtSpeedAsync(OutputPort.C, 100,100,false);
                
                //legoBrick.BatchCommand.TurnMotorAtSpeedForTime(OutputPort.B, 50, 1000, false);
                //legoBrick.BatchCommand.TurnMotorAtPowerForTime(OutputPort.C, 50, 1000, false);
                //await legoBrick.BatchCommand.SendCommandAsync();                
                
                LoadPose(0,0,0,0);
            }
            */            
        }                     

        private async void LockPos()
        {
            await legoBrick.DirectCommand.StopMotorAsync(OutputPort.All, true);
        }
        private async void UnLockPos()
        {
            await legoBrick.DirectCommand.StopMotorAsync(OutputPort.All, false);
        }
        
        private async void LoadPose(int valuePortA, int valuePortB, int valuePortC, int valuePortD) 
        {
            int diffA =  (int)legoBrick.Ports[InputPort.A].SIValue - valuePortA;
            int diffB = (int)legoBrick.Ports[InputPort.B].SIValue - valuePortB;
            int diffC = (int)legoBrick.Ports[InputPort.C].SIValue - valuePortC;
            int power = 30;
            uint step = 0;

            if (diffA > 0)
            {
                power = -30;
                step = (uint)diffA;
            }
            else 
            {
                power = 30;
                diffA = diffA * -1;
                step = (uint)diffA;
            }

            legoBrick.BatchCommand.StepMotorAtPower(OutputPort.A, power, step, true);

                        
            if (diffB > 0)
            {
                power = -30;
                step = (uint)diffB;
            }
            else
            {
                power = 30;
                diffA = diffA * -1;
                step = (uint)diffB;
            }

            legoBrick.BatchCommand.StepMotorAtPower(OutputPort.B, power, step, true);
            
            if (diffC > 0)
            {
                power = -30;
                step = (uint)diffC;
            }
            else
            {
                power = 30;
                diffC = diffC * -1;
                step = (uint)diffC;
            }

            legoBrick.BatchCommand.StepMotorAtPower(OutputPort.C, power, step, true);
            await legoBrick.BatchCommand.SendCommandAsync();

        }


        /*
        private async void LoadPose(int valuePortA, int valuePortB, int valuePortC, int valuePortD)
        {
            if (legoBrick.Ports[InputPort.A].Type == DeviceType.MMotor || legoBrick.Ports[InputPort.A].Type == DeviceType.LMotor)
            {
                if ((int)legoBrick.Ports[InputPort.A].SIValue <= valuePortA)
                {
                    while ((int)legoBrick.Ports[InputPort.A].SIValue <= valuePortA)
                    {
                        await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.A, 30, 25, true);
                        Thread.Sleep(50);
                    }
                }
                else
                {
                    while ((int)legoBrick.Ports[InputPort.A].SIValue >= valuePortA)
                    {
                        await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.A, -30, 25, true);
                        Thread.Sleep(50);
                    }
                }
            }

            if (legoBrick.Ports[InputPort.B].Type == DeviceType.MMotor || legoBrick.Ports[InputPort.B].Type == DeviceType.LMotor)
            {
                if ((int)legoBrick.Ports[InputPort.B].SIValue <= valuePortB)
                {
                    while ((int)legoBrick.Ports[InputPort.B].SIValue <= valuePortB)
                    {
                        await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.B, 30, 25, true);
                        Thread.Sleep(50);
                    }
                }
                else
                {
                    while ((int)legoBrick.Ports[InputPort.B].SIValue >= valuePortB)
                    {
                        await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.B, -30, 25, true);
                        Thread.Sleep(50);
                    }
                }
            }

            if (legoBrick.Ports[InputPort.C].Type == DeviceType.MMotor || legoBrick.Ports[InputPort.C].Type == DeviceType.LMotor)
            {
                if ((int)legoBrick.Ports[InputPort.C].SIValue <= valuePortC)
                {
                    while ((int)legoBrick.Ports[InputPort.C].SIValue <= valuePortC)
                    {
                        await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.C, 30, 25, true);
                        Thread.Sleep(50);
                    }
                }
                else
                {
                    while ((int)legoBrick.Ports[InputPort.C].SIValue >= valuePortC)
                    {
                        await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.C, -30, 25, true);
                        Thread.Sleep(50);
                    }
                }
            }

            if (legoBrick.Ports[InputPort.D].Type == DeviceType.MMotor || legoBrick.Ports[InputPort.D].Type == DeviceType.LMotor)
            {
                if ((int)legoBrick.Ports[InputPort.D].SIValue <= valuePortD)
                {
                    while ((int)legoBrick.Ports[InputPort.D].SIValue <= valuePortD)
                    {
                        await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.D, 30, 25, true);
                        Thread.Sleep(50);
                    }
                }
                else
                {
                    while ((int)legoBrick.Ports[InputPort.D].SIValue >= valuePortC)
                    {
                        await legoBrick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.D, -30, 25, true);
                        Thread.Sleep(50);
                    }
                }
            }

        }
        */      

        /*        
        private void DoWork()
        {
            while (true)
            {
                SendLegoData();
                Thread.Sleep(50);
            }
        }
         */
    }
    
}
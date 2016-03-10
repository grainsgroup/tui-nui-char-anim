using System;
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
using System.Net;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{    
    
    public partial class LegoConnectionSetup : Window
    {
        MainWindow mw;
        public const string USB_CONNECTION = "Usb";
        public const string WIFI_CONNECTION = "WiFi";
        public const string BLUETOOTH_CONNECTION = "bluetooth";


        public LegoConnectionSetup(MainWindow mainWin)
        {
            InitializeComponent();
            mw = mainWin;
            this.ConnectionTypeCheckBox.ItemsSource = new string[] {USB_CONNECTION, WIFI_CONNECTION/*, BLUETOOTH_CONNECTION*/};
            this.IpAddressLabel.Visibility = Visibility.Collapsed;
            this.IpAddressTextBox.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {          
            if (this.ConnectionTypeCheckBox.SelectedItem.Equals(USB_CONNECTION))
            {
                mw.StartLegoConnection(this.ConnectionTypeCheckBox.SelectedItem.ToString(), "");                
                this.Close();
            }
            else 
            {
                IPAddress address;
                Boolean IsValidAddress = IPAddress.TryParse(this.IpAddressTextBox.Text.ToString(), out address);
                if (IsValidAddress)
                {
                    mw.StartLegoConnection(this.ConnectionTypeCheckBox.SelectedItem.ToString(),
                    this.IpAddressTextBox.Text.ToString());
                    this.Close();
                }
                else 
                {
                    MessageBoxResult result = MessageBox.Show(this, "Is not valid IP Address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
        }

       

        private void ConnectionTypeCheckBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ConnectionTypeCheckBox.SelectedItem.ToString().Equals(WIFI_CONNECTION)) 
            {
                this.IpAddressLabel.Visibility = Visibility.Visible;
                this.IpAddressTextBox.Visibility = Visibility.Visible;
                this.ConnectionImage.Source = new BitmapImage(new Uri(@"/Images/WiFi.png", UriKind.Relative)); 
            }
            if (this.ConnectionTypeCheckBox.SelectedItem.ToString().Equals(USB_CONNECTION))
            {
                this.IpAddressLabel.Visibility = Visibility.Collapsed;
                this.IpAddressTextBox.Visibility = Visibility.Collapsed;
                this.ConnectionImage.Source = new BitmapImage(new Uri(@"/Images/Usb.png", UriKind.Relative)); 
            }
        }
    }
}

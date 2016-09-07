using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GHIElectronics.UWP.Shields;
using Microsoft.Azure.Devices.Client;
using Microsoft.Devices.Tpm;
using Newtonsoft.Json;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FezHatDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private static DeviceClient _deviceClient = null;
        private static FEZHAT _hat = null;
        private DispatcherTimer _timer = null;    


        public MainPage()
        {
            this.InitializeComponent();
            
            //init device and shield
            TpmDevice mytpmdevice = new TpmDevice(0);
            _deviceClient = DeviceClient.CreateFromConnectionString(mytpmdevice.GetConnectionString()); //SAS Token: 60 min
            initFezhat();

            //timer setup and start -> 1 msg every second
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();

        }

        private async void _timer_Tick(object sender, object e)
        {
            if (_hat == null)
                return;

            //update UWP app UI
            double temp, light, x, y, z;
            temp = _hat.GetTemperature();
            light = _hat.GetLightLevel();
            _hat.GetAcceleration(out x, out y, out z);
          
            Temp.Text = $"{temp:N2} °C";            
            Light.Text = $"{light:P2}";                 
            Accel.Text = $"x={x:N2} y={y:N2} z={z:N2}";

            //create message
            var message = new
            {
                deviceID = "Batman",
                temperatureToSend = temp,
                lightToSend = light,
                xToSend = x,
                ytoSend = y, 
                zToSend = z               
            };

            var messageString = JsonConvert.SerializeObject(message);
            var messageToSend = new Message(Encoding.UTF8.GetBytes(messageString));


            //send message to IoTHub
            await _deviceClient.SendEventAsync(messageToSend);

        }

        public async void initFezhat()
        {
            _hat = await FEZHAT.CreateAsync();
        }

            
    }
}

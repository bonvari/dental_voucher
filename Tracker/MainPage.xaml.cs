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
using uPLibrary.Nfc;
using uPLibrary.Hardware.Nfc;
using Windows.UI.Core;
using System.Threading.Tasks;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Tracker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string UartBridgeName = "UART0";
        const string ShowCart = "لطفاً کارت خود را مقابل دستگاه قرار دهید";
        const string EnterPassword = "لطفاً گذرواژه خود را وارد نمایید";
        const string ConnectingReader = "در حال برقراری ارتباط با دستگاه";
        const string ReaderPortConfigurationFailed = "خطا در برقراری ارتباط با پورت دستگاه";
        const string ReaderReady = "ارتباط با دستگاه با موفقیت اجام شد";
        const string ReaderOpenFailed = "خطا در ارتباط با دستگاه کارتخوان";
        private INfcReader _nfcReader;

        private string password;

        private readonly CoreDispatcher _dispatcher;
        private string code;

        public MainPage()
        {
            this.InitializeComponent();

            keyPanel.Visibility = Visibility.Collapsed;
            loginPanel.Visibility = Visibility.Collapsed;
            loginView.Visibility = Visibility.Collapsed;

            DataContext = this;
            _dispatcher = Window.Current.Dispatcher;

            //start background task to connect to card Reader.
            var task = new Task(async () => await CreateNfcReader());
            task.Start();
        }

        private async Task CreateNfcReader()
        {
            while (true)
            {
                if (_nfcReader == null || _nfcReader != null && !_nfcReader.IsRunning)
                {
                    SetNfcStatus(ConnectingReader);

                    if (_nfcReader != null)
                    {
                        await Task.Delay(2000);
                        _nfcReader.Close();
                    }

                    try
                    {
                        var portCreated = await Pn532CommunicationHsu.CreateSerialPort(UartBridgeName);

                        _nfcReader = new NfcPn532Reader(portCreated);
                        _nfcReader.TagDetected += nfc_TagDetected;
                        _nfcReader.TagLost += nfc_TagLost;

                        await ReaderOpen();
                    }
                    catch (Exception e)
                    {
                        SetNfcStatus(ReaderPortConfigurationFailed);
                    }

                }
            }
        }

        private async Task ReaderOpen()
        {
            try
            {
                await _nfcReader.Open(NfcTagType.MifareUltralight);
                SetNfcStatus(ReaderReady);
            }
            catch (Exception e)
            {
                SetNfcStatus(ReaderOpenFailed);
            }
        }

        private async void SetNfcStatus(string value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { TxtStatus.Text = value; });
        }

        private async void waitForPassword()
        {
            
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                txtCommand.Text = EnterPassword;
                loginPanel.Visibility = Visibility.Visible;
                keyPanel.Visibility = Visibility.Visible;
                loginView.Visibility = Visibility.Collapsed;

            });
        }

        private async void restart()
        {
            password = "";
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                txtCommand.Text = ShowCart;
                loginPanel.Visibility = Visibility.Collapsed;
                keyPanel.Visibility = Visibility.Collapsed;
                txtPassword.Password = "";
            });
        }

        private async void nfc_TagLost(object sender, NfcTagEventArgs e)
        {
            Debug.WriteLine("LOST " + BitConverter.ToString(e.Connection.ID));

            //perform actions on card removed from NFC field
        }

        private async void nfc_TagDetected(object sender, NfcTagEventArgs e)
        {
            var uId = BitConverter.ToString(e.Connection.ID);
            Debug.WriteLine("DETECTED {0}", uId);
            code = uId;

            waitForPassword();

            //1. Get Person
            //var person = DataService.GetPersonByCardId(uId);

            //if (person == null)
            //{
            //    //Call service to get associated person details
            //    var movementResponseDto = await _personService.PostMovement(uId);

            //    if (movementResponseDto == null)
            //    {
            //        return;
            //    }

            //    person = new Person
            //    {
            //        CardUid = movementResponseDto.CardUid,
            //        Id = movementResponseDto.Id,
            //        Name = movementResponseDto.Name,
            //        InLocation = movementResponseDto.Ingress,
            //        Image = movementResponseDto.Image
            //    };
            }


        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            restart();
        }

        private void Key_Click(object sender, RoutedEventArgs e)
        {
            var text = (sender as Button).Content as string;
            switch (text) {
                case "Back":
                    if (password.Length > 0)
                    {
                        password = password.Remove(password.Length - 1);
                        txtPassword.Password = password;
                    }
                    break;
                case "Enter":
                    login();
                    break;
                default:
                    password += text;
                    txtPassword.Password = password;
                    break;
            }
        }

        private void login()
        {
            string password = this.password;
            //TODO: Create HttpClient and call API
            restart();
            loginView.Visibility = Visibility.Visible;
            loginResult.Text = string.Format("شناسه کاربری: {0} - گذرواژه {1}. عملیات با موفقیت انجام شد.", code, password);
        }
    }
    }

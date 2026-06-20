using AllMightyEye;
using SharpHook;
using SharpHook.Data;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace smallTV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // *************************************************** //
        //                                                     //
        //               Loading config params                 //
        //    *It creates links to values stored in config*    //
        //                                                     //
        // *************************************************** //

        Key HotkeyResetFinishKey => Config.Instance.HotkeyResetFinishKey;
        List<Key> TopLeftBKeys = Config.Instance.TopLeftBKeys;
        List<Key> BottomRightBKeys = Config.Instance.BottomRightBKeys;
        List<Key> ToggleKeys = Config.Instance.ToggleKeys;



        // *************************************************** //
        //                                                     //
        //                 Main Window Events                  //
        //                                                     //
        // *************************************************** //
        public MainWindow()
        {
            InitializeComponent();
            UpdateKeyLabels();
            InitializeMediaPlayer();
        }


        // Adding listeners for key events
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SimpleGlobalHook _hook = new SimpleGlobalHook();

            _hook.KeyPressed += OnKeyPressed;
            _hook.KeyReleased += OnKeyReleased;


            await Task.Run(() => _hook.Run());
        }


        // Function responsible for user creating hotkeys
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (
                (TopLeftBKeys.Count == 0 && e.Key != HotkeyResetFinishKey)
                ||
                (TopLeftBKeys[0] != HotkeyResetFinishKey && !TopLeftBKeys.Contains(e.Key))
                )
            {
                TopLeftBKeys.Insert(0, e.Key);
                Config.Instance.Save();
                e.Handled = true;
                UpdateKeyLabels();
            }
            else
            if (
                (BottomRightBKeys.Count == 0 && e.Key != HotkeyResetFinishKey)
                ||
                (BottomRightBKeys[0] != HotkeyResetFinishKey && !BottomRightBKeys.Contains(e.Key))
                )
            {
                BottomRightBKeys.Insert(0, e.Key);
                Config.Instance.Save();
                e.Handled = true;
                UpdateKeyLabels();
            }
            else
            if (
                (ToggleKeys.Count == 0 && e.Key != HotkeyResetFinishKey)
                ||
                (ToggleKeys[0] != HotkeyResetFinishKey && !ToggleKeys.Contains(e.Key))
                )
            {
                ToggleKeys.Insert(0, e.Key);
                Config.Instance.Save();
                e.Handled = true;
                UpdateKeyLabels();
            }
        }



        // *************************************************** //
        //                                                     //
        //          Handling window key press events           //
        //                                                     //
        // *************************************************** //

        private const int _SecondsBeforeRefresh = 1;
        private DateTime _lastKeyUpdate = DateTime.MinValue;
        private static readonly List<Key> PressedKeys = new List<Key>();
        private void OnKeyPressed(object sender, KeyboardHookEventArgs e)
        {
            // locking so action can't be done simultiniously on multiple threads
            lock (PressedKeys) {

                // To avoid bugs clearing pressed keys after some time of inactivity
                if ((DateTime.Now - _lastKeyUpdate).TotalSeconds >= _SecondsBeforeRefresh)
                {
                    PressedKeys.Clear();
                }
                _lastKeyUpdate = DateTime.Now;


                // Parsing key value from event data and handling special keys
                Key pressedKey = GetKeyFromVirtualCode((int)e.Data.RawCode);
                PressedKeys.Add(pressedKey);


                // Invoking corresponding action if key sequince is met
                if (PressedKeys != null)
                {
                    if (TopLeftBKeys != null && TopLeftBKeys.Contains(HotkeyResetFinishKey) 
                        && PressedKeys.SequenceEqual(TopLeftBKeys[1..]))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            TopLeftBFunction();
                        });
                    }


                    if (BottomRightBKeys != null && BottomRightBKeys.Contains(HotkeyResetFinishKey)
                        && PressedKeys.SequenceEqual(BottomRightBKeys[1..]))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            BottomRightBFunction();
                        });
                    }


                    if (ToggleKeys != null && ToggleKeys.Contains(HotkeyResetFinishKey)
                        && PressedKeys.SequenceEqual(ToggleKeys[1..]))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ToggleFunction();
                        });
                    }
                }
            }
        }


        private void OnKeyReleased(object sender, KeyboardHookEventArgs e)
        {
            lock (PressedKeys)
            {
                Key releasedKey = GetKeyFromVirtualCode((int)e.Data.RawCode);
                PressedKeys.Remove(releasedKey);
            }
        }


        /// <summary>
        /// Gets Key type value from virtual key int code
        /// (also handles special keys)
        /// </summary>
        private static Key GetKeyFromVirtualCode(int keyCode)
        {
            Key pressedKey = KeyInterop.KeyFromVirtualKey(keyCode);
            if (keyCode == 160)
            {
                pressedKey = Key.LeftShift;
            }
            else if (keyCode == 162)
            {
                pressedKey = Key.LeftCtrl;
            }
            else if (keyCode == 164)
            {
                pressedKey = Key.LeftAlt;
            }

            return pressedKey;
        }



        // *************************************************** //
        //                                                     //
        //                   Handling UI keys                  //
        //                                                     //
        // *************************************************** //

        private void ResetTopLeftBKeysButton_Click(object sender, RoutedEventArgs e)
        {
            TopLeftBKeys.Clear();
            UpdateKeyLabels();
            this.Focus();
        }

        private void ResetBottomRightBKeysButton_Click(object sender, RoutedEventArgs e)
        {
            BottomRightBKeys.Clear();
            UpdateKeyLabels();
            this.Focus();
        }

        private void ResetToggleKeysButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleKeys.Clear();
            UpdateKeyLabels();
            this.Focus();
        }


        /// <summary>
        /// Updates hotkey labels text
        /// </summary>
        private void UpdateKeyLabels()
        {
            string keySeparator = " -> ";
            int keySeparatorLength = keySeparator.Length;
            string awaitingNewKeyText = "Enter new hotkey";
            string howToStopText = $" (Press {HotkeyResetFinishKey.ToString()} to stop)";


            string keysString = "";

            if (TopLeftBKeys.Count() == 0)
            {
                TopLeftBKeysLabel.Content = awaitingNewKeyText;
            }
            else
            {
                for (int i = 0; i < TopLeftBKeys.Count; i++)
                {
                    Key k = TopLeftBKeys[i];
                    if (k == HotkeyResetFinishKey)
                        continue;
                    keysString += k.ToString() + keySeparator;
                }

                keysString = keysString.Remove(keysString.Length - keySeparatorLength);

                if (TopLeftBKeys[0] != HotkeyResetFinishKey) 
                    keysString += howToStopText;

                TopLeftBKeysLabel.Content = keysString;
            }
            keysString = "";


            if (BottomRightBKeys.Count() == 0)
            {
                BottomRightBKeysLabel.Content = awaitingNewKeyText;
            }
            else
            {
                for (int i = 0; i < BottomRightBKeys.Count; i++)
                {
                    Key k = BottomRightBKeys[i];
                    if (k == HotkeyResetFinishKey)
                        continue;
                    keysString += k.ToString() + keySeparator;
                }

                keysString = keysString.Remove(keysString.Length - keySeparatorLength);

                if (BottomRightBKeys[0] != HotkeyResetFinishKey)
                    keysString += howToStopText;

                BottomRightBKeysLabel.Content = keysString;
            }
            keysString = "";


            if (ToggleKeys.Count() == 0)
            {
                ToggleKeysLabel.Content = awaitingNewKeyText;
            }
            else
            {
                for (int i = 0; i < ToggleKeys.Count; i++)
                {
                    Key k = ToggleKeys[i];
                    if (k == HotkeyResetFinishKey)
                        continue;
                    keysString += k.ToString() + keySeparator;
                }

                keysString = keysString.Remove(keysString.Length - keySeparatorLength);

                if (ToggleKeys[0] != HotkeyResetFinishKey)
                    keysString += howToStopText;

                ToggleKeysLabel.Content = keysString;
            }
            keysString = "";
        }


        private void PercentSelectedAsDiiferenceTriggerInComboBox(object sender, RoutedEventArgs e)
        {
            IsUsingPercentDifference = true;
        }

        private void PixelSelectedAsDiiferenceTriggerInComboBox(object sender, RoutedEventArgs e)
        {
            IsUsingPercentDifference = false;
        }


        private void TriggerValueChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(TriggerValueTextBox.Text, out float newvalue))
            {
                triggerDifferenceValue = newvalue;
            }
            else
            {
                triggerDifferenceValue = 0;
            }

            TriggerValueTextBox.Text = triggerDifferenceValue.ToString();
        }


        private void SoundCapValueChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(SoundCapValueTextBox.Text, out float newvalue))
            {
                soundCapValue = newvalue;
            }
            else
            {
                soundCapValue = 1;
            }
            if (soundCapValue == 0) soundCapValue = 1;

            SoundCapValueTextBox.Text = soundCapValue.ToString();
        }


        // *************************************************** //
        //                                                     //
        //                   Action functions                  //
        //                                                     //
        // *************************************************** //

        POINT border1 = new POINT();
        POINT border2 = new POINT();
        public void TopLeftBFunction()
        {
            GetCursorPos(out border1);
            Console.Beep(800, 500);
        }


        public void BottomRightBFunction()
        {
            GetCursorPos(out border2);
            Console.Beep(800, 500);
        }


        static Screenshooter scr = new Screenshooter();
        static int[] reference = [];
        static int[] temp = [];
        static int x1 = 0;
        static int y1 = 0;
        static int x2 = 0;
        static int y2 = 0;
        static volatile bool IsThreadActive = false;
        static volatile bool IsUsingPercentDifference = true;
        static float triggerDifferenceValue = 1;
        static float soundCapValue = 15;
        Thread DetectorLoopThread;
        public void ToggleFunction()
        {
            x1 = Math.Min(border1.X, border2.X);
            y1 = Math.Min(border1.Y, border2.Y);
            x2 = Math.Max(border1.X, border2.X);
            y2 = Math.Max(border1.Y, border2.Y);
            int length = (x2 - x1) * (y2 - y1);
            if (length == 0) return;

            reference = new int[length];
            temp = scr.CaptureScreenRectangle(x1, y1, x2 - x1, y2 - y1);
            Buffer.BlockCopy(temp, 0, reference, 0, length * sizeof(int));


            IsThreadActive = !IsThreadActive;
            if (IsThreadActive)
            {
                DetectorLoopThread = new Thread(DetectorLoop);
                DetectorLoopThread.IsBackground = true;
                _mediaPlayer.Play();
                DetectorLoopThread.Start();
                Console.Beep(1200, 500);
            }
            else
            {
                Console.Beep(1200, 500);
                _mediaPlayer.Volume = 0;
                DetectorLoopThread.Join();
                _mediaPlayer.Stop();
                Console.Beep(1200, 500);
            }
        }


        static MediaPlayer _mediaPlayer;
        static void DetectorLoop()
        {
            double mindellay = 200;
            double timediff = 0;
            DateTime lastActivated = DateTime.Now;
            Screenshooter.ImageCompareResult compres;

            float volume = 0;
            while (IsThreadActive)
            {
                timediff = (DateTime.Now - lastActivated).TotalMilliseconds;
                if (timediff < mindellay)
                {
                    Thread.Sleep(10);
                    continue;
                }
                lastActivated = DateTime.Now;


                temp = scr.CaptureScreenRectangle(x1, y1, x2 - x1, y2 - y1);
                compres = Screenshooter.CompareImages(reference, temp);
                if (IsUsingPercentDifference)
                {
                    volume = ((100 - compres.matchingPercent * 100) - triggerDifferenceValue) / soundCapValue;
                }
                else
                {
                    volume = (compres.differenceInPixels - triggerDifferenceValue) / soundCapValue;
                }
                volume = Math.Clamp(volume, 0, 1);
                App.Current.Dispatcher.Invoke(() =>
                {
                    _mediaPlayer.Volume = volume;
                });
            }
        }

        private static void InitializeMediaPlayer()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Open(new Uri("Sounds\\triggersound.mp3", UriKind.Relative));
            _mediaPlayer.MediaEnded += MediaPlayerRestarter;
            _mediaPlayer.Volume = 0;
        }

        private static void MediaPlayerRestarter(object sender, EventArgs e)
        {
            _mediaPlayer.Position = TimeSpan.Zero;
            _mediaPlayer.Play();
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // 2. Import the GetCursorPos function from user32.dll
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);
    }
}
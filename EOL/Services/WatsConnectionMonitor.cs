using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Virinco.WATS.Interface;
using Virinco.WATS.Interface.MES.Software;


public class WatsConnectionMonitor
{
    public readonly TDM _tdm;
    public readonly Software _software;
    private readonly DispatcherTimer _connectionTimer;

    public event Action<bool> ConnectionStatusChanged;
    private bool _isConnected;

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                ConnectionStatusChanged?.Invoke(_isConnected);
            }
        }
    }

    public WatsConnectionMonitor(TDM tdm)
    {

        _tdm = tdm;

        try
        {
            // Initialize the API with Default mode and allow metadata download
            //_tdm.InitializeAPI(TDM.InitializationMode.Syncronous, true);

            // Optional but recommended: Register the client
            //_tdm.RegisterClient(url);
            // Subscribe to client state changed

            _connectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _connectionTimer.Tick += ConnectServer;
            _connectionTimer.Start();


        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize WATS API: {ex.Message}", "WATS Initialization Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private void ConnectServer(object sender , EventArgs e)
    {
        Task.Run(() =>
        {
            try
            {
                bool isconnected = _tdm.ConnectServer(UpdateMetadata: false, Timeout: TimeSpan.FromSeconds(5));

                //Console.WriteLine($"{DateTime.Now:T} - {statusMsg}");
                if(isconnected != IsConnected)
                {
                    IsConnected = isconnected;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ConnectServer exception: {ex.Message}");
            }
        });
    }


    public class InitializationMethod
{
    public bool DownloadMetadata { get; set; }
}
}
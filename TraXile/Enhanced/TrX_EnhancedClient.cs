using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using log4net;
using System.Threading;
using System.Diagnostics.Eventing.Reader;
using TraXile.Enhanced;
using TraXile;

namespace Traxile.Enanced
{
    public class TEApiClient
    {
        // Base http client
        private HttpClient client;

        // Queue
        private Queue<TEApiQueueItem> actQueue;

        // API Key
        private string _apiKey;

        // API Base URL
        private string _apiUrl;

        // Logger
        private ILog _log;

        // Lock for threadsafe queue
        private object _qlock;

        // Check Interval for Worker
        private int _checkInterval = 10000;

        // Q Thread
        private Thread _qThread;

        // Unique Client ID
        private string _clientID;

        // Sucessful Post event
        public TrX_ActivityEventHandler OnActivitySuccess;

        // Parent Logic
        private TrX_CoreLogic _logic;

        // Is Started?
        private bool _started;
        public bool IsStarted => _started;

        private bool _maxQueueLogged;

        // Maximum number of items in Queue
        private int _maxQueueSize;
        public int MaxQueueSize
        {
            get { return _maxQueueSize; }
            set { _maxQueueSize = value; }
        }

        public TEApiClient(string apiUrl, string apiKey, TrX_CoreLogic logic)
        {
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _logic = logic;

            _apiKey = apiKey;
            _apiUrl = apiUrl;

            _qlock = new object();

            _maxQueueSize = 1000;

            client = new HttpClient() { BaseAddress = new Uri(_apiUrl) };

            // generate unique id for remote api - encrypt it to do not allow traxile backend to identify the user!
            _clientID = TrX_Helpers.ComputeSha256Hash($"{Environment.MachineName} {Environment.UserDomainName} {Environment.UserName}");

            actQueue = new Queue<TEApiQueueItem>();

            _log.Info($"Traxile Enhanced API client initialized. Base URL is {_apiUrl}, ClientID is {_clientID}");
        }

        public void Start()
        {
            _log.Info("Starting TEApiClient");
            _qThread = new Thread(new ThreadStart(QueueHandling))
            {
                IsBackground = true,
                Name = "ApiClientQHandlingThread"
            };
            _qThread.Start();
            _started = true;
        }

        public void Stop()
        {
            _qThread.Abort();
            _started = false;
        }

        private void QueueHandling()
        {
            TEApiQueueItem peek;
            while (true)
            {
                Thread.Sleep(_checkInterval);

                while (actQueue.Count > 0)
                {
                    peek = actQueue.Peek();

                    Task<bool> task;
                    bool result = false;

                    if (peek.Type == TEApiQueueItemPostType.SINGLE)
                    {
                        task = PostActivity(peek.Item);
                        task.Wait();
                        result = task.Result;
                    }

                    if (result == true)
                    {
                        lock (_qlock)
                        {
                            actQueue.Dequeue();
                        }

                        TrX_CoreLogicActivityEventArgs args;

                        if (peek.Type == TEApiQueueItemPostType.SINGLE)
                        {
                            args = new TrX_CoreLogicActivityEventArgs(_logic, peek.Item);
                            OnActivitySuccess(args);
                        }
                        else
                        {
                            foreach (TrX_TrackedActivity act in peek.Items)
                            {
                                args = new TrX_CoreLogicActivityEventArgs(_logic, peek.Item);
                                OnActivitySuccess(args);
                            }
                        }

                    }

                    Thread.Sleep(10);
                }
            }
        }

        public bool EnqueueActivity(TrX_TrackedActivity activity)
        {
            if (actQueue.Count < _maxQueueSize)
            {
                activity.QueuedForAPISync = true;
                _maxQueueLogged = false;
                TEApiQueueItem item = new TEApiQueueItem
                {
                    Item = activity
                };
                actQueue.Enqueue(item);
                return true;
            }
            else
            {
                if (!_maxQueueLogged)
                {
                    _log.Warn("Maximum number of items in queue for API sync. Enqueuing suspended.");
                    _maxQueueLogged = true;
                }
                return false;
            }
        }

        private async Task<bool> PostActivity(TrX_TrackedActivity activity)
        {
            HttpRequestMessage req = new HttpRequestMessage();
            try
            {
                _log.Debug($"Try to post activity: {activity.UniqueID}");

                req.Method = HttpMethod.Post;
                req.Headers.Add("Authorization", $"Bearer {_clientID}");
                string content = activity.ToJSON(_clientID);

                req.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage resp = await client.SendAsync(req);

                if (resp.IsSuccessStatusCode)
                {
                    _log.Debug($"Post to te_api successful: {activity.UniqueID}");
                    return true;
                }
                else
                {
                    _log.Error($"Post to te_api not successful: {activity.UniqueID}, error code was: {resp.StatusCode}");
                    string respMessage = await resp.Content.ReadAsStringAsync();
                    _log.Debug($"Error Message from API: {respMessage}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                _log.Error($"Cannot post activity to Backend: {ex.Message}");
                _log.Debug(ex.ToString());
                return false;
            }
        }

    }
}


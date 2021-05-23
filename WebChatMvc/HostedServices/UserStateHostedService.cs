using Application.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebChatMvc.Hubs;

namespace WebChatMvc.HostedServices
{
    public class UserStateHostedService : IHostedService, IDisposable
    {
        public virtual async Task Execute()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var hubContext = scope.ServiceProvider.GetService<IHubContext<WebChatHub>>();
                var userService = scope.ServiceProvider.GetService<UserService>();

                var offlineUsers = await userService.GetUsersOfflineSince(_lastExecutionTime);
                var events = new List<Task>(offlineUsers.Count);
                foreach (var user in offlineUsers)
                    events.Add(hubContext.Clients.All.SendAsync("UserLoggedOut", user));

                await Task.WhenAll(events);
            }
            _lastExecutionTime = DateTimeOffset.Now;
        }

        protected Timer Timer;
        protected IServiceProvider ServiceProvider { get; }

        protected volatile int _intervalInSeconds;
        private volatile bool _running;

        private DateTimeOffset _lastExecutionTime;

        public UserStateHostedService(IServiceProvider services)
        {
            ServiceProvider = services;
            _intervalInSeconds = 60;
            _lastExecutionTime = DateTimeOffset.Now.AddSeconds(-_intervalInSeconds);
        }

        private async void ExecuteJob(object state)
        {
            try
            {
                Timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                await Execute();
            }
            finally
            {
                if (_running)
                    Timer?.Change(TimeSpan.FromSeconds(_intervalInSeconds), TimeSpan.FromSeconds(_intervalInSeconds));
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _running = true;
            Timer = new Timer(ExecuteJob, null, TimeSpan.Zero, TimeSpan.FromSeconds(_intervalInSeconds));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _running = false;
            Timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Timer?.Dispose();
        }
    }
}

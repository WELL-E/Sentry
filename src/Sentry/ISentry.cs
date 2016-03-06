﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry
{
    public interface ISentry
    {
        Task ExecuteAsync();
    }

    public class Sentry : ISentry
    {
        private readonly SentryConfiguration _configuration;

        public Sentry(SentryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Sentry configuration has not been provided.");

            _configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            var tasks = _configuration.Watchers.Select(async watcherConfiguration =>
            {
                var watcher = watcherConfiguration.Watcher;
                try
                {
                    await watcher.ExecuteAsync();
                    await InvokeOnSuccessHooksAsync(watcherConfiguration);

                }
                catch (Exception exception)
                {
                    await InvokeOnFailureHooksAsync(watcherConfiguration, exception);
                    throw new SentryException("There was an error while executing Sentry " +
                                              $"caused by watcher: '{watcher.GetType().Name}'", exception);
                }
                finally
                {
                    await InvokeOnCompletedHooksAsync(watcherConfiguration);
                }
            });
            await Task.WhenAll(tasks);
        }

        private async Task InvokeOnSuccessHooksAsync(WatcherConfiguration watcherConfiguration)
        {
            watcherConfiguration.Hooks.OnSuccess.Invoke();
            await watcherConfiguration.Hooks.OnSuccessAsync();
            _configuration.Hooks.OnSuccess.Invoke();
            await _configuration.Hooks.OnSuccessAsync();
        }

        private async Task InvokeOnFailureHooksAsync(WatcherConfiguration watcherConfiguration, Exception exception)
        {
            watcherConfiguration.Hooks.OnFailure.Invoke(exception);
            await watcherConfiguration.Hooks.OnFailureAsync(exception);
            _configuration.Hooks.OnFailure.Invoke(exception);
            await _configuration.Hooks.OnFailureAsync(exception);
        }

        private async Task InvokeOnCompletedHooksAsync(WatcherConfiguration watcherConfiguration)
        {
            watcherConfiguration.Hooks.OnCompleted.Invoke();
            await watcherConfiguration.Hooks.OnCompletedAsync();
            _configuration.Hooks.OnCompleted.Invoke();
            await _configuration.Hooks.OnCompletedAsync();
        }
    }
}
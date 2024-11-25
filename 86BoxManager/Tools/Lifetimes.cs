using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;

namespace _86BoxManager.Tools
{
    internal static class Lifetimes
    {
        public static (AppBuilder builder, Func<int> after) SetupWithClassicDesktopLifetime1(
            this AppBuilder builder, string[] args, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
        {
            var lifetime = new ClassicDesktopStyleApplicationLifetime
            {
                Args = args,
                ShutdownMode = shutdownMode
            };
            builder.SetupWithLifetime(lifetime);
            int After() => lifetime.Start(args);
            return (builder, After);
        }
    }
}
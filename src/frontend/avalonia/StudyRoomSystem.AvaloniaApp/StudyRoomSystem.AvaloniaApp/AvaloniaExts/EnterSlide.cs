using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Serilog;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.AvaloniaApp.AvaloniaExts;

public class EnterSlide : IPageTransition
{
    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        List<Task> tasks = [];
        var duration = TimeSpan.FromSeconds(0.3);
        var easing = new SplineEasing(0.215, 0.61, 0.355, 1);
        // var easing = new CubicEaseOut();
        if (from is not null)
        {
            Animation fromAnimation = new()
            {
                Easing = easing,
                Children =
                {
                    new KeyFrame()
                    {
                        Cue = new(0),
                        Setters =
                        {
                            new Setter(TranslateTransform.YProperty, 0.0), new Setter(Visual.OpacityProperty, 1.0)
                        },
                    },
                    new KeyFrame()
                    {
                        Cue = new(1),
                        Setters =
                        {
                            new Setter(TranslateTransform.YProperty, forward ? -32.0 : 32.0),
                            new Setter(Visual.OpacityProperty, 0.0)
                        },
                    },
                },
                Duration = duration
            };
            tasks.Add(fromAnimation.RunAsync(from, cancellationToken));
        }

        if (to is not null)
        {
            to.IsVisible = true;
            Animation toAnimation = new()
            {
                Easing = easing,
                Children =
                {
                    new KeyFrame()
                    {
                        Cue = new(0),
                        Setters =
                        {
                            new Setter(TranslateTransform.YProperty, forward ? 32.0 : -32.0),
                            new Setter(Visual.OpacityProperty, 0.0)
                        },
                    },
                    new KeyFrame()
                    {
                        Cue = new(1),
                        Setters =
                        {
                            new Setter(TranslateTransform.YProperty, 0.0), new Setter(Visual.OpacityProperty, 1.0)
                        },
                    },
                },
                Duration = duration
            };
            tasks.Add(toAnimation.RunAsync(to, cancellationToken));
        }
        await Task.WhenAll(tasks);
        if (cancellationToken.IsCancellationRequested)
            return;
        from?.IsVisible = false;
    }
}
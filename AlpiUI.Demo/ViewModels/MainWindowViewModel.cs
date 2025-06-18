using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;

namespace AlpiUI.Demo.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SourceList<string> __messages = new();

    private readonly ReadOnlyObservableCollection<string> _messages;
    public ReadOnlyObservableCollection<string> Messages => _messages;
    
    public ReactiveCommand<Unit, Unit> Refresh { get; }
    
     public MainWindowViewModel()
     {
         __messages
             .Connect()
             .Bind(out _messages)
             .Subscribe();

         Refresh = ReactiveCommand.Create(() =>
         {
             __messages.Clear();
             
             var result = GenerateMessages(); 
         });

         _ = GenerateMessages();
     }

    private async Task GenerateMessages()
    {
        await Task.Run(async () =>
        {
            for (int i = 0; i < 20; i++)
            {
                __messages.Add("Message");
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        });
    }
}
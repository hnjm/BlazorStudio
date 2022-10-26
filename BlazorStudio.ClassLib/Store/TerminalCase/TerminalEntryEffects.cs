﻿using System.Collections.Concurrent;
using System.Diagnostics;
using BlazorStudio.ClassLib.Store.FooterWindowCase;
using Fluxor;

namespace BlazorStudio.ClassLib.Store.TerminalCase;

public class TerminalEntryEffects
{
    private readonly SemaphoreSlim _executeHandleEffectSemaphoreSlim = new(1, 1);
    private readonly ConcurrentQueue<Func<Task>> _handleEffectQueue = new();
    private readonly TerminalEntry _terminalEntry;
    private readonly IState<TerminalSettingsState> _terminalSettingsStateWrap;
    private readonly IState<TerminalState> _terminalStateWrap;

    public TerminalEntryEffects(IState<TerminalState> terminalStateWrap,
        IState<TerminalSettingsState> terminalSettingsStateWrap,
        TerminalEntry terminalEntry)
    {
        _terminalStateWrap = terminalStateWrap;
        _terminalSettingsStateWrap = terminalSettingsStateWrap;
        _terminalEntry = terminalEntry;
    }

    private async Task QueueHandleEffectAsync(Func<Task> func)
    {
        _handleEffectQueue.Enqueue(func);

        try
        {
            await _executeHandleEffectSemaphoreSlim.WaitAsync();

            if (_handleEffectQueue.TryDequeue(out var fifoHandleEffect)) await fifoHandleEffect!.Invoke();
        }
        finally
        {
            _executeHandleEffectSemaphoreSlim.Release();
        }
    }

    public async Task HandleEnqueueProcessOnTerminalEntryAction(
        EnqueueProcessOnTerminalEntryAction enqueueProcessOnTerminalEntryAction,
        IDispatcher dispatcher)
    {
        await QueueHandleEffectAsync(async () =>
        {
            if (enqueueProcessOnTerminalEntryAction.CancellationToken.IsCancellationRequested)
            {
                if (enqueueProcessOnTerminalEntryAction.OnCancelled is not null)
                    enqueueProcessOnTerminalEntryAction.OnCancelled.Invoke();

                return;
            }

            dispatcher.Dispatch(new ClearTerminalEntryOutputStatesAction(_terminalEntry.TerminalEntryKey));

            var process = new Process();

            void EnqueueProcessOnTerminalEntryActionOnKillRequestedEventHandler(object? sender, EventArgs e)
            {
                process.Kill(true);

                dispatcher.Dispatch(new SetTerminalEntryOutputStatesAction(_terminalEntry.TerminalEntryKey,
                    _terminalEntry.ParseOutputFunc("\n--------------\nkilled process\n--------------\n")));
            }

            if (enqueueProcessOnTerminalEntryAction.WorkingDirectoryAbsoluteFilePath is not null)
            {
                process.StartInfo.WorkingDirectory = enqueueProcessOnTerminalEntryAction
                    .WorkingDirectoryAbsoluteFilePath
                    .GetAbsoluteFilePathString();
            }

            var bash = "/bin/bash";

            if (File.Exists(bash))
            {
                process.StartInfo.FileName = bash;
                process.StartInfo.Arguments = $"-c \"{enqueueProcessOnTerminalEntryAction.Command}\"";
            }
            else
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {enqueueProcessOnTerminalEntryAction.Command} 2>&1";
            }

            // Start the child process.
            // 2>&1 combines stdout and stderr
            //process.StartInfo.Arguments = $"";
            // Redirect the output stream of the child process.
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            void OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                dispatcher.Dispatch(new SetTerminalEntryOutputStatesAction(_terminalEntry.TerminalEntryKey,
                    _terminalEntry.ParseOutputFunc(e.Data ?? string.Empty)));

                if (enqueueProcessOnTerminalEntryAction.OnAnyDataReceivedAsync != null)
                    enqueueProcessOnTerminalEntryAction.OnAnyDataReceivedAsync(sender, e);
            }

            if (enqueueProcessOnTerminalEntryAction.OnAnyDataReceivedAsync is not null)
                process.OutputDataReceived += OutputDataReceived;

            try
            {
                enqueueProcessOnTerminalEntryAction.KillRequestedEventHandler +=
                    EnqueueProcessOnTerminalEntryActionOnKillRequestedEventHandler;

                dispatcher.Dispatch(new SetTerminalEntryIsExecutingAction(_terminalEntry.TerminalEntryKey,
                    true));

                enqueueProcessOnTerminalEntryAction.OnStart.Invoke();

                if (_terminalSettingsStateWrap.Value.ShowTerminalOnProcessStarted)
                {
                    List<(TerminalEntryKey key, int index)> terminalTuples = _terminalStateWrap.Value.TerminalEntries
                        .Select((terminal, index) => (terminal.TerminalEntryKey, index))
                        .ToList();

                    var myIndex = terminalTuples
                        .First(x => x.key == _terminalEntry.TerminalEntryKey)
                        .index;

                    dispatcher.Dispatch(new SetActiveTerminalEntryAction(myIndex));
                }

                dispatcher.Dispatch(new SetActiveFooterWindowKindAction(FooterWindowKind.Terminal));

                process.Start();

                if (enqueueProcessOnTerminalEntryAction.OnAnyDataReceivedAsync is not null)
                    process.BeginOutputReadLine();
                else if (enqueueProcessOnTerminalEntryAction.OnAnyDataReceived is not null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();

                    dispatcher.Dispatch(new SetTerminalEntryOutputStatesAction(_terminalEntry.TerminalEntryKey,
                        _terminalEntry.ParseOutputFunc(output)));

                    enqueueProcessOnTerminalEntryAction.OnAnyDataReceived
                        .Invoke(output);
                }
                else
                {
                    var output = await process.StandardOutput.ReadToEndAsync();

                    dispatcher.Dispatch(new SetTerminalEntryOutputStatesAction(_terminalEntry.TerminalEntryKey,
                        _terminalEntry.ParseOutputFunc(output)));
                }

                await process.WaitForExitAsync();
            }
            finally
            {
                if (enqueueProcessOnTerminalEntryAction.OnAnyDataReceivedAsync is not null)
                {
                    process.CancelOutputRead();
                    process.OutputDataReceived -= OutputDataReceived;
                }

                enqueueProcessOnTerminalEntryAction.OnEnd.Invoke(process);

                enqueueProcessOnTerminalEntryAction.KillRequestedEventHandler -=
                    EnqueueProcessOnTerminalEntryActionOnKillRequestedEventHandler;

                dispatcher.Dispatch(new SetTerminalEntryIsExecutingAction(_terminalEntry.TerminalEntryKey,
                    false));
            }
        });
    }
}
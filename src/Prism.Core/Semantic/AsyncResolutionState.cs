// @file AsyncResolutionState.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Prism.Core.Semantic;

internal enum AsyncResolutionState
{
    Unstarted,
    Running,
    Completed,
    Faulted,
    Canceled,
}

internal sealed class AsyncResolutionState<T>
    where T : notnull
{
    public delegate Task<T> AsyncTaskFactory<TContext>(in TContext context)
        where TContext : allows ref struct;

    private readonly Lock _lock = new();
    private Task<T>? _task;

    public bool IsStarted
    {
        get
        {
            using var scope = _lock.EnterScope();
            return _task is not null;
        }
    }

    public bool IsCompleted
    {
        get
        {
            using var scope = _lock.EnterScope();
            return _task is { IsCompleted: true };
        }
    }

    public T Result
    {
        get
        {
            using var scope = _lock.EnterScope();
            return _task is { IsCompletedSuccessfully: true }
                ? _task.Result
                : throw new InvalidOperationException("The task has not completed successfully.");
        }
    }

    public AsyncResolutionState State
    {
        get
        {
            using var scope = _lock.EnterScope();
            return _task switch
            {
                null => AsyncResolutionState.Unstarted,
                { IsCompletedSuccessfully: true } => AsyncResolutionState.Completed,
                { IsFaulted: true } => AsyncResolutionState.Faulted,
                { IsCanceled: true } => AsyncResolutionState.Canceled,
                _ => AsyncResolutionState.Running,
            };
        }
    }

    public Task<T> GetOrStart(Func<Task<T>> factory)
    {
        using var scope = _lock.EnterScope();
        _task ??= Task.Run(factory);
        return _task;
    }

    public Task<T> GetOrStart<TContext>(in TContext context, AsyncTaskFactory<TContext> factory)
        where TContext : allows ref struct
    {
        using var scope = _lock.EnterScope();
        _task ??= factory(in context);
        return _task;
    }

    public bool TryGetCompleted([NotNullWhen(true)] out T? value)
    {
        using var scope = _lock.EnterScope();
        if (_task is { IsCompletedSuccessfully: true })
        {
            value = _task.Result;
            return true;
        }

        value = default;
        return false;
    }
}

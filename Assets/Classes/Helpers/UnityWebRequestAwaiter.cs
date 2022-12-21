using System;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace Classes.Helpers
{
    public class UnityWebRequestAwaiter : INotifyCompletion
    {
        private UnityWebRequestAsyncOperation _asyncOperation;
        private Action _continuation;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation operation)
        {
            _asyncOperation = operation;
            _asyncOperation.completed += OnRequestCompleted;
        }

        public bool IsCompleted { get { return _asyncOperation.isDone; } }

        public void GetResult() { }

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }

        private void OnRequestCompleted(UnityEngine.AsyncOperation operation)
        {
            _continuation?.Invoke();
        }
    }

    public static class Extensions
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation operation)
        {
            return new UnityWebRequestAwaiter(operation);
        }
    }
}

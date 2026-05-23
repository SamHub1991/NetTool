using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DeerFlow.WPF.Core
{
    /// <summary>
    /// 支持线程安全、异步加载和批量操作的 ObservableCollection
    /// 提供加载��态跟踪、批量增删改、通知抑制、异常安全。WPF主线程安全调度。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        private bool _isLoading;
        private bool _suppressNotification;
        private readonly object _syncRoot = new();
        private readonly Dispatcher? _dispatcher = Dispatcher.FromThread(Thread.CurrentThread) ?? Dispatcher.CurrentDispatcher;

        /// <summary>
        /// 是否正在加载数据
        /// </summary>
        public bool IsLoading
        {
            get { lock (_syncRoot) { return _isLoading; } }
            set
            {
                lock (_syncRoot)
                {
                    _isLoading = value;
                    InvokeOnUI(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
                }
            }
        }

        /// <summary>
        /// 线程安全的批量添加（单次通知）
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            lock (_syncRoot)
            {
                try
                {
                    _suppressNotification = true;
                    foreach (var item in items)
                        Add(item);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("AddRange批量添加失败", ex);
                }
                finally
                {
                    _suppressNotification = false;
                    InvokeOnUI(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
                }
            }
        }

        /// <summary>
        /// 异步批量添加
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<T> items)
            => await Task.Run(() => AddRange(items));

        /// <summary>
        /// 线程安全的批量替换（单次通知）
        /// </summary>
        public void ReplaceAll(IEnumerable<T> items)
        {
            lock (_syncRoot)
            {
                try
                {
                    _suppressNotification = true;
                    Clear();
                    foreach (var item in items)
                        Add(item);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("ReplaceAll批量替换失败", ex);
                }
                finally
                {
                    _suppressNotification = false;
                    InvokeOnUI(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
                }
            }
        }

        /// <summary>
        /// 异步批量替换
        /// </summary>
        public async Task ReplaceAllAsync(IEnumerable<T> items)
            => await Task.Run(() => ReplaceAll(items));

        /// <summary>
        /// 线程安全的批量移除（单次通知）
        /// </summary>
        public void RemoveRange(IEnumerable<T> items)
        {
            lock (_syncRoot)
            {
                try
                {
                    _suppressNotification = true;
                    foreach (var item in items.ToList())
                        Remove(item);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("RemoveRange批量移除失败", ex);
                }
                finally
                {
                    _suppressNotification = false;
                    InvokeOnUI(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)));
                }
            }
        }

        /// <summary>
        /// 异步批量移除
        /// </summary>
        public async Task RemoveRangeAsync(IEnumerable<T> items)
            => await Task.Run(() => RemoveRange(items));

        /// <inheritdoc/>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                InvokeOnUI(() => base.OnCollectionChanged(e));
        }

        /// <summary>
        /// 调度到UI线程执行集合变更
        /// </summary>
        private void InvokeOnUI(Action action)
        {
            if (_dispatcher != null && !_dispatcher.CheckAccess())
                _dispatcher.Invoke(action);
            else
                action();
        }
    }
}

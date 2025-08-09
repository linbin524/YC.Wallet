using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace YC.WalletApp.Domain.Utils
{
    /// <summary>
    /// 高可靠定时任务服务（线程安全/自动重试/熔断机制）
    /// </summary>
    public sealed class TimerServiceUtils : IDisposable
    {
        #region 配置参数类
        /// <summary>
        /// 定时任务配置参数
        /// </summary>
        public class TimerConfig
        {
            /// <summary>
            /// 任务唯一标识（必须唯一）
            /// </summary>
            public string TaskId { get; set; }

            /// <summary>
            /// 首次执行延迟（秒，默认0-立即执行）
            /// </summary>
            public int InitialDelay { get; set; } = 0;

            /// <summary>
            /// 正常执行间隔（秒，必须大于0）
            /// </summary>
            public int NormalInterval { get; set; }

            /// <summary>
            /// 重试间隔策略（秒，默认[30,60,180,600,1800]）
            /// </summary>
            public int[] RetryIntervals { get; set; } = { 30, 60, 180, 600, 1800 };

            /// <summary>
            /// 最大重试次数（默认3次）
            /// </summary>
            public int MaxRetries { get; set; } = 3;

            /// <summary>
            /// 任务执行逻辑（异步方法）
            /// </summary>
            public Func<Task> TaskAction { get; set; }

            /// <summary>
            /// 异常处理回调
            /// </summary>
            public Action<Exception> ErrorHandler { get; set; }

            /// <summary>
            /// UI调度器（WPF必须设置）
            /// </summary>
            /// <summary>
            /// 必须使用类型名访问静态属性
            /// </summary>
            public Dispatcher UIDispatcher { get; set; } = Dispatcher.CurrentDispatcher; // 正确访问方式
        }
        #endregion

        #region 核心实现
        // 任务存储字典（线程安全）
        private readonly ConcurrentDictionary<string, (Timer timer, TimerConfig config)> _tasks = new();

        // 重试计数器（线程安全）
        private readonly ConcurrentDictionary<string, int> _retryCounters = new();

        // 释放标志
        private bool _disposed;

        /// <summary>
        /// 创建定时任务
        /// </summary>
        public void ScheduleTask(TimerConfig config)
        {
            ValidateConfig(config);

            // 创建专用Timer实例
            var timer = new Timer(async _ =>
            {
                try
                {
                    await ExecuteTask(config);
                }
                catch (Exception ex)
                {
                    HandleError(config, ex);
                }
            }, null, Timeout.Infinite, Timeout.Infinite);

            // 存储任务信息
            _tasks[config.TaskId] = (timer, config);

            // 首次启动
            StartTimer(config.TaskId, config.InitialDelay * 1000);
        }

        /// <summary>
        /// 停止并移除任务
        /// </summary>
        public void StopTask(string taskId)
        {
            if (_tasks.TryRemove(taskId, out var task))
            {
                task.timer.Dispose();
                _retryCounters.TryRemove(taskId, out _);
            }
        }

        // 验证配置有效性
        private void ValidateConfig(TimerConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(config.TaskId))
                throw new ArgumentException("TaskId不能为空");
            if (config.NormalInterval <= 0)
                throw new ArgumentException("NormalInterval必须大于0");
            if (config.TaskAction == null)
                throw new ArgumentException("TaskAction必须设置");
            if (config.UIDispatcher == null)
                throw new ArgumentException("UIDispatcher必须设置");
        }

        // 执行任务核心逻辑
        private async Task ExecuteTask(TimerConfig config)
        {
            // 在UI线程执行（如果涉及UI操作）
            await config.UIDispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await config.TaskAction().ConfigureAwait(false);
                    HandleSuccess(config.TaskId);
                }
                catch (Exception ex)
                {
                    HandleError(config, ex);
                }
            });
        }

        // 处理任务成功
        private void HandleSuccess(string taskId)
        {
            if (!_tasks.TryGetValue(taskId, out var task)) return;

            // 重置重试计数器
            _retryCounters[taskId] = 0;

            // 按正常间隔调度下次执行
            StartTimer(taskId, task.config.NormalInterval * 1000);
        }

        // 处理任务失败
        private void HandleError(TimerConfig config, Exception ex)
        {
            // 执行错误回调
            config.ErrorHandler?.Invoke(ex);

            // 更新重试计数器
            var retryCount = _retryCounters.AddOrUpdate(
                config.TaskId,
                1,
                (_, v) => v + 1);

            // 超过最大重试次数则停止任务
            if (retryCount >= config.MaxRetries)
            {
                StopTask(config.TaskId);
                return;
            }

            // 计算下次执行间隔
            var delayIndex = Math.Min(retryCount - 1, config.RetryIntervals.Length - 1);
            var delay = config.RetryIntervals[delayIndex] * 1000;

            // 调度重试
            StartTimer(config.TaskId, delay);
        }

        // 启动/重置定时器
        private void StartTimer(string taskId, int delayMilliseconds)
        {
            if (_disposed || !_tasks.TryGetValue(taskId, out var task)) return;

            // 使用Change方法保证线程安全
            task.timer.Change(delayMilliseconds, Timeout.Infinite);
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var task in _tasks.Values)
            {
                task.timer.Dispose();
            }
            _tasks.Clear();
            _retryCounters.Clear();
        }
        #endregion
    }
}

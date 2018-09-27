using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoThread
{
    public class CoThread : IDisposable
    {
        static CoThread()
        {
            ThreadPool.SetMaxThreads(1000, 100);
        }
        #region 实例化实现
        private CoThread()
        {
        }
        /// <summary>
        /// 线程级缓存
        /// </summary>
        [ThreadStatic]
        private static CoThread _innerThread = new CoThread();

        public static CoThread GetInstance() {
            return new CoThread();
            //return _innerThread;
        }
        #endregion

        private int count = 0;

        private ManualResetEvent resetEvent = new ManualResetEvent(false);

        private CoData _data = new CoData();
        public CoData Data { get { return _data; } }

        private  CoAction<object> GetSwappedAction<T>() {
            return p => {
                CoParam cp = p as CoParam;
                try
                {
                    cp.OneParamAction(null);
                    //todo
                }
                catch (Exception ex)
                {
                    CoException cex = new CoException("error", ex);
                    //todo
                    Data.ExceptionQueue.Enqueue(cex);
                }
                finally {
                    if (Interlocked.Decrement(ref count) == 0)
                        resetEvent.Set();
                }
            };
        }

        /// <summary>
        /// ss
        /// </summary>
        /// <example>
        ///     nothing
        /// </example>
        /// <![CDATA[ aa]]>
        /// <remarks>sdfsd </remarks>
        /// <param name="action">ss</param>
        public void Add(CoAction<CoData> action) {
            if (action == null) return;
            Interlocked.Increment(ref count);
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.GetSwappedAction<object>()), new CoParam()
            {
                OneParamAction = o=> { action(this.Data); },
                ParamType = CoParamType.NONE
            });
        }

        public void Add(CoAction<dynamic, CoData> action,dynamic param ) {
            if (action == null) return;
            Interlocked.Increment(ref count);
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.GetSwappedAction<dynamic>()), new CoParam()
            {
                OneParamAction = (p => { action(param, this.Data); }),
                ParamType = CoParamType.DYNAMIC,
                objectParam = param
            });
        }

        public void Add<T>(CoAction<T, CoData> action, T param)
        {
            if (action == null) return;
            Interlocked.Increment(ref count);
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.GetSwappedAction<T>()), new CoParam() {
                OneParamAction = (p => { action((T)param, this.Data); }),
                ParamType = CoParamType.TYPE,
                objectParam = param
            });
        }

        public void BatchAdd<T>(CoAction<T, CoData> action, IEnumerable<T> paramList) {
            if (paramList == null) return;
            foreach (T p in paramList)
                Add(action, p);
        }

        public void BatchAdd(CoAction<dynamic, CoData> action, IEnumerable<dynamic> paramList) {
            if (paramList == null) return;
            foreach (dynamic p in paramList)
                Add(action, p);
        }

        public void BatchAdd(CoAction<CoData> action, int count) {
            for (int i = 0; i < count; i++)
                Add(action);
        }

        /// <summary>
        /// 等待线程执行完
        /// </summary>
        public void Wait() {
            try
            {
                if(count!=0)
                    resetEvent.WaitOne();
            }
            catch (Exception ex)
            {
               //todo
            }
            finally
            {
                resetEvent.Close();
                resetEvent = new ManualResetEvent(false);
            }
        }

        public void ClearAll() {
            //todo
            _data = new CoData();
        }

        public void Dispose()
        {
            Wait();
        }
    }

    public class CoData {
        public ConcurrentDictionary<object, object> Map = new ConcurrentDictionary<object, object>();

        public ConcurrentQueue<object> Queue = new ConcurrentQueue<object>();

        public ConcurrentQueue<object> ExceptionQueue = new ConcurrentQueue<object>();
    }

    public class CoException : Exception {
        public CoException():base() { }

        public CoException(string message, Exception innerException) : base(message, innerException) { }
    }

    class CoParam {
        internal CoParamType ParamType { get; set; }
        internal object objectParam { get; set; }
        internal CoAction<object> OneParamAction { get; set; }
    }

    enum CoParamType {
        NONE = 1,
        DYNAMIC =2,
        TYPE =3
    }

    public delegate void CoAction<in T>(T obj);
    public delegate void CoAction<in T1, in T2>(T1 arg1, T2 arg2);

}

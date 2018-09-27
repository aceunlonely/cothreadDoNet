using System;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("hello good day");
            //TestEasyAction();
            //TestEasyActionT();
            TestMutiThread();
            Console.Read();
        }


        static void TestEasyAction() {
            CoThread.CoThread th = CoThread.CoThread.GetInstance();

            Console.WriteLine("");
            for (int i = 0; i < 100; i++)
            {
                th.Add(coData => {

                    //todo
                    coData.Queue.Enqueue(1);
                });
            }
            th.Wait();
            if (th.Data.Queue.Count == 100)
            {
                Console.WriteLine("Add Success");
            }
            else {
                Console.WriteLine("Add Failed");
            }
            th.ClearAll();

            for (int i = 0; i < 100; i++)
            {
                th.Add(coData => {
                    coData.Queue.Enqueue(1);
                });
                if(i%2 ==0)
                th.Add(coData => {
                    //coData.Queue.Enqueue(1);
                    throw new Exception("test");
                });
            }
            th.Wait();
            if (th.Data.Queue.Count == 100)
            {
                Console.WriteLine("Add1 Success");
            }
            else
            {
                Console.WriteLine("Add1 Failed");
            }
            if (th.Data.ExceptionQueue.Count == 50)
            {
                Console.WriteLine("Add2 Success");
            }
            else
            {
                Console.WriteLine("Add2 Failed");
            }

        }

        static void TestEasyActionT() {

            CoThread.CoThread th = CoThread.CoThread.GetInstance();

            for (int i = 0; i < 100; i++)
            {
                th.Add<T1>((t1, coData) => {
                    coData.Map.AddOrUpdate(t1.P1 + t1.P2, t1.P2,(k,v)=> t1.P2);
                    //todo
                }, new T1() { P1="test" ,P2 = i});
            }
            //等待全部走完
            th.Wait();
            // th.Data.ExceptionQueue.Count 
            if (th.Data.Map.Count == 100)
            {
                Console.WriteLine("Add 1 success:" + th.Data.Map.Count  + " | " + th.Data.ExceptionQueue.Count);
            }
            else
            {
                Console.WriteLine("Add 1 failed:" + th.Data.Map.Count + " | " + th.Data.ExceptionQueue.Count);
            }
            //清楚内部数据
            th.ClearAll();

            for (int i = 0; i < 100; i++) {
                th.Add((dynamic, coData) =>
                {
                    coData.Queue.Enqueue(dynamic.name);
                    coData.Map.AddOrUpdate(dynamic.age, dynamic.name, (k, v) => dynamic.name);
                }, new
                {
                    name = "lxy",
                    age = i
                });
            }
            th.Wait();
            if (th.Data.Map.Count == th.Data.Queue.Count)
            {
                Console.WriteLine("Add 2 success : " + th.Data.Map.Count);
            }
            else
            {
                Console.WriteLine("Add 2 failed");
            }
            th.ClearAll();
        }

        /// <summary>
        /// 测试多线程下的使用
        /// </summary>
        static void TestMutiThread() {
            CoThread.CoThread th = CoThread.CoThread.GetInstance();
            int icount = 20;
            for (int i = 0; i < icount; i++)
            {
                th.Add((dynamic, coData) =>
                {
                    coData.Queue.Enqueue(dynamic.name);
                    #region 内部嵌套调用
                    CoThread.CoThread th1 = CoThread.CoThread.GetInstance();
                    int jcount = 10;
                    for (int j = 0; j < jcount; j++)
                    {
                        th1.Add((d, cd) =>
                        {
                            cd.Queue.Enqueue(d.name);
                        }, new
                        {
                            name = "lisa",
                            age = j
                        });
                    }
                    th1.Wait();
                    if (th1.Data.Queue.Count != jcount)
                    {
                        Console.WriteLine("index " + dynamic.age + " Failed : " + th1.Data.Queue.Count);
                    }
                    else
                    {
                        Console.WriteLine("index " + dynamic.age + " success");
                    }
                    th1.ClearAll();
                    #endregion
                }, new
                {
                    name = "lxy",
                    age = i
                });
            }
            th.Wait();
            if (th.Data.Queue.Count != icount)
            {
                Console.WriteLine("main Failed");
            }
            else
            {
                Console.WriteLine("main success");
            }
            th.ClearAll();

        }
    }

    class T1 {
        public string P1 { get; set; }
        public int P2 { get; set; }
    }
}

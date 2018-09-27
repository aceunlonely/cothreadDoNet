# cothreadDoNet
co in c#

# how use
```c#
//get instance
CoThread.CoThread th = CoThread.CoThread.GetInstance();

for (int i = 0; i < 100; i++)
{
    //create and run thread
    th.Add<T>((param, coData) => {
        //here your code
        if(param.P2%2 ==0)
            throw new Exception(param.P2);
        coData.Map.AddOrUpdate(param.P1 + param.P2, param.P2,(k,v)=> param.P2);
    }, new { P1="test" ,P2 = i});
}
//wait all threads finished
th.Wait();
// th.Data.ExceptionQueue will stores all exceptions in your threads

// check the result 
if (th.Data.Map.Count == 50)
{
    Console.WriteLine("success:" + th.Data.Map.Count  + " | " + th.Data.ExceptionQueue.Count);
}

```
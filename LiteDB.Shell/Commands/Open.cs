using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LiteDB.Shell.Commands
{
    [Help(
        Category = "Database",
        Name = "open",
        Syntax = "open <filename|connectionString>",
        Description = "Open (or create) a new datafile. Can be used a single filename or a connection string with all supported parameters.",
        Examples = new string[] {
            "open mydb.db",
            "open filename=mydb.db; password=johndoe; initial=100Mb"
        }
    )]
    internal class Open : IShellCommand
    {
        public static System.Runtime.Caching.ObjectCache cache= System.Runtime.Caching.MemoryCache.Default;
        public static System.Net.Sockets.TcpListener listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any,5317);
        public bool IsCommand(StringScanner s)
        {
            return s.Scan(@"open\s+").Length > 0;
        }

        public static Env instance;
        public void Execute(StringScanner s, Env env, out bool b2, out object obj)
        {
            b2 = false;
            obj = null;

            var connectionString = new ConnectionString(s.Scan(@".+").TrimToNull());

            env.Open(connectionString);
            instance = env;
            listener.Start();
           var b=Load(env);
            System.Threading.Tasks.Task.Run(async () => await Acc());
            System.Threading.Tasks.Task.Run(async () => await Back(env));
        }
       private class BL
        {
            [BsonId]
            public int id { get; set; } = 0;


            public Dictionary<string, object> v { get; set; } = new Dictionary<string, object>();
            public Dictionary<string,System.DateTimeOffset> off { get; set; } = new Dictionary<string,System.DateTimeOffset>();
        }
        public async  System.Threading.Tasks.Task Acc()
        {
            while (true)
            {
                try
                {
                   
                       var client = await listener.AcceptTcpClientAsync();
                    // var v = new byte[1024];
                    if (client != null)
                        await System.Threading.Tasks.Task.Run(async () => { while (true) await Acc2(client); });
                }
                catch(Exception e)
                {
                    await Acc();
                }
            }
        }
        public async  System.Threading.Tasks.Task Acc2(System.Net.Sockets.TcpClient tcp)
        {
            try
            {
                byte[] bytes = new byte[1024];


                var stream = tcp.GetStream();
                string data;
                int i;
              
                // Loop to receive all the data sent by the client.
                i = stream.Read(bytes, 0, bytes.Length);

                while (i != 0)
                {
                    // Translate data bytes to a ASCII string.
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                   var rws= data.Split('|');

                    switch(rws[0])
                    {
                        case "set":
                            {

                                new SetValue().Execute(new StringScanner(string.Format("set.{0}.value {1}",  rws[1], rws[2])), instance,out var boo,out object obj);
                              if(obj!=null)
                                {
                                  if(obj is string str)
                                    {


                                        var le= System.Text.Encoding.ASCII.GetBytes(str);
                                        stream.Write(le, 0, le.Length);
                                    }
                                }
                              
                            }
                            break;
                        case "get":
                            {

                                new GetValue().Execute(new StringScanner(string.Format("get.{0}", rws[1])), instance,out var boo,out object obj
                                    );
                                if (obj != null)
                                {
                                    if (obj is string str)
                                    {


                                        var le = System.Text.Encoding.ASCII.GetBytes(str);
                                        stream.Write(le, 0, le.Length);
                                    }
                                }
                            }
                            break;
                        case "exp":
                            {

                                new Exp().Execute(new StringScanner(string.Format("exp.{0}.ss{1}", rws[1], rws[2])), instance, out var boo, out object obj
                                    );
                                if (obj != null)
                                {
                                    if (obj is string str)
                                    {


                                        var le = System.Text.Encoding.ASCII.GetBytes(str);
                                        stream.Write(le, 0, le.Length);
                                    }
                                }
                            }
                            break;


                    }

                    //var fs = string.Format("{0}.{1}.value {2}",rws[0], rws[1], rws[2]);
                    //new SetValue().Execute(new StringScanner(fs), instance);
                    // Process the data sent by the client.
                   // data = data.ToLower();

                   // byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                   // // Send back a response.
                   // stream.Write(msg, 0, msg.Length);
                   //Console.Write(String.Format("{0}", data));
              
                    i = stream.Read(bytes, 0, bytes.Length);

                }

            }
            catch(Exception e)
            {
                tcp.Close();
            }
        }
        public static int ID = 0;
        public async static System.Threading.Tasks.Task<bool> Load(Env env)
        {

            try
            {

                var obj = env.Engine.FindById("ky", ID);
                //   await System.Threading.Tasks.Task.Delay(1000);
                if (obj != null)
                {
                   var o= BsonMapper.Global.ToObject<BL>( obj);
                
                    foreach(var a in o.v)
                    {
                        var obj_late = new System.Runtime.Caching.CacheItemPolicy();
                        obj_late.AbsoluteExpiration = o.off[a.Key];
                        
                        obj_late.RemovedCallback = Exp.cb;
                        cache.Add(a.Key, a.Value, obj_late);

                        //   cache[a.Key] = a.Value;
                    }
                }
                if(cache==null)
                {
                    cache = System.Runtime.Caching.MemoryCache.Default;                }
            }
            catch
            {

            }
           // await Back(env);
            return true;

        }
        public static Dictionary<string, System.Runtime.Caching.CacheItemPolicy> kw = new Dictionary<string, System.Runtime.Caching.CacheItemPolicy>();
        public async static System.Threading.Tasks.Task<bool> Back(Env env)
        {
            while (true)
            {
                var bl = new BL();
                bl.id = ID;
                foreach (var r in cache.ToList())
                {

                    bl.v.Add(r.Key, r.Value);
                }
                foreach (var r in kw)
                {
                 
                
                    bl.off.Add(r.Key, r.Value.AbsoluteExpiration);
                }

                var doc = BsonMapper.Global.ToDocument<BL>(bl);
                env.Engine.Upsert("ky", doc);
                await System.Threading.Tasks.Task.Delay(1000);

            }
          
           
        }
    }
}
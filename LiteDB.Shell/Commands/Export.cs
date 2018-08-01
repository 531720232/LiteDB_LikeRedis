using System;
using System.IO;
using System.Linq;
namespace LiteDB.Shell.Commands
{
    [Help(
        Category = "Database",
        Name = "export",
        Syntax = "db.<collection>.export <filename>",
        Description = "Export collection as JSON file",
        Examples = new string[] {
            "db.customers.export C:\\Temp\\customers.json"
        }
    )]
    internal class Export : IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return s.Match(@"db.[\w$]*.export\s*");
        }

        public void Execute(StringScanner s, Env env, out bool b, out object obj)
        {
            b = false;
            obj = null;
            var colname = s.Scan(@"db.([\w$]*).export\s*", 1);
            var filename = s.Scan(".*").TrimToNull();

            var counter = env.Engine.Count(colname);
            var index = 0;

            using (var fs = new FileStream(filename, System.IO.FileMode.CreateNew))
            {
                using (var writer = new StreamWriter(fs))
                {
                    writer.WriteLine("[");

                    foreach (var doc in env.Engine.FindAll(colname))
                    {
                        var json = JsonSerializer.Serialize(doc, false, true);

                        index++;

                        writer.Write(json);

                        if (index < counter) writer.Write(",");

                        writer.WriteLine();
                    }

                    writer.Write("]");
                    writer.Flush();
                }
            }

            env.Display.WriteLine($"File {filename} created with {counter} documents");
        }
    }
    [Help(
      Category = "Database",
      Name = "保存key-value",
      Syntax = "set.<key>.value <filename>",
      Description = "保存key-value",
      Examples = new string[] {
            "set.customers.value C:\\Temp\\customers.json"
      }
  )]
    internal class SetValue : IShellCommand
    {
     
        public bool IsCommand(StringScanner s)
        {
            return s.Match(@"set.[\w$]*.value\s*");
        }
        internal class key_value
        {
           
            
            [LiteDB.BsonId]
            public string key { get; set; }
            public string value { get; set; }
        }
        public void Execute(StringScanner s, Env env, out bool b, out object sd)
        {
            b = false;
            sd = null;
            var key = s.Scan(@"set.([\w$]*).value\s*", 1);
            var value = s.Scan(".*").TrimToNull();
           
            var obj = new System.Runtime.Caching.CacheItemPolicy();
        

            Open.cache.Set(key, value, obj);
            //    var new_key = new key_value { key = key, value = value };

            //var doc=    LiteDB.BsonMapper.Global.ToDocument(typeof(key_value), new_key);

            sd = "OK";// value.Length;

        //      var bools= env.Engine.Upsert("key_value",(doc),BsonType.ObjectId);
        //    var wrrw =System.Linq.Enumerable.ToList(doc.Values)[0].RawValue;
            env.Display.WriteLine($"存储 {key} 值为 {value}");
        }
    }
    [Help(
     Category = "Database",
     Name = "读取key-value",
     Syntax = "get.<key>",
     Description = "读取key-value",
     Examples = new string[] {
            "get.customers"
     }
 )]
    internal class GetValue : IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return s.Match(@"get.[\w$]*");
        }

        public void Execute(StringScanner s, Env env, out bool b, out object obj)
        {
            b = false;
            obj = null;
            var key = s.Scan(@"get.([\w$]*)", 1);


          
         var rw=   Open.cache.Get(key);
            obj = rw;
            env.Display.WriteLine($"{rw}");
            //var keyw = env.Engine.FindById("key_value", key);
            //var js = keyw.AsDocument["value"];
            //env.Display.WriteLine($" {key} 值为{js.RawValue} ");//{keyw.Values.ToArray()[1].RawValue}");
        }
    }
    [Help(
    Category = "Database",
    Name = "设定key的过期期限",
    Syntax = "exp.<key>.ss <多少秒(后过期)>",
    Description = "设定key的过期期限",
    Examples = new string[] {
            "exp.customers.ss 200"
    }
)]
    internal class Exp : IShellCommand
    {

        public bool IsCommand(StringScanner s)
        {
            return s.Match(@"exp.[\w$]*.ss\s*");
        }
        internal class key_value
        {


            [LiteDB.BsonId]
            public string key { get; set; }
            public string value { get; set; }
        }
        public static System.Runtime.Caching.CacheEntryRemovedCallback cb = (x) =>
        {
           Console.WriteLine($" {x.CacheItem.Key} 已过期");
        };
        public void Execute(StringScanner s, Env env, out bool b, out object obj2)
        {
            b = false;
            obj2 = null;
            var key = s.Scan(@"exp.([\w$]*).ss\s*", 1);
            var value = s.Scan(".*").TrimToNull();
            int.TryParse(value, out var ss);

            var obj = new System.Runtime.Caching.CacheItemPolicy();
            var l = System.DateTime.Now.AddSeconds(ss);
            obj.AbsoluteExpiration = l;
            obj.RemovedCallback=cb;

        var os=    Open.cache.Get(key);//, value, obj);
            if(os==null)
            {
                env.Display.WriteError(new Exception($"{key}不存在"));
                return;
            }
            Open.cache.Set(key, os, obj);

            //    var new_key = new key_value { key = key, value = value };

            //var doc=    LiteDB.BsonMapper.Global.ToDocument(typeof(key_value), new_key);
            Open.kw[key] = obj;


            //      var bools= env.Engine.Upsert("key_value",(doc),BsonType.ObjectId);
            //    var wrrw =System.Linq.Enumerable.ToList(doc.Values)[0].RawValue;
            env.Display.WriteLine($"设定 {key} 过期时间为 {l}");
        }
    }

}
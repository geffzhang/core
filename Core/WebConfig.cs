﻿using System.Text;

namespace Greatbone.Core
{
    ///
    /// The configurative settings for a web service.
    /// 
    /// <remark>
    /// The strong semantic allows the web folder hierarchy to be established during object initialization.
    /// </remark>
    /// <code>
    /// public class FooService : WebService
    /// {
    ///     public FooService(WebConfig cfg) : base(cfg)
    ///     {
    ///         Make&lt;BarFolder&gt;("bar");
    ///     }
    /// }
    /// </code>
    ///
    public class WebConfig : WebFolderContext, IData
    {
        /// The shard identifier when one service is divided into many shards
        public string shard;

        /// The bind addresses in URI (scheme://host:port) format.
        public string addresses;

        /// The database connectivity attributes.
        public DbConfig db;

        /// The logging level, default to warning (3)
        public int logging = 3;

        // connection string
        volatile string connstr;

        public WebConfig(string name)
        {
            this.key = name;
        }

        ///
        /// Let the file directory name same as the service name.
        ///
        public override string Directory => key;

        ///
        /// The json object model.
        ///
        public JObj Model { get; private set; }

        public bool? File { get; private set; }

        /// The services/addresses that this service references or depends on.
        public JObj Cluster => Model?["cluster"];

        public JObj Extra => Model?["extra"];

        public void ReadData(IDataInput i, byte flags = 0)
        {
            i.Get(nameof(shard), ref shard);
            i.Get(nameof(addresses), ref addresses);
            i.Get(nameof(db), ref db);
            i.Get(nameof(logging), ref logging);
        }

        public void WriteData<R>(IDataOutput<R> o, byte flags = 0) where R : IDataOutput<R>
        {
            o.Put(nameof(shard), shard);
            o.Put(nameof(addresses), addresses);
            o.Put(nameof(db), db);
            o.Put(nameof(logging), logging);
        }

        ///
        /// Try to load from the $web.json file.
        ///
        public bool TryLoad()
        {
            string path = GetFilePath("$web.json");
            if (System.IO.File.Exists(path))
            {
                JObj jo = JsonUtility.FileToJObj(path);
                if (jo != null)
                {
                    Model = jo;
                    ReadData(jo); // override
                    return (File = true).Value;
                }
            }
            return (File = false).Value;
        }

        public string ConnectionString
        {
            get
            {
                if (connstr == null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Host=").Append(db.host);
                    sb.Append(";Port=").Append(db.port);
                    sb.Append(";Database=").Append(db.database ?? Key);
                    sb.Append(";Username=").Append(db.username);
                    sb.Append(";Password=").Append(db.password);
                    sb.Append(";Read Buffer Size=").Append(1024 * 32);
                    sb.Append(";Write Buffer Size=").Append(1024 * 32);
                    sb.Append(";No Reset On Close=").Append(true);

                    connstr = sb.ToString();
                }
                return connstr;
            }
        }
    }
}
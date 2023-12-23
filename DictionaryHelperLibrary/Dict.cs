using System;
using System.IO;
using System.Text.Json.Serialization;

namespace DictionaryHelperLibrary
{
    public class Dict
    {
        //反序列化用的无参构造函数
        public Dict() { }
        public Dict(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty("path");
            DictPath = path;
            Name = Path.GetFileName(Path.GetDirectoryName(path)) ?? Guid.NewGuid().ToString();
        }
        public string Name { get; set; } = string.Empty;
        public string DictPath { get; set; } = string.Empty;

        public bool _active { get; set; }
        [JsonIgnore]
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                if (value)
                {
                    EbwinHelper.EnActive(this);
                }
                else
                {
                    EbwinHelper.InActive(this);
                }
                _active = value;
            }
        }
        public int Priority { get; set; }
    }
}

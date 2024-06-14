namespace DictionaryHelperLibrary
{
    public class Dict
    {
        //反序列化用的无参构造函数
        public Dict() { }
        public Dict(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            DictPath = path;
            Name = Path.GetFileName(Path.GetDirectoryName(path)) ?? Guid.NewGuid().ToString();
        }
        public string Name { get; set; } = string.Empty;
        public string DictPath { get; set; } = string.Empty;

        public bool Active { get; set; } = true;

        public bool GetActive()
        {
            return Active;
        }
        public void SetActive(bool value)
        {
            if (value)
            {
                EbwinHelper.EnActive(this);
            }
            else
            {
                EbwinHelper.InActive(this);
            }
            Active = value;
        }
        public int Priority { get; set; }
    }
}

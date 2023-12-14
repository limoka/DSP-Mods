namespace crecheng.DSPModSave
{
    public class ModSaveData
    {
        public string name;
        public long begin;
        public long end;

        public ModSaveData(string name, long begin, long end)
        {
            this.name = name;
            this.begin = begin;
            this.end = end;
        }
    }
}
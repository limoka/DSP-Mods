namespace crecheng.DSPModSave
{
    internal class ModSaveEntry
    {
        public string name;
        public long begin;
        public long end;

        public ModSaveEntry(string name, long begin, long end)
        {
            this.name = name;
            this.begin = begin;
            this.end = end;
        }
    }
}
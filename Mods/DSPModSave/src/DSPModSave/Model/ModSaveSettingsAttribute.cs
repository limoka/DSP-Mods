using System;

namespace crecheng.DSPModSave
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModSaveSettingsAttribute : Attribute
    {
        private LoadOrder _loadOrder;
        
        public LoadOrder LoadOrder
        {
            get => _loadOrder;
            set => _loadOrder = value;
        }
    }
}
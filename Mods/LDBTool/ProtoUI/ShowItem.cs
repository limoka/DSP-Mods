using UnityExplorer;

namespace xiaoye97.UI
{
    public class ShowItem
    {
        object proto;
        public ShowItem(object proto)
        {
            this.proto = proto;
        }

        public void Show()
        {
            InspectorManager.Inspect(proto);
        }
    }
}

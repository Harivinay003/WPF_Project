using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VirtualEMS.Library;

namespace VirtualEMS.DataServices
{
    public interface iDbRepository
    {
        IEnumerable<AlarmTag> GetAlarmTags();
        IEnumerable<AlarmParameter> GetAlarmParameters();
        IEnumerable<Category> GetCategories();
        IEnumerable<FieldDevice> GetFieldDevices();
        IEnumerable<IODevice> GetIODevices();
        IEnumerable<SerialDevice> GetSerialDevices();
        IEnumerable<SerialDeviceDriver> GetSerialDeviceDrivers();
        IEnumerable<SerialDeviceParameter> GetSerialDeviceParameters();
        IEnumerable<SerialDeviceReadBlock> GetSerialDeviceReadBlocks();
        IEnumerable<SerialDeviceRegister> GetSerialDeviceRegisters();
        IEnumerable<Tag> GetTags();
        IEnumerable<Page> GetPages();
        IEnumerable<Node> GetNodes();
        IEnumerable<ItemCalculation> GetItemCalculations();
        IEnumerable<Group> GetGroups();
        IEnumerable<TrendTag> GetTrendTags();
        IEnumerable<TrendParameter> GetTrendParameters();
        IEnumerable<Node> GetChildNodes(int nodeId);

        // Live table related methods
        Live GetLiveValueByParameterId( int serDevId, int paramId);
        Live GetLiveValueByTagId(int tagId);
        Dictionary<int, double> GetLiveValuesByTagIds(IEnumerable<int> tagIds);
        Dictionary<int, double> GetLiveValuesByParameterIds(IEnumerable<Tuple<int, int>> parameterIds);


        // Page configuration
        Page GetPageById(int pageId);

        Page GetPageByName(string pageName);
        PageItem GetPageItem(int pageId, string controlName);
        PageItem GetPageItemById(int pageItemId);
        PageItem CreatePageItem(PageItem pageItem);
        PageItemTag GetPageItemTag(int pageItemId);
        void SavePageItemTag(PageItemTag pageItemTag);
        void DeletePageItemTag(int pageItemId);

        IEnumerable<PageItemTag> GetPageItemTags(int pageId);

    }
}

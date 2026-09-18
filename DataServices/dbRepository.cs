using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VirtualEMS.Library;
using Microsoft.Data.SqlClient;

namespace VirtualEMS.DataServices
{
    public class dbRepository : iDbRepository
    {
        private readonly AppDbContext context;

        public dbRepository(AppDbContext context)
        {
            this.context = context;
        }
        public IEnumerable<AlarmTag> GetAlarmTags()
        {
            return context.AlarmTags;
        }
        public IEnumerable<AlarmParameter> GetAlarmParameters()
        {
            return context.AlarmParameters;
        }

        public IEnumerable<Category> GetCategories()
        {
            return context.Categories;
        }

        public IEnumerable<FieldDevice> GetFieldDevices()
        {
            return context.FieldDevices.Include(f=>f.IODevice).Include(f=>f.Tags);
        }

        public IEnumerable<IODevice> GetIODevices()
        {
            return context.IODevices;
        }
        public IEnumerable<Page> GetPages()
        {
            return context.Pages
                .Include(p => p.PageItems)
                    .ThenInclude(pi => pi.PageItemTags)
                        .ThenInclude(pit => pit.Tag)
                .Include(p => p.PageItems)
                    .ThenInclude(pi => pi.PageItemParameters)
                        .ThenInclude(pip => pip.SerialDevice)
                .Include(p => p.PageItems)
                    .ThenInclude(pi => pi.PageItemParameters)
                        .ThenInclude(pip => pip.SerialDeviceParameter)
                .ToList();
        }

        public IEnumerable<SerialDevice> GetSerialDevices()
        {
            return context.SerialDevices;
        }

        public IEnumerable<SerialDeviceDriver> GetSerialDeviceDrivers()
        {
            return context.SerialDeviceDrivers;
        }

        public IEnumerable<SerialDeviceParameter> GetSerialDeviceParameters()
        {
            return context.SerialDeviceParameters;
        }

        public IEnumerable<SerialDeviceReadBlock> GetSerialDeviceReadBlocks()
        {
            return context.SerialDeviceReadBlocks;
        }

        public IEnumerable<SerialDeviceRegister> GetSerialDeviceRegisters()
        {
            return context.SerialDeviceRegisters;
        }

        public IEnumerable<Tag> GetTags()
        {
            return context.Tags;
        }

        public IEnumerable<Node> GetNodes()
        {
            return context.Nodes
                .Include(n=>n.Feeder)
                .Include(n=>n.FieldDevice);
        }

        public IEnumerable<ItemCalculation> GetItemCalculations()
        {
            return context.ItemCalculations.Include(I=>I.Components);
        }

        public IEnumerable<Group> GetGroups()
        {
            return context.Groups.Include(g => g.GroupItems)
                .ThenInclude(gi => gi.SerialDevice)
                .Include(g => g.GroupItems)
                .ThenInclude(gi => gi.FieldDevice);
        }

        public IEnumerable<TrendTag> GetTrendTags()
        {
            return context.TrendTags
                .Include(t => t.Tag)
                .Include(t => t.FieldDevice);
        }
        public IEnumerable<TrendParameter> GetTrendParameters()
        {
            return context.TrendParameters
                .Include(p => p.SerialDeviceParameter)
                .Include(p => p.SerialDevice);
        }

        public IEnumerable<Node> GetChildNodes(int nodeId)
        {
            return context.Nodes
                .Where(n => n.ParentId == nodeId)
                .Include(n => n.Feeder)
                .Include(n => n.FieldDevice);
        }

        // User related methods
        public VirtualEMS.Library.User GetUserByUsername(string username)
        {
            return context.Set<VirtualEMS.Library.User>().FirstOrDefault(u => u.Username == username);
        }

        public void UpdateUser(VirtualEMS.Library.User user)
        {
            context.Set<VirtualEMS.Library.User>().Update(user);
            context.SaveChanges();
        }

        // Live table related methods

        /// <summary>
        /// Gets a live value by the composite key (DevId, SerDevId, ParamId).
        /// ParamId contains TagId for EthernetDevice/PLC or SerialDeviceParameterId for Gateway.
        /// </summary>
        public Live GetLiveValueByParameterId( int serDevId, int paramId)
        {
            var connection = context.Database.GetDbConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT DevId, SerDevId, ParamId, Time, Value FROM Live WHERE SerDevId = @serDevId AND ParamId = @paramId";
                command.Parameters.Add(new SqlParameter("@serDevId", serDevId));
                command.Parameters.Add(new SqlParameter("@paramId", paramId));

                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (var result = command.ExecuteReader())
                {
                    if (result.Read())
                    {
                        return new Live
                        {
                            DevId = (int)result["DevId"],
                            SerDevId = (int)result["SerDevId"],
                            ParamId = (int)result["ParamId"],
                            Time = (DateTime)result["Time"],
                            Value = result["Value"] == DBNull.Value ? null : (double?)result["Value"]
                        };
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Gets a live value by Tag ID. For EthernetDevice/PLC devices.
        /// </summary>
        public Live GetLiveValueByTagId(int tagId)
        {
            var connection = context.Database.GetDbConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT DevId, SerDevId, ParamId, Time, Value FROM Live WHERE ParamId = @tagId AND SerDevId = 0";
                command.Parameters.Add(new SqlParameter("@tagId", tagId));

                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (var result = command.ExecuteReader())
                {
                    if (result.Read())
                    {
                        return new Live
                        {
                            DevId = (int)result["DevId"],
                            SerDevId = (int)result["SerDevId"],
                            ParamId = (int)result["ParamId"],
                            Time = (DateTime)result["Time"],
                            Value = result["Value"] == DBNull.Value ? null : (double?)result["Value"]
                        };
                    }
                }
            }
            return null;
        }

        public Dictionary<int, double> GetLiveValuesByTagIds(IEnumerable<int> tagIds)
        {
            if (tagIds == null || !tagIds.Any())
                return new Dictionary<int, double>();

            var liveValueActions = new Dictionary<int, double>();
            var connection = context.Database.GetDbConnection();
            using (var command = connection.CreateCommand())
            {
                var parameterList = string.Join(",", tagIds.Select((id, index) => $"@tagId{index}"));
                command.CommandText = $"SELECT DevId, SerDevId, ParamId, Time, Value FROM Live WHERE ParamId IN ({parameterList})";

                for (int i = 0; i < tagIds.Count(); i++)
                {
                    command.Parameters.Add(new SqlParameter($"@tagId{i}", tagIds.ElementAt(i)));
                }

                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        int paramId = (int)result["ParamId"];
                        double value = result["Value"] == DBNull.Value ? 0.0 : (double)result["Value"];
                        liveValueActions[paramId] = value;
                    }
                }
            }
            return liveValueActions;
        }

        public Dictionary<int, double> GetLiveValuesByParameterIds(IEnumerable<Tuple<int, int>> parameterIds)
        {
            if (parameterIds == null || !parameterIds.Any())
                return new Dictionary<int, double>();

            var liveValueActions = new Dictionary<int, double>();
            var connection = context.Database.GetDbConnection();
            using (var command = connection.CreateCommand())
            {
                var conditionList = new List<string>();
                int paramIndex = 0;

                foreach (var (devId, paramId) in parameterIds)
                {
                    conditionList.Add($"(DevId = @devId{paramIndex} AND ParamId = @paramId{paramIndex})");
                    command.Parameters.Add(new SqlParameter($"@devId{paramIndex}", devId));
                    command.Parameters.Add(new SqlParameter($"@paramId{paramIndex}", paramId));
                    paramIndex++;
                }

                command.CommandText = $"SELECT DevId, SerDevId, ParamId, Time, Value FROM Live WHERE {string.Join(" OR ", conditionList)}";

                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        int paramId = (int)result["ParamId"];
                        double value = result["Value"] == DBNull.Value ? 0.0 : (double)result["Value"];

                        liveValueActions[paramId] = value;
                    }
                }
            }
            return liveValueActions;
        }

        public Page GetPageById(int pageId)
        {
            return context.Pages
                .Include(p => p.PageItems)
                    .ThenInclude(pi => pi.PageItemTags)
                        .ThenInclude(pit => pit.Tag)

                .Include(p => p.PageItems)
                    .ThenInclude(pi => pi.PageItemParameters)
                        .ThenInclude(pip => pip.SerialDevice)

                .Include(p => p.PageItems)
                    .ThenInclude(pi => pi.PageItemParameters)
                        .ThenInclude(pip => pip.SerialDeviceParameter)

                .FirstOrDefault(p => p.Id == pageId);
        }

        public PageItem GetPageItem(
    int pageId,
    string controlName)
{
    return context.PageItems
        .Include(pi => pi.PageItemTags)
            .ThenInclude(pit => pit.Tag)

        .Include(pi => pi.PageItemParameters)
            .ThenInclude(pip => pip.SerialDevice)

        .Include(pi => pi.PageItemParameters)
            .ThenInclude(pip => pip.SerialDeviceParameter)

        .FirstOrDefault(pi =>
            pi.PageId == pageId &&
            pi.Name == controlName);
}

        public Page GetPageByName(string pageName)
        {
            return context.Pages
                .FirstOrDefault(p => p.Name == pageName);
        }

        public PageItem GetPageItemById(int pageItemId)
        {
            return context.PageItems
                .Include(pi => pi.PageItemTags)
                    .ThenInclude(pit => pit.Tag)

                .Include(pi => pi.PageItemParameters)
                    .ThenInclude(pip => pip.SerialDevice)

                .Include(pi => pi.PageItemParameters)
                    .ThenInclude(pip => pip.SerialDeviceParameter)

                .FirstOrDefault(pi => pi.Id == pageItemId);
        }

        public PageItem CreatePageItem(PageItem pageItem)
        {
            context.PageItems.Add(pageItem);
            context.SaveChanges();

            return pageItem;
        }

        public PageItemTag GetPageItemTag(int pageItemId)
        {
            return context.PageItemTags
                .Include(pit => pit.Tag)
                .FirstOrDefault(pit => pit.PageItemId == pageItemId);
        }

        public IEnumerable<PageItemTag> GetPageItemTags(int pageId)
        {
            return context.PageItemTags
                .Include(x => x.PageItem)
                .Include(x => x.Tag)
                .Where(x => x.PageItem.PageId == pageId)
                .ToList();
        }

        public void SavePageItemTag(PageItemTag pageItemTag)
        {
            var existing = context.PageItemTags
                .FirstOrDefault(x =>
                    x.PageItemId == pageItemTag.PageItemId);

            if (existing == null)
            {
                context.PageItemTags.Add(pageItemTag);
            }
            else
            {
                existing.TagId = pageItemTag.TagId;
                existing.Formula = pageItemTag.Formula;
            }

            context.SaveChanges();
        }

        public void DeletePageItemTag(int pageItemId)
        {
            var existing = context.PageItemTags
                .FirstOrDefault(x =>
                    x.PageItemId == pageItemId);

            if (existing != null)
            {
                context.PageItemTags.Remove(existing);
                context.SaveChanges();
            }
        }

    }
}

using System.Collections.Generic;
using System.Linq;
using VirtualEMS.DataServices;
using VirtualEMS.Library;

namespace WPFSCADA.Services
{
    public class ConfigurationService
    {
        private readonly iDbRepository _repository;

        public ConfigurationService(iDbRepository repository)
        {
            _repository = repository;
        }

        // ============================================================
        // CREATE PAGE ITEM IF IT DOES NOT EXIST
        // ============================================================

        public void EnsurePageItem(
            int pageId,
            string controlName,
            string configKey)
        {
            if (string.IsNullOrWhiteSpace(controlName))
                return;

            var pageItem =
                _repository.GetPageItem(
                    pageId,
                    controlName);

            if (pageItem != null)
                return;

            _repository.CreatePageItem(
                new PageItem
                {
                    PageId = pageId,
                    Name = controlName,
                    Title = configKey,
                    Description =
                        $"Configuration for {controlName}"
                });
        }

        // ============================================================
        // GET ASSIGNED TAG FOR ONE CONTROL
        // ============================================================

        public string GetAssignedTag(
            int pageId,
            string controlName)
        {
            if (string.IsNullOrWhiteSpace(controlName))
                return null;

            var pageItem =
                _repository.GetPageItem(
                    pageId,
                    controlName);

            if (pageItem == null)
                return null;

            var pageItemTag =
                _repository.GetPageItemTag(
                    pageItem.Id);

            if (pageItemTag?.Tag == null)
                return null;

            return pageItemTag.Tag.Name;
        }

        // ============================================================
        // GET ASSIGNED TAGS FOR MULTIPLE CONTROLS
        // ============================================================

        public Dictionary<string, string>
            LoadAssignedTags(
                int pageId,
                IEnumerable<string> controlNames)
        {
            var assignedTags =
                new Dictionary<string, string>();

            if (controlNames == null)
                return assignedTags;

            foreach (var controlName in controlNames)
            {
                if (string.IsNullOrWhiteSpace(controlName))
                    continue;

                var tagName =
                    GetAssignedTag(
                        pageId,
                        controlName);

                if (string.IsNullOrWhiteSpace(tagName))
                    continue;

                assignedTags[controlName] =
                    tagName;
            }

            return assignedTags;
        }

        // ============================================================
        // LOAD CONTROL NAME -> TAG ID
        // ============================================================

        public Dictionary<string, int>
            LoadControlTagMappings(
                int pageId)
        {
            var mappings =
                new Dictionary<string, int>();

            var pageItemTags =
                _repository
                    .GetPageItemTags(pageId)
                    .ToList();

            foreach (var item in pageItemTags)
            {
                if (item.PageItem == null)
                    continue;

                mappings[item.PageItem.Name] =
                    item.TagId;
            }

            return mappings;
        }

        // ============================================================
        // GET TAG ID
        // ============================================================

        public int? GetTagId(
            Dictionary<string, int> mappings,
            string controlName)
        {
            if (mappings == null)
                return null;

            if (string.IsNullOrWhiteSpace(controlName))
                return null;

            if (mappings.TryGetValue(
                    controlName,
                    out int tagId))
            {
                return tagId;
            }

            return null;
        }

        // ============================================================
        // GET PAGE
        // ============================================================

        public Page GetPage(
            string pageName)
        {
            if (string.IsNullOrWhiteSpace(pageName))
                return null;

            return _repository
                .GetPageByName(pageName);
        }

        // ============================================================
        // GET PAGE ITEM
        // ============================================================

        public PageItem GetPageItem(
            int pageId,
            string controlName)
        {
            if (string.IsNullOrWhiteSpace(controlName))
                return null;

            return _repository.GetPageItem(
                pageId,
                controlName);
        }
    }
}
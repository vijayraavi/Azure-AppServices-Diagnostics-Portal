using Diagnostics.ScriptHost;
using Diagnostics.ScriptHost.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Diagnostics.Tests.ScriptHostTests
{
    public class CacheServiceTests
    {
        private CompilationCache<string, Definition> _cache;

        public CacheServiceTests()
        {
            _cache = new CompilationCache<string, Definition>();
        }

        [Fact]
        public void TestAddItemToCache()
        {
            Definition item = BuildDefinitionAttribute();
            _cache.AddOrUpdate(item.Id, item);
            Assert.True(_cache.TryGetValue(item.Id, out Definition itemInCache));
            Assert.Equal(item, itemInCache);
        }

        [Fact]
        public void TestUpdateItemInCache()
        {
            Definition item = BuildDefinitionAttribute();
            _cache.AddOrUpdate(item.Id, item);

            Definition updatedItem = item;
            updatedItem.Name = "update name";
            _cache.AddOrUpdate(updatedItem.Id, updatedItem);

            Assert.True(_cache.TryGetValue(item.Id, out Definition itemInCache));
            Assert.Equal(updatedItem, itemInCache);

            List<Definition> items = _cache.GetAll().ToList();
            Assert.Single(items);
        }

        [Fact]
        public void TestRemoveItemFromCache()
        {
            Definition item = BuildDefinitionAttribute();
            _cache.AddOrUpdate(item.Id, item);

            Assert.True(_cache.RemoveValue(item.Id, out Definition removedItem));
            Assert.Equal(item, removedItem);
            Assert.Empty(_cache.GetAll());
        }

        private Definition BuildDefinitionAttribute()
        {
            return new Definition()
            {
                Id = "mockId",
                Name = "mockName",
                Description = "some description"
            };
        }
    }
}

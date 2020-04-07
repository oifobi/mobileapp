using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toggl.Shared.Extensions;
using Toggl.Shared.Models;
using Toggl.Storage.Realm.Models;

namespace Toggl.Storage.Realm.Migrations
{
    public static class MigrationExtensions
    {
        public static void GenerateUniqueId<T>(this Migration migration) where T : RealmObject, IModifiableId
        {
            migration.NewRealm
                .All<T>()
                .ForEach(entity => entity.UniqueId = Guid.NewGuid().ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CommonSupport
{
    public interface IDBPersistent
    {
        /// <summary>
        /// Mark not persistenct, since its persistence is automated.
        /// </summary>
        [DBPersistence(false)]
        long? Id { get; set;  }

        event GeneralHelper.GenericDelegate<IDBPersistent> PersistenceDataUpdatedEvent;
    }
}

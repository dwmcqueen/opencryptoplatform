using System;
using System.Collections.Generic;
using System.Text;

namespace CommonSupport
{
    /// <summary>
    /// Indicate with this attribute that persistance is desired on any child class of IDBPersistent
    /// - if applied on class indicates the default persistency for classes properties.
    /// - if applied to property indicates specific instructions how to map property to DB
    /// </summary>
    public class DBPersistenceAttribute : Attribute
    {
        public enum PersistenceTypeEnum
        {
            None,
            Default,
            Binary
        }

        public enum PersistenceModeEnum
        {
            Default, // Both read and write access.
            ReadOnly, // Read only access (get param).
        }

        PersistenceTypeEnum _persistenceType = PersistenceTypeEnum.Default;
        /// <summary>
        /// 
        /// </summary>
        public PersistenceTypeEnum PersistenceType
        {
            get { return _persistenceType; }
        }

        PersistenceModeEnum _persistenceMode = PersistenceModeEnum.Default;
        /// <summary>
        /// 
        /// </summary>
        public PersistenceModeEnum PersistenceMode
        {
            get { return _persistenceMode; }
        }

        //string _mappedName;
        ///// <summary>
        ///// 
        ///// </summary>
        //public string MappedName
        //{
        //    get { return _mappedName; }
        //}

        //public bool IsNameMapped
        //{
        //    get { return string.IsNullOrEmpty(_mappedName) == false; }
        //}

        ///// <summary>
        ///// Perform a name mapped persistence.
        ///// </summary>
        //public DBPersistenceAttribute(string mappedName)
        //{
        //    _mappedName = mappedName;
        //}

        /// <summary>
        /// Perform a default persistance, 
        /// </summary>
        public DBPersistenceAttribute(bool persist)
        {
            if (persist == false)
            {
                _persistenceType = PersistenceTypeEnum.None;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DBPersistenceAttribute(PersistenceTypeEnum persistType)
        {
            _persistenceType = persistType;
        }

        /// <summary>
        /// 
        /// </summary>
        public DBPersistenceAttribute(PersistenceModeEnum persistenceMode)
        {
            _persistenceMode = persistenceMode;
        }

        /// <summary>
        /// 
        /// </summary>
        public DBPersistenceAttribute(PersistenceTypeEnum persistType, PersistenceModeEnum persistenceMode)
        {
            _persistenceType = persistType;
            _persistenceMode = persistenceMode;
        }
    }
}

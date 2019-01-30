using System;

namespace WK.FPTest.Models.Tollbridge
{
    public sealed class CosmosDbSettingsCtx : IEquatable<CosmosDbSettingsCtx>
    {
        public Uri Uri { get; set; }
        public string WriteMasterKey { get; set; }
        public string ReadMasterKey { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }

        public bool Equals(CosmosDbSettingsCtx other)
        {
            if (other == null) return false;
            return string.Equals(WriteMasterKey, other.WriteMasterKey)
                && string.Equals(ReadMasterKey, other.ReadMasterKey)
                && string.Equals(DatabaseName, other.DatabaseName)
                && string.Equals(CollectionName, other.CollectionName)
                && Equals(Uri, other.Uri);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as CosmosDbSettingsCtx);
        }

        public override int GetHashCode()
        {
            return (WriteMasterKey + ReadMasterKey + DatabaseName + CollectionName + Uri).GetHashCode();
        }
    }
}

namespace WebUI.Infrastructure
{
    using System;
    using System.Xml.Serialization;

    [Serializable]
    public class Content
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("guid")]
        public Guid Key { get; set; }

        [XmlAttribute("createDate")]
        public DateTime CreateDate { get; set; }

        [XmlAttribute("updateDate")]
        public DateTime UpdateDate { get; set; }

        [XmlAttribute("creatorID")]
        public int CreatorId { get; set; }

        [XmlAttribute("nodeName")]
        public string Name { get; set; }

        [XmlAttribute("parentID")]
        public int ParentId { get; set; }

        [XmlAttribute("parentGuid")]
        public string ParentGuid { get; set; }

        [XmlAttribute("sortOrder")]
        public int SortOrder { get; set; }

        [XmlAttribute("nodeTypeAlias")]
        public string ContentTypeAlias { get; set; }

        [XmlAttribute("nodeType")]
        public int ContentTypeId { get; set; }

        [XmlAttribute("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        [XmlAttribute("expireDate")]
        public DateTime ExpireDate { get; set; }

        [XmlAttribute("writerID")]
        public int WriterId { get; set; }

        [XmlAttribute("templateID")]
        public int TemplateId { get; set; }

        ////public PropertyCollection Properties { get; set; }

        ////public ITemplate Template { get; set; }

        ////public IContentType ContentType { get; private set; }

        ////public int Level { get; set; }

        ////public Guid Version { get; private set; }

        ////public string Path { get; set; }

        ////public bool HasIdentity { get; private set; }

        ////public bool Published { get; private set; }

        ////public string Language { get; set; }

        ////public IEnumerable<PropertyGroup> PropertyGroups { get; private set; }

        ////public IEnumerable<PropertyType> PropertyTypes { get; private set; }

        ////public bool Trashed { get; private set; }

        ////public ContentStatus Status { get; private set; }

        ////public void ChangeContentType(IContentType contentType)
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////public void ChangeContentType(IContentType contentType, bool clearProperties)
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////public void ChangePublishedState(PublishedState state)
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////public bool HasProperty(string propertyTypeAlias)
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////public object GetValue(string propertyTypeAlias)
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////public TPassType GetValue<TPassType>(string propertyTypeAlias)
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////public void SetValue(string propertyTypeAlias, object value)
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////public bool IsValid()
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////public void ChangeTrashedState(bool isTrashed, int parentId = -20)
        ////{
        ////    throw new NotImplementedException();
        ////}
    }
}
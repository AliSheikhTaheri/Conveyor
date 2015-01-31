namespace AST.ContentConveyor7
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
    }
}
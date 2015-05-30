using MediaBrowser.Model.Plugins;
using System;
using System.Collections.Generic;

namespace DVBViewer.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public List<TunerUserConfiguration> TunerHostsConfiguration { get; set; }

        public PluginConfiguration()
        {

        }
    }

    public class ConfigurationField
    {
        public FieldType Type { get; set; }
        public string defaultValue { get; set; }
        public string Description { get; set; }
        public string Label { get; set; }
        public string ParentId { get; set; }
        public string Name { get; set; }
    }

    public class SelectOptions
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public string Color { get; set; }
    }

    public enum FieldType
    {
        Private = 0,
        Hidden = 1,
        Text = 2,
        Checkbox = 3,
        Select = 4
    }

    public class SelectField : ConfigurationField
    {
        public SelectOptions Options { get; set; }
    }
    public class UserField
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class TunerUserConfiguration
    {
        public string ServerId { get; set; }
        public string ServerName { get; set; }
        public string ServerType { get; set; }
        public List<UserField> UserFields { get; set; }

    }

    public class FieldBuilder
    {
        public String Type { get; set; }
        public ConfigurationField[] DefaultConfigurationFields { get; set; }

    }


}
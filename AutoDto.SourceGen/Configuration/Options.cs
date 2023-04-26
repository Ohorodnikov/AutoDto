using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.Configuration;

internal interface IOption
{
    string Key { get; }
    Action<string> Setter { get; }

    void SetDefault();
}

internal class BaseOption<TValue> : IOption
{
    private readonly TValue _defaultValue;

    public BaseOption(string key, TValue defaultValue, Action<string> setter)
    {
        Key = key;
        _defaultValue = defaultValue;
        Setter = setter;
    }

    public string Key { get; }
    public Action<string> Setter { get; }

    public void SetDefault() 
    { 
        if (_defaultValue != null)
            Setter(_defaultValue.ToString()); 
    }
}

/// <summary>
/// Just to avoid writing generic type
/// </summary>
internal static class OptionFactory
{
    public static BaseOption<T> New<T>(string key, T defaultValue, Action<string> setter) 
        => new BaseOption<T>(key, defaultValue, setter);
}

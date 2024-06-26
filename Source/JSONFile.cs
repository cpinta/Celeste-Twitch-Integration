﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

public class JSONFile
{
    public string path { get; private set; }
    Dictionary<string, string> values = new Dictionary<string, string>();

    public JSONFile(string INIPath)
    {
        path = INIPath;
        LoadJson(path);
    }

    public void LoadJson(string path)
    {
        using FileStream openStream = File.OpenRead(path);
        values = JsonSerializer.Deserialize<Dictionary<string, string>>(openStream);
    }

    public string Get(string key)
    {
        return values[key];
    }
}
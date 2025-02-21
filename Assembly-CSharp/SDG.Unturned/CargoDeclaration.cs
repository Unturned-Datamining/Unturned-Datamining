using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class CargoDeclaration
{
    internal List<string> lines = new List<string>();

    public void Append(string key, bool value)
    {
        string text = (value ? "yes" : "no");
        lines.Add("| " + key + " = " + text);
    }

    public void Append(string key, string value)
    {
        lines.Add("| " + key + " = " + value);
    }

    public void Append(string key, Guid guid)
    {
        lines.Add($"| {key} = {guid:N}");
    }

    public void Append(string key, byte value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, ushort value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, uint value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, ulong value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, sbyte value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, short value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, int value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, long value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, float value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, double value)
    {
        lines.Add($"| {key} = {value}");
    }

    public void Append(string key, Color32 value)
    {
        lines.Add("| " + key + " = " + Palette.hex(value));
    }

    public void Append(string key, object value)
    {
        lines.Add($"| {key} = {value}");
    }
}

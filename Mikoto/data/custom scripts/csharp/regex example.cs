using System;
using System.Text.RegularExpressions;

string Process(string input)
{
    return Regex.Replace(input, @"\s+", "");
}
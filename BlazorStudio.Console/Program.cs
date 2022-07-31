﻿// See https://aka.ms/new-console-template for more information

using System.Text;

var builder = new StringBuilder();

var rows = 600;
var columns = 600;

var columnMarkerSpacing = 100;

var spacer = ' ';

for (int i = 0; i < rows; i++)
{
    var rowMarker = $"// {i}";

    builder.Append(rowMarker);

    for (int j = rowMarker.Length; j < columns; j++)
    {
        if (j % columnMarkerSpacing == 0)
        {
            var columnMarker = $"col: {j}";
            builder.Append(columnMarker);

            j += columnMarker.Length;
        }
        else
        {
            builder.Append(spacer);
        }
    }

    builder.AppendLine();
}

File.WriteAllText("../testFile.txt", builder.ToString());

Console.WriteLine("Done");

await Task.Delay(1000);
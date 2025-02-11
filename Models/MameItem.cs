using Microsoft.Data.Sqlite;

namespace MameLauncher.Models;

public record MameItem
{
	public MameItem(string name, string src, string romof, string desc, string year, string manufact) =>
			(ShortName, SourceFile, Parent, Description, Year, Manufacturer) = (name, src, romof, desc, year, manufact);
	public MameItem(SqliteDataReader r) =>
		(ShortName, SourceFile, Parent, Description, Year, Manufacturer) = (r.GetString(0), r.GetString(1), r.GetString(2),
			r.GetString(3), r.GetString(4), r.GetString(5));

	public string ShortName { get; }
	public string SourceFile { get; }
	public string Parent { get; }
	public string Description { get; }
	public string Year { get; }
	public string Manufacturer { get; }
}

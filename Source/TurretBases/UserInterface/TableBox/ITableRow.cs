namespace TurretBases.UserInterface.TableBox
{
	public interface ITableRow
	{
		/// <summary>
		/// Shown when mouse is over the row.
		/// </summary>
		string[]? Tooltip { get; }

		bool Enabled { get; }

		bool HasColumn(TableColumn column);

		string this[TableColumn key]
		{
			get;
			set;
		}
	}
}
